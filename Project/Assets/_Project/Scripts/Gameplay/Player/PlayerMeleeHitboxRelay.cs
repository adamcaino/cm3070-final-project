using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerMeleeHitboxRelay : MonoBehaviour
{
  PlayerWeapon owner;

  public void Initialize(PlayerWeapon playerWeapon)
  {
    owner = playerWeapon;
  }

  void OnTriggerEnter(Collider other)
  {
    owner?.HandleMeleeTriggerEnter(other);
  }
}