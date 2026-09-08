using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Displays the player's current weapon icon and remaining special-weapon uses.
public class WeaponSlotUI : MonoBehaviour
{
  const string PLAYERTAG = "Player";

  [SerializeField] Slider manaCapacitySlider;
  [SerializeField] Image weaponIcon;
  [SerializeField] Image manaFillImage;
  [SerializeField] Sprite defaultWeaponSprite;
  [SerializeField] Color specialChargeColor = new Color(0.2f, 0.55f, 1f, 1f);

  Color defaultChargeColor = Color.white;

  PlayerWeapon playerWeapon;

  // Subscribes to dungeon readiness so the runtime player can be found after generation.
  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;
  }

  // Removes the dungeon readiness subscription.
  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
  }

  // Refreshes the player weapon reference after the dungeon has been generated.
  void HandleDungeonReady()
  {
    RefreshPlayerWeaponReference();
  }

  // Rebinds weapon events to the current player and refreshes the displayed weapon.
  void RefreshPlayerWeaponReference()
  {
    // Unsubscribe from old weapon if it exists
    if (playerWeapon != null)
    {
      playerWeapon.OnWeaponChanged -= HandleWeaponChanged;
      playerWeapon.OnManaChanged -= HandleManaChanged;
    }

    GameObject player = GameObject.FindGameObjectWithTag(PLAYERTAG);
    if (player == null)
    {
      Debug.LogWarning("WeaponSlotUI: no GameObject tagged 'Player' found in the loaded scenes.");
      return;
    }

    playerWeapon = player.GetComponent<PlayerWeapon>();
    if (playerWeapon == null)
    {
      Debug.LogWarning("WeaponSlotUI: player has no PlayerWeapon component.");
      return;
    }

    playerWeapon.OnWeaponChanged += HandleWeaponChanged;
    playerWeapon.OnManaChanged += HandleManaChanged;

    ResolveChargeFillImage();

    HandleWeaponChanged(playerWeapon.Current);
  }

  // Removes weapon event subscriptions before the UI object is destroyed.
  void OnDestroy()
  {
    if (playerWeapon == null) return;

    playerWeapon.OnWeaponChanged -= HandleWeaponChanged;
    playerWeapon.OnManaChanged -= HandleManaChanged;
  }

  // Updates the icon and capacity slider when the equipped weapon changes.
  void HandleWeaponChanged(WeaponData weapon)
  {
    bool isSpecial = weapon != null && weapon.IsSpecial;

    weaponIcon.sprite = isSpecial ? weapon.icon : defaultWeaponSprite;

    if (manaFillImage != null)
    {
      manaFillImage.color = isSpecial ? specialChargeColor : defaultChargeColor;
    }

    manaCapacitySlider.maxValue = isSpecial ? weapon.maxUses : 0;
    manaCapacitySlider.value = isSpecial ? playerWeapon.RemainingUses : 0;
  }

  // Updates the remaining-use slider when a special weapon is consumed.
  void HandleManaChanged(int current, int max) => manaCapacitySlider.value = current;

  // Caches the current fill image and its baseline colour when one is available.
  void ResolveChargeFillImage()
  {
    if (manaFillImage == null && manaCapacitySlider != null && manaCapacitySlider.fillRect != null)
    {
      manaFillImage = manaCapacitySlider.fillRect.GetComponent<Image>();
    }

    if (manaFillImage != null)
    {
      defaultChargeColor = manaFillImage.color;
    }
  }
}
