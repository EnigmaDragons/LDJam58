using System;

[Serializable]
public class Group
{
    public int peopleCount = 0;
    public ExhibitTag[] Fascinations;
    public ExhibitTag[] Disinterests;
    
    public int seasonScore;
}