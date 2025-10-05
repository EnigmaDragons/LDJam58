using UnityEngine;
using System.Collections;

public class SquareImageRotator : MonoBehaviour
{
    [Header("Scale Animation Settings")]
    [SerializeField] private float animationSpeed = 2f; // scale speed multiplier
    [SerializeField] private float hideDelay = 0.1f; // delay before hiding during transition
    
    [Header("Target Transform")]
    [SerializeField] private Transform targetTransform; // the square image to animate
    
    private bool isAnimating = false;
    private int currentAnimationPhase = 0; // 0: Y, 1: X, 2: Y again
    private Vector3 originalScale;
    
    private void Start()
    {
        if (targetTransform == null)
            targetTransform = transform;
        
        originalScale = targetTransform.localScale;
    }
    
    public void StartAnimationSequence()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimationSequence());
        }
    }
    
    private IEnumerator AnimationSequence()
    {
        isAnimating = true;
        currentAnimationPhase = 0;
        
        // Phase 1: Scale Y to simulate Y-axis rotation
        yield return StartCoroutine(ScaleAxis(Vector3.up, 0f));
        
        // Hide and transition to X scale
        SetVisibility(false);
        yield return new WaitForSeconds(hideDelay);
        
        // Phase 2: Scale X to simulate X-axis rotation
        currentAnimationPhase = 1;
        SetVisibility(true);
        yield return StartCoroutine(ScaleAxis(Vector3.right, 0f));
        
        // Hide and transition to final Y scale
        SetVisibility(false);
        yield return new WaitForSeconds(hideDelay);
        
        // Phase 3: Scale Y again to simulate Y-axis rotation
        currentAnimationPhase = 2;
        SetVisibility(true);
        yield return StartCoroutine(ScaleAxis(Vector3.up, 0f));
        
        isAnimating = false;
    }
    
    private IEnumerator ScaleAxis(Vector3 axis, float targetScale)
    {
        var startScale = targetTransform.localScale;
        var targetScaleVector = originalScale;
        
        // Modify the target scale based on the axis
        if (axis == Vector3.up)
        {
            targetScaleVector.y = targetScale;
        }
        else if (axis == Vector3.right)
        {
            targetScaleVector.x = targetScale;
        }
        
        var elapsedTime = 0f;
        var animationDuration = 1f / animationSpeed;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            var progress = elapsedTime / animationDuration;
            targetTransform.localScale = Vector3.Lerp(startScale, targetScaleVector, progress);
            yield return null;
        }
        
        targetTransform.localScale = targetScaleVector;
    }
    
    private void SetVisibility(bool visible)
    {
        if (targetTransform.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.enabled = visible;
        }
        else if (targetTransform.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
        else if (targetTransform.TryGetComponent<UnityEngine.UI.Image>(out var image))
        {
            image.enabled = visible;
        }
        else
        {
            // Fallback: disable/enable the entire GameObject
            targetTransform.gameObject.SetActive(visible);
        }
    }
    
    // Public methods for external control
    public void ResetAnimation()
    {
        if (targetTransform != null)
        {
            targetTransform.localScale = originalScale;
            SetVisibility(true);
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
}
