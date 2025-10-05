using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMinimalUI : OnMessage<GameLost>
{
    [SerializeField] private CanvasGroup _mainCanvas;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _statsText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Button _tryAgainButton;
    [SerializeField] private Button _quitButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float _fadeInDuration = 1.5f;
    [SerializeField] private float _textDelay = 0.2f;
    
    private readonly string[] _failureMessages = {
        "The T-Rex skeleton is now a coffee table at someone's house",
        "Your museum is now a Spirit Halloween store", 
        "Turns out people don't want to visit an empty room with one rock",
        "The pigeons have claimed the sculpture garden",
        "Congratulations! You've created the world's most expensive storage unit",
        "Even the gift shop closed. THE GIFT SHOP."
    };
    
    private Sequence _animationSequence;

    protected override void AfterEnable()
    {
        _mainCanvas.alpha = 0f;
        SetupInitialState();
    }
    
    protected override void AfterDisable()
    {
        _animationSequence?.Kill(true);
        if (_tryAgainButton != null) _tryAgainButton.onClick.RemoveAllListeners();
        if (_quitButton != null) _quitButton.onClick.RemoveAllListeners();
    }

    protected override void Execute(GameLost msg)
    {
        ShowGameOver();
    }
        
    private void SetupInitialState()
    {
        // Set initial states for animation
        _titleText.transform.localScale = Vector3.zero;
        _statsText.alpha = 0f;
        _messageText.alpha = 0f;
        _tryAgainButton.transform.localScale = Vector3.zero;
        _quitButton.transform.localScale = Vector3.zero;
        
        // Setup button listeners
        _tryAgainButton.onClick.AddListener(OnTryAgain);
        _quitButton.onClick.AddListener(OnQuit);


        _titleText.text = "MUSEUM CLOSED\n(permanently)";
        _statsText.text = "TOTAL VISITORS: 3\nEXHIBITS PLACED: 1/47\nMONEY LEFT: -$84,523\nRATING: 1 Star";
        _messageText.text = _failureMessages[Random.Range(0, _failureMessages.Length)];
    }
    
    private void ShowGameOver()
    {
        _animationSequence?.Kill(true);
        
        // Pick random failure message
        _messageText.text = _failureMessages[Random.Range(0, _failureMessages.Length)];
        
        // Create animation sequence
        _animationSequence = DOTween.Sequence();
        
        // Fade in main canvas
        _animationSequence.Append(_mainCanvas.DOFade(1f, _fadeInDuration).SetEase(Ease.OutCubic));
        
        // Title bounces in
        _animationSequence.Append(_titleText.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack));
        
        // Stats fade in
        _animationSequence.Append(_statsText.DOFade(1f, 0.5f).SetDelay(_textDelay));
        
        // Message fades in
        _animationSequence.Append(_messageText.DOFade(1f, 0.5f).SetDelay(_textDelay));
        
        // Buttons bounce in
        _animationSequence.Append(_tryAgainButton.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(_textDelay));
        _animationSequence.Join(_quitButton.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(_textDelay));
        
        // Lock camera movement
        Message.Publish(new LockCameraMovement());
    }
    
    private void OnTryAgain()
    {
        FadeOut(() => {
            Message.Publish(new NavigateToSceneRequested("GameScene"));
        });
    }
    
    private void OnQuit()
    {
        FadeOut(() => {
            Application.Quit();
        });
    }
    
    private void FadeOut(System.Action onComplete)
    {
        var fadeSequence = DOTween.Sequence();
        fadeSequence.Append(_mainCanvas.DOFade(0f, 1f).SetEase(Ease.InCubic));
        fadeSequence.OnComplete(() => onComplete?.Invoke());
    }
}
