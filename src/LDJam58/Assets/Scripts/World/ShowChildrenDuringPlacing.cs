using UnityEngine;
using Game.Messages;
using Game.TilePlacement;

namespace Assets.Scripts.World
{
public class ShowChildrenDuringPlacing : OnMessage<StartPlacement, StopPlacement>
{
    protected override void AfterEnable()
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
    }

    protected override void Execute(StartPlacement msg)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(true);
    }

    protected override void Execute(StopPlacement msg)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
    }
}
}
