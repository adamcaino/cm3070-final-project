using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Generates the Game Over screen hierarchy in the UI scene and binds the controller and buttons.
static class GameOverSetup
{
  const string UIScenePath = "Assets/_Project/Scenes/UI.unity";

  static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.75f);
  static readonly Color PanelColor = new Color(0.12f, 0.12f, 0.14f, 0.95f);
  static readonly Color ButtonColor = new Color(1f, 1f, 1f, 0.08f);

  [MenuItem("Tools/Project Setup/Configure Game Over Screen")]
  static void ConfigureGameOverScreen()
  {
    Scene scene = EditorSceneManager.OpenScene(UIScenePath, OpenSceneMode.Single);

    if (Object.FindFirstObjectByType<GameOverScreen>() != null)
    {
      Debug.Log("Game Over screen already configured in UI scene; skipping.");
      return;
    }

    Canvas canvas = Object.FindFirstObjectByType<Canvas>();
    if (canvas == null)
    {
      Debug.LogError("GameOverSetup: no Canvas found in UI scene. Run 'Configure UI Scene' first.");
      return;
    }

    Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    var root = new GameObject("GameOverScreen", typeof(RectTransform));
    root.transform.SetParent(canvas.transform, false);
    StretchFull(root.GetComponent<RectTransform>());

    GameObject background = CreateBackground(root.transform);
    GameObject panel = CreatePanel(root.transform);

    CreateLabel(panel.transform, "Title", "GAME OVER", 36, legacyFont);
    Button newGameButton = CreateButton(panel.transform, "NewGameButton", "New Game", legacyFont);
    InputField seedInputField = CreateSeedInputField(panel.transform, legacyFont);
    Button loadSeedButton = CreateButton(panel.transform, "LoadSeedButton", "Load Seed", legacyFont);
    Button exitButton = CreateButton(panel.transform, "ExitButton", "Exit", legacyFont);

    GameOverScreen controller = root.AddComponent<GameOverScreen>();

    var serializedController = new SerializedObject(controller);
    serializedController.FindProperty("background").objectReferenceValue = background;
    serializedController.FindProperty("panel").objectReferenceValue = panel;
    serializedController.FindProperty("seedInputField").objectReferenceValue = seedInputField;
    serializedController.ApplyModifiedProperties();

    UnityEventTools.AddPersistentListener(newGameButton.onClick, controller.NewGame);
    UnityEventTools.AddPersistentListener(loadSeedButton.onClick, controller.LoadSeed);
    UnityEventTools.AddPersistentListener(exitButton.onClick, controller.Exit);

    EditorSceneManager.SaveScene(scene);
    Debug.Log("Game Over screen configured in UI scene: GameOverScreen (New Game/Load Seed/Exit) wired up.");
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

  static GameObject CreatePanel(Transform parent)
  {
    var go = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
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

  static InputField CreateSeedInputField(Transform parent, Font font)
  {
    var go = new GameObject("SeedInputField", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image),
      typeof(InputField), typeof(LayoutElement));
    go.transform.SetParent(parent, false);

    go.GetComponent<Image>().color = ButtonColor;
    go.GetComponent<LayoutElement>().preferredHeight = 48f;

    var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
    textGO.transform.SetParent(go.transform, false);
    RectTransform textRect = textGO.GetComponent<RectTransform>();
    textRect.anchorMin = Vector2.zero;
    textRect.anchorMax = Vector2.one;
    textRect.offsetMin = new Vector2(10, 6);
    textRect.offsetMax = new Vector2(-10, -6);

    Text text = textGO.GetComponent<Text>();
    text.font = font;
    text.fontSize = 20;
    text.color = Color.white;
    text.alignment = TextAnchor.MiddleLeft;
    text.supportRichText = false;

    var placeholderGO = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
    placeholderGO.transform.SetParent(go.transform, false);
    RectTransform placeholderRect = placeholderGO.GetComponent<RectTransform>();
    placeholderRect.anchorMin = Vector2.zero;
    placeholderRect.anchorMax = Vector2.one;
    placeholderRect.offsetMin = new Vector2(10, 6);
    placeholderRect.offsetMax = new Vector2(-10, -6);

    Text placeholder = placeholderGO.GetComponent<Text>();
    placeholder.text = "Seed (number)";
    placeholder.font = font;
    placeholder.fontSize = 20;
    placeholder.fontStyle = FontStyle.Italic;
    placeholder.color = new Color(1f, 1f, 1f, 0.4f);
    placeholder.alignment = TextAnchor.MiddleLeft;

    InputField inputField = go.GetComponent<InputField>();
    inputField.textComponent = text;
    inputField.placeholder = placeholder;
    inputField.contentType = InputField.ContentType.IntegerNumber;
    inputField.lineType = InputField.LineType.SingleLine;

    return inputField;
  }
}
