using UnityEngine;

[RequireComponent(typeof(Collider))]
// Provides the shared trigger, rotation, effect, and cleanup behaviour for pickups.
public abstract class PickupBase : MonoBehaviour
{
  [SerializeField] float rotationSpeed = 90f;
  Transform cachedTransform;

  // Caches the transform and configures the required collider as a trigger.
  void Awake()
  {
    cachedTransform = transform;
    GetComponent<Collider>().isTrigger = true;
  }

  // Rotates the pickup around the world Y axis while it remains in the scene.
  void Update()
  {
    if (Mathf.Approximately(rotationSpeed, 0f)) return;


    cachedTransform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
  }

  // Applies the pickup when the player enters its trigger and removes it after success.
  void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player")) return;
    if (!TryApplyEffect(other)) return;

    PlayPickupVfx();
    Destroy(gameObject);
  }

  // Attempts to apply the pickup-specific effect and reports whether it succeeded.
  protected abstract bool TryApplyEffect(Collider player);

  // Provides a hook for pickup-specific visual effects after a successful collection.
  protected virtual void PlayPickupVfx() { }

  // Provides a hook for pickup-specific audio effects after a successful collection.
  protected virtual void PlayPickupSfx() { }

  // Instantiates a visual effect at the pickup's current position when a prefab is supplied.
  protected void SpawnVfx(GameObject vfxPrefab)
  {
    if (vfxPrefab != null)
    {
      Instantiate(vfxPrefab, transform.position, Quaternion.identity);
    }
  }
}
