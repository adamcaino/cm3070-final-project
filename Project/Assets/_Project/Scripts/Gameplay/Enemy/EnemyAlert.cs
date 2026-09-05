using UnityEngine;

// Reacts to this enemy being damaged or spotting the player by playing an alert VFX/SFX and
// relaying the alert to nearby enemies, who are woken into PositionState even without having
// seen the player themselves. Not attached to boss prefabs - bosses neither alert nor relay.
[RequireComponent(typeof(EnemyController))]
public class EnemyAlert : MonoBehaviour
{
  [Header("Alert VFX")]
  [SerializeField] GameObject alertVfxPrefab;
  [SerializeField, Min(0f)] float vfxLifetime = 2f;

  [Header("Relay")]
  [SerializeField, Min(0f)] float alertRadius = 8f;

  readonly Collider[] overlapBuffer = new Collider[16];

  EnemyController controller;
  LayerMask enemyMask = 1 << 11;

  void Awake()
  {
    controller = GetComponent<EnemyController>();
  }

  void OnEnable()
  {
    controller.Health.OnDamaged += HandleDamaged;
    controller.Detection.OnPlayerSpotted += HandlePlayerSpotted;
  }

  void OnDisable()
  {
    controller.Health.OnDamaged -= HandleDamaged;
    controller.Detection.OnPlayerSpotted -= HandlePlayerSpotted;
  }

  void HandleDamaged(Vector3 hitPoint) => Trigger(relayToNearby: true);
  void HandlePlayerSpotted() => Trigger(relayToNearby: true);

  // Called on an ally by a neighbor's relay - reacts the same way but does not itself relay
  // further, so one alert spreads exactly one hop instead of cascading across the whole level.
  public void ReceiveAlert() => Trigger(relayToNearby: false);

  void Trigger(bool relayToNearby)
  {
    PlayAlertVfx();

    if (relayToNearby)
    {
      RelayToNearbyEnemies();
    }
    else if (!controller.IsFrozen)
    {
      controller.Wake();
    }

    // Turn off this script to prevent re-alerting
    enabled = false;
  }

  void PlayAlertVfx()
  {
    if (alertVfxPrefab == null) return;

    // Offset to spawn above the enemy's head
    Vector3 spawnPosOffset = new Vector3(0, 2, 0);

    // Spawn the alert VFX
    Instantiate(alertVfxPrefab, transform.position + spawnPosOffset, transform.rotation);
  }

  void RelayToNearbyEnemies()
  {
    int count = Physics.OverlapSphereNonAlloc(transform.position, alertRadius, overlapBuffer, enemyMask, QueryTriggerInteraction.Ignore);
    for (int i = 0; i < count; i++)
    {
      EnemyAlert ally = overlapBuffer[i].GetComponentInParent<EnemyAlert>();
      if (ally == null || ally == this || ally.controller.Health.IsDead)
      {
        continue;
      }

      ally.ReceiveAlert();
    }
  }
}
