using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Displays boss health and remains hidden until a boss encounter starts tracking a Health component.
public class BossHealthBarUI : MonoBehaviour
{
  [SerializeField, Min(0f)] float punchStrength = 0.15f;
  [SerializeField, Min(0f)] float punchDuration = 0.18f;

  Health health;
  Slider slider;
  Vector3 baseScale;
  Coroutine activePunch;

  // Caches the slider and hides the boss health bar until an encounter begins.
  void Awake()
  {
    slider = GetComponent<Slider>();
    baseScale = transform.localScale;
    gameObject.SetActive(false);
  }

  // Binds the bar to a boss health component and displays its current value.
  public void BeginTracking(Health bossHealth)
  {
    if (bossHealth == null) return;

    health = bossHealth;
    slider.maxValue = health.MaxHealth;
    slider.value = health.CurrentHealth;

    health.OnDamaged += HandleDamaged;
    health.OnHealed += HandleHealed;
    health.OnDied += HandleDied;

    gameObject.SetActive(true);
  }

  // Removes boss health subscriptions and hides the bar.
  public void StopTracking()
  {
    if (health == null) return;

    health.OnDamaged -= HandleDamaged;
    health.OnHealed -= HandleHealed;
    health.OnDied -= HandleDied;
    health = null;

    gameObject.SetActive(false);
  }

  // Removes boss health subscriptions before the UI object is destroyed.
  void OnDestroy()
  {
    if (health == null) return;

    health.OnDamaged -= HandleDamaged;
    health.OnHealed -= HandleHealed;
    health.OnDied -= HandleDied;
  }

  // Updates the bar and starts its damage punch animation.
  void HandleDamaged(Vector3 _)
  {
    slider.value = health.CurrentHealth;

    if (activePunch != null)
    {
      StopCoroutine(activePunch);
    }
    activePunch = StartCoroutine(PunchRoutine());
  }

  // Updates the bar after the boss is healed.
  void HandleHealed(Vector3 _) => slider.value = health.CurrentHealth;

  // Sets the bar to zero after the boss dies.
  void HandleDied() => slider.value = 0;

  // Scales the bar briefly and restores its authored scale.
  IEnumerator PunchRoutine()
  {
    float elapsed = 0f;
    while (elapsed < punchDuration)
    {
      elapsed += Time.deltaTime;
      float t = Mathf.Clamp01(elapsed / punchDuration);
      // Damped sine: kicks out immediately then settles back to baseScale by t=1.
      float scaleFactor = 1f + punchStrength * (1f - t) * Mathf.Sin(t * Mathf.PI);
      transform.localScale = baseScale * scaleFactor;
      yield return null;
    }

    transform.localScale = baseScale;
    activePunch = null;
  }
}
