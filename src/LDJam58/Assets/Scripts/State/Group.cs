using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Group
{
    public int peopleCount = 0;
    public ExhibitTag[] Fascinations;
    public ExhibitTag[] Disinterests;

    public Dictionary<string, int> ScoredExhibts = new Dictionary<string, int>();
    public int SeasonScore => ScoredExhibts.Values.Sum();
}