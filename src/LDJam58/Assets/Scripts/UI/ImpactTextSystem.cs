using UnityEngine;

public class ImpactTextSystem : OnMessage<ImpactText>
{
    [SerializeField] private TravelingText travelingTextPrefab;
    
    protected override void Execute(ImpactText msg)
    {
        var travelingText = Instantiate(travelingTextPrefab, msg.Position, Quaternion.identity, transform);
        travelingText.Init(msg.Text, msg.Target, msg.Message, msg.Position);
    }
}