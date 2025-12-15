using UnityEngine;
using UnityEngine.UI;

public class PixelZoomLens : MonoBehaviour
{
    private Texture2D sourceTexture;
    private Image displayImage;
    private Texture2D zoomTexture;
    private int lensSize = 300;
    private int pixelSampleSize = 10;
    private float currentZoom = 1f;
    
    public void Initialize(Texture2D texture, Image image)
    {
        sourceTexture = texture;
        displayImage = image;
        zoomTexture = new Texture2D(lensSize, lensSize, TextureFormat.RGBA32, false);
        zoomTexture.filterMode = FilterMode.Point;
        
        if (displayImage != null)
        {
            displayImage.sprite = Sprite.Create(zoomTexture, new Rect(0, 0, lensSize, lensSize), new Vector2(0.5f, 0.5f));
        }
    }
    
    public void UpdateZoom(Texture2D texture, Vector2 center, float zoom)
    {
        if (texture == null || zoomTexture == null) return;
        
        sourceTexture = texture;
        currentZoom = zoom;
        
        int sourceWidth = texture.width;
        int sourceHeight = texture.height;
        
        int centerX = Mathf.RoundToInt(center.x * sourceWidth);
        int centerY = Mathf.RoundToInt(center.y * sourceHeight);
        
        int startX = Mathf.Clamp(centerX - pixelSampleSize / 2, 0, sourceWidth - 1);
        int startY = Mathf.Clamp(centerY - pixelSampleSize / 2, 0, sourceHeight - 1);
        int endX = Mathf.Clamp(centerX + pixelSampleSize / 2 - 1, 0, sourceWidth - 1);
        int endY = Mathf.Clamp(centerY + pixelSampleSize / 2 - 1, 0, sourceHeight - 1);
        
        float pixelScale = (float)lensSize / pixelSampleSize;
        float radius = lensSize * 0.5f;
        float centerPos = lensSize * 0.5f;
        
        for (int y = 0; y < lensSize; y++)
        {
            for (int x = 0; x < lensSize; x++)
            {
                float dx = x - centerPos;
                float dy = y - centerPos;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (distance > radius)
                {
                    zoomTexture.SetPixel(x, y, Color.clear);
                }
                else
                {
                    int sourceX = startX + Mathf.FloorToInt(x / pixelScale);
                    int sourceY = startY + Mathf.FloorToInt(y / pixelScale);
                    
                    sourceX = Mathf.Clamp(sourceX, startX, endX);
                    sourceY = Mathf.Clamp(sourceY, startY, endY);
                    
                    Color pixel = texture.GetPixel(sourceX, sourceY);
                    zoomTexture.SetPixel(x, y, pixel);
                }
            }
        }
        
        zoomTexture.Apply();
    }
    
    private void OnDestroy()
    {
        if (zoomTexture != null)
        {
            Destroy(zoomTexture);
        }
    }
}

