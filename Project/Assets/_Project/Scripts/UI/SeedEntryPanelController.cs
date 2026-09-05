using UnityEngine;
using UnityEngine.UI;

// Lets the player type a specific world seed before loading the gameplay scene. Falls back to a
// fresh random seed if the field is empty or unparsable, same as GameOverScreen.LoadSeed. Confirming
// hands off to MainMenuController's intro transition rather than loading the scene directly, so
// both New Game and Load Seed share the same fade-out / scripted-walk sequence.
public class SeedEntryPanelController : MonoBehaviour
{
  const int MaxSeedLength = 8;

  [SerializeField] MainMenuController mainMenu;
  [SerializeField] InputField seedInputField;
  [SerializeField] Button confirmButton;
  [SerializeField] Button backButton;

  Text validationText;

  void Awake()
  {
    confirmButton.onClick.AddListener(OnConfirm);
    backButton.onClick.AddListener(OnBack);

    CreateValidationText();
    seedInputField.onValueChanged.AddListener(HandleSeedChanged);
    HandleSeedChanged(seedInputField.text);
  }

  void OnDestroy()
  {
    if (seedInputField != null) seedInputField.onValueChanged.RemoveListener(HandleSeedChanged);
  }

  void OnConfirm()
  {
    if (!IsValidSeed(seedInputField.text)) return;

    int seed = int.TryParse(seedInputField.text, out int parsed) ? parsed : System.Environment.TickCount;
    PendingSeed.Set(seed);
    mainMenu.BeginGameplayTransition();
  }

  void HandleSeedChanged(string value)
  {
    bool valid = IsValidSeed(value);
    validationText.gameObject.SetActive(!valid);
    confirmButton.interactable = valid;
  }

  static bool IsValidSeed(string value)
  {
    if (string.IsNullOrEmpty(value)) return true;
    if (value.Length > MaxSeedLength) return false;

    for (int index = 0; index < value.Length; index++)
    {
      if (!char.IsDigit(value[index])) return false;
    }

    return true;
  }

  void CreateValidationText()
  {
    GameObject warningObject = new GameObject("SeedValidationWarning", typeof(RectTransform), typeof(Text));
    warningObject.transform.SetParent(transform, false);

    RectTransform warningTransform = warningObject.GetComponent<RectTransform>();
    warningTransform.anchorMin = new Vector2(0.5f, 0.5f);
    warningTransform.anchorMax = new Vector2(0.5f, 0.5f);
    warningTransform.pivot = new Vector2(0.5f, 0.5f);
    warningTransform.anchoredPosition = new Vector2(0f, -18f);
    warningTransform.sizeDelta = new Vector2(280f, 32f);

    validationText = warningObject.GetComponent<Text>();
    validationText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    validationText.fontSize = 16;
    validationText.color = Color.red;
    validationText.alignment = TextAnchor.UpperCenter;
    validationText.raycastTarget = false;
    validationText.text = "Invalid seed number (use up to 8 digits).";
  }

  void OnBack()
  {
    mainMenu.ShowMain();
  }
}
