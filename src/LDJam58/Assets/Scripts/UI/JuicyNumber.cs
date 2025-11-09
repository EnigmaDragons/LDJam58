using UnityEngine;

public class JuicyNumber : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private RisingText risingTextPrefab;
    [SerializeField] private PoppingText poppingText;

    private int _oldValue;

    public void SetValue(int value)
    {
        _oldValue = value;
        poppingText.SetText(value.ToString());
    }
    
    public void UpdateValue(int newValue)
    {
        RisingText risingText;
        if (newValue > _oldValue)
        {
            risingText = Instantiate(risingTextPrefab, target.position, Quaternion.identity, transform);
            risingText.Init(GameText.Positive($"+{newValue - _oldValue}"));
        }
        else if (newValue < _oldValue)
        {
            risingText = Instantiate(risingTextPrefab, target.position, Quaternion.identity, transform);
            risingText.Init(GameText.Negative($"-{_oldValue - newValue}"));
        }
        poppingText.UpdateText(newValue.ToString());
        _oldValue = newValue;
    }
    
    
}