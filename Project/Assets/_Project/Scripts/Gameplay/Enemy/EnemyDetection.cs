using System;
using UnityEngine;

/// <summary>
/// Standalone vision component - a cone (radius + angle) plus a line-of-sight raycast so an enemy
/// can't see the player through a wall it's technically within radius of. Edge-triggered: fires
/// OnPlayerSpotted/OnPlayerLost only on the transition, not every frame, so states can subscribe in
/// Enter and unsubscribe in Exit without re-triggering themselves.
/// </summary>
public class EnemyDetection : MonoBehaviour
{
  const string PLAYERTAG = "Player";

  [SerializeField, Min(0f)] float visionRadius = 8f;
  [SerializeField, Range(0f, 360f)] float visionAngle = 120f;
  [SerializeField] LayerMask obstacleMask;
  [SerializeField] float eyeHeight = 1.5f;

  Transform player;

  public bool CanSeePlayer { get; private set; }

  public event Action OnPlayerSpotted;
  public event Action OnPlayerLost;

  void Awake()
  {
    GameObject playerObject = GameObject.FindGameObjectWithTag(PLAYERTAG);
    player = playerObject != null ? playerObject.transform : null;
  }

  void Update()
  {
    bool canSeePlayerNow = player != null && EvaluateVisibility();

    if (canSeePlayerNow && !CanSeePlayer)
    {
      CanSeePlayer = true;
      OnPlayerSpotted?.Invoke();
    }
    else if (!canSeePlayerNow && CanSeePlayer)
    {
      CanSeePlayer = false;
      OnPlayerLost?.Invoke();
    }
  }

  bool EvaluateVisibility()
  {
    Vector3 eyePosition = transform.position + (Vector3.up * eyeHeight);
    Vector3 toPlayer = player.position - eyePosition;

    if (toPlayer.sqrMagnitude > visionRadius * visionRadius)
    {
      return false;
    }

    if (Vector3.Angle(transform.forward, toPlayer) > visionAngle * 0.5f)
    {
      return false;
    }

    // Obstacle mask should exclude the player's own layer - a hit here means something is blocking the view.
    return !Physics.Raycast(eyePosition, toPlayer.normalized, toPlayer.magnitude, obstacleMask);
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color.yellow;
    Vector3 eyePosition = transform.position + (Vector3.up * eyeHeight);
    Gizmos.DrawWireSphere(eyePosition, visionRadius);

    Vector3 forward = transform.forward;
    Vector3 rightBoundary = Quaternion.Euler(0f, visionAngle * 0.5f, 0f) * forward;
    Vector3 leftBoundary = Quaternion.Euler(0f, -visionAngle * 0.5f, 0f) * forward;
    Gizmos.DrawLine(eyePosition, eyePosition + rightBoundary * visionRadius);
    Gizmos.DrawLine(eyePosition, eyePosition + leftBoundary * visionRadius);

    if (player == null)
    {
      return;
    }

    Vector3 toPlayer = player.position - eyePosition;
    bool blocked = Physics.Raycast(eyePosition, toPlayer.normalized, out RaycastHit hit, toPlayer.magnitude, obstacleMask);

    Gizmos.color = blocked ? Color.red : Color.green;
    Gizmos.DrawLine(eyePosition, blocked ? hit.point : player.position);

    if (blocked)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(hit.point, 0.15f);
    }
  }
}
