using Unity.Cinemachine;
using UnityEngine;

// Sits on the death-cam vcam itself. On player death, boosts its own priority above the gameplay
// vcam so CinemachineBrain blends into it (the pan/zoom out), then keeps it slowly auto-orbiting
// the corpse for as long as the Game Over screen is up - driving the same HorizontalAxis the player
// normally free-looks with, just from this script instead of input.
[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class PlayerDeathCamera : MonoBehaviour
{
  [Header("References")]
  [SerializeField] CinemachineCamera gameplayVcam;
  [SerializeField] CinemachineBrain brain;

  [Header("Blend")]
  [Tooltip("How long the pan/zoom out to the death cam takes.")]
  [SerializeField, Min(0.1f)] float blendDuration = 4f;
  [Tooltip("Added to the gameplay vcam's priority so the death cam always wins once active.")]
  [SerializeField] int activePriorityOffset = 100;

  [Header("Orbit")]
  [SerializeField, Min(0f)] float orbitSpeed = 6f;

  CinemachineCamera deathVcam;
  CinemachineOrbitalFollow orbitalFollow;
  bool isActive;

  void Awake()
  {
    deathVcam = GetComponent<CinemachineCamera>();
    orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
  }

  void OnEnable()
  {
    PlayerDiedSignal.Raised += HandlePlayerDied;
  }

  void OnDisable()
  {
    PlayerDiedSignal.Raised -= HandlePlayerDied;
  }

  void HandlePlayerDied()
  {
    if (brain != null)
    {
      brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendDuration);
    }

    int basePriority = gameplayVcam != null ? gameplayVcam.Priority.Value : 0;
    deathVcam.Priority = basePriority + activePriorityOffset;
    isActive = true;
  }

  void Update()
  {
    if (!isActive) return;

    orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(
      orbitalFollow.HorizontalAxis.Value + (orbitSpeed * Time.deltaTime));
  }
}
