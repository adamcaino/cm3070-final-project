using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;







public class PlayerCameraZoom : MonoBehaviour
{
  [Header("Input")]
  [SerializeField] InputActionReference zoomAction;

  [Header("Target")]
  [SerializeField] CinemachineOrbitalFollow orbitalFollow;

  [Header("Zoom Range")]
  [SerializeField, Min(0.1f)] float minDistance = 3f;
  [SerializeField, Min(0.1f)] float maxDistance = 12f;
  [SerializeField, Min(0f)] float zoomSpeed = 5f;

  float targetDistance;

  void Awake()
  {
    if (orbitalFollow != null)
    {
      targetDistance = orbitalFollow.Radius;
    }
  }

  void OnEnable()
  {
    zoomAction?.action.Enable();
  }

  void OnDisable()
  {
    zoomAction?.action.Disable();
  }

  void Update()
  {
    if (orbitalFollow == null || zoomAction == null)
    {
      return;
    }

    float zoomInput = zoomAction.action.ReadValue<float>();
    if (Mathf.Abs(zoomInput) > 0.0001f)
    {
      targetDistance = Mathf.Clamp(targetDistance - zoomInput, minDistance, maxDistance);
    }

    orbitalFollow.Radius = Mathf.Lerp(orbitalFollow.Radius, targetDistance, zoomSpeed * Time.deltaTime);
  }
}
