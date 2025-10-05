using UnityEngine;
using System.Collections;
using DG.Tweening;

public class SquareImageRotator : MonoBehaviour
{
    [Header("Scale Animation Settings")]
    [SerializeField] private float animationSpeed = 2f; // scale speed multiplier
    [SerializeField] private float hideDelay = 0.1f; // delay before hiding during transition
    
    [Header("Target Transform")]
    [SerializeField] private RectTransform targetRectTransform; // the square image to animate
    
    private bool isAnimating = false;
    private int currentAnimationPhase = 0; // 0: Y, 1: X, 2: Y again
    private Vector3 originalScale;
    private Sequence animationSequence;
    
    private void Start()
    {
        if (targetRectTransform == null)
            targetRectTransform = GetComponent<RectTransform>();
        
        originalScale = targetRectTransform.localScale;
    }
    
    public void StartAnimationSequence()
    {
        if (!isAnimating)
        {
            StartDOTweenSequence();
        }
    }
    
    private void StartDOTweenSequence()
    {
        isAnimating = true;
        currentAnimationPhase = 0;
        
        // Kill any existing sequence
        animationSequence?.Kill();
        
        // Create new sequence
        animationSequence = DOTween.Sequence();
        
        var animationDuration = 1f / animationSpeed;
        
        // Phase 1: Scale Y to simulate Y-axis rotation
        animationSequence.Append(targetRectTransform.DOScaleY(0f, animationDuration))
            .OnComplete(() => currentAnimationPhase = 0);
        
        // Hide and transition to X scale
        animationSequence.AppendCallback(() => SetVisibility(false))
            .AppendInterval(hideDelay);
        
        // Phase 2: Scale X to simulate X-axis rotation
        animationSequence.AppendCallback(() => {
            currentAnimationPhase = 1;
            SetVisibility(true);
        });
        animationSequence.Append(targetRectTransform.DOScaleX(0f, animationDuration));
        
        // Hide and transition to final Y scale
        animationSequence.AppendCallback(() => SetVisibility(false))
            .AppendInterval(hideDelay);
        
        // Phase 3: Scale Y again to simulate Y-axis rotation
        animationSequence.AppendCallback(() => {
            currentAnimationPhase = 2;
            SetVisibility(true);
        });
        animationSequence.Append(targetRectTransform.DOScaleY(0f, animationDuration));
        
        // Complete the sequence
        animationSequence.OnComplete(() => {
            isAnimating = false;
        });
    }
    
    
    private void SetVisibility(bool visible)
    {
        if (targetRectTransform.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.enabled = visible;
        }
        else if (targetRectTransform.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
        else if (targetRectTransform.TryGetComponent<UnityEngine.UI.Image>(out var image))
        {
            image.enabled = visible;
        }
        else
        {
            // Fallback: disable/enable the entire GameObject
            targetRectTransform.gameObject.SetActive(visible);
        }
    }
    
    // Public methods for external control
    public void ResetAnimation()
    {
        if (targetRectTransform != null)
        {
            animationSequence?.Kill();
            targetRectTransform.localScale = originalScale;
            SetVisibility(true);
            isAnimating = false;
        }
    }
    
    public bool IsAnimating()
    {
        return isAnimating;
    }
    
    public int GetCurrentPhase()
    {
        return currentAnimationPhase;
    }
    
    // For testing in editor
    [ContextMenu("Start Animation Sequence")]
    private void TestAnimationSequence()
    {
        StartAnimationSequence();
    }
    
    private void OnDestroy()
    {
        animationSequence?.Kill();
    }
}
