using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Texture tempat pemain menggambar. Piksel ditulis ke buffer lalu diunggah ke texture sekali per goresan.
/// Titik-titik di antara dua posisi mouse diisi agar garis tidak putus saat mouse digerakkan cepat.
/// </summary>
public class PaintTexture : MonoBehaviour
{
    private static readonly Color32 WarnaLatar = new Color32(255, 238, 229, 255);

    [Header("Brush")]
    [SerializeField] private Color brushColor = Color.black;

    public int brushRadius = 5;

    public PatternEvaluator patternEvaluator;

    private Texture2D drawingTexture;

    private RawImage targetImage;

    private Color32[] _buffer;
    private Color32 _warnaKuas;

    private int width;
    private int height;

    // Titik terakhir pada goresan yang sedang berjalan (untuk menyambung garis)
    private bool _adaTitikSebelumnya;
    private int _terakhirX;
    private int _terakhirY;

    public void Initialize(RawImage image, int textureWidth, int textureHeight)
    {
        targetImage = image;

        width = textureWidth;
        height = textureHeight;

        _warnaKuas = brushColor;
        _buffer = new Color32[width * height];

        drawingTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Clear();

        targetImage.texture = drawingTexture;
    }

    public void Clear()
    {
        for (int i = 0; i < _buffer.Length; i++)
        {
            _buffer[i] = WarnaLatar;
        }

        drawingTexture.SetPixels32(_buffer);
        drawingTexture.Apply();

        _adaTitikSebelumnya = false;
    }

    /// <summary>
    /// Dipanggil saat mouse dilepas, supaya goresan berikutnya tidak tersambung ke goresan sebelumnya.
    /// </summary>
    public void AngkatKuas()
    {
        _adaTitikSebelumnya = false;
    }

    public void PaintNormalized(float x, float y)
    {
        int pixelX = Mathf.RoundToInt(x * width);
        int pixelY = Mathf.RoundToInt(y * height);

        if (_adaTitikSebelumnya)
        {
            LukisGaris(_terakhirX, _terakhirY, pixelX, pixelY);
        }
        else
        {
            DrawCircle(pixelX, pixelY);
        }

        _terakhirX = pixelX;
        _terakhirY = pixelY;
        _adaTitikSebelumnya = true;

        drawingTexture.SetPixels32(_buffer);
        drawingTexture.Apply();
    }

    // Melukis lingkaran di sepanjang garis dari titik awal ke titik akhir, berjarak setengah radius.
    private void LukisGaris(int awalX, int awalY, int akhirX, int akhirY)
    {
        float jarak = Vector2.Distance(new Vector2(awalX, awalY), new Vector2(akhirX, akhirY));

        int langkah = Mathf.Max(1, Mathf.CeilToInt(jarak / Mathf.Max(1f, brushRadius * 0.5f)));

        for (int i = 1; i <= langkah; i++)
        {
            float t = (float)i / langkah;

            DrawCircle(Mathf.RoundToInt(Mathf.Lerp(awalX, akhirX, t)), Mathf.RoundToInt(Mathf.Lerp(awalY, akhirY, t)));
        }
    }

    private void DrawCircle(int centerX, int centerY)
    {
        for (int x = -brushRadius; x <= brushRadius; x++)
        {
            for (int y = -brushRadius; y <= brushRadius; y++)
            {
                if (x * x + y * y > brushRadius * brushRadius) continue;

                int px = centerX + x;
                int py = centerY + y;

                if (px < 0 || px >= width) continue;

                if (py < 0 || py >= height) continue;

                _buffer[py * width + px] = _warnaKuas;

                patternEvaluator.CheckPixel(px, py);
            }
        }
    }
}
