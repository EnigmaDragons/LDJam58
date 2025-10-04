using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts;
using System.Linq;
using Game.Messages;

public class ExhibitPickerView : MonoBehaviour
{
    [SerializeField] private Image _exhibitImage;
    [SerializeField] private KeyValueLabel _exhibitNameLabel;
    [SerializeField] private KeyValueLabel _sizeLabel;
    [SerializeField] private KeyValueLabel _rarityLabel;
    [SerializeField] private KeyValueLabel _enjoymentLabel;
    [SerializeField] private KeyValueLabel _popularityLabel;
    [SerializeField] private TextMeshProUGUI _tagsLabel;
    [SerializeField] private Button _pickButton;
    
    private ExhibitTileType _exhibitTileType;
    public void Init(ExhibitTileType exhibits)
    {
        _exhibitTileType = exhibits;
        _pickButton.onClick.AddListener(PickExhibit);
        
        // Handle missing sprite gracefully
        _exhibitImage.sprite = exhibits.ExhibitSprite ?? GetDefaultSprite();
        
        _exhibitNameLabel.Init("Name", exhibits.DisplayName);
        _sizeLabel.Init("Size", exhibits.Size.x + "x" + exhibits.Size.y);
        _rarityLabel.Init("Rarity", exhibits.Rarity.ToString());
        _enjoymentLabel.Init("Enjoyment", exhibits.Enjoyment.ToString());
        _popularityLabel.Init("Popularity", exhibits.Popularity.ToString());
        _tagsLabel.text = string.Join(", ", exhibits.Tags.Select(t => {
            var s = t.ToString();
            var noUnderscore = s.Contains("_") ? s.Substring(s.IndexOf('_') + 1) : s;
            var withSpaces = System.Text.RegularExpressions.Regex.Replace(noUnderscore, "(\\B[A-Z])", " $1");
            return withSpaces;
        }));
    }
    
    private Sprite GetDefaultSprite()
    {
        // Try to load a default sprite from Resources or use Unity's default
        var defaultSprite = Resources.Load<Sprite>("DefaultExhibitSprite");
        return defaultSprite;
    }

    private void PickExhibit()
    {
        Message.Publish(new StartPlacement(_exhibitTileType));
        Message.Publish(new ClosePickMenu());
    }
}
