using UnityEngine;

public class RotatePickup : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 90f;

    void Update()
    {
        // Rotate the pickup for a simple visual effect.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
