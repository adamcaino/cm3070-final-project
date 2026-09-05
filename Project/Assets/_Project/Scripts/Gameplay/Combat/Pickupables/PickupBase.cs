using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class PickupBase : MonoBehaviour
{
  [SerializeField] float rotationSpeed = 90f;
  Transform cachedTransform;

  void Awake()
  {
    cachedTransform = transform;
    GetComponent<Collider>().isTrigger = true;
  }

  void Update()
  {
    if (Mathf.Approximately(rotationSpeed, 0f)) return;


    cachedTransform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
  }

  void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player")) return;
    if (!TryApplyEffect(other)) return;

    PlayPickupVfx();
    Destroy(gameObject);
  }

  protected abstract bool TryApplyEffect(Collider player);

  protected virtual void PlayPickupVfx() { }
  protected virtual void PlayPickupSfx() { }

  protected void SpawnVfx(GameObject vfxPrefab)
  {
    if (vfxPrefab != null)
    {
      Instantiate(vfxPrefab, transform.position, Quaternion.identity);
    }
  }
}
