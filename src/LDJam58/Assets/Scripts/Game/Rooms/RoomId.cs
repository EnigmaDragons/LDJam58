using UnityEngine;

public class RoomId : MonoBehaviour
{
    public string Id { get; private set; }
    
    void Awake()
    {
        Id = System.Guid.NewGuid().ToString();
    }
    
    public static string GetRoomId(Transform transform)
    {
        var current = transform;
        
        while (current != null)
        {
            var roomId = current.GetComponent<RoomId>();
            if (roomId != null)
            {
                return roomId.Id;
            }
            
            current = current.parent;
        }
        
        return null;
    }
}
