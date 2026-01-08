using SFB;
using System.Collections.Generic;
using System.Linq;
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
    public Image DCTLegendImage;
    public TMPro.TMP_Text DCTMinLabel;
    public TMPro.TMP_Text DCTMaxLabel;
    public Transform QMatrixParent;
    public Transform QDCTParent;
    public Transform QuantizedParent;
    public Image QuantizedImage;
    public Image ZigZagMatrixPixelImage;
    public Image ZigZagArrayPixelImage;
    public Image LensImage;
    public Transform ZigZagMatrixNumberParent;
    public Transform ZigZagArrayNumberParent;
    public Transform RLEParent;
    public Transform HuffmanParent;
    public Transform HuffmanTreeParent;

    [Header("Other")]
    public Vector2Int SelectedBlock = new Vector2Int(0, 0);
    public const int BLOCK_SIZE = 8;
    public int Width, Height;
    public JpegChannel SelectedChannel = JpegChannel.Y;
    public int JpegQuality = 50;
    public TMP_Text[] QMatrixCells = new TMP_Text[64];
    public TMP_Text[] QDCTCells = new TMP_Text[64];
    public TMP_Text[] QuantizedCells = new TMP_Text[64];
    public TMP_Text[] ZigZagMatrixNumberCells = new TMP_Text[64];
    public TMP_Text[] ZigZagArrayNumberCells = new TMP_Text[64];
    public TMP_Text[] RLECells = new TMP_Text[64];
    public TMP_Text[] HuffmanCells = new TMP_Text[64];
    public TMP_Text FinalCodeText;

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
    Texture2D dctLegendTex;
    Texture2D zigZagMatrixTex;
    Texture2D zigZagArrayTex;
    Texture2D lensTex;
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
        float[,] dctBlock = new float[BLOCK_SIZE, BLOCK_SIZE];
        float sqrt2 = Mathf.Sqrt(2f);
        
        for (int u = 0; u < BLOCK_SIZE; u++)
        {
            for (int v = 0; v < BLOCK_SIZE; v++)
            {
                float cu = (u == 0) ? 1f / sqrt2 : 1f;
                float cv = (v == 0) ? 1f / sqrt2 : 1f;
                
                float sum = 0f;
                
                for (int x = 0; x < BLOCK_SIZE; x++)
                {
                    for (int y = 0; y < BLOCK_SIZE; y++)
                    {
                        float cosX = Mathf.Cos((2f * x + 1f) * u * Mathf.PI / (2f * BLOCK_SIZE));
                        float cosY = Mathf.Cos((2f * y + 1f) * v * Mathf.PI / (2f * BLOCK_SIZE));
                        sum += block[x, y] * cosX * cosY;
                    }
                }
                
                dctBlock[u, v] = 0.25f * cu * cv * sum;
            }
        }
        
        return dctBlock;
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
                int yy = y;

                float v = (dct[x, y] - min) / (max - min);
                v = Mathf.Clamp01(v);
                Color c = GetHeatmapColor(v);

                for (int sy = 0; sy < scale; sy++)
                    for (int sx = 0; sx < scale; sx++)
                        tex.SetPixel(x * scale + sx, (7 - yy) * scale + sy, c);
            }

        tex.Apply();
        return tex;
    }

    Color GetHeatmapColor(float t)
    {
        t = Mathf.Clamp01(t);
        
        if (t < 0.25f)
        {
            float localT = t / 0.25f;
            return Color.Lerp(new Color(0, 0, 1, 1), new Color(0, 1, 1, 1), localT);
        }
        else if (t < 0.5f)
        {
            float localT = (t - 0.25f) / 0.25f;
            return Color.Lerp(new Color(0, 1, 1, 1), new Color(0, 1, 0, 1), localT);
        }
        else if (t < 0.75f)
        {
            float localT = (t - 0.5f) / 0.25f;
            return Color.Lerp(new Color(0, 1, 0, 1), new Color(1, 1, 0, 1), localT);
        }
        else
        {
            float localT = (t - 0.75f) / 0.25f;
            return Color.Lerp(new Color(1, 1, 0, 1), new Color(1, 0, 0, 1), localT);
        }
    }

    public void RefreshDCTImage()
    {
        if (dctTex != null)
            Destroy(dctTex);

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

        dctTex = GetSelectedBlockDCTTexture();

        DCTImage.sprite = Sprite.Create(
            dctTex,
            new Rect(0, 0, dctTex.width, dctTex.height),
            new Vector2(0.5f, 0.5f)
        );

        if (DCTLegendImage != null)
        {
            if (dctLegendTex != null)
                Destroy(dctLegendTex);

            dctLegendTex = CreateHeatmapLegend(20, 200, min, max);
            DCTLegendImage.sprite = Sprite.Create(
                dctLegendTex,
                new Rect(0, 0, dctLegendTex.width, dctLegendTex.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        if (DCTMinLabel != null)
            DCTMinLabel.text = min.ToString("F1");

        if (DCTMaxLabel != null)
            DCTMaxLabel.text = max.ToString("F1");
    }

    Texture2D CreateHeatmapLegend(int width, int height, float min, float max)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            t = 1f - t;
            Color c = GetHeatmapColor(t);

            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return tex;
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

    public static Vector2Int[] GetZigZagOrder()
    {
        Vector2Int[] order = new Vector2Int[64];
        int index = 0;
        int x = 0, y = 0;
        bool goingUp = true;

        while (index < 64)
        {
            order[index] = new Vector2Int(x, y);
            index++;

            if (goingUp)
            {
                if (y == 0 || x == 7)
                {
                    goingUp = false;
                    if (x == 7)
                        y++;
                    else
                        x++;
                }
                else
                {
                    x++;
                    y--;
                }
            }
            else
            {
                if (x == 0 || y == 7)
                {
                    goingUp = true;
                    if (y == 7)
                        x++;
                    else
                        y++;
                }
                else
                {
                    x--;
                    y++;
                }
            }
        }

        return order;
    }

    public float[] ZigZagRead(float[,] matrix)
    {
        Vector2Int[] order = GetZigZagOrder();
        float[] result = new float[64];

        for (int i = 0; i < 64; i++)
        {
            result[i] = matrix[order[i].x, order[i].y];
        }

        return result;
    }

    public float[] GetSelectedBlockZigZag()
    {
        float[,] quantized = GetSelectedBlockQuantized();
        return ZigZagRead(quantized);
    }

    public Texture2D GetZigZagMatrixTexture(float[,] matrix, int step = -1, int scale = 32)
    {
        Texture2D tex = new Texture2D(8 * scale, 8 * scale, TextureFormat.RGB24, false);
        
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                if (matrix[x, y] < min) min = matrix[x, y];
                if (matrix[x, y] > max) max = matrix[x, y];
            }
        
        Vector2Int[] order = GetZigZagOrder();
        
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int yy = 7 - y;
                float v = (matrix[x, y] - min) / (max - min);
                v = Mathf.Clamp01(v);
                
                Color c = new Color(v, v, v);
                
                if (step >= 0)
                {
                    bool isInPath = false;
                    bool isCurrent = false;
                    
                    for (int j = 0; j <= step && j < 64; j++)
                    {
                        if (order[j].x == x && order[j].y == y)
                        {
                            isInPath = true;
                            if (j == step)
                                isCurrent = true;
                            break;
                        }
                    }
                    
                    if (isCurrent)
                        c = Color.yellow;
                    else if (isInPath)
                        c = Color.green;
                }
                
                for (int sy = 0; sy < scale; sy++)
                    for (int sx = 0; sx < scale; sx++)
                        tex.SetPixel(x * scale + sx, yy * scale + sy, c);
            }
        
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    public Texture2D GetZigZagArrayTexture(float[] array, int currentIndex = -1, int scale = 8)
    {
        int width = 64 * scale;
        int height = scale;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int i = 0; i < 64; i++)
        {
            if (array[i] < min) min = array[i];
            if (array[i] > max) max = array[i];
        }
        
        for (int i = 0; i < 64; i++)
        {
            float v = (array[i] - min) / (max - min);
            v = Mathf.Clamp01(v);
            
            Color c = new Color(v, v, v);
            
            if (currentIndex >= 0)
            {
                if (i == currentIndex)
                    c = Color.yellow;
                else if (i < currentIndex)
                    c = Color.green;
            }
            
            for (int sy = 0; sy < scale; sy++)
                for (int sx = 0; sx < scale; sx++)
                    tex.SetPixel(i * scale + sx, sy, c);
        }
        
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    public void RefreshZigZagMatrixImage(int step = -1)
    {
        if (zigZagMatrixTex != null)
            Destroy(zigZagMatrixTex);

        float[,] quantized = GetSelectedBlockQuantized();
        zigZagMatrixTex = GetZigZagMatrixTexture(quantized, step);

        if (ZigZagMatrixPixelImage != null)
        {
            ZigZagMatrixPixelImage.sprite = Sprite.Create(
                zigZagMatrixTex,
                new Rect(0, 0, zigZagMatrixTex.width, zigZagMatrixTex.height),
                new Vector2(0.5f, 0.5f)
            );
        }
        
        ShowZigZagMatrixNumbers(quantized);
    }

    public void RefreshZigZagArrayImage(int currentIndex = -1)
    {
        if (zigZagArrayTex != null)
            Destroy(zigZagArrayTex);

        float[] zigzagArray = GetSelectedBlockZigZag();
        zigZagArrayTex = GetZigZagArrayTexture(zigzagArray, currentIndex);

        if (ZigZagArrayPixelImage != null)
        {
            ZigZagArrayPixelImage.sprite = Sprite.Create(
                zigZagArrayTex,
                new Rect(0, 0, zigZagArrayTex.width, zigZagArrayTex.height),
                new Vector2(0.5f, 0.5f)
            );
        }
        
        ShowZigZagArrayNumbers(zigzagArray);
    }

    public void ShowZigZagMatrixNumbers(float[,] matrix)
    {
        if (ZigZagMatrixNumberCells == null || ZigZagMatrixNumberCells[0] == null)
            return;

        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int index = y * 8 + x;
                if (ZigZagMatrixNumberCells[index] != null)
                {
                    int value = Mathf.RoundToInt(matrix[x, y]);
                    ZigZagMatrixNumberCells[index].text = value.ToString();
                    ZigZagMatrixNumberCells[index].color = GetZigZagCellColor(index);
                }
            }
    }

    private Color GetZigZagCellColor(int index) {
        Vector2Int[] order = GetZigZagOrder();
        
        for (int i = 0; i < 64; i++)
        {
            if (order[i].y * 8 + order[i].x == index)
            {
                float hue = i / 63f;
                return Color.HSVToRGB(hue, 1f, 0.6f);
            }
        }
        
        return Color.white;
    }

    public void ShowZigZagArrayNumbers(float[] array)
    {
        if (ZigZagArrayNumberCells == null || ZigZagArrayNumberCells[0] == null)
            return;

        for (int i = 0; i < 64; i++)
        {
            if (ZigZagArrayNumberCells[i] != null)
            {
                int value = Mathf.RoundToInt(array[i]);
                ZigZagArrayNumberCells[i].text = value.ToString();
                float hue = i / 63f;
                ZigZagArrayNumberCells[i].color = Color.HSVToRGB(hue, 1f, 0.6f);
            }
        }
    }


    public void UpdateZigZagPanel()
    {
        RefreshZigZagMatrixImage();
        RefreshZigZagArrayImage();
    }

    class HuffmanNode
    {
        public int? value;
        public string rleKey;
        public int frequency;
        public HuffmanNode left;
        public HuffmanNode right;
        public string code = "";

        public bool IsLeaf => value.HasValue || !string.IsNullOrEmpty(rleKey);
    }

    public struct RLEPair
    {
        public int run;
        public int value;
        
        public RLEPair(int r, int v)
        {
            run = r;
            value = v;
        }
    }

    List<RLEPair> rleData = new List<RLEPair>();
    Dictionary<string, string> huffmanCodes = new Dictionary<string, string>();
    HuffmanNode huffmanRoot;

    public void UpdateHuffmanPanel()
    {
        float[] zigzagArray = GetSelectedBlockZigZag();
        rleData = RLEEncode(zigzagArray);
        ShowRLEData(rleData);
        BuildHuffmanTree(rleData);
        ShowHuffmanData(rleData);
        VisualizeHuffmanTree();
        ShowFinalCode();
    }

    List<RLEPair> RLEEncode(float[] array)
    {
        List<RLEPair> rle = new List<RLEPair>();
        int run = 0;
        
        for (int i = 0; i < array.Length; i++)
        {
            int value = Mathf.RoundToInt(array[i]);
            
            if (value == 0)
            {
                run++;
            }
            else
            {
                rle.Add(new RLEPair(run, value));
                run = 0;
            }
        }
        
        if (rle.Count == 0 || run > 0)
        {
            rle.Add(new RLEPair(run, 0));
        }
        
        return rle;
    }

    public void ShowRLEData(List<RLEPair> rle)
    {
        if (RLECells == null || RLECells[0] == null)
            return;

        for (int i = 0; i < 64 && i < RLECells.Length; i++)
        {
            if (RLECells[i] != null)
            {
                if (i < rle.Count)
                {
                    RLECells[i].text = $"({rle[i].run},{rle[i].value})";
                }
                else
                {
                    RLECells[i].text = "";
                }
            }
        }
    }

    void BuildHuffmanTree(List<RLEPair> rle)
    {
        Dictionary<string, int> frequencies = new Dictionary<string, int>();
        
        for (int i = 0; i < rle.Count; i++)
        {
            string key = $"({rle[i].run},{rle[i].value})";
            if (frequencies.ContainsKey(key))
                frequencies[key]++;
            else
                frequencies[key] = 1;
        }

        List<HuffmanNode> nodes = new List<HuffmanNode>();
        foreach (var kvp in frequencies)
        {
            nodes.Add(new HuffmanNode { rleKey = kvp.Key, frequency = kvp.Value });
        }

        if (nodes.Count == 0) return;

        while (nodes.Count > 1)
        {
            nodes = nodes.OrderBy(n => n.frequency).ToList();
            
            HuffmanNode left = nodes[0];
            HuffmanNode right = nodes[1];
            
            HuffmanNode parent = new HuffmanNode
            {
                frequency = left.frequency + right.frequency,
                left = left,
                right = right
            };
            
            nodes.RemoveAt(0);
            nodes.RemoveAt(0);
            nodes.Add(parent);
        }

        huffmanRoot = nodes[0];
        huffmanCodes.Clear();
        BuildCodes(huffmanRoot, "");
    }

    void BuildCodes(HuffmanNode node, string code)
    {
        if (node == null) return;
        
        node.code = code;
        
        if (node.IsLeaf)
        {
            if (node.value.HasValue)
            {
                huffmanCodes[node.value.Value.ToString()] = code;
            }
            else if (!string.IsNullOrEmpty(node.rleKey))
            {
                huffmanCodes[node.rleKey] = code;
            }
        }
        else
        {
            BuildCodes(node.left, code + "0");
            BuildCodes(node.right, code + "1");
        }
    }

    public void ShowHuffmanData(List<RLEPair> rle)
    {
        if (HuffmanCells == null || HuffmanCells[0] == null)
            return;

        for (int i = 0; i < 64 && i < HuffmanCells.Length; i++)
        {
            if (HuffmanCells[i] != null)
            {
                if (i < rle.Count)
                {
                    string key = $"({rle[i].run},{rle[i].value})";
                    if (huffmanCodes.ContainsKey(key))
                    {
                        HuffmanCells[i].text = huffmanCodes[key];
                    }
                    else
                    {
                        HuffmanCells[i].text = "";
                    }
                }
                else
                {
                    HuffmanCells[i].text = "";
                }
            }
        }
    }

    public string GenerateFinalCode()
    {
        if (rleData == null || rleData.Count == 0)
            return "";

        System.Text.StringBuilder bitstream = new System.Text.StringBuilder();

        for (int i = 0; i < rleData.Count; i++)
        {
            string key = $"({rleData[i].run},{rleData[i].value})";
            if (huffmanCodes.ContainsKey(key))
            {
                bitstream.Append(huffmanCodes[key]);
            }
        }

        string eobKey = "(0,0)";
        if (huffmanCodes.ContainsKey(eobKey))
        {
            bitstream.Append(huffmanCodes[eobKey]);
        }
        else
        {
            bitstream.Append("EOB");
        }

        return bitstream.ToString();
    }

    public void ShowFinalCode()
    {
        if (FinalCodeText == null)
            return;

        string finalCode = GenerateFinalCode();
        FinalCodeText.text = finalCode;
    }

    public void VisualizeHuffmanTree()
    {
        if (HuffmanTreeParent == null || huffmanRoot == null) return;

        foreach (Transform child in HuffmanTreeParent)
        {
            Destroy(child.gameObject);
        }

        RectTransform containerRect = HuffmanTreeParent.GetComponent<RectTransform>();
        if (containerRect == null) return;

        float containerWidth = containerRect.rect.width;
        if (containerWidth <= 0) containerWidth = 800f;

        int maxDepth = GetTreeDepth(huffmanRoot);
        bool isCompact = maxDepth > 5;

        if (huffmanRoot.IsLeaf)
        {
            CreateTreeNode(huffmanRoot, HuffmanTreeParent, new Vector2(0, containerRect.rect.height * 0.5f - 100f), 0, isCompact);
        }
        else
        {
            float rootY = containerRect.rect.height * 0.5f - 100f;
            DrawTree(huffmanRoot, HuffmanTreeParent, new Vector2(0, rootY), containerWidth, 0, isCompact);
        }
    }

    int CountLeafNodes(HuffmanNode node)
    {
        if (node == null) return 0;
        if (node.IsLeaf) return 1;
        return CountLeafNodes(node.left) + CountLeafNodes(node.right);
    }

    int GetTreeDepth(HuffmanNode node)
    {
        if (node == null) return 0;
        if (node.IsLeaf) return 1;
        return 1 + Mathf.Max(GetTreeDepth(node.left), GetTreeDepth(node.right));
    }

    void DrawTree(HuffmanNode node, Transform parent, Vector2 position, float containerWidth, int depth, bool isCompact = false)
    {
        if (node == null) return;

        if (!node.IsLeaf)
        {
            float verticalSpacing = isCompact ? 60f : 90f;
            float childContainerWidth = containerWidth * 0.5f;
            float yOffset = -verticalSpacing;

            Vector2? leftPos = null;
            Vector2? rightPos = null;

            if (node.left != null)
            {
                leftPos = position + new Vector2(-containerWidth * 0.25f, yOffset);
            }

            if (node.right != null)
            {
                rightPos = position + new Vector2(containerWidth * 0.25f, yOffset);
            }

            if (leftPos.HasValue)
            {
                DrawLine(position, leftPos.Value, parent);
                CreateEdgeLabel("0", position, leftPos.Value, parent, isCompact);
            }

            if (rightPos.HasValue)
            {
                DrawLine(position, rightPos.Value, parent);
                CreateEdgeLabel("1", position, rightPos.Value, parent, isCompact);
            }

            CreateTreeNode(node, parent, position, depth, isCompact);

            if (node.left != null && leftPos.HasValue)
            {
                DrawTree(node.left, parent, leftPos.Value, childContainerWidth, depth + 1, isCompact);
            }

            if (node.right != null && rightPos.HasValue)
            {
                DrawTree(node.right, parent, rightPos.Value, childContainerWidth, depth + 1, isCompact);
            }
        }
        else
        {
            CreateTreeNode(node, parent, position, depth, isCompact);
        }
    }

    void CreateEdgeLabel(string label, Vector2 start, Vector2 end, Transform parent, bool isCompact = false)
    {
        GameObject labelObj = new GameObject("EdgeLabel");
        labelObj.transform.SetParent(parent, false);

        float fontSize = isCompact ? 10f : 16f;
        float labelSize = isCompact ? 18f : 25f;
        float offset = isCompact ? -10f : -15f;

        RectTransform rect = labelObj.AddComponent<RectTransform>();
        Vector2 midPoint = (start + end) * 0.5f + new Vector2(0, offset);
        rect.anchoredPosition = midPoint;
        rect.sizeDelta = new Vector2(labelSize, labelSize);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        TMP_Text text = labelObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.red;
        text.fontStyle = FontStyles.Bold;
    }

    Texture2D circleSprite;

    Texture2D GetCircleSprite(int size)
    {
        if (circleSprite != null && circleSprite.width == size) return circleSprite;

        if (circleSprite != null) Destroy(circleSprite);

        circleSprite = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float radius = size * 0.5f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    circleSprite.SetPixel(x, y, Color.white);
                }
                else
                {
                    circleSprite.SetPixel(x, y, Color.clear);
                }
            }
        }

        circleSprite.Apply();
        return circleSprite;
    }

    void CreateTreeNode(HuffmanNode node, Transform parent, Vector2 position, int depth, bool isCompact = false)
    {
        GameObject nodeObj = new GameObject("HuffmanNode");
        nodeObj.transform.SetParent(parent, false);
        
        float nodeSize = isCompact ? 50f : 80f;
        float fontSize = isCompact ? 12f : 18f;
        
        RectTransform rect = nodeObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(nodeSize, nodeSize);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        Image bg = nodeObj.AddComponent<Image>();
        bg.sprite = Sprite.Create(GetCircleSprite(Mathf.RoundToInt(nodeSize)), new Rect(0, 0, nodeSize, nodeSize), new Vector2(0.5f, 0.5f));
        bg.color = node.IsLeaf ? new Color(0.2f, 0.6f, 0.9f) : new Color(0.8f, 0.8f, 0.8f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(nodeObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
        if (node.IsLeaf)
        {
            if (node.value.HasValue)
            {
                text.text = node.value.Value.ToString() + "\n" + node.frequency;
            }
            else if (!string.IsNullOrEmpty(node.rleKey))
            {
                text.text = node.rleKey + "\n" + node.frequency;
            }
            else
            {
                text.text = node.frequency.ToString();
            }
        }
        else
        {
            text.text = node.frequency.ToString();
        }
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;
    }

    void DrawLine(Vector2 start, Vector2 end, Transform parent)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(parent, false);

        RectTransform rect = lineObj.AddComponent<RectTransform>();
        Vector2 direction = end - start;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rect.anchoredPosition = (start + end) * 0.5f;
        rect.sizeDelta = new Vector2(distance, 2f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.localEulerAngles = new Vector3(0, 0, angle);

        Image line = lineObj.AddComponent<Image>();
        line.color = Color.black;
    }

    public void UpdateLensBlock(int blockX, int blockY, Vector2 screenPosition)
    {
        if (sourceTexture == null || LensImage == null || ycbcr == null)
            return;

        blockX = Mathf.Clamp(blockX, 0, Width / BLOCK_SIZE - 1);
        blockY = Mathf.Clamp(blockY, 0, Height / BLOCK_SIZE - 1);

        int sx = blockX * BLOCK_SIZE;
        int sy = blockY * BLOCK_SIZE;

        float[,] block = new float[8, 8];
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

        float min = float.MaxValue;
        float max = float.MinValue;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                if (block[x, y] < min) min = block[x, y];
                if (block[x, y] > max) max = block[x, y];
            }

        int scale = 32;
        int borderWidth = 2;

        if (lensTex != null)
            Destroy(lensTex);

        lensTex = new Texture2D(8 * scale + borderWidth * 2, 8 * scale + borderWidth * 2, TextureFormat.RGB24, false);

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                float v = (block[x, y] - min) / (max - min);
                Color c = new Color(v, v, v);

                for (int scaleY = 0; scaleY < scale; scaleY++)
                {
                    for (int scaleX = 0; scaleX < scale; scaleX++)
                    {
                        int texX = borderWidth + x * scale + scaleX;
                        int texY = borderWidth + (7 - y) * scale + scaleY;
                        lensTex.SetPixel(texX, texY, c);
                    }
                }
            }
        }

        for (int i = 0; i < borderWidth; i++)
        {
            for (int j = 0; j < 8 * scale + borderWidth * 2; j++)
            {
                lensTex.SetPixel(j, i, Color.yellow);
                lensTex.SetPixel(j, 8 * scale + borderWidth * 2 - 1 - i, Color.yellow);
            }
            for (int j = 0; j < 8 * scale + borderWidth * 2; j++)
            {
                lensTex.SetPixel(i, j, Color.yellow);
                lensTex.SetPixel(8 * scale + borderWidth * 2 - 1 - i, j, Color.yellow);
            }
        }

        lensTex.filterMode = FilterMode.Point;
        lensTex.Apply();

        LensImage.enabled = true;
        LensImage.sprite = Sprite.Create(
            lensTex,
            new Rect(0, 0, lensTex.width, lensTex.height),
            new Vector2(0.5f, 0.5f)
        );

        RectTransform lensRect = LensImage.rectTransform;
        RectTransform canvasRect = lensRect.root as RectTransform;
        
        if (canvasRect != null)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                null,
                out localPoint))
            {
                Vector2 offset = new Vector2(150f, 150f);
                lensRect.anchoredPosition = localPoint + offset;
            }
        }
    }

    public void HideLens()
    {
        if (LensImage != null)
        {
            LensImage.enabled = false;
            LensImage.sprite = null;
        }
        if (lensTex != null)
        {
            Destroy(lensTex);
            lensTex = null;
        }
    }
}

