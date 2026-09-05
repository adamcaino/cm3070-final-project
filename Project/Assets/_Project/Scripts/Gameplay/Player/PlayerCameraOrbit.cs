using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;








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
