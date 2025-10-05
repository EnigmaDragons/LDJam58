using Game.Messages;
using UnityEngine;
using DG.Tweening;

// TODO: Juice the Change!
public class PickThreeController : OnMessage<BeginPickThree, ClosePickMenu>
{
    [SerializeField] private CanvasGroup _panelGroup;
    [SerializeField] private ExhibitPickerView _one;
    [SerializeField] private ExhibitPickerView _two;
    [SerializeField] private ExhibitPickerView _three;
    
    protected override void Execute(ClosePickMenu msg)
    {
        // Kill any existing animations
        _one.transform.DOKill();
        _two.transform.DOKill();
        _three.transform.DOKill();
        
        // Close instantly with no animation
        _one.gameObject.SetActive(false);
        _two.gameObject.SetActive(false);
        _three.gameObject.SetActive(false);
        _panelGroup.alpha = 0f;
        _panelGroup.gameObject.SetActive(false);
    }

    protected override void AfterEnable()
    {
        _one.gameObject.SetActive(false);
        _two.gameObject.SetActive(false);
        _three.gameObject.SetActive(false);
    }
    
    

    protected override void Execute(BeginPickThree msg)
    {
        _panelGroup.alpha = 1f;
        _panelGroup.gameObject.SetActive(true);
        // Initialize all exhibits first
        _one.Init(msg.Exhibits[0]);
        _two.Init(msg.Exhibits[1]);
        _three.Init(msg.Exhibits[2]);
        
        // Kill any existing animations
        _one.transform.DOKill();
        _two.transform.DOKill();
        _three.transform.DOKill();
        
        // Set initial scale to 0 and activate
        _one.transform.localScale = Vector3.zero;
        _two.transform.localScale = Vector3.zero;
        _three.transform.localScale = Vector3.zero;
        
        _one.gameObject.SetActive(true);
        _two.gameObject.SetActive(true);
        _three.gameObject.SetActive(true);
        
        // Create sequence for scale bounce-in animations
        var sequence = DOTween.Sequence();
        
        // First option bounces in
        sequence.Append(_one.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        
        // Second option bounces in after a short delay
        sequence.Append(_two.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(0.1f));
        
        // Third option bounces in after another short delay
        sequence.Append(_three.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(0.1f));
    }
}
