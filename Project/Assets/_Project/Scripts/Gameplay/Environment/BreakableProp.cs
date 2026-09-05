using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AudioSource))]
public class BreakableProp : MonoBehaviour
{
  [Header("VFX")]
  [SerializeField] GameObject breakVfxPrefab;
  [SerializeField] Transform breakableMesh;

  [Header("SFX")]
  [SerializeField] AudioClip breakSfxClip;
  [SerializeField] Vector2 pitchRange = new Vector2(0.8f, 1.2f);

  [Header("Loot")]
  [Tooltip("When enabled, spawns healthPotionPrefab when this prop breaks.")]
  [SerializeField] bool dropsHealthPotion;
  [SerializeField] GameObject healthPotionPrefab;

  Health health;
  AudioSource audioSource;
  Collider[] colliders;
  Renderer[] renderers;

  void Awake()
  {
    health = GetComponent<Health>();
    audioSource = GetComponent<AudioSource>();
    colliders = GetComponentsInChildren<Collider>();
    renderers = GetComponentsInChildren<Renderer>();
  }

  void OnEnable()
  {
    health.OnDied += HandleDied;
  }

  void OnDisable()
  {
    health.OnDied -= HandleDied;
  }

  void HandleDied()
  {
    SpawnBreakVfx();
    DropHealthPotion();

    // Hide immediately so the break reads instantly, but keep the GameObject
    // (and its AudioSource) alive long enough for PlayOneShot to finish.
    SetVisible(false);
    Destroy(gameObject, PlayBreakSfx());
  }

  void SpawnBreakVfx()
  {
    if (breakVfxPrefab == null) return;

    Instantiate(breakVfxPrefab, breakableMesh.position, Quaternion.identity);
  }

  float PlayBreakSfx()
  {
    if (breakSfxClip == null) return 0f;

    float pitch = Random.Range(pitchRange.x, pitchRange.y);
    audioSource.pitch = pitch;
    audioSource.PlayOneShot(breakSfxClip);

    return breakSfxClip.length / pitch;
  }

  void DropHealthPotion()
  {
    if (!dropsHealthPotion || healthPotionPrefab == null) return;

    Instantiate(healthPotionPrefab, breakableMesh.position, Quaternion.identity);
  }

  void SetVisible(bool visible)
  {
    foreach (Renderer r in renderers) r.enabled = visible;
    foreach (Collider c in colliders) c.enabled = visible;
  }
}
