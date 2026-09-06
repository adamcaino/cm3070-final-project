using UnityEngine;

// Stores the room metadata used to identify a generated entrance door.
public class RoomEntranceDoor : MonoBehaviour
{
  [SerializeField] RoomRole role;
  [SerializeField] int roomId;

  public RoomRole Role => role;
  public int RoomId => roomId;

  // Assigns the room role and identifier associated with this door.
  public void Configure(RoomRole role, int roomId)
  {
    this.role = role;
    this.roomId = roomId;
  }
}
