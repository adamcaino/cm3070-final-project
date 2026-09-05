using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Applies burn and freeze effects by modifying movement, animation speed, and material tint.
[RequireComponent(typeof(Health))]
public class StatusEffectReceiver : MonoBehaviour, IAfflictable
{
  const float BurnTickInterval = 1f;

  [SerializeField] Color freezeColour = new Color(0.6f, 0.85f, 1f, 1f);
  [SerializeField] GameObject burningEffectPrefab;

  Health health;
  NavMeshAgent agent;
  Animator animator;
  HitFlash hitFlash;
  EnemyController enemyController;
  Renderer[] renderers;
  MaterialPropertyBlock propertyBlock;
  Color[] cachedColours;
  Coroutine activeEffect;
  GameObject activeBurningEffect;
  float cachedAgentSpeed;
  float cachedAnimatorSpeed;
  bool isFrozen;

  static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
  static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

  public bool IsFrozen => isFrozen;

  void Awake()
  {
    health = GetComponent<Health>();
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponentInChildren<Animator>();
    hitFlash = GetComponent<HitFlash>();
    enemyController = GetComponent<EnemyController>();
    renderers = GetComponentsInChildren<Renderer>();
    propertyBlock = new MaterialPropertyBlock();
  }

  void OnEnable()
  {
    health.OnDied += HandleDied;
  }

  void OnDisable()
  {
    health.OnDied -= HandleDied;
  }

  void HandleDied() => EndBurn();

  // Cancels any current effect and starts the matching affliction routine.
  public void ApplyAffliction(AfflictionType type, float duration, int magnitude, GameObject source)
  {
    if (activeEffect != null)
    {
      StopCoroutine(activeEffect);
      activeEffect = null;
    }
    EndFreeze();
    EndBurn();

    switch (type)
    {
      case AfflictionType.Burn:
        activeEffect = StartCoroutine(BurnRoutine(duration, magnitude, source));
        break;
      case AfflictionType.Freeze:
        activeEffect = StartCoroutine(FreezeRoutine(duration));
        break;
    }
  }

  IEnumerator BurnRoutine(float duration, int magnitude, GameObject source)
  {
    if (burningEffectPrefab != null)
    {
      activeBurningEffect = Instantiate(burningEffectPrefab, GetMeshCenter(), Quaternion.identity, transform);
    }

    float elapsed = 0f;
    while (elapsed < duration)
    {
      yield return new WaitForSeconds(BurnTickInterval);
      elapsed += BurnTickInterval;

      if (health.IsDead)
      {
        EndBurn();
        yield break;
      }

      health.TakeDamage(magnitude, source, health.transform.position);
    }

    EndBurn();
    activeEffect = null;
  }

  void EndBurn()
  {
    if (activeBurningEffect == null) return;

    Destroy(activeBurningEffect);
    activeBurningEffect = null;
  }

  Vector3 GetMeshCenter()
  {
    bool hasBounds = false;
    Bounds bounds = default;

    foreach (Renderer rend in renderers)
    {
      if (rend == null) continue;

      if (!hasBounds)
      {
        bounds = rend.bounds;
        hasBounds = true;
      }
      else
      {
        bounds.Encapsulate(rend.bounds);
      }
    }

    return hasBounds ? bounds.center : transform.position;
  }

  IEnumerator FreezeRoutine(float duration)
  {
    if (agent == null)
    {
      activeEffect = null;
      yield break;
    }

    cachedAgentSpeed = agent.speed;
    agent.speed = 0f;

    if (animator != null)
    {
      cachedAnimatorSpeed = animator.speed;
      animator.speed = 0f;
    }

    if (hitFlash != null)
    {
      hitFlash.Cancel();
      hitFlash.enabled = false;
    }

    ApplyFreezeTint();

    if (enemyController != null)
    {
      enemyController.IsFrozen = true;

      foreach (IAttack attack in enemyController.Attacks)
      {
        attack.Interrupt();
      }
    }

    isFrozen = true;

    yield return new WaitForSeconds(duration);

    EndFreeze();
    activeEffect = null;
  }

  void ApplyFreezeTint()
  {
    if (renderers == null || renderers.Length == 0)
    {
      return;
    }

    cachedColours = new Color[renderers.Length];

    for (int i = 0; i < renderers.Length; i++)
    {
      Renderer rend = renderers[i];
      if (rend == null)
      {
        continue;
      }

      cachedColours[i] = rend.sharedMaterial != null && rend.sharedMaterial.HasProperty(BaseColorPropertyId)
        ? rend.sharedMaterial.GetColor(BaseColorPropertyId)
        : (rend.sharedMaterial != null && rend.sharedMaterial.HasProperty(ColorPropertyId)
          ? rend.sharedMaterial.GetColor(ColorPropertyId)
          : Color.white);

      rend.GetPropertyBlock(propertyBlock);
      propertyBlock.SetColor(BaseColorPropertyId, freezeColour);
      propertyBlock.SetColor(ColorPropertyId, freezeColour);
      rend.SetPropertyBlock(propertyBlock);
    }
  }

  void RestoreTint()
  {
    if (renderers == null || cachedColours == null)
    {
      return;
    }

    for (int i = 0; i < renderers.Length; i++)
    {
      Renderer rend = renderers[i];
      if (rend == null)
      {
        continue;
      }

      rend.GetPropertyBlock(propertyBlock);
      propertyBlock.SetColor(BaseColorPropertyId, cachedColours[i]);
      propertyBlock.SetColor(ColorPropertyId, cachedColours[i]);
      rend.SetPropertyBlock(propertyBlock);
    }

    cachedColours = null;
  }

  void EndFreeze()
  {
    if (!isFrozen)
    {
      return;
    }

    agent.speed = cachedAgentSpeed;

    if (animator != null)
    {
      animator.speed = cachedAnimatorSpeed;
    }

    RestoreTint();

    if (hitFlash != null)
    {
      hitFlash.enabled = true;
    }

    if (enemyController != null)
    {
      enemyController.IsFrozen = false;
    }

    isFrozen = false;
  }
}
