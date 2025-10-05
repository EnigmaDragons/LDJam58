using Game.Messages;
using UnityEngine;

public class HideTargetWhilePlacing : OnMessage<StartPlacement, StopPlacement, ExhibitPlaced>
{
    [SerializeField] private GameObject target;

    protected override void Execute(StartPlacement msg)
    {
        target.SetActive(false);
    }

    protected override void Execute(StopPlacement msg)
    {
        target.SetActive(true);
    }

    protected override void Execute(ExhibitPlaced msg)
    {
        target.SetActive(true);
    }
}
