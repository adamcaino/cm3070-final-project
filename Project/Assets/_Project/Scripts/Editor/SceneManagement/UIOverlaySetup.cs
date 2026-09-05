using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Creates the shared UI overlay and bootstraps gameplay scenes with the UISceneLoader pattern.
static class UIOverlaySetup
{
  const string UIScenePath = "Assets/_Project/Scenes/UI.unity";
  static readonly string[] GameplayScenePaths =
  {
    "Assets/_Project/Scenes/Dungeon.unity",
    "Assets/_Project/Scenes/Sandbox.unity",
  };

  [MenuItem("Tools/Project Setup/Configure UI Scene")]
  static void ConfigureUIScene()
  {
    Scene scene = EditorSceneManager.OpenScene(UIScenePath, OpenSceneMode.Single);

    foreach (GameObject root in scene.GetRootGameObjects())
    {
      if (root.GetComponent<Camera>() != null || root.GetComponent<Light>() != null)
      {
        Object.DestroyImmediate(root);
      }
    }

    if (Object.FindFirstObjectByType<Canvas>() == null)
    {
      var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
      var canvas = canvasGO.GetComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;

      var scaler = canvasGO.GetComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920, 1080);
      scaler.matchWidthOrHeight = 0.5f;
    }

    if (Object.FindFirstObjectByType<EventSystem>() == null)
    {
      new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    EditorSceneManager.SaveScene(scene);
    Debug.Log("UI scene configured: Canvas (Screen Space - Overlay) + EventSystem (Input System) ready.");
  }

  [MenuItem("Tools/Project Setup/Wire Up Gameplay Scenes")]
  static void WireUpGameplayScenes()
  {
    foreach (string path in GameplayScenePaths)
    {
      Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

      if (Object.FindFirstObjectByType<UISceneLoader>() == null)
      {
        new GameObject("UIBootstrap", typeof(UISceneLoader));
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Added UIBootstrap to {path}");
      }
    }
  }
}
