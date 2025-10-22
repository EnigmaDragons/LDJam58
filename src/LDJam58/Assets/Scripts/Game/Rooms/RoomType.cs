using System;
using System.Linq;

[Serializable]
public class RoomType
{
    public string Name;
    public ExhibitTag[] Requirement;
    public int Multiplier;

    public bool GivesAdjacencyBonus => Requirement.Length > 1;

    public bool IsFeaturedTag(ExhibitTag tag) => Requirement.Any(t => t == tag);
}
