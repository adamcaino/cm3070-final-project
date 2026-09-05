using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows Alert_Dot_White above every enemy TargetLockController currently considers a lock-on
/// candidate (in range, in the lock cone, unobstructed) and Alert_Dot_Red above the locked target.
/// Lives in the UI scene as a screen-space overlay child; repositions icons every LateUpdate by
/// projecting each candidate's world position through the main camera. White icons are pooled since
/// the candidate set size changes frame to frame; the red icon is a single reused instance.
/// </summary>
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

  void Awake()
  {
    selfRect = GetComponent<RectTransform>();
  }

  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;
  }

  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
  }

  void HandleDungeonReady()
  {
    RefreshPlayerLockController();
  }

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

  void LateUpdate()
  {
    if (mainCamera == null) return;

    UpdateRedIcon();
    UpdateWhiteIcons();
  }

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

  RectTransform GetFromPool()
  {
    if (whitePool.Count > 0)
    {
      return whitePool.Dequeue();
    }

    return Instantiate(whiteIconPrefab, selfRect).GetComponent<RectTransform>();
  }

  void ReturnToPool(RectTransform icon)
  {
    icon.gameObject.SetActive(false);
    whitePool.Enqueue(icon);
  }
}
