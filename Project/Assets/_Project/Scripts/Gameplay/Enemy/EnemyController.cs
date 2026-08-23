using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

  // Stops state-driven movement/facing while frozen without disabling the component.
  public bool IsFrozen { get; set; }

  public NavMeshAgent Agent { get; private set; }
  public Animator Animator { get; private set; }
  public Health Health { get; private set; }
  public EnemyDetection Detection { get; private set; }
  public StatusEffectReceiver StatusEffects { get; private set; }
  public Transform Player { get; private set; }
  public IReadOnlyList<IAttack> Attacks { get; private set; }

  public float RoamRadius => roamRadius;
  public float RoamWaitTime => roamWaitTime;
  public float RotationSpeed => rotationSpeed;
  public float AggroMemoryDuration => aggroMemoryDuration;

  // Shared movement state for animation and gameplay logic.
  public bool IsMoving => Agent.velocity.sqrMagnitude > movingSpeedThreshold * movingSpeedThreshold;

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
    if (IsFrozen) return;

    currentState?.Tick(this);
  }

  public void ChangeState(IEnemyState newState)
  {
    currentState?.Exit(this);
    currentState = newState;
    currentState?.Enter(this);
  }

  // Smoothly faces the target for aware states using manual rotation.
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
