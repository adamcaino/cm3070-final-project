using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
// Changes a boss from stage one to stage two when its health reaches the threshold.
public class BossPhaseController : MonoBehaviour
{
  [Tooltip("Health fraction at or below which the boss enters stage 2.")]
  [SerializeField, Range(0f, 1f)] float phaseTwoThreshold = 0.5f;

  Health health;

  public int CurrentStage { get; private set; } = 1;

  public event Action<int> OnStageChanged;

  // Caches the health component used to evaluate the phase threshold.
  void Awake()
  {
    health = GetComponent<Health>();
  }

  // Subscribes to health damage events.
  void OnEnable()
  {
    health.OnDamaged += HandleDamaged;
  }

  // Removes the health damage event subscription.
  void OnDisable()
  {
    health.OnDamaged -= HandleDamaged;
  }

  // Evaluates the current health fraction and raises the stage-change event once.
  void HandleDamaged(Vector3 _)
  {
    float healthPercent = health.MaxHealth <= 0 ? 0f : (float)health.CurrentHealth / health.MaxHealth;

    if (CurrentStage == 2) return;
    if (healthPercent > phaseTwoThreshold) return;

    CurrentStage = 2;

    OnStageChanged?.Invoke(CurrentStage);
  }
}
