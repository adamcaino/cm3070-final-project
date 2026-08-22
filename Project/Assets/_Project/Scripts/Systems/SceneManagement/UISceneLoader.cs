using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drop this on an empty GameObject in every gameplay scene (Dungeon, Sandbox, ...). On Awake it makes
/// sure the persistent UI scene is loaded additively alongside whatever gameplay scene just started,
/// so the HUD exists no matter which scene you pressed Play from or travelled to.
///
/// Guarded with GetSceneByName(...).isLoaded rather than just always loading, because entering Play mode
/// directly from a scene that already has the UI scene open in the Editor (or loading Dungeon -> Sandbox
/// without ever unloading UI) would otherwise try to load it twice.
/// </summary>
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
