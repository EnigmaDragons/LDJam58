using System;
using System.Collections.Generic;
using System.Linq;
using Game.Messages;
using UnityEngine;

namespace Game.ExhibitPool
{
    public class ExhibitPoolPickerV3 : OnMessage<StartExhibitPick, ExhibitPicked>
    {
        [SerializeField] private ExhibitPoolObject exhibitPoolObject;

        private Dictionary<ExhibitRarity, List<ExhibitTileType>> _pools;
        
        private void Start()
        {
            _pools = exhibitPoolObject.Exhibits
                .Select(ExhibitDataConverter.ConvertToExhibitTileType)
                .GroupBy(x => x.Rarity)
                .ToDictionary(x => x.Key, x => x.ToList());
        }
        
        protected override void Execute(StartExhibitPick msg)
        {
            if (CurrentGameState.ReadOnly.isPicking)
            {
                Debug.LogWarning("Already picking exhibits. Ignoring StartExhibitPick message.");
                return;
            }
            var guaranteed = CurrentGameState.ReadOnly.GuaranteedPick;
            CurrentGameState.UpdateState(gs =>
            {
                gs.isPicking = true;
                gs.GuaranteedPick = "";
            });
            var result = new List<ExhibitTileType>();
            if (!string.IsNullOrEmpty(guaranteed))
            {
                var exhibit = _pools.Values.SelectMany(x => x).FirstOrDefault(x => x.DisplayName.Equals(guaranteed, StringComparison.InvariantCultureIgnoreCase));
                if (exhibit != null) 
                    result.Add(exhibit);
            }
            while (result.Count < 3)
                result.Add(RollExhibit());
            Message.Publish(new BeginPickThree(result.ToArray()));
        }

        private ExhibitTileType RollExhibit()
        {
            var rarity = RollRarity();
            var exhibitTag = RollTag(rarity);
            return _pools[rarity].Where(x => x.Tags.Contains(exhibitTag)).Random();
        }

        private ExhibitRarity RollRarity()
        {
            if (ChooseRarity(ExhibitRarity.Mythic, CurrentGameState.ReadOnly.MythicChance, ExhibitRarity.Common, ExhibitRarity.Rare, ExhibitRarity.Exotic))
                return ExhibitRarity.Mythic;
            if (ChooseRarity(ExhibitRarity.Exotic, CurrentGameState.ReadOnly.ExoticChance, ExhibitRarity.Common, ExhibitRarity.Rare))
                return ExhibitRarity.Exotic;
            if (ChooseRarity(ExhibitRarity.Rare, CurrentGameState.ReadOnly.RareChance, ExhibitRarity.Common))
                return ExhibitRarity.Rare;
            return ExhibitRarity.Common;
        }

        private ExhibitTag RollTag(ExhibitRarity rarity)
            => _pools[rarity].SelectMany(x => x.Tags).Distinct().Random();

        private bool ChooseRarity(ExhibitRarity rarity, int percent, params ExhibitRarity[] lowerRarities)
        {
            if (_pools[rarity].Count == 0)
                return false;
            if (lowerRarities.All(x => _pools[x].Count == 0))
                return true;
            return Rng.Chance(percent / 100f);
        }

        protected override void Execute(ExhibitPicked msg)
        {
            _pools[msg.Exhibit.Rarity].Remove(msg.Exhibit);
        }
    }
}