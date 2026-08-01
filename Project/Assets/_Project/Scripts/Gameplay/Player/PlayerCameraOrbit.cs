using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads the Look action and drives OrbitalFollow's Horizontal/Vertical axes directly, so the camera
/// orbits purely from mouse/stick input. Forces WorldSpace binding on Awake so the orbit frame is
/// independent of the player's own rotation - OrbitalFollow's other binding modes tie the orbit frame
/// to the target's forward, which would make the camera passively drag along behind PlayerLocomotion
/// rotating the player, undoing free camera control.
/// </summary>
public class PlayerCameraOrbit : MonoBehaviour
{
  [Header("Input")]
  [SerializeField] InputActionReference lookAction;

  [Header("Target")]
  [SerializeField] CinemachineOrbitalFollow orbitalFollow;

  [Header("Sensitivity")]
  [SerializeField] float horizontalSpeed = 1f;
  [SerializeField] float verticalSpeed = 1f;
  [SerializeField] bool invertVertical;

  void Awake()
  {
    if (orbitalFollow != null)
    {
      orbitalFollow.TrackerSettings.BindingMode = Unity.Cinemachine.TargetTracking.BindingMode.WorldSpace;
    }
  }

  void OnEnable()
  {
    lookAction?.action.Enable();
  }

  void OnDisable()
  {
    lookAction?.action.Disable();
  }

  void Update()
  {
    if (orbitalFollow == null || lookAction == null)
    {
      return;
    }

    Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
    float verticalSign = invertVertical ? 1f : -1f;

    orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(
      orbitalFollow.HorizontalAxis.Value + (lookInput.x * horizontalSpeed));

    orbitalFollow.VerticalAxis.Value = orbitalFollow.VerticalAxis.ClampValue(
      orbitalFollow.VerticalAxis.Value + (lookInput.y * verticalSpeed * verticalSign));
  }

  // Called once by spawn placement, before PlayerLocomotion starts slaving the player's facing to the
  // camera - otherwise the camera's leftover default yaw would win on the first frame and spin the
  // player to face it instead of the camera adopting the player's authored spawn facing. With
  // WorldSpace binding, HorizontalAxis 0 already lines up with world yaw 0 the same way transform
  // eulerAngles.y does, so this is a direct copy, just wrapped into the axis's -180..180 range.
  public void SnapYawToTarget(Transform target)
  {
    if (orbitalFollow == null || target == null)
    {
      return;
    }

    float yaw = target.eulerAngles.y;
    if (yaw > 180f)
    {
      yaw -= 360f;
    }

    orbitalFollow.HorizontalAxis.Value = orbitalFollow.HorizontalAxis.ClampValue(yaw);
  }
}
