using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Messages;

public class ExhibitPickerView : MonoBehaviour
{
    [SerializeField] private Image _exhibitImage;
    [SerializeField] private TextMeshProUGUI _exhibitNameLabel;
    [SerializeField] private Image _frameImage;
    [SerializeField] private Image _backFrameImage;

    [SerializeField] private TextMeshProUGUI _sizeLabel;
    [SerializeField] private TextMeshProUGUI _rarityLabel;
    [SerializeField] private OneToElevenMeter _enjoymentMeter;
    [SerializeField] private OneToElevenMeter _popularityMeter;
    [SerializeField] private TagsDisplayView _tagsView;
    [SerializeField] private Button _pickButton;
    
    [SerializeField] private Sprite _commonFrameSprite;
    [SerializeField] private Sprite _rareFrameSprite;
    [SerializeField] private Sprite _exoticFrameSprite;
    [SerializeField] private Sprite _mythicFrameSprite;
    
    [SerializeField] private Sprite _commonBackFrameSprite;
    [SerializeField] private Sprite _rareBackFrameSprite;
    [SerializeField] private Sprite _exoticBackFrameSprite;
    [SerializeField] private Sprite _mythicBackFrameSprite;
    
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
        
        UpdateRarityFrame();
    }
    
    private Sprite GetDefaultSprite()
    {
        // Try to load a default sprite from Resources or use Unity's default
        var defaultSprite = Resources.Load<Sprite>("DefaultExhibitSprite");
        return defaultSprite;
    }

    private void UpdateRarityFrame()
    {
        if (_frameImage != null)
        {
            _frameImage.sprite = _exhibitTileType.Rarity switch
            {
                ExhibitRarity.Common => _commonFrameSprite,
                ExhibitRarity.Rare => _rareFrameSprite,
                ExhibitRarity.Exotic => _exoticFrameSprite,
                ExhibitRarity.Mythic => _mythicFrameSprite,
                _ => _commonFrameSprite
            };
        }
        
        if (_backFrameImage != null)
        {
            _backFrameImage.sprite = _exhibitTileType.Rarity switch
            {
                ExhibitRarity.Common => _commonBackFrameSprite,
                ExhibitRarity.Rare => _rareBackFrameSprite,
                ExhibitRarity.Exotic => _exoticBackFrameSprite,
                ExhibitRarity.Mythic => _mythicBackFrameSprite,
                _ => _commonBackFrameSprite
            };
        }
    }

    private void PickExhibit()
    {
        Message.Publish(new StartPlacement(_exhibitTileType));
        Message.Publish(new ExhibitPicked(_exhibitTileType));
        Message.Publish(new ClosePickMenu());
    }
}
