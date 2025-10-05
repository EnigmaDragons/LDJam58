using UnityEngine;

public class HideTargetWhilePlacing : OnMessage<StartPlacement, StopPlacement>
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
}
