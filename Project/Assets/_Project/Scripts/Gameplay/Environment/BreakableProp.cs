using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AudioSource))]
// Hides, effects, and removes a prop after its health reaches zero.
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
  Renderer[] breakableMeshRenderers;

  // Caches the health, audio, collider, and renderer components used during destruction.
  void Awake()
  {
    health = GetComponent<Health>();
    audioSource = GetComponent<AudioSource>();
    colliders = GetComponentsInChildren<Collider>();
    renderers = GetComponentsInChildren<Renderer>();
    breakableMeshRenderers = breakableMesh != null
      ? breakableMesh.GetComponentsInChildren<Renderer>()
      : System.Array.Empty<Renderer>();
  }

  // Subscribes to the prop's death event when it becomes active.
  void OnEnable()
  {
    health.OnDied += HandleDied;
  }

  // Removes the death subscription when the prop is disabled.
  void OnDisable()
  {
    health.OnDied -= HandleDied;
  }

  // Spawns break effects, hides the prop, and delays destruction until its sound finishes.
  void HandleDied()
  {
    SpawnBreakVfx();
    DropHealthPotion();

    // Hide immediately so the break reads instantly, but keep the GameObject
    // (and its AudioSource) alive long enough for PlayOneShot to finish.
    SetVisible(false);
    Destroy(gameObject, PlayBreakSfx());
  }

  // Spawns the configured break effect at the breakable mesh position.
  void SpawnBreakVfx()
  {
    if (breakVfxPrefab == null) return;

    Instantiate(breakVfxPrefab, GetBreakableMeshCenter(), Quaternion.identity);
  }

  // Calculates the world-space center of the breakable mesh using renderer bounds.
  Vector3 GetBreakableMeshCenter()
  {
    if (breakableMesh == null) return transform.position;
    if (breakableMeshRenderers == null || breakableMeshRenderers.Length == 0) return breakableMesh.position;

    Bounds bounds = breakableMeshRenderers[0].bounds;
    for (int i = 1; i < breakableMeshRenderers.Length; i++)
    {
      bounds.Encapsulate(breakableMeshRenderers[i].bounds);
    }

    return bounds.center;
  }

  // Plays the break sound at a random pitch and returns its adjusted playback duration.
  float PlayBreakSfx()
  {
    if (breakSfxClip == null) return 0f;

    float pitch = Random.Range(pitchRange.x, pitchRange.y);
    audioSource.pitch = pitch;
    audioSource.PlayOneShot(breakSfxClip);

    return breakSfxClip.length / pitch;
  }

  // Spawns a health potion when the prop is configured to drop one.
  void DropHealthPotion()
  {
    if (!dropsHealthPotion || healthPotionPrefab == null) return;

    Instantiate(healthPotionPrefab, breakableMesh.position, Quaternion.identity);
  }

  // Enables or disables all child renderers and colliders.
  void SetVisible(bool visible)
  {
    foreach (Renderer r in renderers) r.enabled = visible;
    foreach (Collider c in colliders) c.enabled = visible;
  }
}
