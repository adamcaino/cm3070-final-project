using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Lives in the persistent UI scene. Shows itself when GameOverSignal is raised, then reloads the
// gameplay scene the player died in for New Game / Load Seed, handing DungeonLevelGenerator a
// PendingSeed to consume on the way back up.
public class GameOverScreen : MonoBehaviour
{
  [Header("Panels")]
  [SerializeField] GameObject background;
  [SerializeField] GameObject panel;

  [Header("Load Seed")]
  [SerializeField] InputField seedInputField;

  [Header("Input")]
  [SerializeField] InputActionAsset playerControls;
  [SerializeField] string playerActionMapName = "Player";

  [Header("Main Menu Transition")]
  [SerializeField] AudioMixer mixer;
  [SerializeField] ScreenFader screenFader;
  [SerializeField] string mainMenuSceneName = "MainMenu";
  [SerializeField, Min(0f)] float menuTransitionFadeDuration = 1f;

  [Header("Win/Lose")]
  [SerializeField] string gameOverTitleText = "GAME OVER";
  [SerializeField] string victoryTitleText = "YOU WIN!";
  [SerializeField, Min(0f)] float victoryScreenDelay = 2.5f;
  [SerializeField] AudioClip victoryAudioPrefab;
  [SerializeField] GameObject victoryVfxPrefab;
  [SerializeField, Min(0f)] float victoryVfxDistance = 4f;

  string gameplaySceneName;
  bool isTransitioning;
  Coroutine showVictoryRoutine;
  Text titleText;

  // Resolves the title label and starts with the game-over panel hidden.
  void Awake()
  {
    titleText = ResolveTitleText();
    SetVisible(false);
  }

  // Subscribes to defeat and victory signals from gameplay scenes.
  void OnEnable()
  {
    GameOverSignal.Raised += Show;
    GameOverSignal.VictoryRaised += ShowVictory;
  }

  // Removes gameplay signal subscriptions and stops pending victory presentation.
  void OnDisable()
  {
    GameOverSignal.Raised -= Show;
    GameOverSignal.VictoryRaised -= ShowVictory;

    if (showVictoryRoutine != null)
    {
      StopCoroutine(showVictoryRoutine);
      showVictoryRoutine = null;
    }
  }

  // Displays the game-over panel and disables player controls.
  void Show(string sceneName)
  {
    gameplaySceneName = sceneName;
    SetPanelTitle(gameOverTitleText);
    SetPlayerControlsEnabled(false);
    SetVisible(true);
    SetCursorLocked(false);
  }

  // Begins the delayed victory-panel presentation for the supplied gameplay scene.
  void ShowVictory(string sceneName)
  {
    gameplaySceneName = sceneName;

    if (showVictoryRoutine != null)
    {
      StopCoroutine(showVictoryRoutine);
    }

    SetPlayerControlsEnabled(false);
    SetVisible(false);
    SetCursorLocked(true);
    showVictoryRoutine = StartCoroutine(ShowVictoryRoutine());
  }

  // Waits before displaying victory feedback, effects, and the victory-screen signal.
  IEnumerator ShowVictoryRoutine()
  {
    yield return new WaitForSeconds(victoryScreenDelay);

    SetPanelTitle(victoryTitleText);
    SetVisible(true);
    SpawnVictoryVfx();
    PlayVictorySfx();
    SetCursorLocked(false);
    GameOverSignal.RaiseVictoryScreenShown(gameplaySceneName);
    showVictoryRoutine = null;
  }

  // Places victory effects near the upper corners of the visible victory panel.
  void SpawnVictoryVfx()
  {
    if (victoryVfxPrefab == null || panel == null) return;

    Camera mainCamera = Camera.main;
    RectTransform panelTransform = panel.GetComponent<RectTransform>();
    if (mainCamera == null || panelTransform == null) return;

    Vector3[] corners = new Vector3[4];
    panelTransform.GetWorldCorners(corners);

    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null) return;

    float playerYRotation = player.transform.eulerAngles.y;
    SpawnVictoryVfxAtCorner(mainCamera, corners[1], playerYRotation - 90f);
    SpawnVictoryVfxAtCorner(mainCamera, corners[2], playerYRotation + 90f);
  }

  // Converts a panel corner into world space and instantiates one victory effect.
  void SpawnVictoryVfxAtCorner(Camera mainCamera, Vector3 corner, float yRotation)
  {
    Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, corner);
    Vector3 position = mainCamera.ScreenToWorldPoint(
      new Vector3(screenPosition.x, screenPosition.y, victoryVfxDistance));
    Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);

    Instantiate(victoryVfxPrefab, position, rotation);
  }

  // Plays the configured victory sound.
  void PlayVictorySfx()
  {
    if (victoryAudioPrefab == null) return;

    GetComponent<AudioSource>()?.PlayOneShot(victoryAudioPrefab);
  }

  // Stores a fresh seed and reloads the gameplay scene.
  public void NewGame()
  {
    PendingSeed.Set(System.Environment.TickCount);
    LoadGameplayScene();
  }

  // Stores a parsed seed when available and reloads the gameplay scene.
  public void LoadSeed()
  {
    if (seedInputField != null && int.TryParse(seedInputField.text, out int seed))
    {
      PendingSeed.Set(seed);
    }

    LoadGameplayScene();
  }

  // Starts the transition back to the main menu.
  public void Exit()
  {
    if (isTransitioning) return;

    isTransitioning = true;
    Time.timeScale = 1f;
    StartCoroutine(LoadMainMenuRoutine());
  }

  // Fades screen and audio before loading the main menu.
  IEnumerator LoadMainMenuRoutine()
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

  // Interpolates master volume during the menu transition.
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

  // Restores gameplay timing and loads the stored gameplay scene.
  void LoadGameplayScene()
  {
    if (string.IsNullOrEmpty(gameplaySceneName)) return;

    Time.timeScale = 1f;
    SetPlayerControlsEnabled(true);
    SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
  }

  // Shows or hides the game-over background and panel.
  void SetVisible(bool visible)
  {
    background.SetActive(visible);
    panel.SetActive(visible);
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

  // Applies the cursor state used by gameplay or the UI.
  void SetCursorLocked(bool locked)
  {
    Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    Cursor.visible = !locked;
  }

  // Updates the title label after resolving it when necessary.
  void SetPanelTitle(string value)
  {
    if (titleText == null)
    {
      titleText = ResolveTitleText();
    }

    if (titleText != null)
    {
      titleText.text = value;
    }
  }

  // Finds the named title text or falls back to the first child Text component.
  Text ResolveTitleText()
  {
    if (panel == null) return null;

    Transform titleTransform = panel.transform.Find("Title");
    if (titleTransform != null)
    {
      return titleTransform.GetComponent<Text>();
    }

    return panel.GetComponentInChildren<Text>(true);
  }
}
