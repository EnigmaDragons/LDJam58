using UnityEngine;

public class RoomId : MonoBehaviour
{
    public string Id { get; private set; }
    
    void Awake()
    {
        Id = System.Guid.NewGuid().ToString();
    }
}
