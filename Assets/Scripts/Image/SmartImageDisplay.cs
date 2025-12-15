using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SmartImageDisplay : MonoBehaviour
{
    [SerializeField] private SmartImageObject smartImageObject;
    [SerializeField] private bool enablePixelZoom = true;
    [SerializeField] private float zoomLevel = 1f;
    [SerializeField] private Vector2 zoomCenter = Vector2.zero;
    
    private Image imageComponent;
    private RectTransform rectTransform;
    private PixelZoomLens zoomLens;
    
    private void Awake()
    {
        imageComponent = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        if (smartImageObject != null)
        {
            smartImageObject.RegisterDisplay(this);
            UpdateImage();
        }
    }
    
    private void OnDestroy()
    {
        if (smartImageObject != null)
        {
            smartImageObject.UnregisterDisplay(this);
        }
    }
    
    public void SetSmartImageObject(SmartImageObject smartObj)
    {
        if (smartImageObject != null)
        {
            smartImageObject.UnregisterDisplay(this);
        }
        
        smartImageObject = smartObj;
        
        if (smartImageObject != null)
        {
            smartImageObject.RegisterDisplay(this);
            UpdateImage();
        }
    }
    
    public void UpdateImage()
    {
        if (imageComponent != null && smartImageObject != null)
        {
            imageComponent.sprite = smartImageObject.Sprite;
        }
    }
    
    private void Update()
    {
        if (enablePixelZoom && Input.GetKey(KeyCode.LeftShift) && smartImageObject != null && smartImageObject.Texture != null)
        {
            if (zoomLens == null)
            {
                CreateZoomLens();
            }
            
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out mousePos);
            
            Rect rect = rectTransform.rect;
            Vector2 normalizedPos = new Vector2(
                (mousePos.x - rect.x) / rect.width,
                (mousePos.y - rect.y) / rect.height
            );
            
            zoomCenter = normalizedPos;
            UpdateZoomLens();
        }
        else
        {
            if (zoomLens != null)
            {
                zoomLens.gameObject.SetActive(false);
            }
        }
    }
    
    private void CreateZoomLens()
    {
        GameObject lensObject = new GameObject("PixelZoomLens");
        lensObject.transform.SetParent(transform.parent, false);
        
        RectTransform lensRect = lensObject.AddComponent<RectTransform>();
        lensRect.sizeDelta = new Vector2(300, 300);
        lensRect.anchoredPosition = new Vector2(200, 200);
        
        Image lensImage = lensObject.AddComponent<Image>();
        lensImage.raycastTarget = false;
        
        Outline outline = lensObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
        outline.useGraphicAlpha = true;
        
        zoomLens = lensObject.AddComponent<PixelZoomLens>();
        zoomLens.Initialize(smartImageObject.Texture, lensImage);
    }
    
    private void UpdateZoomLens()
    {
        if (zoomLens != null && smartImageObject != null && smartImageObject.Texture != null)
        {
            zoomLens.gameObject.SetActive(true);
            zoomLens.UpdateZoom(smartImageObject.Texture, zoomCenter, zoomLevel);
            
            Vector2 mousePos = Input.mousePosition;
            RectTransform lensRect = zoomLens.GetComponent<RectTransform>();
            lensRect.position = new Vector3(mousePos.x + 100, mousePos.y + 100, 0);
        }
    }
    
    public void SetZoomLevel(float level)
    {
        zoomLevel = Mathf.Clamp(level, 1f, 50f);
    }
}

