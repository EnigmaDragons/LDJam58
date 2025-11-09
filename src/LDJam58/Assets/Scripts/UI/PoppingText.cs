using TMPro;
using UnityEngine;

public class PoppingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float fontSize;
    [SerializeField] private float popFontSize;
    [SerializeField] private float shrinkSeconds;

    private float _t;

    public void SetText(string text)
    {
        label.text = text;
        _t = 0;
    }
    
    public void UpdateText(string text)
    {
        label.text = text;
        label.fontSize = popFontSize;
        _t = shrinkSeconds;
    }

    private void Update()
    {
        if (_t <= 0)
            return;
        _t -= Time.deltaTime;
        label.fontSize = Mathf.Lerp(fontSize, popFontSize, Mathf.Max(0, _t) / shrinkSeconds);
    }
}