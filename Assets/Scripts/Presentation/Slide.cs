using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Slide : MonoBehaviour
{
    [SerializeField] private string slideTitle = "Untitled Slide";
    
    public string Title => slideTitle;
    
    private CanvasGroup canvasGroup;
    private GameObject leftArrow;
    private GameObject rightArrow;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        Transform leftArrowTransform = transform.Find("LeftArrow");
        if (leftArrowTransform != null)
        {
            leftArrow = leftArrowTransform.gameObject;
        }
        
        Transform rightArrowTransform = transform.Find("RightArrow");
        if (rightArrowTransform != null)
        {
            rightArrow = rightArrowTransform.gameObject;
        }
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
    
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
    
    public void SetActiveState(bool active)
    {
        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = active ? 1f : 0f;
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;
        }
    }
    
    public void UpdateNavigationArrows(bool isCurrentSlide, bool isFirstSlide, bool isLastSlide)
    {
        if (leftArrow != null)
        {
            leftArrow.SetActive(isCurrentSlide && !isFirstSlide);
        }
        
        if (rightArrow != null)
        {
            rightArrow.SetActive(isCurrentSlide && !isLastSlide);
        }
    }
}

