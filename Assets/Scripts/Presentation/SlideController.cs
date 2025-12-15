using System.Collections;
using UnityEngine;

public enum SlideTransitionType
{
    Fade,
    Slide,
    Instant
}

public class SlideController : MonoBehaviour
{
    [SerializeField] private Slide[] slides;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private SlideTransitionType transitionType = SlideTransitionType.Slide;
    [SerializeField] private bool allowKeyboardNavigation = true;
    [SerializeField] private float slideDistance = 10f;
    
    private int currentSlideIndex = 0;
    private bool isTransitioning = false;
    private CanvasGroup[] slideCanvasGroups;
    
    private void Awake()
    {
        InitializeCanvasGroups();
    }
    
    private void InitializeCanvasGroups()
    {
        slideCanvasGroups = new CanvasGroup[slides != null ? slides.Length : 0];
        
        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null)
            {
                CanvasGroup canvasGroup = slides[i].GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = slides[i].gameObject.AddComponent<CanvasGroup>();
                }
                slideCanvasGroups[i] = canvasGroup;
            }
        }
    }
    
    private void Start()
    {
        InitializeSlides();
    }
    
    private void Update()
    {
        if (allowKeyboardNavigation && !isTransitioning)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                GoToNextSlide();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                GoToPreviousSlide();
            }
        }
    }
    
    private void InitializeSlides()
    {
        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("No slides assigned to SlideController!");
            return;
        }
        
        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null)
            {
                slides[i].gameObject.SetActive(true);
                
                if (slideCanvasGroups != null && i < slideCanvasGroups.Length && slideCanvasGroups[i] != null)
                {
                    bool isActive = i == 0;
                    slideCanvasGroups[i].alpha = isActive ? 1f : 0f;
                    slideCanvasGroups[i].interactable = isActive;
                    slideCanvasGroups[i].blocksRaycasts = isActive;
                }
            }
        }
        
        currentSlideIndex = 0;
        UpdateNavigationArrows();
    }
    
    public void GoToNextSlide()
    {
        if (currentSlideIndex < slides.Length - 1)
        {
            GoToSlide(currentSlideIndex + 1);
        }
    }
    
    public void GoToPreviousSlide()
    {
        if (currentSlideIndex > 0)
        {
            GoToSlide(currentSlideIndex - 1);
        }
    }
    
    public void GoToSlide(int index)
    {
        if (index < 0 || index >= slides.Length || slides[index] == null)
        {
            return;
        }
        
        if (isTransitioning)
        {
            return;
        }
        
        StartCoroutine(TransitionToSlide(index));
    }
    
    private IEnumerator TransitionToSlide(int targetIndex)
    {
        isTransitioning = true;
        
        Slide currentSlide = slides[currentSlideIndex];
        Slide targetSlide = slides[targetIndex];
        
        int direction = targetIndex > currentSlideIndex ? 1 : -1;
        
        switch (transitionType)
        {
            case SlideTransitionType.Fade:
                yield return StartCoroutine(FadeTransition(currentSlide, targetSlide));
                break;
            case SlideTransitionType.Slide:
                yield return StartCoroutine(SlideTransition(currentSlide, targetSlide, direction));
                break;
            case SlideTransitionType.Instant:
                if (currentSlide != null) currentSlide.Hide();
                if (targetSlide != null) targetSlide.Show();
                break;
        }
        
        currentSlideIndex = targetIndex;
        isTransitioning = false;
        
        UpdateNavigationArrows();
    }
    
    private void UpdateNavigationArrows()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null)
            {
                slides[i].UpdateNavigationArrows(i == currentSlideIndex, i == 0, i == slides.Length - 1);
            }
        }
    }
    
    private IEnumerator FadeTransition(Slide currentSlide, Slide targetSlide)
    {
        CanvasGroup currentGroup = null;
        CanvasGroup targetGroup = null;
        
        if (currentSlideIndex >= 0 && currentSlideIndex < slideCanvasGroups.Length)
        {
            currentGroup = slideCanvasGroups[currentSlideIndex];
        }
        
        int targetIdx = -1;
        if (targetSlide != null)
        {
            for (int i = 0; i < slides.Length; i++)
            {
                if (slides[i] == targetSlide)
                {
                    targetIdx = i;
                    break;
                }
            }
        }
        
        if (targetIdx >= 0 && targetIdx < slideCanvasGroups.Length)
        {
            targetGroup = slideCanvasGroups[targetIdx];
        }
        
        if (targetSlide != null)
        {
            targetSlide.Show();
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            float curveValue = transitionCurve.Evaluate(t);
            
            if (currentGroup != null)
            {
                currentGroup.alpha = 1f - curveValue;
            }
            
            if (targetGroup != null)
            {
                targetGroup.alpha = curveValue;
                targetGroup.interactable = curveValue > 0.5f;
                targetGroup.blocksRaycasts = curveValue > 0.5f;
            }
            
            yield return null;
        }
        
        if (currentGroup != null)
        {
            currentGroup.alpha = 0f;
            currentGroup.interactable = false;
            currentGroup.blocksRaycasts = false;
        }
        
        if (targetGroup != null)
        {
            targetGroup.alpha = 1f;
            targetGroup.interactable = true;
            targetGroup.blocksRaycasts = true;
        }
    }
    
    private IEnumerator SlideTransition(Slide currentSlide, Slide targetSlide, int direction)
    {
        if (targetSlide != null)
        {
            targetSlide.Show();
        }
        
        RectTransform currentRect = currentSlide != null ? currentSlide.GetComponent<RectTransform>() : null;
        RectTransform targetRect = targetSlide != null ? targetSlide.GetComponent<RectTransform>() : null;
        
        if (currentRect == null || targetRect == null)
        {
            yield break;
        }
        
        Vector2 currentStartPos = currentRect.anchoredPosition;
        Vector2 currentEndPos = currentStartPos + new Vector2(-direction * slideDistance * 100f, 0f);
        
        Vector2 targetStartPos = targetRect.anchoredPosition;
        Vector2 targetEndPos = targetStartPos;
        targetStartPos += new Vector2(direction * slideDistance * 100f, 0f);
        
        if (targetRect != null)
        {
            targetRect.anchoredPosition = targetStartPos;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            float curveValue = transitionCurve.Evaluate(t);
            
            if (currentRect != null)
            {
                currentRect.anchoredPosition = Vector2.Lerp(currentStartPos, currentEndPos, curveValue);
            }
            
            if (targetRect != null)
            {
                targetRect.anchoredPosition = Vector2.Lerp(targetStartPos, targetEndPos, curveValue);
            }
            
            yield return null;
        }
        
        if (currentRect != null)
        {
            currentRect.anchoredPosition = currentEndPos;
        }
        
        if (targetRect != null)
        {
            targetRect.anchoredPosition = targetEndPos;
        }
        
        if (currentSlide != null)
        {
            currentSlide.Hide();
        }
    }
    
    public int GetCurrentSlideIndex()
    {
        return currentSlideIndex;
    }
    
    public int GetTotalSlides()
    {
        return slides != null ? slides.Length : 0;
    }
    
    public Slide GetCurrentSlide()
    {
        if (currentSlideIndex >= 0 && currentSlideIndex < slides.Length)
        {
            return slides[currentSlideIndex];
        }
        return null;
    }
    
    public static SlideController Instance { get; private set; }
    
    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

