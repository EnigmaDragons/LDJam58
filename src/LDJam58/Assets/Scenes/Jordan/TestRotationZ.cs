using UnityEngine;

public class TestRotationZ : MonoBehaviour
{
    public float rotationSpeed = 0.1F;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, Time.deltaTime * rotationSpeed));

    }
}
