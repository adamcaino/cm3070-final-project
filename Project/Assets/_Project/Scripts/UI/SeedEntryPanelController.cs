using UnityEngine;
using UnityEngine.UI;

// Lets the player type a specific world seed before loading the gameplay scene. Falls back to a
// fresh random seed if the field is empty or unparsable, same as GameOverScreen.LoadSeed. Confirming
// hands off to MainMenuController's intro transition rather than loading the scene directly, so
// both New Game and Load Seed share the same fade-out / scripted-walk sequence.
// Validates an optional seed entry and passes the selected seed to the main-menu transition.
public class SeedEntryPanelController : MonoBehaviour
{
  const int MaxSeedLength = 8;

  [SerializeField] MainMenuController mainMenu;
  [SerializeField] InputField seedInputField;
  [SerializeField] Button confirmButton;
  [SerializeField] Button backButton;

  Text validationText;

  // Registers button and input callbacks, creates validation text, and validates the initial value.
  void Awake()
  {
    confirmButton.onClick.AddListener(OnConfirm);
    backButton.onClick.AddListener(OnBack);

    CreateValidationText();
    seedInputField.onValueChanged.AddListener(HandleSeedChanged);
    HandleSeedChanged(seedInputField.text);
  }

  // Removes the seed input listener before the panel is destroyed.
  void OnDestroy()
  {
    if (seedInputField != null) seedInputField.onValueChanged.RemoveListener(HandleSeedChanged);
  }

  // Stores the parsed or random seed and starts the gameplay transition.
  void OnConfirm()
  {
    if (!IsValidSeed(seedInputField.text)) return;

    int seed = int.TryParse(seedInputField.text, out int parsed) ? parsed : System.Environment.TickCount;
    PendingSeed.Set(seed);
    mainMenu.BeginGameplayTransition();
  }

  // Updates validation feedback and confirm-button interactability.
  void HandleSeedChanged(string value)
  {
    bool valid = IsValidSeed(value);
    validationText.gameObject.SetActive(!valid);
    confirmButton.interactable = valid;
  }

  // Accepts an empty value or a non-negative numeric seed within the length limit.
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

  // Creates and configures the validation message displayed below the input field.
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

  // Returns to the main menu panel.
  void OnBack()
  {
    mainMenu.ShowMain();
  }
}
