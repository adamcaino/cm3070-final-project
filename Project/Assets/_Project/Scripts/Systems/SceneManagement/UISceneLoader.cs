using UnityEngine;
using UnityEngine.SceneManagement;

// Loads the persistent UI scene additively when it is not already loaded.
public class UISceneLoader : MonoBehaviour
{
  [SerializeField] string uiSceneName = "UI";

  // Checks the configured UI scene and loads it before gameplay begins.
  void Awake()
  {
    Scene uiScene = SceneManager.GetSceneByName(uiSceneName);
    if (uiScene.isLoaded)
    {
      return;
    }

    SceneManager.LoadScene(uiSceneName, LoadSceneMode.Additive);
  }
}
