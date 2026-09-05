using UnityEngine;

public class RoomEntranceDoor : MonoBehaviour
{
  [SerializeField] RoomRole role;
  [SerializeField] int roomId;

  public RoomRole Role => role;
  public int RoomId => roomId;

  public void Configure(RoomRole role, int roomId)
  {
    this.role = role;
    this.roomId = roomId;
  }
}
