using UnityEngine;

[CreateAssetMenu]
public class PoolRules : ScriptableObject
{
    [SerializeField] private string pickExhibitCheat;
    [SerializeField] private int mythicPercent;
    [SerializeField] private int exoticPercent;
    [SerializeField] private int rarePercent;

    public string PickExhibitCheat => pickExhibitCheat;
    public int MythicPercent => mythicPercent;
    public int ExoticPercent => exoticPercent;
    public int RarePercent => rarePercent;
}