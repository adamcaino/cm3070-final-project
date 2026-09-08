using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Shows one-time tutorial prompts for special weapon pickups and pauses gameplay until dismissed.
public class SpecialWeaponTutorialPrompt : MonoBehaviour
{
  [Header("Panel")]
  [SerializeField] GameObject root;
  [SerializeField] GameObject background;
  [SerializeField] GameObject panel;
  [SerializeField] Text weaponNameText;
  [SerializeField] Image weaponIcon;
  [SerializeField] Text effectText;
  [SerializeField] Button continueButton;

  [Header("Input")]
  [SerializeField] InputActionAsset playerControls;
  [SerializeField] string playerActionMapName = "Player";

  static SpecialWeaponTutorialPrompt instance;
  readonly HashSet<AfflictionType> shownTutorials = new HashSet<AfflictionType>();

  float cachedTimeScale = 1f;
  bool isOpen;

  // Resolves the singleton instance and keeps the prompt hidden at startup.
  void Awake()
  {
    instance = this;
    ResolveUiReferences();
    SetVisible(false);

    if (continueButton != null)
    {
      continueButton.onClick.AddListener(HandleContinueClicked);
    }
  }

  // Attempts to bind optional UI references from expected child names when not assigned.
  void ResolveUiReferences()
  {
    if (root == null)
    {
      root = gameObject;
    }

    SanitizeRootTarget();

    if (weaponNameText == null)
    {
      weaponNameText = FindComponentInRoot<Text>("WeaponName");
    }

    if (weaponIcon == null)
    {
      weaponIcon = FindComponentInRoot<Image>("Icon");
    }

    if (background == null)
    {
      background = FindChildInRoot("Background");
    }

    if (panel == null)
    {
      panel = FindChildInRoot("Panel");
    }

    if (effectText == null)
    {
      effectText = FindComponentInRoot<Text>("Effect");
    }

    if (continueButton == null && root != null)
    {
      continueButton = root.GetComponentInChildren<Button>(true);
    }
  }

  // Removes button listeners and restores simulation if this object is destroyed while open.
  void OnDestroy()
  {
    if (continueButton != null)
    {
      continueButton.onClick.RemoveListener(HandleContinueClicked);
    }

    if (instance == this)
    {
      instance = null;
    }

    if (isOpen)
    {
      ResumeGameplay();
    }
  }

  // Displays the matching tutorial once for each special weapon type.
  public static void ShowForPickup(WeaponData weapon)
  {
    if (weapon == null || !weapon.IsSpecial)
    {
      return;
    }

    EnsureInstance();
    if (instance == null)
    {
      Debug.LogWarning($"SpecialWeaponTutorialPrompt not found in the scene. Add the component to your gameplay UI to show tutorial prompts for '{weapon.weaponName}'.");
      return;
    }

    instance.TryShow(weapon);
  }

  // Resolves an existing instance, including inactive objects hidden at startup.
  static void EnsureInstance()
  {
    if (instance != null)
    {
      return;
    }

    instance = FindFirstObjectByType<SpecialWeaponTutorialPrompt>(FindObjectsInactive.Include);
  }

  // Displays tutorial content if this special-weapon type has not been shown yet.
  void TryShow(WeaponData weapon)
  {
    AfflictionType weaponType = weapon.afflictionType;
    if (!IsSupportedSpecialType(weaponType))
    {
      return;
    }

    if (shownTutorials.Contains(weaponType))
    {
      return;
    }

    shownTutorials.Add(weaponType);

    ApplyTutorialContent(weapon);
    PauseGameplay();
    SetVisible(true);
  }

  // Applies the localized name/effect and shared charge notes for the selected special weapon.
  void ApplyTutorialContent(WeaponData weapon)
  {
    if (weapon == null)
    {
      return;
    }

    AfflictionType weaponType = weapon.afflictionType;

    if (weaponNameText != null)
    {
      weaponNameText.text = string.IsNullOrWhiteSpace(weapon.weaponName) ? GetDefaultName(weaponType) : weapon.weaponName;
    }

    if (weaponIcon != null)
    {
      weaponIcon.sprite = weapon.icon;
      weaponIcon.enabled = weapon.icon != null;
    }

    switch (weaponType)
    {
      case AfflictionType.Burn:
        if (effectText != null) effectText.text = "Effect: Deals double damage.";
        break;

      case AfflictionType.Freeze:
        if (effectText != null) effectText.text = "Effect: Freezes enemies for 2 seconds on hit.";
        break;

      default:
        if (effectText != null) effectText.text = "";
        break;
    }
  }

