using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts;
using System.Linq;
using Game.Messages;

public class ExhibitPickerView : MonoBehaviour
{
    [SerializeField] private Image _exhibitImage;
    [SerializeField] private TextMeshProUGUI _exhibitNameLabel;

    [SerializeField] private TextMeshProUGUI _sizeLabel;
    [SerializeField] private TextMeshProUGUI _rarityLabel;
    [SerializeField] private OneToElevenMeter _enjoymentMeter;
    [SerializeField] private OneToElevenMeter _popularityMeter;
    [SerializeField] private TagsDisplayView _tagsView;
    [SerializeField] private Button _pickButton;
    
    private ExhibitTileType _exhibitTileType;
    public void Init(ExhibitTileType exhibits)
    {
        _exhibitTileType = exhibits;
        _pickButton.onClick.AddListener(PickExhibit);
        
        // Handle missing sprite gracefully
        _exhibitImage.sprite = exhibits.ExhibitSprite ?? GetDefaultSprite();
        
        _exhibitNameLabel.text = exhibits.DisplayName;
        _sizeLabel.text = exhibits.Size.x + "x" + exhibits.Size.y;
        _enjoymentMeter.SetValue(exhibits.Enjoyment);
        _rarityLabel.text = exhibits.Rarity.ToString();
        _popularityMeter.SetValue(exhibits.Popularity);
        _tagsView.SetTags(exhibits.Tags);
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
        Message.Publish(new ExhibitPicked(_exhibitTileType));
        Message.Publish(new ClosePickMenu());
    }
}
