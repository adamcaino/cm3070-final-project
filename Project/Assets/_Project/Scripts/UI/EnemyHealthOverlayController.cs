using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Displays pooled enemy health bars in the UI overlay and positions them over enemy world positions.
[RequireComponent(typeof(RectTransform))]
public class EnemyHealthOverlayController : MonoBehaviour
{
  const string PLAYERTAG = "Player";
  const string EXCLUDE_HEALTH_BAR_NAME = "Enemy_Slime";
  const string CLONE_SUFFIX = "(Clone)";

  [Header("Prefabs")]
  [SerializeField] GameObject healthBarPrefab;

  [Header("Positioning")]
  [SerializeField, Min(0f)] float worldHeightOffset = 2f;

  [Header("Behavior")]
  [SerializeField, Min(0.1f)] float rescanInterval = 0.5f;

  sealed class TrackedEnemy
  {
    public Health Health;
    public EnemyController Controller;
    public EnemyAlert Alert;
    public RectTransform BarRect;
    public Slider Slider;
    public CanvasGroup CanvasGroup;
  }

  RectTransform selfRect;
  Camera mainCamera;
  float rescanElapsed;

  readonly Dictionary<Health, TrackedEnemy> trackedEnemies = new Dictionary<Health, TrackedEnemy>();
  readonly Queue<RectTransform> barPool = new Queue<RectTransform>();
  readonly List<Health> staleKeys = new List<Health>();

  // Caches the overlay parent used by pooled bars.
  void Awake()
  {
    selfRect = GetComponent<RectTransform>();
  }

  // Subscribes to dungeon readiness and performs an initial scan if already ready.
  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;

