using UnityEngine;

namespace Game.Display
{
    public class BillboardCanvas : MonoBehaviour
    {
        private Transform mainCameraTransform;

        void Start()
        {
            // Find the main camera in the scene
            mainCameraTransform = Camera.main.transform;
            if (mainCameraTransform == null)
            {
                Debug.LogError("No Main Camera found in the scene! Please tag your camera as 'MainCamera'.");
                enabled = false; // Disable the script if no camera is found
            }
        }

        void LateUpdate()
        {
            if (mainCameraTransform != null)
            {
                // Make the Canvas look at the camera
                transform.LookAt(transform.position + mainCameraTransform.forward);

                // Optional: If you only want the Canvas to rotate on the Y-axis (like a nameplate)
                // transform.rotation = Quaternion.Euler(0f, mainCameraTransform.eulerAngles.y, 0f);
            }
        }
    }
}