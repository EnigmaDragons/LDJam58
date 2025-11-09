using TMPro;
using UnityEngine;

public class TravelingText : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float travelSeconds = 1;
    [SerializeField] private AnimationCurve curve;

    private Vector3 _startingPosition;
    private Vector3 _endingPosition;
    private float _t;
    private object _message;

    public void Init(string text, TextMeshProUGUI target, object message, Vector3 startingPosition)
    {
        label.text = text;
        label.color = target.color;
        _startingPosition = startingPosition;
        _endingPosition = target.GetComponent<RectTransform>().position;
        _message = message;
    }

    private void Update()
    {
        _t += Time.deltaTime;
        if (_t > travelSeconds)
        {
            Message.Publish(_message);
            Destroy(gameObject);
        }
        else
        {
            var point = curve.Evaluate(_t / travelSeconds);
            rect.anchoredPosition = Vector2.Lerp(_startingPosition, _endingPosition, point);
        }
    }
}