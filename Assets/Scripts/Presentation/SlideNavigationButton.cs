using UnityEngine;
using UnityEngine.UI;

public class SlideNavigationButton : MonoBehaviour
{
    [SerializeField] private bool isNextButton = true;
    
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }
    
    private void OnButtonClick()
    {
        if (SlideController.Instance != null)
        {
            if (isNextButton)
            {
                SlideController.Instance.GoToNextSlide();
            }
            else
            {
                SlideController.Instance.GoToPreviousSlide();
            }
        }
    }
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}

