using UnityEngine;
using UnityEngine.UI;

// Displays the owning enemy's health and keeps its world-space canvas aligned to the player camera.
public class EnemyHealthBarUI : MonoBehaviour
{
  [SerializeField, Min(0f)] float deathFadeDuration = 0.4f;

  Health health;
  Slider slider;
  Canvas worldCanvas;
  Transform canvasTransform;
  Camera playerCamera;
  EnemyController enemyController;
  EnemyAlert enemyAlert;
  bool isVisible;
  bool isFadingOnDeath;
  float deathFadeElapsed;

  // Caches local component references and attempts to bind the nearest enemy health.
  void Awake()
  {
    slider = GetComponent<Slider>();
    worldCanvas = GetComponentInParent<Canvas>();
    canvasTransform = worldCanvas != null ? worldCanvas.transform : null;
    health = GetComponentInParent<Health>();
    enemyController = GetComponentInParent<EnemyController>();
    enemyAlert = GetComponentInParent<EnemyAlert>();
    isVisible = ShouldShowBar();

    if (worldCanvas != null)
    {
      worldCanvas.enabled = isVisible;
      canvasGroup = EnsureCanvasGroup();
      canvasGroup.alpha = isVisible ? 1f : 0f;
    }
  }

  CanvasGroup canvasGroup;

  // Subscribes to health events and initializes slider bounds/value.
  void OnEnable()
  {
    if (health == null)
    {
      health = GetComponentInParent<Health>();
    }

    if (health == null || slider == null)
    {
      return;
    }

    slider.maxValue = health.MaxHealth;
    slider.value = health.CurrentHealth;

    health.OnDamaged += HandleDamaged;
    health.OnHealed += HandleHealed;
    health.OnDied += HandleDied;

    isFadingOnDeath = false;
    deathFadeElapsed = 0f;

    if (canvasGroup != null && isVisible)
    {
      canvasGroup.alpha = 1f;
    }
  }

  // Removes health event subscriptions.
  void OnDisable()
  {
    if (health == null)
    {
      return;
    }

    health.OnDamaged -= HandleDamaged;
    health.OnHealed -= HandleHealed;
    health.OnDied -= HandleDied;
  }

  // Keeps the world canvas assigned to the active player camera and facing that camera.
  void LateUpdate()
  {
    UpdateVisibility();

    if (isFadingOnDeath)
    {
      TickDeathFade();
    }

    if (!isVisible)
    {
      return;
    }

    if (canvasTransform == null)
    {
      return;
    }

    ResolvePlayerCamera();
    if (playerCamera == null)
    {
      return;
    }

    if (worldCanvas != null && worldCanvas.worldCamera != playerCamera)
    {
      worldCanvas.worldCamera = playerCamera;
    }

    Vector3 toCamera = canvasTransform.position - playerCamera.transform.position;
    if (toCamera.sqrMagnitude > 0.0001f)
    {
      canvasTransform.rotation = Quaternion.LookRotation(toCamera, playerCamera.transform.up);
    }
  }

  // Toggles the bar while the enemy is actively engaged.
  void UpdateVisibility()
  {
    bool shouldShow = ShouldShowBar();
    if (isVisible == shouldShow)
    {
      return;
    }

    isVisible = shouldShow;

    if (worldCanvas != null)
    {
      worldCanvas.enabled = isVisible;
    }

    if (canvasGroup != null)
    {
      canvasGroup.alpha = isVisible ? 1f : 0f;
    }
  }

  bool ShouldShowBar()
  {
    if (health != null && health.IsDead)
    {
      return false;
    }

    if (enemyController != null)
    {
      return enemyController.IsAlerted;
    }

    // Fallback for setups without an EnemyController reference.
    return enemyAlert == null || !enemyAlert.enabled;
  }

  // Updates the slider when the enemy takes damage.
  void HandleDamaged(Vector3 _)
  {
    if (slider != null && health != null)
    {
      slider.value = health.CurrentHealth;
    }
  }

  // Updates the slider when the enemy is healed.
  void HandleHealed(Vector3 _)
  {
    if (slider != null && health != null)
    {
      slider.value = health.CurrentHealth;
    }
  }

  // Forces the slider empty when the enemy dies.
  void HandleDied()
  {
    if (slider != null)
    {
      slider.value = 0;
    }

    if (canvasGroup == null)
    {
      if (worldCanvas != null)
      {
        worldCanvas.enabled = false;
      }
      return;
    }

    if (deathFadeDuration <= 0f)
    {
      canvasGroup.alpha = 0f;
      if (worldCanvas != null)
      {
        worldCanvas.enabled = false;
      }
      return;
    }

    isFadingOnDeath = true;
    deathFadeElapsed = 0f;
  }

  // Resolves and caches the current main camera.
  void ResolvePlayerCamera()
  {
    if (playerCamera != null && playerCamera.isActiveAndEnabled)
    {
      return;
    }

    playerCamera = Camera.main;
  }

  // Fades the health bar canvas to full transparency after death.
  void TickDeathFade()
  {
    if (canvasGroup == null)
    {
      isFadingOnDeath = false;
      return;
    }

    deathFadeElapsed += Time.deltaTime;
    float t = Mathf.Clamp01(deathFadeElapsed / deathFadeDuration);
    canvasGroup.alpha = 1f - t;

    if (t < 1f)
    {
      return;
    }

    isFadingOnDeath = false;
    if (worldCanvas != null)
    {
      worldCanvas.enabled = false;
    }
  }

  CanvasGroup EnsureCanvasGroup()
  {
    CanvasGroup existingGroup = worldCanvas.GetComponent<CanvasGroup>();
    if (existingGroup != null)
    {
      return existingGroup;
    }

    return worldCanvas.gameObject.AddComponent<CanvasGroup>();
  }
}