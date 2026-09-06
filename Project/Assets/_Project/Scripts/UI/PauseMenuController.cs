using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Owns pause state, player input locking, panel switching, and the quit-to-menu transition.
[RequireComponent(typeof(AudioSource))]
public class PauseMenuController : MonoBehaviour
{
  [Header("Input")]
  [SerializeField] InputActionReference pauseAction;
  [SerializeField] InputActionAsset playerControls;
  [SerializeField] string playerActionMapName = "Player";

  [Header("Panels")]
  [SerializeField] GameObject background;
  [SerializeField] GameObject pausePanel;
  [SerializeField] GameObject optionsPanel;

  [Header("Audio")]
  [SerializeField] AudioClip menuOpenClip;
  [SerializeField] AudioClip menuCloseClip;

  [Header("Scenes")]
  [Tooltip("Scene loaded when quitting to the main menu. Must be added to Build Settings.")]
  [SerializeField] string mainMenuSceneName = "MainMenu";

  [Header("Transition")]
  [SerializeField] AudioMixer mixer;
  [SerializeField] ScreenFader screenFader;
  [SerializeField, Min(0f)] float menuTransitionFadeDuration = 1f;

  AudioSource audioSource;
  bool isTransitioning;

  public bool IsPaused { get; private set; }

  // Caches audio and initializes the hidden menu and locked cursor state.
  void Awake()
  {
    audioSource = GetComponent<AudioSource>();
    SetMenuVisible(false);
    SetCursorLocked(true);
  }

  // Enables the pause action and subscribes to its callback.
  void OnEnable()
  {
    if (pauseAction == null) return;

    pauseAction.action.Enable();
    pauseAction.action.performed += HandlePausePressed;
  }

  // Removes the pause callback and disables the pause action.
  void OnDisable()
  {
    if (pauseAction == null) return;

    pauseAction.action.performed -= HandlePausePressed;
    pauseAction.action.Disable();
  }

  // Toggles between paused and resumed gameplay.
  void HandlePausePressed(InputAction.CallbackContext context)
  {
    if (IsPaused)
    {
      Resume();
    }
    else
    {
      Pause();
    }
  }

  // Stops time, disables player controls, opens the menu, and plays its sound.
  void Pause()
  {
    IsPaused = true;
    Time.timeScale = 0f;
    SetPlayerControlsEnabled(false);
    SetMenuVisible(true);
    SetCursorLocked(false);
    PlayClip(menuOpenClip);
  }

  // Restores time, player controls, cursor lock, and gameplay menu visibility.
  public void Resume()
  {
    IsPaused = false;
    Time.timeScale = 1f;
    SetPlayerControlsEnabled(true);
    SetMenuVisible(false);
    SetCursorLocked(true);
    PlayClip(menuCloseClip);
  }

  // Replaces the pause panel with the options panel.
  public void OpenOptions()
  {
    pausePanel.SetActive(false);
    optionsPanel.SetActive(true);
  }

  // Replaces the options panel with the pause panel.
  public void CloseOptions()
  {
    optionsPanel.SetActive(false);
    pausePanel.SetActive(true);
  }

  // Starts the audio, screen, and scene transition back to the main menu.
  public void QuitToMenu()
  {
    if (isTransitioning) return;

    isTransitioning = true;
    Time.timeScale = 1f;
    SetPlayerControlsEnabled(true);
    SetCursorLocked(false);
    StartCoroutine(QuitToMenuRoutine());
  }

  // Fades audio and screen before loading the main menu scene.
  IEnumerator QuitToMenuRoutine()
  {
    float savedMasterVolume = AudioMixerVolume.GetSaved(AudioMixerVolume.MasterParam);

    if (screenFader != null)
    {
      Coroutine screenFade = screenFader.FadeOutAndStart(menuTransitionFadeDuration);
      yield return FadeAudio(savedMasterVolume, 0f, menuTransitionFadeDuration);
      yield return screenFade;
    }
    else
    {
      yield return FadeAudio(savedMasterVolume, 0f, menuTransitionFadeDuration);
    }

    SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
  }

  // Interpolates the saved master volume without relying on game time scale.
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

  // Shows or hides the pause background and panels.
  void SetMenuVisible(bool visible)
  {
    background.SetActive(visible);
    pausePanel.SetActive(visible);
    optionsPanel.SetActive(false);
  }

  // Enables or disables the configured player action map.
  void SetPlayerControlsEnabled(bool isEnabled)
  {
    InputActionMap map = playerControls != null ? playerControls.FindActionMap(playerActionMapName) : null;
    if (map == null) return;

    if (isEnabled)
    {
      map.Enable();
    }
    else
    {
      map.Disable();
    }
  }

  // Applies the cursor lock and visibility state for gameplay or menus.
  void SetCursorLocked(bool locked)
  {
    Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    Cursor.visible = !locked;
  }

  // Plays a menu sound when a clip is configured.
  void PlayClip(AudioClip clip)
  {
    if (clip == null) return;

    audioSource.PlayOneShot(clip);
  }
}
