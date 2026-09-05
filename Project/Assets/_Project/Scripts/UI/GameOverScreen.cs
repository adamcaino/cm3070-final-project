using UnityEngine;
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

  string gameplaySceneName;

  void Awake()
  {
    SetVisible(false);
  }

  void OnEnable()
  {
    GameOverSignal.Raised += Show;
  }

  void OnDisable()
  {
    GameOverSignal.Raised -= Show;
  }

  void Show(string sceneName)
  {
    gameplaySceneName = sceneName;
    SetVisible(true);
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
  }

  public void NewGame()
  {
    PendingSeed.Set(System.Environment.TickCount);
    LoadGameplayScene();
  }

  public void LoadSeed()
  {
    if (seedInputField != null && int.TryParse(seedInputField.text, out int seed))
    {
      PendingSeed.Set(seed);
    }

    LoadGameplayScene();
  }

  public void Exit()
  {
    Time.timeScale = 1f;
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
  }

  void LoadGameplayScene()
  {
    if (string.IsNullOrEmpty(gameplaySceneName)) return;

    Time.timeScale = 1f;
    SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
  }

  void SetVisible(bool visible)
  {
    background.SetActive(visible);
    panel.SetActive(visible);
  }
}
