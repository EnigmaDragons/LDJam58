using UnityEngine;


public class CameraContinualDolly : MonoBehaviour
{
    [SerializeField] private Vector3 velocity;

    private void FixedUpdate() => transform.position += velocity * Time.deltaTime;
}

