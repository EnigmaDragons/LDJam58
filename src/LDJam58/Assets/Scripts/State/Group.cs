using System;
using System.Collections.Generic;
using System.Linq;
using Game.NPC;

[Serializable]
public class Group
{
    public int peopleCount = 0;
    public ExhibitTag[] Fascinations;
    public ExhibitTag[] Disinterests;

    public Dictionary<string, NpcMood> ExhibitReactions = new Dictionary<string, NpcMood>();
    public Dictionary<string, int> ScoredExhibits = new Dictionary<string, int>();
    public int SeasonScore => ScoredExhibits.Values.Sum();
}