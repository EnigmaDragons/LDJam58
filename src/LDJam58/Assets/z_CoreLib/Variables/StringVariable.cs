using UnityEngine;

public class StringVariable : ScriptableObject
{
    [SerializeField]
    [TextArea]
    private string value = "";

    public string Value
    {
        get { return value; }
        set { this.value = value; }
    }

    public void SetValue(string str) => value = str;
}
