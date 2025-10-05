using Game.Messages;
using UnityEngine;

public class GameManager : OnMessage<AdvancePeriod, BeginPickThree, StartPlacement, StopPlacement, ExhibitPlaced>
{
    [SerializeField] private ProgressionConfig _progressionConfig;

    private void Start()
    {
        InitStateForCurrentPeriod();
    }

    private void InitStateForCurrentPeriod()
    {
        var currentPeriod = GetCurrentPeriod();
        CurrentGameState.UpdateState(gs => {
            gs.currentNumExhibitsToPickThisPeriod = currentPeriod.NumExhibitsToPick;
            gs.currentAppeal = currentPeriod.TargetAppeal;
            gs.currentNumVisitingGroups = currentPeriod.NumVisitingGroups;
            return gs;
        });

        Message.Publish(new PeriodInitiatized(currentPeriod));
    }

    private ProgressionPeriodConfig GetCurrentPeriod()
    {
        return _progressionConfig.GetPeriod(CurrentGameState.ReadOnly.currentPeriodIndex);
    }

    protected override void Execute(AdvancePeriod msg)
    {
        if (CurrentGameState.ReadOnly.currentPeriodIndex + 1 >= _progressionConfig.Count)
        {
            Message.Publish(new GameWon());
            return;
        } 

        CurrentGameState.UpdateState(gs => {
            gs.currentPeriodIndex++;
            return gs;
        });

        InitStateForCurrentPeriod();
    }

    protected override void Execute(BeginPickThree msg)
    {
        CurrentGameState.UpdateState(gs => {
            gs.currentNumExhibitsToPickThisPeriod -= 1;
            return gs;
        });
    }

    protected override void Execute(StartPlacement msg)
    {
        CurrentGameState.UpdateState(gs => {
            gs.isPicking = false;
            gs.isPlacing = true;
            return gs;
        });
    }

    protected override void Execute(StopPlacement msg)
    {
        CurrentGameState.UpdateState(gs => {
            gs.isPlacing = false;
            return gs;
        });
    }

    protected override void Execute(ExhibitPlaced msg)
    {
        CurrentGameState.UpdateState(gs => {
            gs.isPlacing = false;
            return gs;
        });
    }
}
