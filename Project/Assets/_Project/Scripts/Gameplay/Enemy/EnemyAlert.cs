using UnityEngine;

// Reacts to this enemy being damaged or spotting the player by playing an alert VFX/SFX and
// relaying the alert to nearby enemies, who are woken into PositionState even without having
// seen the player themselves. Not attached to boss prefabs - bosses neither alert nor relay.
[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyDetection))]
public class EnemyAlert : MonoBehaviour
{
  [Header("Alert VFX")]
  [SerializeField] GameObject alertVfxPrefab;

  [Header("Relay")]
  [SerializeField, Min(0f)] float alertRadius = 8f;

  readonly Collider[] overlapBuffer = new Collider[16];

  EnemyController controller;
  Health health;
  EnemyDetection detection;
  LayerMask enemyMask = 1 << 11;

  // Caches the controller, health, and detection components used by the alert flow.
  void Awake()
  {
    controller = GetComponent<EnemyController>();
    health = GetComponent<Health>();
    detection = GetComponent<EnemyDetection>();
  }

  // Subscribes to damage and player-spotted events.
  void OnEnable()
  {
    health.OnDamaged += HandleDamaged;
    detection.OnPlayerSpotted += HandlePlayerSpotted;
  }

  // Removes damage and player-spotted event subscriptions.
  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;
    detection.OnPlayerSpotted -= HandlePlayerSpotted;
  }

  // Triggers the alert and relays it after the enemy takes damage.
  void HandleDamaged(Vector3 hitPoint) => Trigger(relayToNearby: true);

  // Triggers the alert and relays it after the enemy spots the player.
  void HandlePlayerSpotted() => Trigger(relayToNearby: true);

  // Called on an ally by a neighbor's relay - reacts the same way but does not itself relay
  // further, so one alert spreads exactly one hop instead of cascading across the whole level.
  public void ReceiveAlert() => Trigger(relayToNearby: false);

  // Plays the alert, wakes the enemy, optionally relays one hop, and disables this component.
  void Trigger(bool relayToNearby)
  {
    PlayAlertVfx();

    if (!controller.IsFrozen)
    {
      controller.Wake();
    }

    if (relayToNearby)
    {
      RelayToNearbyEnemies();
    }

    // Turn off this script to prevent re-alerting
    enabled = false;
  }

  // Spawns the configured alert effect above the enemy.
  void PlayAlertVfx()
  {
    if (alertVfxPrefab == null) return;

    // Offset to spawn above the enemy's head
    Vector3 spawnPosOffset = new Vector3(0, 2, 0);

    // Spawn the alert VFX
    Instantiate(alertVfxPrefab, transform.position + spawnPosOffset, transform.rotation);
  }

  // Notifies nearby living allies without allowing the alert to cascade further.
  void RelayToNearbyEnemies()
  {
    int count = Physics.OverlapSphereNonAlloc(transform.position, alertRadius, overlapBuffer, enemyMask, QueryTriggerInteraction.Ignore);
    for (int i = 0; i < count; i++)
    {
      EnemyAlert ally = overlapBuffer[i].GetComponentInParent<EnemyAlert>();
      if (ally == null || ally == this || ally.health.IsDead)
      {
        continue;
      }

      ally.ReceiveAlert();
    }
  }
}
