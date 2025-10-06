using UnityEngine;

public class GameGuideController : OnMessage<ShowGameGuideRequested>
{
    [SerializeField] private GameObject gameGuide;

    protected override void AfterEnable()
    {
        gameGuide.SetActive(false);
    }

    protected override void Execute(ShowGameGuideRequested msg)
    {
        gameGuide.SetActive(true);
    }
}
