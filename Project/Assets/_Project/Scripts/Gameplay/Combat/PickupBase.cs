using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class PickupBase : MonoBehaviour
{
  void Awake()
  {
    GetComponent<Collider>().isTrigger = true;
  }

  void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player")) return;
    if (!TryApplyEffect(other)) return;

    PlayPickupVfx();
    PlayPickupSfx();
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

  protected void PlaySfx(AudioClip clip)
  {
    if (clip != null)
    {
      AudioSource.PlayClipAtPoint(clip, transform.position);
    }
  }
}
