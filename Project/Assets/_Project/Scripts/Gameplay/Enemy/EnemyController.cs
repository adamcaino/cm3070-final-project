using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Thin context object for the state machine - holds the current state and shared references/tuning that
/// every state needs (agent, animator, player, attacks) so states themselves stay plain C# classes with
/// no MonoBehaviour/Inspector concerns of their own. Death is handled here rather than inside whichever
/// state is active, since it's a valid interrupt from any state, not a transition any one state decides on.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyDetection))]
[RequireComponent(typeof(StatusEffectReceiver))]
public class EnemyController : MonoBehaviour
{
  const string PLAYERTAG = "Player";

  [Header("Roaming")]
  [SerializeField, Min(0f)] float roamRadius = 10f;
  [SerializeField, Min(0f)] float roamWaitTime = 2f;

  [Header("Attacking")]
  [Tooltip("Turn sharpness for facing the player - higher snaps faster, lower gives a slower, more visible slerp.")]
  [SerializeField, Min(0f)] float rotationSpeed = 8f;

  [Header("Aggro")]
  [Tooltip("How long the enemy keeps chasing/facing the player's last known position after losing sight of them, before giving up and going back to roaming.")]
  [SerializeField, Min(0f)] float aggroMemoryDuration = 5f;

  [Header("Movement")]
  [SerializeField, Min(0f)] float movingSpeedThreshold = 0.1f;

  IEnemyState currentState;

  public NavMeshAgent Agent { get; private set; }
  public Animator Animator { get; private set; }
  public Health Health { get; private set; }
  public EnemyDetection Detection { get; private set; }
  public Transform Player { get; private set; }
  public IReadOnlyList<IAttack> Attacks { get; private set; }

  public float RoamRadius => roamRadius;
  public float RoamWaitTime => roamWaitTime;
  public float RotationSpeed => rotationSpeed;
  public float AggroMemoryDuration => aggroMemoryDuration;

  // Single source of truth for "is this enemy currently moving" - both animation (EnemyAnimationDriver)
  // and gameplay logic (e.g. PositionState gating attacks) read this instead of each computing their own
  // velocity check, so they can't disagree about the enemy's movement state on any given frame.
  public bool IsMoving => Agent.velocity.sqrMagnitude > movingSpeedThreshold * movingSpeedThreshold;

  void Awake()
  {
    Agent = GetComponent<NavMeshAgent>();
    Animator = GetComponent<Animator>();
    Health = GetComponent<Health>();
    Detection = GetComponent<EnemyDetection>();
    Attacks = GetComponents<IAttack>();

    GameObject playerObject = GameObject.FindGameObjectWithTag(PLAYERTAG);
    Player = playerObject != null ? playerObject.transform : null;
  }

  void OnEnable()
  {
    Health.OnDied += HandleDied;
  }

  void OnDisable()
  {
    Health.OnDied -= HandleDied;
  }

  void Start()
  {
    ChangeState(new RoamState());
  }

  void Update()
  {
    currentState?.Tick(this);
  }

  public void ChangeState(IEnemyState newState)
  {
    currentState?.Exit(this);
    currentState = newState;
    currentState?.Enter(this);
  }

  // Shared by any "aware" state (Position, Attack) that wants to keep turning to face the player under
  // manual control rather than the NavMeshAgent's own movement-coupled rotation. Slerps rather than
  // rotating at a constant angular speed so the turn eases out instead of snapping to face the target.
  public void FaceTowards(Vector3 worldPosition)
  {
    Vector3 direction = worldPosition - transform.position;
    direction.y = 0f;
    if (direction.sqrMagnitude < 0.0001f)
    {
      return;
    }

    Quaternion targetRotation = Quaternion.LookRotation(direction);
    float t = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
  }

  void HandleDied()
  {
    ChangeState(new DeadState());
  }
}
