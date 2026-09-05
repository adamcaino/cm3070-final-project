using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boss equivalent of HealthBarUI - same slider-wiring/punch-scale pattern, but stays hidden until a boss
/// encounter explicitly starts tracking a Health (see BossRoomEncounter), instead of finding the player by
/// tag on Start. Sits on the BossHealthSlider prefab instance in the HUD canvas.
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
  [SerializeField, Min(0f)] float punchStrength = 0.15f;
  [SerializeField, Min(0f)] float punchDuration = 0.18f;

  Health health;
  Slider slider;
  Vector3 baseScale;
  Coroutine activePunch;

  void Awake()
  {
    slider = GetComponent<Slider>();
    baseScale = transform.localScale;
    gameObject.SetActive(false);
  }

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

  public void StopTracking()
  {
    if (health == null) return;

    health.OnDamaged -= HandleDamaged;
    health.OnHealed -= HandleHealed;
    health.OnDied -= HandleDied;
    health = null;

    gameObject.SetActive(false);
  }

  void OnDestroy()
  {
    if (health == null) return;

    health.OnDamaged -= HandleDamaged;
    health.OnHealed -= HandleHealed;
    health.OnDied -= HandleDied;
  }

  void HandleDamaged(Vector3 _)
  {
    slider.value = health.CurrentHealth;

    if (activePunch != null)
    {
      StopCoroutine(activePunch);
    }
    activePunch = StartCoroutine(PunchRoutine());
  }

  void HandleHealed(Vector3 _) => slider.value = health.CurrentHealth;

  void HandleDied() => slider.value = 0;

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
