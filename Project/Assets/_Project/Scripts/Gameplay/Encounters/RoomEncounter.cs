using System;
using System.Collections.Generic;
using UnityEngine;

// Coordinates encounter activation, door locking, and encounter completion events.
public abstract class RoomEncounter : MonoBehaviour, ITriggerable
{
  [Tooltip("Doors this encounter locks/unlocks. Assignable directly in the Inspector for a hand-built room, or set at runtime via SetDoors (e.g. DungeonBossPlacer3D).")]
  [SerializeField] List<Door> doors = new List<Door>();

  bool started;

  public event Action OnEncounterStarted;
  public event Action OnEncounterCleared;

  // Replaces the doors controlled by this encounter with the supplied collection.
  public void SetDoors(IReadOnlyList<Door> encounterDoors)
  {
    doors.Clear();
    if (encounterDoors != null)
    {
      doors.AddRange(encounterDoors);
    }
  }

  // Starts the encounter once, locks its doors, and notifies subscribers.
  public void OnTriggered(Vector3 sourcePosition)
  {
    if (started) return;

    started = true;
    SetDoorsLocked(true);
    OnEncounterStarted?.Invoke();
  }

  // Unlocks the encounter doors and notifies subscribers that the encounter is cleared.
  protected void CompleteEncounter()
  {
    if (!started) return;

    SetDoorsLocked(false);
    OnEncounterCleared?.Invoke();
  }

  // Applies the requested lock state to every assigned door.
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