  // Prevents accidental assignment to the top-level Canvas that would hide all UI on startup.
  void SanitizeRootTarget()
  {
    if (root == null || root == gameObject)
    {
      return;
    }

    if (root.GetComponent<Canvas>() != null)
    {
      Debug.LogWarning("SpecialWeaponTutorialPrompt root was assigned to a Canvas. Reverting to the prompt GameObject to avoid disabling the entire UI.");
      root = gameObject;
    }
  }

  // Provides fallback labels when weapon data omits display names.
  string GetDefaultName(AfflictionType weaponType)
  {
    switch (weaponType)
    {
      case AfflictionType.Burn:
        return "Jalapeñouch";
      case AfflictionType.Freeze:
        return "Frozone";
      default:
        return "Special Weapon";
    }
  }

  // Pauses game time, disables player controls, and unlocks the cursor for UI interaction.
  void PauseGameplay()
  {
    isOpen = true;
    cachedTimeScale = Time.timeScale;
    Time.timeScale = 0f;
    SetPlayerControlsEnabled(false);
    SetCursorLocked(false);
  }

  // Restores gameplay timing, controls, and cursor lock.
  void ResumeGameplay()
  {
    isOpen = false;
    Time.timeScale = cachedTimeScale <= 0f ? 1f : cachedTimeScale;
    SetPlayerControlsEnabled(true);
    SetCursorLocked(true);
  }

  // Handles the Continue button by hiding the prompt and resuming gameplay.
  void HandleContinueClicked()
  {
    if (!isOpen)
    {
      return;
    }

    SetVisible(false);
    ResumeGameplay();
  }

  // Shows or hides the tutorial root panel.
  void SetVisible(bool visible)
  {
    bool handledByVisualRoots = false;

    if (background != null)
    {
      background.SetActive(visible);
      handledByVisualRoots = true;
    }

    if (panel != null)
    {
      panel.SetActive(visible);
      handledByVisualRoots = true;
    }

    if (handledByVisualRoots)
    {
      return;
    }

    if (root != null)
    {
      root.SetActive(visible);
    }
    else
    {
      gameObject.SetActive(visible);
    }
  }

  // Enables or disables the configured player action map.
  void SetPlayerControlsEnabled(bool isEnabled)
  {
    InputActionMap map = playerControls != null ? playerControls.FindActionMap(playerActionMapName) : null;
    if (map == null)
    {
      return;
    }

    if (isEnabled)
    {
      map.Enable();
    }
    else
    {
      map.Disable();
    }
  }

  // Applies gameplay or menu cursor state.
  void SetCursorLocked(bool locked)
  {
    Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    Cursor.visible = !locked;
  }

  // Identifies which special-weapon types need a tutorial panel.
  bool IsSupportedSpecialType(AfflictionType weaponType)
  {
    return weaponType == AfflictionType.Burn || weaponType == AfflictionType.Freeze;
  }

  // Finds a named direct child under the configured root.
  GameObject FindChildInRoot(string childName)
  {
    if (root == null)
    {
      return null;
    }

    Transform child = root.transform.Find(childName);
    if (child == null)
    {
      return null;
    }

    return child.gameObject;
  }

  // Finds a named component under the configured root hierarchy.
  T FindComponentInRoot<T>(string childName) where T : Component
  {
    if (root == null)
    {
      return null;
    }

    Transform child = root.transform.Find(childName);
    if (child == null)
    {
      T[] components = root.GetComponentsInChildren<T>(true);
      for (int i = 0; i < components.Length; i++)
      {
        if (components[i].name == childName)
        {
          return components[i];
        }
      }

      return null;
    }

    return child.GetComponent<T>();
  }
}