using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Top-level navigation for the Main Menu scene: switches between the main panel and the
// Seed Entry / Options / Controls sub-panels, and owns New Game / Exit since those don't
// belong to any single sub-panel.
[RequireComponent(typeof(AudioSource))]
public class MainMenuController : MonoBehaviour
{
  [Header("Panels")]
  [SerializeField] GameObject mainPanel;
  [SerializeField] SeedEntryPanelController seedEntryPanel;
  [SerializeField] SettingsPanelController optionsPanel;
  [SerializeField] ControlsPanelController controlsPanel;

  [Header("Buttons")]
  [SerializeField] Button newGameButton;
  [SerializeField] Button loadSeedButton;
  [SerializeField] Button optionsButton;
  [SerializeField] Button controlsButton;
  [SerializeField] Button exitButton;

  [Header("Audio")]
  [SerializeField] AudioMixer mixer;
  [SerializeField] AudioClip menuOpenClip;
  [SerializeField] AudioClip menuCloseClip;

  [Header("Scenes")]
  [Tooltip("Gameplay scene loaded on New Game / Load Seed. Must be added to Build Settings.")]
  [SerializeField] string gameplaySceneName = "Dungeon";

  [Header("Intro Transition")]
  [Tooltip("Full-screen fader that darkens the menu while the NPC walks forward.")]
  [SerializeField] ScreenFader screenFader;
  [Tooltip("Seconds for the screen to fade to black; the NPC walk runs for the same duration.")]
  [SerializeField, Min(0f)] float introFadeDuration = 2f;
  [Tooltip("Seconds for the Main Menu to fade in when the scene loads.")]
  [SerializeField, Min(0f)] float menuFadeInDuration = 1f;
  [SerializeField] PlayerAnimationDriver introAnimationDriver;
  [SerializeField] ScriptedForwardWalker introWalker;

  AudioSource audioSource;

  // Caches audio, applies saved volume, registers buttons, and opens the main panel.
  void Awake()
  {
    audioSource = GetComponent<AudioSource>();
    AudioMixerVolume.ApplySaved(mixer);

    if (screenFader == null)
    {
      screenFader = CreateMenuFadeOverlay();
    }

    newGameButton.onClick.AddListener(OnNewGame);
    loadSeedButton.onClick.AddListener(OnLoadSeed);
    optionsButton.onClick.AddListener(OnOptions);
    controlsButton.onClick.AddListener(OnControls);
    exitButton.onClick.AddListener(OnExit);

    ShowMain(false);
  }

  // Starts the menu audio and screen fade-in.
  void Start()
  {
    StartCoroutine(FadeInAudioAndScreen());
  }

  // Creates a screen-space fade overlay when none is assigned.
  ScreenFader CreateMenuFadeOverlay()
  {
    GameObject overlayObject = new GameObject("Main Menu Fade Overlay");
    Canvas canvas = overlayObject.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 1000;
    overlayObject.AddComponent<CanvasScaler>();

    Image image = overlayObject.AddComponent<Image>();
    RectTransform rectTransform = image.rectTransform;
    rectTransform.anchorMin = Vector2.zero;
    rectTransform.anchorMax = Vector2.one;
    rectTransform.offsetMin = Vector2.zero;
    rectTransform.offsetMax = Vector2.zero;

    ScreenFader fader = overlayObject.AddComponent<ScreenFader>();
    overlayObject.transform.SetParent(transform, false);
    return fader;
  }

  // Shows the main menu and plays the return sound.
  public void ShowMain()
  {
    ShowMain(true);
  }

  // Activates the main panel and hides all menu sub-panels.
  void ShowMain(bool playCloseSound)
  {
    mainPanel.SetActive(true);
    seedEntryPanel.gameObject.SetActive(false);
    optionsPanel.gameObject.SetActive(false);
    controlsPanel.gameObject.SetActive(false);
    if (playCloseSound)
    {
      PlayClip(menuCloseClip);
    }
  }

  // Stores a fresh seed and starts the gameplay transition.
  void OnNewGame()
  {
    PendingSeed.Set(System.Environment.TickCount);
    BeginGameplayTransition();
  }

  // Opens the seed-entry panel.
  void OnLoadSeed()
  {
    OpenSubPanel(seedEntryPanel.gameObject);
  }

  // Called here for New Game, and by SeedEntryPanelController once a seed has been confirmed -
  // both paths share the same fade-out / scripted-walk / scene-load sequence.
  // Disables menu buttons and starts the intro fade and scene-load sequence.
  public void BeginGameplayTransition()
  {
    SetButtonsInteractable(false);
    StartCoroutine(GameplayTransitionRoutine());
  }

  // Opens the options panel.
  void OnOptions()
  {
    OpenSubPanel(optionsPanel.gameObject);
  }

  // Opens the controls panel.
  void OnControls()
  {
    OpenSubPanel(controlsPanel.gameObject);
  }

  // Stops play mode in the editor or quits the application in a build.
  void OnExit()
  {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }

  // Hides the main panel and opens a selected sub-panel.
  void OpenSubPanel(GameObject panel)
  {
    mainPanel.SetActive(false);
    panel.SetActive(true);
    PlayClip(menuOpenClip);
  }

  // Plays a menu audio clip when one is configured.
  void PlayClip(AudioClip clip)
  {
    if (clip == null) return;

    audioSource.PlayOneShot(clip);
  }

  // Enables or disables every top-level menu button.
  void SetButtonsInteractable(bool interactable)
  {
    newGameButton.interactable = interactable;
    loadSeedButton.interactable = interactable;
    optionsButton.interactable = interactable;
    controlsButton.interactable = interactable;
    exitButton.interactable = interactable;
  }

  // Runs the intro walk, screen/audio fade, and gameplay scene load.
  IEnumerator GameplayTransitionRoutine()
  {
    float savedMasterVolume = AudioMixerVolume.GetSaved(AudioMixerVolume.MasterParam);

    if (introAnimationDriver != null) introAnimationDriver.enabled = false;
    if (introWalker != null)
    {
      introWalker.StartWalking();
      introWalker.enabled = true;
    }

    if (screenFader != null)
    {
      yield return FadeOutAudioAndScreen(savedMasterVolume);
    }
    else
    {
      yield return FadeAudio(savedMasterVolume, 0f, introFadeDuration);
    }

    SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
  }

  // Runs screen and audio fade-out operations together.
  IEnumerator FadeOutAudioAndScreen(float savedMasterVolume)
  {
    Coroutine screenFade = screenFader.FadeOutAndStart(introFadeDuration);
    yield return FadeAudio(savedMasterVolume, 0f, introFadeDuration);
    yield return screenFade;
  }

  // Interpolates master volume during a menu transition.
  IEnumerator FadeAudio(float from, float to, float duration)
  {
    float elapsed = 0f;
    while (elapsed < duration)
    {
      elapsed += Time.deltaTime;
      float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
      AudioMixerVolume.SetRuntime(mixer, AudioMixerVolume.MasterParam, Mathf.Lerp(from, to, t));
      yield return null;
    }

    AudioMixerVolume.SetRuntime(mixer, AudioMixerVolume.MasterParam, to);
  }

  // Starts the menu from silence and transparency, then restores saved volume.
  IEnumerator FadeInAudioAndScreen()
  {
    AudioMixerVolume.SetRuntime(mixer, AudioMixerVolume.MasterParam, 0f);
    Coroutine screenFade = screenFader.FadeInAndStart(menuFadeInDuration);
    yield return FadeAudio(0f, AudioMixerVolume.GetSaved(AudioMixerVolume.MasterParam), menuFadeInDuration);
    yield return screenFade;
  }
}