    if (DungeonReadySignal.IsReady)
    {
      HandleDungeonReady();
    }
  }

  // Removes subscriptions and returns active bars to the pool.
  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
    ClearTrackedEnemies();
  }

  // Refreshes runtime references and collects currently spawned enemies.
  void HandleDungeonReady()
  {
    ResolveRuntimeReferences();
    ScanForEnemies();
    rescanElapsed = 0f;
  }

  // Updates camera references, scans periodically, and positions active bars.
  void LateUpdate()
  {
    if (healthBarPrefab == null)
    {
      return;
    }

    ResolveMainCamera();
    if (mainCamera == null)
    {
      return;
    }

    rescanElapsed += Time.deltaTime;
    if (rescanElapsed >= rescanInterval)
    {
      rescanElapsed = 0f;
      ScanForEnemies();
    }

    staleKeys.Clear();

    foreach (KeyValuePair<Health, TrackedEnemy> pair in trackedEnemies)
    {
      Health health = pair.Key;
      TrackedEnemy entry = pair.Value;

      if (health == null)
      {
        staleKeys.Add(pair.Key);
        continue;
      }

      UpdateAlertState(entry);
      UpdateHealthVisuals(entry);

      if (!ShouldShowBar(entry))
      {
        entry.BarRect.gameObject.SetActive(false);
        continue;
      }

      if (entry.Health.IsDead)
      {
        staleKeys.Add(health);
        continue;
      }

      PositionBar(entry.BarRect, health.transform);
    }

    for (int i = 0; i < staleKeys.Count; i++)
    {
      RemoveTrackedEnemy(staleKeys[i]);
    }
  }

  // Resolves player and camera references used by this overlay.
  void ResolveRuntimeReferences()
  {
    GameObject player = GameObject.FindGameObjectWithTag(PLAYERTAG);
    if (player == null)
    {
      return;
    }

    ResolveMainCamera();
  }

  // Scans all enemy controllers and starts tracking newly discovered non-boss enemies.
  void ScanForEnemies()
  {
    EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

    for (int i = 0; i < enemies.Length; i++)
    {
      EnemyController enemy = enemies[i];
      if (enemy == null || enemy.IsBoss)
      {
        continue;
      }

      if (ShouldExcludeEnemy(enemy))
      {
        continue;
      }

      Health health = enemy.Health;
      if (health == null || trackedEnemies.ContainsKey(health))
      {
        continue;
      }

      RectTransform barRect = GetBarFromPool();
      Slider slider = barRect.GetComponent<Slider>();
      CanvasGroup group = EnsureCanvasGroup(barRect);

      TrackedEnemy entry = new TrackedEnemy
      {
        Health = health,
        Controller = enemy,
        Alert = enemy.GetComponent<EnemyAlert>(),
        BarRect = barRect,
        Slider = slider,
        CanvasGroup = group
      };

      bool shouldShow = ShouldShowBar(entry);
      if (entry.CanvasGroup != null)
      {
        entry.CanvasGroup.alpha = shouldShow ? 1f : 0f;
      }

      barRect.gameObject.SetActive(shouldShow);
      trackedEnemies.Add(health, entry);
      UpdateHealthVisuals(entry);
    }
  }

  // Synchronizes fade alpha with current visibility state.
  void UpdateAlertState(TrackedEnemy entry)
  {
    if (entry.CanvasGroup == null)
    {
      return;
    }

    entry.CanvasGroup.alpha = ShouldShowBar(entry) ? 1f : 0f;
  }

  bool ShouldShowBar(TrackedEnemy entry)
  {
    if (entry == null || entry.Health == null || entry.Health.IsDead)
    {
      return false;
    }

    if (entry.Controller != null)
    {
      return entry.Controller.IsAlerted;
    }

    return entry.Alert == null || !entry.Alert.enabled;
  }

  // Synchronizes slider limits and current value with the tracked health component.
  void UpdateHealthVisuals(TrackedEnemy entry)
  {
    if (entry.Slider == null || entry.Health == null)
    {
      return;
    }

    entry.Slider.maxValue = entry.Health.MaxHealth;
    entry.Slider.value = entry.Health.CurrentHealth;
  }

  // Excludes configured enemy names, accounting for Unity's runtime clone suffix.
  bool ShouldExcludeEnemy(EnemyController enemy)
  {
    string enemyName = enemy.name;
    if (enemyName.EndsWith(CLONE_SUFFIX))
    {
      enemyName = enemyName.Substring(0, enemyName.Length - CLONE_SUFFIX.Length);
    }

    return enemyName == EXCLUDE_HEALTH_BAR_NAME;
  }

  // Projects a world point into overlay space and hides bars behind the camera.
  void PositionBar(RectTransform barRect, Transform target)
  {
    Vector3 worldPosition = target.position + (Vector3.up * worldHeightOffset);
    Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPosition);

    if (screenPoint.z < 0f)
    {
      barRect.gameObject.SetActive(false);
      return;
    }

    RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRect, screenPoint, null, out Vector2 localPoint);
    barRect.anchoredPosition = localPoint;
    barRect.gameObject.SetActive(true);
  }

  // Returns all active bars to the pool and clears tracking state.
  void ClearTrackedEnemies()
  {
    foreach (KeyValuePair<Health, TrackedEnemy> pair in trackedEnemies)
    {
      ReturnBarToPool(pair.Value.BarRect);
    }

    trackedEnemies.Clear();
    staleKeys.Clear();
  }

  // Removes one tracked enemy and recycles its UI bar.
  void RemoveTrackedEnemy(Health health)
  {
    if (!trackedEnemies.TryGetValue(health, out TrackedEnemy entry))
    {
      return;
    }

    ReturnBarToPool(entry.BarRect);
    trackedEnemies.Remove(health);
  }

  // Pulls a bar from the pool or instantiates a new one under this overlay.
  RectTransform GetBarFromPool()
  {
    if (barPool.Count > 0)
    {
      return barPool.Dequeue();
    }

    return Instantiate(healthBarPrefab, selfRect).GetComponent<RectTransform>();
  }

  // Hides a bar and stores it for reuse.
  void ReturnBarToPool(RectTransform barRect)
  {
    if (barRect == null)
    {
      return;
    }

    barRect.gameObject.SetActive(false);
    barPool.Enqueue(barRect);
  }

  // Ensures pooled bar roots have a CanvasGroup for alpha fading.
  CanvasGroup EnsureCanvasGroup(RectTransform barRect)
  {
    CanvasGroup group = barRect.GetComponent<CanvasGroup>();
    if (group != null)
    {
      return group;
    }

    return barRect.gameObject.AddComponent<CanvasGroup>();
  }

  // Keeps a valid camera cached across scene loads.
  void ResolveMainCamera()
  {
    if (mainCamera != null && mainCamera.isActiveAndEnabled)
    {
      return;
    }

    mainCamera = Camera.main;
  }
}