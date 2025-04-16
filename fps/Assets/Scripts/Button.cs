using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    public float hoverScale = 1.2f;
    public float animationSpeed = 10f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    
    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }
    
    void Update()
    {
        // 平滑过渡到目标缩放
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}