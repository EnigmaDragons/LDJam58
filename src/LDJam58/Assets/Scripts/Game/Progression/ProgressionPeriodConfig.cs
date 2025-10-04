using UnityEngine;

[CreateAssetMenu(fileName = "ProgressionPeriodConfig", menuName = "ProgressionPeriodConfig")]
public class ProgressionPeriodConfig : ScriptableObject
{
    [SerializeField] private int _numExhibitsToPick = 0;
    [SerializeField] private int _targetAppeal = 0;
    [SerializeField] private int _numVisitingGroups = 0;

    public int NumExhibitsToPick => _numExhibitsToPick;
    public int TargetAppeal => _targetAppeal;
    public int NumVisitingGroups => _numVisitingGroups;
}
