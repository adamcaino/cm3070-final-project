using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneLoader : MonoBehaviour
{
  [SerializeField] string uiSceneName = "UI";

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
