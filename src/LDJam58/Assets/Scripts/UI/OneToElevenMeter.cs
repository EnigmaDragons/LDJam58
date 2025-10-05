using UnityEngine;

public class OneToElevenMeter : MonoBehaviour
{
    [SerializeField] private GameObject[] _meterObjects;

    public void SetValue(int value)
    {
        for (int i = 0; i < _meterObjects.Length; i++)
        {
            _meterObjects[i].SetActive(i < value);
        }
    }
}