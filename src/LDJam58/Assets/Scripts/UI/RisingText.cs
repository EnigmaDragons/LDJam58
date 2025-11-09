using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RisingText : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float yTarget;
    [SerializeField] private float seconds;
    [SerializeField] private Gradient fadeOut;

    private Vector2 _startingPosition;
    private Vector2 _endingPosition;
    private float _t;

    public void Init(string text)
    {
        _startingPosition = rect.anchoredPosition;
        _endingPosition = rect.anchoredPosition + new Vector2(0, yTarget);
        label.text = text;
    }
    
    private void Update()
    {
        _t += Time.deltaTime;
        if (_t > seconds)
        {
            Destroy(gameObject);
        }
        else
        {
            var point = (_t / seconds);
            var direction = _endingPosition - _startingPosition;
            rect.anchoredPosition = _startingPosition + point * direction;
            label.color = fadeOut.Evaluate(point);
        }
    }
    
}