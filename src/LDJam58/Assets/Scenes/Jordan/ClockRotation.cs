using UnityEngine;

public class ClockRotation : MonoBehaviour
{
    public float rotationSpeed = 0.1F;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(Time.deltaTime * rotationSpeed, 0, 0));

    }
}
