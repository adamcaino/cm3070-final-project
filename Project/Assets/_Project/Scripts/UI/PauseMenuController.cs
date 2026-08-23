using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns pause state for the whole game: toggles Time.timeScale, disables the Player action map so
/// gameplay input can't leak through while paused, and swaps between the pause and options panels.
/// Lives on an always-active GameObject so its Pause input keeps firing while the menu itself is hidden.
/// </summary>
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

  AudioSource audioSource;

  public bool IsPaused { get; private set; }

  void Awake()
  {
    audioSource = GetComponent<AudioSource>();
    SetMenuVisible(false);
    SetCursorLocked(true);
  }

  void OnEnable()
  {
    if (pauseAction == null) return;

    pauseAction.action.Enable();
    pauseAction.action.performed += HandlePausePressed;
  }

  void OnDisable()
  {
    if (pauseAction == null) return;

    pauseAction.action.performed -= HandlePausePressed;
    pauseAction.action.Disable();
  }

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

  void Pause()
  {
    IsPaused = true;
    Time.timeScale = 0f;
    SetPlayerControlsEnabled(false);
    SetMenuVisible(true);
    SetCursorLocked(false);
    PlayClip(menuOpenClip);
  }

  public void Resume()
  {
    IsPaused = false;
    Time.timeScale = 1f;
    SetPlayerControlsEnabled(true);
    SetMenuVisible(false);
    SetCursorLocked(true);
    PlayClip(menuCloseClip);
  }

  public void OpenOptions()
  {
    pausePanel.SetActive(false);
    optionsPanel.SetActive(true);
  }

  public void CloseOptions()
  {
    optionsPanel.SetActive(false);
    pausePanel.SetActive(true);
  }

  public void QuitToMenu()
  {
    Time.timeScale = 1f;
    SetPlayerControlsEnabled(true);
    SetCursorLocked(false);
    SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
  }

  void SetMenuVisible(bool visible)
  {
    background.SetActive(visible);
    pausePanel.SetActive(visible);
    optionsPanel.SetActive(false);
  }

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

  void SetCursorLocked(bool locked)
  {
    Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    Cursor.visible = !locked;
  }

  void PlayClip(AudioClip clip)
  {
    if (clip == null) return;

    audioSource.PlayOneShot(clip);
  }
}
