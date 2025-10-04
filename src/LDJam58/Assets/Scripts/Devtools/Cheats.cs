using UnityEngine;
using UnityEngine.EventSystems;

public class Cheats : MonoBehaviour
{

#if UNITY_EDITOR
    private void Update()
    {
        var isHoldingLeftShift = Input.GetKey(KeyCode.LeftShift);
        if (isHoldingLeftShift && Input.GetKeyDown(KeyCode.I))
            EventPublisher.FadeInScene();

        if (isHoldingLeftShift && Input.GetKeyDown(KeyCode.F))
            EventPublisher.FadeOutScene();

        // Mouse hover debugger
        if (Input.GetKey(KeyCode.M))
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            if (results.Count > 0)
            {
                var topMostUI = results[0];
                Debug.Log($"UI Hover: {topMostUI.gameObject.name} (Layer: {topMostUI.gameObject.layer}, Tag: {topMostUI.gameObject.tag})");
            }
            else
            {
                Debug.Log("UI Hover: No UI element detected");
            }
        }
    }
#endif
}

