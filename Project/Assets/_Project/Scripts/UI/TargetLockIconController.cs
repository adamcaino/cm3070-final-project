using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Displays pooled candidate icons and a single locked-target icon in the screen-space UI overlay.
[RequireComponent(typeof(RectTransform))]
public class TargetLockIconController : MonoBehaviour
{
  const string PLAYERTAG = "Player";

  [SerializeField] GameObject whiteIconPrefab;
  [SerializeField] GameObject redIconPrefab;
  [SerializeField, Min(0f)] float worldHeightOffset = 2f;

  TargetLockController lockController;
  Camera mainCamera;
  RectTransform selfRect;
  RectTransform redIcon;

  readonly Dictionary<Health, RectTransform> activeWhiteIcons = new Dictionary<Health, RectTransform>();
  readonly Queue<RectTransform> whitePool = new Queue<RectTransform>();
  readonly List<Health> staleHealth = new List<Health>();

  // Caches the overlay RectTransform used to parent and position lock-on icons.
  void Awake()
  {
    selfRect = GetComponent<RectTransform>();
  }

  // Subscribes to dungeon readiness so lock-on references are resolved after generation.
  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;
  }

  // Removes the dungeon readiness subscription.
  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
  }

  // Resolves the player lock controller and creates the reusable locked-target icon.
  void HandleDungeonReady()
  {
    RefreshPlayerLockController();
  }

  // Finds the player controller, camera, and red locked-target icon.
  void RefreshPlayerLockController()
  {
    GameObject player = GameObject.FindGameObjectWithTag(PLAYERTAG);
    if (player == null)
    {
      Debug.LogWarning("TargetLockIconController: no GameObject tagged 'Player' found in the loaded scenes.");
      enabled = false;
      return;
    }

    lockController = player.GetComponent<TargetLockController>();
    if (lockController == null)
    {
      Debug.LogWarning("TargetLockIconController: player has no TargetLockController component.");
      enabled = false;
      return;
    }

    mainCamera = Camera.main;

    if (redIcon == null)
    {
      redIcon = Instantiate(redIconPrefab, selfRect).GetComponent<RectTransform>();
      redIcon.gameObject.SetActive(false);
    }
  }

  // Updates the locked and candidate icon positions after camera movement.
  void LateUpdate()
  {
    if (mainCamera == null) return;

    UpdateRedIcon();
    UpdateWhiteIcons();
  }

  // Shows and positions the red icon for the current locked target.
  void UpdateRedIcon()
  {
    if (!lockController.IsLocked)
    {
      if (redIcon.gameObject.activeSelf)
      {
        redIcon.gameObject.SetActive(false);
      }
      return;
    }

    PositionIcon(redIcon, lockController.LockedTarget);
  }

  // Synchronizes pooled white icons with the current range candidate set.
  void UpdateWhiteIcons()
  {
    Transform lockedTarget = lockController.LockedTarget;
    HashSet<Health> candidates = lockController.RangeCandidates;

    // Reordered so the presence check (safe on a destroyed key) always runs before .transform is
    // touched - CollectCandidates already filters dead/destroyed Health, so anything still in
    // candidates is guaranteed alive.
    staleHealth.Clear();
    foreach (KeyValuePair<Health, RectTransform> pair in activeWhiteIcons)
    {
      Health health = pair.Key;
      if (!candidates.Contains(health) || health.transform == lockedTarget)
      {
        staleHealth.Add(health);
      }
    }

    for (int i = 0; i < staleHealth.Count; i++)
    {
      Health health = staleHealth[i];
      ReturnToPool(activeWhiteIcons[health]);
      activeWhiteIcons.Remove(health);
    }

    foreach (Health candidate in candidates)
    {
      if (candidate.transform == lockedTarget) continue;

      if (!activeWhiteIcons.TryGetValue(candidate, out RectTransform icon))
      {
        icon = GetFromPool();
        activeWhiteIcons.Add(candidate, icon);
      }

      PositionIcon(icon, candidate.transform);
    }
  }

  // Projects a world target into overlay coordinates and hides it when behind the camera.
  void PositionIcon(RectTransform icon, Transform target)
  {
    Vector3 worldPosition = target.position + (Vector3.up * worldHeightOffset);
    Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPosition);

    if (screenPoint.z < 0f)
    {
      icon.gameObject.SetActive(false);
      return;
    }

    RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRect, screenPoint, null, out Vector2 localPoint);
    icon.anchoredPosition = localPoint;
    icon.gameObject.SetActive(true);
  }

  // Reuses a pooled icon or instantiates a new white candidate icon.
  RectTransform GetFromPool()
  {
    if (whitePool.Count > 0)
    {
      return whitePool.Dequeue();
    }

    return Instantiate(whiteIconPrefab, selfRect).GetComponent<RectTransform>();
  }

  // Hides and stores a candidate icon for later reuse.
  void ReturnToPool(RectTransform icon)
  {
    icon.gameObject.SetActive(false);
    whitePool.Enqueue(icon);
  }
}
