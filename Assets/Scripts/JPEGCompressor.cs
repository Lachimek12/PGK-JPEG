using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class JPEGCompressor : MonoBehaviour
{
    [Header("Images")]
    public Image SourceImage;
    public Image ImageY;
    public Image ImageCb;
    public Image ImageCr;
    public Image BlockImage;
    public Image SelectorImage;
    public Image DCTImage;

    [Header("Other")]
    public Vector2Int SelectedBlock = new Vector2Int(0, 0);
    public const int BLOCK_SIZE = 8;
    public int Width, Height;
    public JpegChannel SelectedChannel = JpegChannel.Y;

    Texture2D blockTex;
    Texture2D dctTex;
    Texture2D sourceTexture;
    PixelYCbCr[,] ycbcr;

    struct PixelYCbCr
    {
        public float Y;
        public float Cb;
        public float Cr;
    }

    public enum JpegChannel
    {
        Y,
        Cb,
        Cr
    }

    public void Start()
    {
        SetSourceTexture(SourceImage.sprite.texture);
    }

    public void LoadBMP()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Wczytaj obraz", "", extensions, false);

        if (paths.Length == 0 || !File.Exists(paths[0]))
            return;

        byte[] data = File.ReadAllBytes(paths[0]);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        tex.LoadImage(data);
        tex.Apply();

        SetSourceTexture(tex);

        SourceImage.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    void SetSourceTexture(Texture2D tex)
    {
        sourceTexture = tex;

        Width = tex.width;
        Height = tex.height;

        ConvertToYCbCr();
        GenerateTextures();
    }

    void ConvertToYCbCr()
    {
        ycbcr = new PixelYCbCr[Width, Height];
        Color[] pixels = sourceTexture.GetPixels();

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                Color c = pixels[y * Width + x];

                PixelYCbCr p;
                p.Y = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
                p.Cb = -0.1687f * c.r - 0.3313f * c.g + 0.5f * c.b + 0.5f;
                p.Cr = 0.5f * c.r - 0.4187f * c.g - 0.0813f * c.b + 0.5f;

                ycbcr[x, y] = p;
            }
    }

    void GenerateTextures()
    {
        Texture2D texY = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        Texture2D texCb = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        Texture2D texCr = new Texture2D(Width, Height, TextureFormat.RGB24, false);

        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                PixelYCbCr p = ycbcr[x, y];

                texY.SetPixel(x, y, new Color(p.Y, p.Y, p.Y));
                texCb.SetPixel(x, y, new Color(p.Y, p.Y, p.Cb));
                texCr.SetPixel(x, y, new Color(p.Cr, p.Y, p.Y));
            }

        texY.Apply();
        texCb.Apply();
        texCr.Apply();

        ImageY.sprite = MakeSprite(texY);
        ImageCb.sprite = MakeSprite(texCb);
        ImageCr.sprite = MakeSprite(texCr);
    }

    Sprite MakeSprite(Texture2D tex)
    {
        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    public void SetSelectedChannel(JpegChannel channel)
    {
        SelectedChannel = channel;
        switch (SelectedChannel)
        {
            case JpegChannel.Y:
                SelectorImage.sprite = ImageY.sprite;
                break;
            case JpegChannel.Cb:
                SelectorImage.sprite = ImageCb.sprite;
                break;
            case JpegChannel.Cr:
                SelectorImage.sprite = ImageCr.sprite;
                break;
        }
    }

    public void SelectBlockFromPixel(int px, int py)
    {
        int bx = Mathf.Clamp(px / BLOCK_SIZE, 0, Width / BLOCK_SIZE - 1);
        int by = Mathf.Clamp(py / BLOCK_SIZE, 0, Height / BLOCK_SIZE - 1);
        SelectedBlock = new Vector2Int(bx, by);
    }

    public float[,] GetSelectedBlock()
    {
        float[,] block = new float[8, 8];
        int sx = SelectedBlock.x * BLOCK_SIZE;
        int sy = SelectedBlock.y * BLOCK_SIZE;

        for (int y = 0; y < BLOCK_SIZE; y++)
            for (int x = 0; x < BLOCK_SIZE; x++)
            {
                var p = ycbcr[sx + x, sy + y];
                block[x, y] = SelectedChannel switch
                {
                    JpegChannel.Y => p.Y - 0.5f,
                    JpegChannel.Cb => p.Cb - 0.5f,
                    JpegChannel.Cr => p.Cr - 0.5f,
                    _ => 0
                };
            }

        return block;
    }

    public Texture2D GetSelectedBlockTexture(int scale = 32)
    {
        Texture2D tex = new Texture2D(8 * scale, 8 * scale, TextureFormat.RGB24, false);

        float[,] block = GetSelectedBlock();

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int yy = 7 - y;

                float v = block[x, y] + 0.5f;
                Color c = new Color(v, v, v);

                for (int sy = 0; sy < scale; sy++)
                    for (int sx = 0; sx < scale; sx++)
                        tex.SetPixel(x * scale + sx, yy * scale + sy, c);
            }

        tex.Apply();
        return tex;
    }

    public void RefreshBlockImage()
    {
        if (blockTex != null)
            Destroy(blockTex);

        blockTex = GetSelectedBlockTexture();

        BlockImage.sprite = Sprite.Create(
            blockTex,
            new Rect(0, 0, blockTex.width, blockTex.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    public float[,] DCT(float[,] block)
    {
        float[,] F = new float[BLOCK_SIZE, BLOCK_SIZE];

        for (int u = 0; u < BLOCK_SIZE; u++)
            for (int v = 0; v < BLOCK_SIZE; v++)
            {
                float sum = 0f;

                for (int x = 0; x < BLOCK_SIZE; x++)
                    for (int y = 0; y < BLOCK_SIZE; y++)
                    {
                        float pixel = block[x, y];
                        sum += pixel *
                            Mathf.Cos(((2 * x + 1) * u * Mathf.PI) / (2 * BLOCK_SIZE)) *
                            Mathf.Cos(((2 * y + 1) * v * Mathf.PI) / (2 * BLOCK_SIZE));
                    }

                float cu = (u == 0) ? Mathf.Sqrt(1f / BLOCK_SIZE) : Mathf.Sqrt(2f / BLOCK_SIZE);
                float cv = (v == 0) ? Mathf.Sqrt(1f / BLOCK_SIZE) : Mathf.Sqrt(2f / BLOCK_SIZE);

                F[u, v] = cu * cv * sum;
            }

        return F;
    }

    public Texture2D GetSelectedBlockDCTTexture(int scale = 32)
    {
        float[,] block = GetSelectedBlock();
        float[,] dct = DCT(block);

        float globalMin = -0.5f;
        float globalMax = 0.5f;

        Texture2D tex = new Texture2D(8 * scale, 8 * scale, TextureFormat.RGB24, false);

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int yy = 7 - y;

                float v = (dct[x, y] - globalMin) / (globalMax - globalMin);
                v = Mathf.Clamp01(v);
                Color c = new Color(v, v, v);

                for (int sy = 0; sy < scale; sy++)
                    for (int sx = 0; sx < scale; sx++)
                        tex.SetPixel(x * scale + sx, yy * scale + sy, c);
            }

        tex.Apply();
        return tex;
    }

    public void RefreshDCTImage()
    {
        if (dctTex != null)
            Destroy(dctTex);

        dctTex = GetSelectedBlockDCTTexture();

        DCTImage.sprite = Sprite.Create(
            dctTex,
            new Rect(0, 0, dctTex.width, dctTex.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}
