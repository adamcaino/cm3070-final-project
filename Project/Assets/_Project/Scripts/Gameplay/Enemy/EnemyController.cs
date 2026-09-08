using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Coordinates state changes, aggro flow, and freeze behavior for the enemy.
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(EnemyDetection))]
[RequireComponent(typeof(StatusEffectReceiver))]
public class EnemyController : MonoBehaviour
{
  const string PLAYERTAG = "Player";
  static readonly int PlayerSpottedParam = Animator.StringToHash("playerSpotted");

  [Header("Startup")]
  [Tooltip("Start in IdleState (no movement, no reaction to detection/damage) instead of RoamState - for an enemy that should stay dormant until something explicit wakes it, e.g. a boss waiting on its room's encounter trigger.")]
  [SerializeField] bool startIdle;

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

  [Header("Death")]
  [Tooltip("Time to smoothly settle the NavMeshAgent's base offset to 0 on death, so flying/hovering enemies come down to the ground for their death animation instead of snapping.")]
  [SerializeField, Min(0f)] float deathBaseOffsetSettleDuration = 0.5f;

  IEnemyState currentState;
  bool isBoss;

  public bool IsFrozen { get; set; }

  public NavMeshAgent Agent { get; private set; }
  public Animator Animator { get; private set; }
  public Health Health { get; private set; }
  public EnemyDetection Detection { get; private set; }
  public StatusEffectReceiver StatusEffects { get; private set; }
  public Transform Player { get; private set; }
  public IReadOnlyList<IAttack> Attacks { get; private set; }
  public bool IsBoss => isBoss;
  public bool IsAlerted { get; private set; }

  public float RoamRadius => roamRadius;
  public float RoamWaitTime => roamWaitTime;
  public float RotationSpeed => rotationSpeed;
  public float AggroMemoryDuration => aggroMemoryDuration;
  public float DeathBaseOffsetSettleDuration => deathBaseOffsetSettleDuration;

  public bool IsMoving => Agent.velocity.sqrMagnitude > movingSpeedThreshold * movingSpeedThreshold;

  // Caches required components, finds the player, and identifies boss instances.
  void Awake()
  {
    Agent = GetComponent<NavMeshAgent>();
    Animator = GetComponent<Animator>();
    Health = GetComponent<Health>();
    Detection = GetComponent<EnemyDetection>();
    StatusEffects = GetComponent<StatusEffectReceiver>();
    Attacks = GetComponents<IAttack>();

    GameObject playerObject = GameObject.FindGameObjectWithTag(PLAYERTAG);
    Player = playerObject != null ? playerObject.transform : null;

    // Bosses are authored with a BossRoomEncounter on the same GameObject.
    isBoss = TryGetComponent<BossRoomEncounter>(out _);
  }

  // Subscribes to health and player-death events.
  void OnEnable()
  {
    Health.OnDied += HandleDied;
    PlayerDiedSignal.Raised += HandlePlayerDied;
  }

  // Removes health and player-death event subscriptions.
  void OnDisable()
  {
    Health.OnDied -= HandleDied;
    PlayerDiedSignal.Raised -= HandlePlayerDied;
  }

  // Selects the initial idle or roaming state.
  void Start()
  {
    ChangeState(startIdle ? (IEnemyState)new IdleState() : new RoamState());
  }

  // Activates the enemy and moves it into the positioning state.
  public void Wake()
  {
    if (isBoss)
    {
      Animator?.SetTrigger(PlayerSpottedParam);
    }

    ChangeState(new PositionState());
  }

  // Ticks the current state unless the enemy is frozen.
  void Update()
  {
    if (IsFrozen) return;

    currentState?.Tick(this);
  }

  // Exits the current state and enters the supplied state.
  public void ChangeState(IEnemyState newState)
  {
    currentState?.Exit(this);
    currentState = newState;
    currentState?.Enter(this);
    SyncAlertState(newState);
  }

  // Keeps the runtime alert state aligned to active combat states.
  void SyncAlertState(IEnemyState state)
  {
    IsAlerted = state is PositionState || state is AttackState;
  }


  // Smoothly rotates the enemy toward a world position on the horizontal plane.
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

  // Changes to the dead state when the enemy's health reaches zero.
  void HandleDied()
  {
    ChangeState(new DeadState());
  }

  // Changes living enemies to the victory state when the player dies.
  void HandlePlayerDied()
  {
    if (Health.IsDead) return;

    ChangeState(new VictoryState());
  }
}
