using SFB;
using System.IO;
using TMPro;
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
    public Image QDCTImage;
    public Transform QMatrixParent;
    public Transform QDCTParent;
    public Transform QuantizedParent;
    public Image QuantizedImage;

    [Header("Other")]
    public Vector2Int SelectedBlock = new Vector2Int(0, 0);
    public const int BLOCK_SIZE = 8;
    public int Width, Height;
    public JpegChannel SelectedChannel = JpegChannel.Y;
    public int JpegQuality = 50;
    public TMP_Text[] QMatrixCells = new TMP_Text[64];
    public TMP_Text[] QDCTCells = new TMP_Text[64];
    public TMP_Text[] QuantizedCells = new TMP_Text[64];

    public readonly int[,] LumaQ =
    {
        {16,11,10,16,24,40,51,61},
        {12,12,14,19,26,58,60,55},
        {14,13,16,24,40,57,69,56},
        {14,17,22,29,51,87,80,62},
        {18,22,37,56,68,109,103,77},
        {24,35,55,64,81,104,113,92},
        {49,64,78,87,103,121,120,101},
        {72,92,95,98,112,100,103,99}
    };

    public readonly int[,] ChromaQ =
    {
        {17,18,24,47,99,99,99,99},
        {18,21,26,66,99,99,99,99},
        {24,26,56,99,99,99,99,99},
        {47,66,99,99,99,99,99,99},
        {99,99,99,99,99,99,99,99},
        {99,99,99,99,99,99,99,99},
        {99,99,99,99,99,99,99,99},
        {99,99,99,99,99,99,99,99}
    };

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
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        SetSourceTexture(tex);

        SourceImage.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );

        SelectedBlock = new Vector2Int(0, 0);
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

        texY.filterMode = FilterMode.Point;
        texCb.filterMode = FilterMode.Point;
        texCr.filterMode = FilterMode.Point;
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
                float value = SelectedChannel switch
                {
                    JpegChannel.Y => p.Y,
                    JpegChannel.Cb => p.Cb,
                    JpegChannel.Cr => p.Cr,
                    _ => 0
                };
                block[x, y] = value * 255f;
            }

        return block;
    }

    public Texture2D GetSelectedBlockTexture(int scale = 32)
    {
        float[,] block = GetSelectedBlock();

        float min = float.MaxValue;
        float max = float.MinValue;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                if (block[x, y] < min) min = block[x, y];
                if (block[x, y] > max) max = block[x, y];
            }

        Texture2D tex = new Texture2D(8 * scale, 8 * scale, TextureFormat.RGB24, false);

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int yy = y;
                int xx = x;
                float v = (block[x, y] - min) / (max - min);
                Color c = new Color(v, v, v);

                for (int sy = 0; sy < scale; sy++)
                    for (int sx = 0; sx < scale; sx++)
                        tex.SetPixel(xx * scale + sx, yy * scale + sy, c);
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
                        float pixel = block[x, y] - 128f;
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

        float min = float.MaxValue;
        float max = float.MinValue;

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                if (dct[x, y] < min) min = dct[x, y];
                if (dct[x, y] > max) max = dct[x, y];
            }

        Texture2D tex = new Texture2D(8 * scale, 8 * scale, TextureFormat.RGB24, false);

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int yy = 7 - y;

                float v = (dct[x, y] - min) / (max - min);
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

    public float[,] GetSelectedBlockQuantized()
    {
        float[,] block = GetSelectedBlock();
        float[,] dct = DCT(block);
        bool isLuma = SelectedChannel == JpegChannel.Y;
        int[,] Q = GetScaledQ(JpegQuality, isLuma);

        float[,] q = new float[8, 8];

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                q[x, y] = Mathf.Round(dct[x, y] / Q[y, x]);

        return q;
    }

    public int QualityToScale(int quality)
    {
        quality = Mathf.Clamp(quality, 1, 100);
        return quality < 50 ? 5000 / quality : 200 - quality * 2;
    }

    public int[,] GetScaledQ(int quality, bool luminance)
    {
        int[,] baseQ = luminance ? LumaQ : ChromaQ;
        int scale = QualityToScale(quality);
        int[,] q = new int[8, 8];

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int v = (baseQ[y, x] * scale + 50) / 100;
                q[y, x] = Mathf.Clamp(v, 1, 255);
            }

        return q;
    }

    public void UpdateQuantizationPanel()
    {
        float[,] block = GetSelectedBlock();
        float[,] dct = DCT(block);
        ShowQDCT(dct);

        bool isLuma = SelectedChannel == JpegChannel.Y;
        int[,] Q = GetScaledQ(JpegQuality, isLuma);

        float[,] qFloat = new float[8, 8];
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                qFloat[x, y] = Q[y, x];

        ShowMatrix(qFloat);

        ShowQuantized(GetSelectedBlockQuantized());
    }

    public void ShowMatrix(float[,] matrix)
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int index = y * 8 + x;
                int value = Mathf.RoundToInt(matrix[x, y]);
                QMatrixCells[index].text = value.ToString();
            }
    }

    public void ShowQDCT(float[,] matrix)
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int index = y * 8 + x;
                int value = Mathf.RoundToInt(matrix[x, y]);
                QDCTCells[index].text = value.ToString();
            }
    }

    public void ShowQuantized(float[,] matrix)
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int index = y * 8 + x;
                int value = Mathf.RoundToInt(matrix[x, y]);
                QuantizedCells[index].text = value.ToString();
            }
    }
}
