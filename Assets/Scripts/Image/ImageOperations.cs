using UnityEngine;

public static class ImageOperations
{
    public static void ApplyBrightness(Texture2D texture, float brightness)
    {
        if (texture == null) return;
        
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(
                Mathf.Clamp01(pixels[i].r * brightness),
                Mathf.Clamp01(pixels[i].g * brightness),
                Mathf.Clamp01(pixels[i].b * brightness),
                pixels[i].a
            );
        }
        texture.SetPixels(pixels);
    }
    
    public static void ApplyContrast(Texture2D texture, float contrast)
    {
        if (texture == null) return;
        
        float factor = (259f * (contrast + 1f)) / (259f - contrast);
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(
                Mathf.Clamp01((factor * (pixels[i].r - 0.5f)) + 0.5f),
                Mathf.Clamp01((factor * (pixels[i].g - 0.5f)) + 0.5f),
                Mathf.Clamp01((factor * (pixels[i].b - 0.5f)) + 0.5f),
                pixels[i].a
            );
        }
        texture.SetPixels(pixels);
    }
    
    public static void ApplySaturation(Texture2D texture, float saturation)
    {
        if (texture == null) return;
        
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            float gray = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
            pixels[i] = new Color(
                Mathf.Lerp(gray, pixels[i].r, saturation),
                Mathf.Lerp(gray, pixels[i].g, saturation),
                Mathf.Lerp(gray, pixels[i].b, saturation),
                pixels[i].a
            );
        }
        texture.SetPixels(pixels);
    }
    
    public static void ApplyGrayscale(Texture2D texture)
    {
        if (texture == null) return;
        
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            float gray = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
            pixels[i] = new Color(gray, gray, gray, pixels[i].a);
        }
        texture.SetPixels(pixels);
    }
    
    public static void ApplyInvert(Texture2D texture)
    {
        if (texture == null) return;
        
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(1f - pixels[i].r, 1f - pixels[i].g, 1f - pixels[i].b, pixels[i].a);
        }
        texture.SetPixels(pixels);
    }
    
    public static void ApplyColorTint(Texture2D texture, Color tint)
    {
        if (texture == null) return;
        
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(
                pixels[i].r * tint.r,
                pixels[i].g * tint.g,
                pixels[i].b * tint.b,
                pixels[i].a
            );
        }
        texture.SetPixels(pixels);
    }
}

