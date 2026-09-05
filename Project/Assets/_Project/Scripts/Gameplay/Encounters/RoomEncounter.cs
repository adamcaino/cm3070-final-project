using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RoomEncounter : MonoBehaviour, ITriggerable
{
  [Tooltip("Doors this encounter locks/unlocks. Assignable directly in the Inspector for a hand-built room, or set at runtime via SetDoors (e.g. DungeonBossPlacer3D).")]
  [SerializeField] List<Door> doors = new List<Door>();

  bool started;

  public event Action OnEncounterStarted;
  public event Action OnEncounterCleared;

  public void SetDoors(IReadOnlyList<Door> encounterDoors)
  {
    doors.Clear();
    if (encounterDoors != null)
    {
      doors.AddRange(encounterDoors);
    }
  }

  public void OnTriggered(Vector3 sourcePosition)
  {
    if (started) return;

    started = true;
    SetDoorsLocked(true);
    OnEncounterStarted?.Invoke();
  }

  protected void CompleteEncounter()
  {
    if (!started) return;

    SetDoorsLocked(false);
    OnEncounterCleared?.Invoke();
  }

  void SetDoorsLocked(bool locked)
  {
    foreach (Door door in doors)
    {
      if (door != null)
      {
        door.SetLocked(locked);
      }
    }
  }
}
