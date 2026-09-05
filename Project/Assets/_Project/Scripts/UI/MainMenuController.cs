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
  [SerializeField] PlayerAnimationDriver introAnimationDriver;
  [SerializeField] ScriptedForwardWalker introWalker;

  AudioSource audioSource;

  void Awake()
  {
    audioSource = GetComponent<AudioSource>();
    AudioMixerVolume.ApplySaved(mixer);

    newGameButton.onClick.AddListener(OnNewGame);
    loadSeedButton.onClick.AddListener(OnLoadSeed);
    optionsButton.onClick.AddListener(OnOptions);
    controlsButton.onClick.AddListener(OnControls);
    exitButton.onClick.AddListener(OnExit);

    ShowMain();
  }

  public void ShowMain()
  {
    mainPanel.SetActive(true);
    seedEntryPanel.gameObject.SetActive(false);
    optionsPanel.gameObject.SetActive(false);
    controlsPanel.gameObject.SetActive(false);
    PlayClip(menuCloseClip);
  }

  void OnNewGame()
  {
    PendingSeed.Set(System.Environment.TickCount);
    BeginGameplayTransition();
  }

  void OnLoadSeed()
  {
    OpenSubPanel(seedEntryPanel.gameObject);
  }

  // Called here for New Game, and by SeedEntryPanelController once a seed has been confirmed -
  // both paths share the same fade-out / scripted-walk / scene-load sequence.
  public void BeginGameplayTransition()
  {
    SetButtonsInteractable(false);
    StartCoroutine(GameplayTransitionRoutine());
  }

  void OnOptions()
  {
    OpenSubPanel(optionsPanel.gameObject);
  }

  void OnControls()
  {
    OpenSubPanel(controlsPanel.gameObject);
  }

  void OnExit()
  {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }

  void OpenSubPanel(GameObject panel)
  {
    mainPanel.SetActive(false);
    panel.SetActive(true);
    PlayClip(menuOpenClip);
  }

  void PlayClip(AudioClip clip)
  {
    if (clip == null) return;

    audioSource.PlayOneShot(clip);
  }

  void SetButtonsInteractable(bool interactable)
  {
    newGameButton.interactable = interactable;
    loadSeedButton.interactable = interactable;
    optionsButton.interactable = interactable;
    controlsButton.interactable = interactable;
    exitButton.interactable = interactable;
  }

  IEnumerator GameplayTransitionRoutine()
  {
    if (introAnimationDriver != null) introAnimationDriver.enabled = false;
    if (introWalker != null)
    {
      introWalker.StartWalking();
      introWalker.enabled = true;
    }

    if (screenFader != null)
    {
      yield return screenFader.FadeOutAndStart(introFadeDuration);
    }

    SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
  }
}
