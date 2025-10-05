using System;

[Serializable]
public class RoomType
{
    public string Name;
    public ExhibitTag[] Requirement;
    public int Multiplier;

    public bool GivesAdjacencyBonus => Requirement.Length > 1;
}
