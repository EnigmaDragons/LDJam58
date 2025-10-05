using System;
using UnityEngine;

public class ShowChildrenDuringPlacing : OnMessage<StartPlacement, StopPlacement>
{
    private Renderer[] cachedRenderers = Array.Empty<Renderer>();

    protected override void AfterEnable()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>();
        var shouldBeActive = CurrentGameState.ReadOnly.isPlacing;
        foreach (var renderer in cachedRenderers)
            renderer.enabled = shouldBeActive;
    }

    protected override void Execute(StartPlacement msg)
    {
        foreach (var renderer in cachedRenderers)
            renderer.enabled = true;
    }

    protected override void Execute(StopPlacement msg)
    {
        foreach (var renderer in cachedRenderers)
            renderer.enabled = false;
    }
}
