using UnityEngine;

public class LockCameraWhileTargetActive : MonoBehaviour
{
    [SerializeField] private GameObject target;

    private bool _targetIsActive;

    private void Update()
    {
        if (_targetIsActive == target.activeSelf)
            return;

        _targetIsActive = target.activeSelf;
        if (_targetIsActive)
        {
            Message.Publish(new UnlockCameraMovement());
        }
        else
        {
            Message.Publish(new LockCameraMovement());
        }
    }
}