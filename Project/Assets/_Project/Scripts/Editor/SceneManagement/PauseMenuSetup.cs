using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-off setup command that builds the pause menu hierarchy (background dim, pause panel,
/// options panel) under the UI scene's Canvas and wires it to a new PauseMenuController, mirroring
/// UIOverlaySetup's pattern. Idempotent - re-running skips if a PauseMenuController already exists.
/// </summary>
static class PauseMenuSetup
{
  const string UIScenePath = "Assets/_Project/Scenes/UI.unity";
  const string InputActionsPath = "Assets/_Project/Settings/PlayerControls.inputactions";
  const string MenuOpenClipPath = "Assets/_Project/Audio/SFX/UI/UI_Menu_Open.wav";
  const string MenuCloseClipPath = "Assets/_Project/Audio/SFX/UI/UI_Menu_Close.wav";

  static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.6f);
  static readonly Color PanelColor = new Color(0.12f, 0.12f, 0.14f, 0.95f);
  static readonly Color ButtonColor = new Color(1f, 1f, 1f, 0.08f);

  [MenuItem("Tools/Project Setup/Configure Pause Menu")]
  static void ConfigurePauseMenu()
  {
    Scene scene = EditorSceneManager.OpenScene(UIScenePath, OpenSceneMode.Single);

    if (Object.FindFirstObjectByType<PauseMenuController>() != null)
    {
      Debug.Log("Pause menu already configured in UI scene; skipping.");
      return;
    }

    Canvas canvas = Object.FindFirstObjectByType<Canvas>();
    if (canvas == null)
    {
      Debug.LogError("PauseMenuSetup: no Canvas found in UI scene. Run 'Configure UI Scene' first.");
      return;
    }

    Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    var root = new GameObject("PauseMenu", typeof(RectTransform));
    root.transform.SetParent(canvas.transform, false);
    StretchFull(root.GetComponent<RectTransform>());

    GameObject background = CreateBackground(root.transform);
    GameObject pausePanel = CreatePanel(root.transform, "PausePanel");
    GameObject optionsPanel = CreatePanel(root.transform, "OptionsPanel");

    CreateLabel(pausePanel.transform, "Title", "PAUSED", 32, legacyFont);
    Button resumeButton = CreateButton(pausePanel.transform, "ResumeButton", "Resume", legacyFont);
    Button optionsButton = CreateButton(pausePanel.transform, "OptionsButton", "Options", legacyFont);
    Button quitButton = CreateButton(pausePanel.transform, "QuitToMenuButton", "Quit to Menu", legacyFont);

    CreateLabel(optionsPanel.transform, "Title", "OPTIONS", 32, legacyFont);
    Button backButton = CreateButton(optionsPanel.transform, "BackButton", "Back", legacyFont);

    AudioSource audioSource = root.AddComponent<AudioSource>();
    audioSource.playOnAwake = false;
    audioSource.spatialBlend = 0f;

    PauseMenuController controller = root.AddComponent<PauseMenuController>();

    InputActionAsset playerControls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
    InputActionReference pauseActionReference = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath)
      .OfType<InputActionReference>()
      .FirstOrDefault(reference => reference.action.name == "Pause");

    if (pauseActionReference == null)
    {
      Debug.LogWarning("PauseMenuSetup: could not find a 'Pause' InputActionReference in PlayerControls.inputactions.");
    }

    AudioClip menuOpenClip = AssetDatabase.LoadAssetAtPath<AudioClip>(MenuOpenClipPath);
    AudioClip menuCloseClip = AssetDatabase.LoadAssetAtPath<AudioClip>(MenuCloseClipPath);

    var serializedController = new SerializedObject(controller);
    serializedController.FindProperty("pauseAction").objectReferenceValue = pauseActionReference;
    serializedController.FindProperty("playerControls").objectReferenceValue = playerControls;
    serializedController.FindProperty("background").objectReferenceValue = background;
    serializedController.FindProperty("pausePanel").objectReferenceValue = pausePanel;
    serializedController.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;
    serializedController.FindProperty("menuOpenClip").objectReferenceValue = menuOpenClip;
    serializedController.FindProperty("menuCloseClip").objectReferenceValue = menuCloseClip;
    serializedController.ApplyModifiedProperties();

    UnityEventTools.AddPersistentListener(resumeButton.onClick, controller.Resume);
    UnityEventTools.AddPersistentListener(optionsButton.onClick, controller.OpenOptions);
    UnityEventTools.AddPersistentListener(quitButton.onClick, controller.QuitToMenu);
    UnityEventTools.AddPersistentListener(backButton.onClick, controller.CloseOptions);

    EditorSceneManager.SaveScene(scene);
    Debug.Log("Pause menu configured in UI scene: PauseMenu (Resume/Options/Quit) wired up.");
  }

  static void StretchFull(RectTransform rect)
  {
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
  }

  static GameObject CreateBackground(Transform parent)
  {
    var go = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    go.transform.SetParent(parent, false);
    StretchFull(go.GetComponent<RectTransform>());

    Image image = go.GetComponent<Image>();
    image.color = BackgroundColor;
    image.raycastTarget = true;

    go.SetActive(false);
    return go;
  }

  static GameObject CreatePanel(Transform parent, string name)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
      typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
    go.transform.SetParent(parent, false);

    RectTransform rect = go.GetComponent<RectTransform>();
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.sizeDelta = new Vector2(420, 0);
    rect.anchoredPosition = Vector2.zero;

    go.GetComponent<Image>().color = PanelColor;

    VerticalLayoutGroup layout = go.GetComponent<VerticalLayoutGroup>();
    layout.padding = new RectOffset(32, 32, 32, 32);
    layout.spacing = 16f;
    layout.childAlignment = TextAnchor.UpperCenter;
    layout.childControlWidth = true;
    layout.childControlHeight = true;
    layout.childForceExpandWidth = true;
    layout.childForceExpandHeight = false;

    go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

    go.SetActive(false);
    return go;
  }

  static void CreateLabel(Transform parent, string name, string text, int fontSize, Font font)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(LayoutElement));
    go.transform.SetParent(parent, false);

    Text label = go.GetComponent<Text>();
    label.text = text;
    label.font = font;
    label.fontSize = fontSize;
    label.alignment = TextAnchor.MiddleCenter;
    label.color = Color.white;

    go.GetComponent<LayoutElement>().preferredHeight = fontSize + 12;
  }

  static Button CreateButton(Transform parent, string name, string label, Font font)
  {
    var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
    go.transform.SetParent(parent, false);

    go.GetComponent<Image>().color = ButtonColor;
    go.GetComponent<LayoutElement>().preferredHeight = 48f;

    var textGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
    textGO.transform.SetParent(go.transform, false);
    StretchFull(textGO.GetComponent<RectTransform>());

    Text text = textGO.GetComponent<Text>();
    text.text = label;
    text.font = font;
    text.fontSize = 20;
    text.alignment = TextAnchor.MiddleCenter;
    text.color = Color.white;

    return go.GetComponent<Button>();
  }
}
