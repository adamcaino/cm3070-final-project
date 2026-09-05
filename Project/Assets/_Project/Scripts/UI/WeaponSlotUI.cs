using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WeaponSlotUI : MonoBehaviour
{
  const string PLAYERTAG = "Player";

  [SerializeField] Slider manaCapacitySlider;
  [SerializeField] Image weaponIcon;
  [SerializeField] Sprite defaultWeaponSprite;

  PlayerWeapon playerWeapon;

  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;
  }

  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
  }

  void HandleDungeonReady()
  {
    RefreshPlayerWeaponReference();
  }

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

    HandleWeaponChanged(playerWeapon.Current);
  }

  void OnDestroy()
  {
    if (playerWeapon == null) return;

    playerWeapon.OnWeaponChanged -= HandleWeaponChanged;
    playerWeapon.OnManaChanged -= HandleManaChanged;
  }

  void HandleWeaponChanged(WeaponData weapon)
  {
    bool isSpecial = weapon != null && weapon.IsSpecial;

    weaponIcon.sprite = isSpecial ? weapon.icon : defaultWeaponSprite;

    manaCapacitySlider.maxValue = isSpecial ? weapon.maxUses : 0;
    manaCapacitySlider.value = isSpecial ? playerWeapon.RemainingUses : 0;
  }

  void HandleManaChanged(int current, int max) => manaCapacitySlider.value = current;
}
