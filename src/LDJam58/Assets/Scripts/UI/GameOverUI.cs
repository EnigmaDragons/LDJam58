using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameOverUI : OnMessage<GameLost>
{
    [Header("Main Panel")]
    [SerializeField] private CanvasGroup _mainCanvasGroup;
    [SerializeField] private RectTransform _mainPanel;
    [SerializeField] private Image _mainPanelBackground;
    [SerializeField] private Image _mainPanelBorder;
    [SerializeField] private Image _accentStripe;
    
    [Header("Achievement Badge")]
    [SerializeField] private CanvasGroup _achievementBadge;
    [SerializeField] private TextMeshProUGUI _achievementTitle;
    [SerializeField] private TextMeshProUGUI _achievementDescription;
    [SerializeField] private Image _badgeIcon;
    [SerializeField] private Image _ribbon;
    
    [Header("Main Title")]
    [SerializeField] private TextMeshProUGUI _mainTitle;
    [SerializeField] private TextMeshProUGUI _subtitle;
    
    [Header("Stats Display")]
    [SerializeField] private CanvasGroup _statsGroup;
    [SerializeField] private RectTransform _visitorsStat;
    [SerializeField] private RectTransform _exhibitsStat;
    [SerializeField] private RectTransform _moneyStat;
    [SerializeField] private RectTransform _ratingStat;
    [SerializeField] private TextMeshProUGUI _visitorsText;
    [SerializeField] private TextMeshProUGUI _exhibitsText;
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private TextMeshProUGUI _ratingText;
    
    [Header("Failure Message")]
    [SerializeField] private CanvasGroup _failureMessageGroup;
    [SerializeField] private TextMeshProUGUI _failureMessageText;
    [SerializeField] private RectTransform _speechBubble;
    
    [Header("Buttons")]
    [SerializeField] private CanvasGroup _buttonsGroup;
    [SerializeField] private Button _tryAgainButton;
    [SerializeField] private Button _acceptDefeatButton;
    [SerializeField] private TextMeshProUGUI _tryAgainText;
    [SerializeField] private TextMeshProUGUI _acceptDefeatText;
    
    [Header("Visual Flourishes")]
    [SerializeField] private RectTransform _tumbleweed;
    [SerializeField] private Image _closedSign;
    [SerializeField] private Image _cobweb;
    [SerializeField] private Image _wiltedPlant;
    [SerializeField] private Image _forLeaseSign;
    [SerializeField] private RectTransform[] _sadnessParticles;
    
    [Header("Colors")]
    [SerializeField] private Color _lightGray = new Color(0.85f, 0.85f, 0.85f, 1f); // #D9D9D9
    [SerializeField] private Color _charcoal = new Color(0.17f, 0.16f, 0.15f, 1f); // #2C2826
    [SerializeField] private Color _mediumBlue = new Color(0.29f, 0.48f, 0.65f, 1f); // #4A7BA7
    [SerializeField] private Color _veryLightGray = new Color(0.92f, 0.92f, 0.92f, 1f); // #EBEBEB
    
    [Header("Animation Settings")]
    [SerializeField] private float _fadeInDuration = 2f;
    [SerializeField] private float _sequenceDelay = 0.3f;
    [SerializeField] private float _bounceIntensity = 1.2f;
    
    private Sequence _mainSequence;
    private readonly string[] _failureMessages = {
        "The T-Rex skeleton is now a coffee table at someone's house",
        "Your museum is now a Spirit Halloween store",
        "Turns out people don't want to visit an empty room with one rock",
        "The pigeons have claimed the sculpture garden",
        "Congratulations! You've created the world's most expensive storage unit",
        "Even the gift shop closed. THE GIFT SHOP.",
        "Your museum is now a parking lot",
        "The exhibits have unionized and gone on strike",
        "Turns out 'museum' and 'warehouse' are not the same thing",
        "Your museum is now a trendy co-working space"
    };
    
    private readonly string[] _achievementTitles = {
        "WORLD'S SHORTEST MUSEUM CAREER",
        "SPEED RUN: BANKRUPTCY EDITION",
        "ACHIEVEMENT: EMPTY ROOM MASTER",
        "TROPHY: PIGEON MAGNET",
        "BADGE: GIFT SHOP DESTROYER"
    };
    
    private readonly string[] _achievementDescriptions = {
        "Opened and closed faster than a pop-up shop",
        "Broke records in financial ruin",
        "Perfected the art of nothing",
        "Attracted more birds than humans",
        "Closed the gift shop before it could help"
    };

    protected override void AfterEnable()
    {
        SetupInitialState();
        SetupColors();
        SetupButtonListeners();
    }
    
    protected override void AfterDisable()
    {
        _mainSequence?.Kill(true);
        _tryAgainButton.onClick.RemoveAllListeners();
        _acceptDefeatButton.onClick.RemoveAllListeners();
    }

    protected override void Execute(GameLost msg)
    {
        ShowGameOverScreen();
    }
    
    private void SetupInitialState()
    {
        // Start everything hidden
        _mainCanvasGroup.alpha = 0f;
        _achievementBadge.alpha = 0f;
        _statsGroup.alpha = 0f;
        _failureMessageGroup.alpha = 0f;
        _buttonsGroup.alpha = 0f;
        
        // Set initial scales for bounce animations
        _achievementBadge.transform.localScale = Vector3.zero;
        _mainTitle.transform.localScale = Vector3.zero;
        _subtitle.transform.localScale = Vector3.zero;
        
        // Set initial positions for stats (slightly offset for staggered effect)
        _visitorsStat.anchoredPosition = new Vector2(-100f, _visitorsStat.anchoredPosition.y);
        _exhibitsStat.anchoredPosition = new Vector2(-50f, _exhibitsStat.anchoredPosition.y);
        _moneyStat.anchoredPosition = new Vector2(50f, _moneyStat.anchoredPosition.y);
        _ratingStat.anchoredPosition = new Vector2(100f, _ratingStat.anchoredPosition.y);
        
        // Tilt one stat card slightly
        _moneyStat.rotation = Quaternion.Euler(0f, 0f, -5f);
        
        // Set initial positions for visual elements
        _tumbleweed.anchoredPosition = new Vector2(-200f, _tumbleweed.anchoredPosition.y);
        _closedSign.transform.localScale = Vector3.zero;
        _wiltedPlant.transform.localScale = Vector3.zero;
        _forLeaseSign.transform.localScale = Vector3.zero;
        
        // Setup sadness particles
        foreach (var particle in _sadnessParticles)
        {
            particle.anchoredPosition = new Vector2(
                Random.Range(-200f, 200f), 
                Random.Range(-100f, 100f)
            );
        }
        
        // Set fake stats
        _visitorsText.text = "TOTAL VISITORS: 3";
        _exhibitsText.text = "EXHIBITS PLACED: 1/47";
        _moneyText.text = "MONEY LEFT: -$84,523";
        _ratingText.text = "RATING: ½ ★";
    }
    
    private void SetupColors()
    {
        if (_mainPanelBackground != null)
            _mainPanelBackground.color = _lightGray;
        if (_mainPanelBorder != null)
            _mainPanelBorder.color = _charcoal;
        if (_accentStripe != null)
            _accentStripe.color = _mediumBlue;
            
        _mainTitle.color = _charcoal;
        _subtitle.color = _charcoal;
        _failureMessageText.color = _charcoal;
        
        _tryAgainText.color = Color.white;
        _acceptDefeatText.color = _charcoal;
    }
    
    private void SetupButtonListeners()
    {
        _tryAgainButton.onClick.AddListener(OnTryAgainClicked);
        _acceptDefeatButton.onClick.AddListener(OnAcceptDefeatClicked);
    }
    
    private void ShowGameOverScreen()
    {
        _mainSequence?.Kill(true);
        
        // Select random failure message and achievement
        var randomFailureMessage = _failureMessages[Random.Range(0, _failureMessages.Length)];
        var randomAchievementIndex = Random.Range(0, _achievementTitles.Length);
        
        _failureMessageText.text = randomFailureMessage;
        _achievementTitle.text = _achievementTitles[randomAchievementIndex];
        _achievementDescription.text = _achievementDescriptions[randomAchievementIndex];
        
        // Start the main sequence
        _mainSequence = DOTween.Sequence();
        
        // Fade in from white (main canvas)
        _mainSequence.Append(_mainCanvasGroup.DOFade(1f, _fadeInDuration).SetEase(Ease.OutCubic));
        
        // Achievement badge bounces in
        _mainSequence.Append(_achievementBadge.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack));
        _mainSequence.Join(_achievementBadge.DOFade(1f, 0.6f));
        
        // Main title and subtitle
        _mainSequence.Append(_mainTitle.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        _mainSequence.Join(_subtitle.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        
        // Stats slide in with stagger
        _mainSequence.Append(_visitorsStat.DOAnchorPosX(0f, 0.4f).SetEase(Ease.OutCubic));
        _mainSequence.Join(_visitorsStat.DOAnchorPosY(_visitorsStat.anchoredPosition.y + 10f, 0.2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo));
        _mainSequence.Join(_statsGroup.DOFade(1f, 0.4f));
        
        _mainSequence.Append(_exhibitsStat.DOAnchorPosX(0f, 0.4f).SetEase(Ease.OutCubic).SetDelay(0.1f));
        _mainSequence.Join(_exhibitsStat.DOAnchorPosY(_exhibitsStat.anchoredPosition.y + 10f, 0.2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo).SetDelay(0.1f));
        
        _mainSequence.Append(_moneyStat.DOAnchorPosX(0f, 0.4f).SetEase(Ease.OutCubic).SetDelay(0.2f));
        _mainSequence.Join(_moneyStat.DOAnchorPosY(_moneyStat.anchoredPosition.y + 10f, 0.2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo).SetDelay(0.2f));
        
        _mainSequence.Append(_ratingStat.DOAnchorPosX(0f, 0.4f).SetEase(Ease.OutCubic).SetDelay(0.3f));
        _mainSequence.Join(_ratingStat.DOAnchorPosY(_ratingStat.anchoredPosition.y + 10f, 0.2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo).SetDelay(0.3f));
        
        // Failure message appears
        _mainSequence.Append(_failureMessageGroup.DOFade(1f, 0.5f));
        _mainSequence.Join(_speechBubble.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        
        // Buttons fade in
        _mainSequence.Append(_buttonsGroup.DOFade(1f, 0.5f));
        
        // Visual flourishes start animating
        StartVisualFlourishAnimations();
        
        // Lock camera movement during game over
        Message.Publish(new LockCameraMovement());
    }
    
    private void StartVisualFlourishAnimations()
    {
        // Tumbleweed rolls across
        _tumbleweed.DOAnchorPosX(200f, 4f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetDelay(1f);
        
        // Closed sign bounces in
        _closedSign.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack).SetDelay(2f);
        
        // Cobweb fades in
        _cobweb.DOFade(0.7f, 1f).SetDelay(2.5f);
        
        // Wilted plant appears
        _wiltedPlant.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(3f);
        
        // For lease sign
        _forLeaseSign.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(3.5f);
        
        // Sadness particles float around
        foreach (var particle in _sadnessParticles)
        {
            particle.DOAnchorPos(
                new Vector2(
                    particle.anchoredPosition.x + Random.Range(-50f, 50f),
                    particle.anchoredPosition.y + Random.Range(-30f, 30f)
                ), 
                Random.Range(3f, 6f)
            ).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetDelay(Random.Range(1f, 3f));
        }
        
        // Accent stripe droops slightly
        _accentStripe.transform.DORotate(new Vector3(0f, 0f, -2f), 2f).SetEase(Ease.OutCubic).SetDelay(1f);
    }
    
    private void OnTryAgainClicked()
    {
        // Fade out and restart
        var fadeSequence = DOTween.Sequence();
        fadeSequence.Append(_mainCanvasGroup.DOFade(0f, 1f).SetEase(Ease.InCubic));
        fadeSequence.OnComplete(() => {
            Message.Publish(new UnlockCameraMovement());
            // You can add logic here to restart the game
            // For now, just hide the UI
            gameObject.SetActive(false);
        });
    }
    
    private void OnAcceptDefeatClicked()
    {
        // Fade out and go to main menu or quit
        var fadeSequence = DOTween.Sequence();
        fadeSequence.Append(_mainCanvasGroup.DOFade(0f, 1.5f).SetEase(Ease.InCubic));
        fadeSequence.OnComplete(() => {
            Message.Publish(new UnlockCameraMovement());
            // You can add logic here to go to main menu
            // For now, just hide the UI
            gameObject.SetActive(false);
        });
    }
}
