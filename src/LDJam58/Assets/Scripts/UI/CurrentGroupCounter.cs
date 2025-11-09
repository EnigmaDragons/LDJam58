using UnityEngine;

public class CurrentGroupCounter : OnMessage<GroupCountChanged>
{
    [SerializeField] private JuicyNumber counter;

    private void Start()
    {
        counter.SetValue(CurrentGameState.ReadOnly.currentGroups.Count);
    }
    
    protected override void Execute(GroupCountChanged msg)
    {
        counter.UpdateValue(CurrentGameState.ReadOnly.currentGroups.Count);
    }
}

