using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Messages;
using UnityEngine.EventSystems;

public class ExhibitPickerView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    
    [SerializeField] private Sprite _commonHoverSprite;
    [SerializeField] private Sprite _rareHoverSprite;
    [SerializeField] private Sprite _exoticHoverSprite;
    [SerializeField] private Sprite _mythicHoverSprite;
    
    [SerializeField] private Material _commonMaterial;
    [SerializeField] private Material _rareMaterial;
    [SerializeField] private Material _exoticMaterial;
    [SerializeField] private Material _mythicMaterial;
    
    [SerializeField] private Image _hoverImage;
    
    private ExhibitTileType _exhibitTileType;
    
    private void Start()
    {
        // Initially hide hover image
        if (_hoverImage != null)
        {
            _hoverImage.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        _hoverImage.gameObject.SetActive(false);
        if (_pickButton != null)
            _pickButton.onClick.RemoveListener(PickExhibit);
    }
    
    public void Init(ExhibitTileType exhibits)
    {
        _exhibitTileType = exhibits;
        if (_pickButton != null)
        {
            _pickButton.onClick.RemoveListener(PickExhibit);
            _pickButton.onClick.AddListener(PickExhibit);
        }
        
        // Handle missing sprite gracefully
        _exhibitImage.sprite = exhibits.ExhibitSprite ?? GetDefaultSprite();
        
        _hoverImage.gameObject.SetActive(false);
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
        
        UpdateFrontPanelMaterial();
    }
    
    private void UpdateFrontPanelMaterial()
    {
        if (_frameImage != null)
        {
            _frameImage.material = _exhibitTileType.Rarity switch
            {
                ExhibitRarity.Common => _commonMaterial,
                ExhibitRarity.Rare => _rareMaterial,
                ExhibitRarity.Exotic => _exoticMaterial,
                ExhibitRarity.Mythic => _mythicMaterial,
                _ => _commonMaterial
            };
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hoverImage != null && _exhibitTileType != null)
        {
            _hoverImage.sprite = GetHoverSpriteForRarity(_exhibitTileType.Rarity);
            _hoverImage.gameObject.SetActive(true);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hoverImage != null)
        {
            _hoverImage.gameObject.SetActive(false);
        }
    }
    
    private Sprite GetHoverSpriteForRarity(ExhibitRarity rarity)
    {
        return rarity switch
        {
            ExhibitRarity.Common => _commonHoverSprite,
            ExhibitRarity.Rare => _rareHoverSprite,
            ExhibitRarity.Exotic => _exoticHoverSprite,
            ExhibitRarity.Mythic => _mythicHoverSprite,
            _ => _commonHoverSprite
        };
    }

    private void PickExhibit()
    {
        Message.Publish(new StartPlacement(_exhibitTileType));
        Message.Publish(new ExhibitPicked(_exhibitTileType));
        Message.Publish(new ClosePickMenu());
    }
}
