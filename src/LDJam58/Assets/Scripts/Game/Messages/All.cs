using Assets.Scripts;

public class AdvancePeriod
{

}

public class GameWon
{

}

public class GameLost
{

}

public class ExhibitPicked
{
    public ExhibitTileType Exhibit;

    public ExhibitPicked(ExhibitTileType exhibit)
    {
        Exhibit = exhibit;
    }
}

public class SeasonInitialized 
{
    public ProgressionPeriodConfig Period;

    public SeasonInitialized(ProgressionPeriodConfig period)
    {
        Period = period;
    }
}

public class LockCameraMovement
{

}

public class UnlockCameraMovement
{

}

public class RotationChanged
{
    public float RotationAngle;

    public RotationChanged(float rotationAngle)
    {
        RotationAngle = rotationAngle;
    }
}
