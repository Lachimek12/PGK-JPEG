using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Smart Image", menuName = "Presentation/Smart Image Object")]
public class SmartImageObject : ScriptableObject
{
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Texture2D originalTexture;
    
    private Texture2D workingCopy;
    private Sprite workingSprite;
    private List<SmartImageDisplay> registeredDisplays = new List<SmartImageDisplay>();
    
    public Sprite OriginalSprite
    {
        get => originalSprite;
        set
        {
            originalSprite = value;
            if (value != null && value.texture != null)
            {
                originalTexture = value.texture as Texture2D;
                CreateWorkingCopy();
            }
        }
    }
    
    public Texture2D OriginalTexture
    {
        get => originalTexture;
        set
        {
            originalTexture = value;
            if (value != null)
            {
                if (originalSprite == null)
                {
                    originalSprite = Sprite.Create(value, new Rect(0, 0, value.width, value.height), new Vector2(0.5f, 0.5f));
                }
                CreateWorkingCopy();
            }
        }
    }
    
    public Sprite Sprite
    {
        get
        {
            if (workingSprite == null && workingCopy != null)
            {
                workingSprite = Sprite.Create(workingCopy, new Rect(0, 0, workingCopy.width, workingCopy.height), new Vector2(0.5f, 0.5f));
            }
            return workingSprite != null ? workingSprite : originalSprite;
        }
    }
    
    public Texture2D Texture
    {
        get => workingCopy != null ? workingCopy : originalTexture;
    }
    
    private void CreateWorkingCopy()
    {
        if (originalTexture == null) return;
        
        if (workingCopy != null)
        {
            Destroy(workingCopy);
        }
        if (workingSprite != null)
        {
            Destroy(workingSprite);
        }
        
        workingCopy = CreateReadableCopy(originalTexture);
        if (workingCopy != null)
        {
            workingSprite = Sprite.Create(workingCopy, new Rect(0, 0, workingCopy.width, workingCopy.height), new Vector2(0.5f, 0.5f));
            NotifyDisplays();
        }
    }
    
    private Texture2D CreateReadableCopy(Texture2D source)
    {
        if (source == null) return null;
        
        RenderTexture renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, renderTexture);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        
        Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        copy.Apply();
        
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);
        
        return copy;
    }
    
    public void ResetToOriginal()
    {
        CreateWorkingCopy();
    }
    
    public Texture2D GetWorkingCopy()
    {
        if (workingCopy == null && originalTexture != null)
        {
            CreateWorkingCopy();
        }
        return workingCopy;
    }
    
    public void ApplyOperation(System.Action<Texture2D> operation)
    {
        Texture2D copy = GetWorkingCopy();
        if (copy != null && operation != null)
        {
            operation(copy);
            copy.Apply();
            
            if (workingSprite != null)
            {
                Destroy(workingSprite);
            }
            workingSprite = Sprite.Create(copy, new Rect(0, 0, copy.width, copy.height), new Vector2(0.5f, 0.5f));
            NotifyDisplays();
        }
    }
    
    public void RegisterDisplay(SmartImageDisplay display)
    {
        if (!registeredDisplays.Contains(display))
        {
            registeredDisplays.Add(display);
        }
    }
    
    public void UnregisterDisplay(SmartImageDisplay display)
    {
        registeredDisplays.Remove(display);
    }
    
    private void NotifyDisplays()
    {
        foreach (var display in registeredDisplays)
        {
            if (display != null)
            {
                display.UpdateImage();
            }
        }
    }
    
    private void OnEnable()
    {
        registeredDisplays.Clear();
        if (originalTexture != null && workingCopy == null)
        {
            CreateWorkingCopy();
        }
    }
    
    private void OnDestroy()
    {
        if (workingCopy != null)
        {
            Destroy(workingCopy);
        }
        if (workingSprite != null)
        {
            Destroy(workingSprite);
        }
    }
}

