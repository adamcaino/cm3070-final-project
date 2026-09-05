using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBarUI : MonoBehaviour
{
  const string PLAYERTAG = "Player";

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
  }

  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;
    RefreshHealthReference(false);
  }

  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;

    if (health == null) return;

    health.OnDamaged -= HandleDamaged;
    health.OnHealed -= HandleHealed;
    health.OnDied -= HandleDied;
  }

  void HandleDungeonReady()
  {
    RefreshHealthReference(true);
  }

  void RefreshHealthReference(bool logIfMissing)
  {
    // Unsubscribe from old health if it exists
    if (health != null)
    {
      health.OnDamaged -= HandleDamaged;
      health.OnHealed -= HandleHealed;
      health.OnDied -= HandleDied;
    }

    GameObject player = GameObject.FindGameObjectWithTag(PLAYERTAG);
    if (player == null)
    {
      if (logIfMissing)
      {
        Debug.LogWarning("HealthBarUI: no GameObject tagged 'Player' found in the loaded scenes.");
      }
      return;
    }

    health = player.GetComponent<Health>();
    if (health == null)
    {
      Debug.LogWarning("HealthBarUI: player has no Health component.");
      return;
    }

    slider.maxValue = health.MaxHealth;
    slider.value = health.CurrentHealth;

    health.OnDamaged += HandleDamaged;
    health.OnHealed += HandleHealed;
    health.OnDied += HandleDied;
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
