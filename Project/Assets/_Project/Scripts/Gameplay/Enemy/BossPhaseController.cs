using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossPhaseController : MonoBehaviour
{
  [Tooltip("Health fraction at or below which the boss enters stage 2.")]
  [SerializeField, Range(0f, 1f)] float phaseTwoThreshold = 0.5f;

  Health health;

  public int CurrentStage { get; private set; } = 1;

  public event Action<int> OnStageChanged;

  void Awake()
  {
    health = GetComponent<Health>();
  }

  void OnEnable()
  {
    health.OnDamaged += HandleDamaged;
  }

  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;
  }

  void HandleDamaged(Vector3 _)
  {
    float healthPercent = health.MaxHealth <= 0 ? 0f : (float)health.CurrentHealth / health.MaxHealth;

    if (CurrentStage == 2) return;
    if (healthPercent > phaseTwoThreshold) return;

    CurrentStage = 2;

    OnStageChanged?.Invoke(CurrentStage);
  }
}
