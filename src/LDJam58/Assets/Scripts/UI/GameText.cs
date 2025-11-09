public static class GameText
{
    public static string Neutral(string strToWrap)
        => $"<color=black>{strToWrap}</color>";
    
    public static string Positive(string strToWrap)
        => $"<color=blue>{strToWrap}</color>";

    public static string Negative(string strToWrap)
        => $"<color=red>{strToWrap}</color>";   
}