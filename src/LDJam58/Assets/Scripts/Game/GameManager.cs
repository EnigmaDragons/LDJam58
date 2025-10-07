using System.Collections.Generic;
using System.Linq;
using Game.Messages;
using Sirenix.Utilities;
using UnityEngine;

public class GameManager : OnMessage<AdvancePeriod, BeginPickThree, StartPlacement, StopPlacement, ExhibitPlaced, OpenMuseum, GameWon>
{
    [SerializeField] private ProgressionConfig _progressionConfig;
    [SerializeField] private LayerMask hover;
    
    private Camera _camera;

    private void Start()
    {
        CurrentGameState.UpdateState(gs => {
            gs.progressionConfig = _progressionConfig;
            return gs;
        });
        InitStateForCurrentPeriod();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            CurrentGameState.UpdateState(x => x.showDetails = !x.showDetails);
        
        if (CurrentGameState.ReadOnly.isPlacing || CurrentGameState.ReadOnly.isPicking || CurrentGameState.ReadOnly.isShowingMuseum)
            return;
        if(_camera == null) 
            _camera = Camera.main;
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hover))
        {
            if (CurrentGameState.ReadOnly.isPlacing || CurrentGameState.ReadOnly.isPicking || CurrentGameState.ReadOnly.isShowingMuseum)
                return;
            var newTarget = hit.collider.GetComponentInParent<ExhibitPrefab>();
            var state = CurrentGameState.ReadOnly;
            if (state.focusedExhibit != newTarget.ExhibitTileType.DisplayName || state.focusedRoom != state.Exhibits[newTarget.ExhibitTileType.DisplayName].roomId)
                CurrentGameState.UpdateState(x =>
                {
                    x.focusedExhibit = newTarget.ExhibitTileType.DisplayName;
                    x.focusedRoom = x.Exhibits[newTarget.ExhibitTileType.DisplayName].roomId;
                });
        }
    }

    private void InitStateForCurrentPeriod()
    {
        var currentPeriod = GetCurrentPeriod();
        CurrentGameState.UpdateState(gs => {
            gs.currentNumExhibitsToPickThisPeriod = currentPeriod.NumExhibitsToPick;
            gs.currentTargetAppeal = currentPeriod.TargetAppeal;
            gs.currentGroups = Enumerable.Range(0, currentPeriod.NumVisitingGroups).Select(_ => VisitorGenerator.Generate(ExhibitTag.None, new HashSet<ExhibitTag>())).ToList();
            gs.seasonScore = 0;
            gs.Rooms.Values.ForEach(x => x.seasonScore = 0);
            gs.Exhibits.Values.ForEach(x => x.seasonScore = 0);
            return gs;
        });

        Message.Publish(new SeasonInitialized(currentPeriod));
    }

    private ProgressionPeriodConfig GetCurrentPeriod()
    {
        return _progressionConfig.GetPeriod(CurrentGameState.ReadOnly.currentSeasonIndex);
    }

    protected override void Execute(AdvancePeriod msg)
    {
        if (CurrentGameState.ReadOnly.currentSeasonIndex + 1 >= _progressionConfig.Count)
        {
            Message.Publish(new GameWon());
            return;
        } 

        CurrentGameState.UpdateState(gs => {
            gs.currentSeasonIndex++;
            gs.isShowingMuseum = false;
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
            Enumerable.Range(0, msg.exhibit.Popularity).ToList().ForEach(_ => {
                gs.currentGroups.Add(VisitorGenerator.Generate(
                    msg.exhibit.Tags.Count > 0 
                        ? msg.exhibit.Tags.First() 
                        : ExhibitTag.None, 
                    msg.exhibit.Tags.ToHashSet()));
            });
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

    protected override void Execute(OpenMuseum msg)
    {
        CurrentGameState.UpdateState(x => x.isShowingMuseum = true);
        CurrentGameState.CalculateRoundScore();
        Message.Publish(new SummarizeSeason());
    }

    protected override void Execute(GameWon msg)
    {
        Message.Publish(new NavigateToSceneRequested("CreditsScene"));
    }
}
