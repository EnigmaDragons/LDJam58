using System;
using UnityEngine;

public class ShowChildrenDuringPlacing : OnMessage<StartPlacement, StopPlacement, ExhibitPlaced>
{
    private Renderer[] cachedRenderers = Array.Empty<Renderer>();

    protected override void AfterEnable()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>();
        var shouldBeActive = CurrentGameState.ReadOnly == null || CurrentGameState.ReadOnly.isPlacing;
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

    protected override void Execute(ExhibitPlaced msg)
    {
        foreach (var renderer in cachedRenderers)
            renderer.enabled = false;
    }
}
