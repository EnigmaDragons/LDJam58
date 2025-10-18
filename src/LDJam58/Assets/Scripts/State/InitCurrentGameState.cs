using UnityEngine;

public sealed class InitCurrentGameState : MonoBehaviour
{
    [SerializeField] private PoolRules poolRules;

    void Awake()
    {
        CurrentGameState.Init();
        CurrentGameState.UpdateState(x =>
        {
            x.GuaranteedPick = poolRules.PickExhibitCheat;
            x.MythicChance = poolRules.MythicPercent;
            x.ExoticChance = poolRules.ExoticPercent;
            x.RareChance = poolRules.RarePercent;
        });
    }
}
