using TMPro;
using UnityEngine;

public class ImpactGameUI : OnMessage<ChangeGroupCount>
{
    [SerializeField] private TextMeshProUGUI groupCounter;
    
    protected override void Execute(ChangeGroupCount msg)
    {
        Message.Publish(new ImpactText() { Message = new GroupCountChanged(), Position = msg.UiNumberPosition, Target = groupCounter, Text = msg.Amount.ToString() });
    }
}