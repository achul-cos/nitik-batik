using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menilai seberapa tepat goresan pemain mengikuti pola batik.
/// Pola dibaca sekali menjadi mask; setiap piksel yang dicat dicocokkan dengan mask itu.
/// Tampilan progres hanya diperbarui sekali per frame, bukan per piksel.
/// </summary>
public class PatternEvaluator : MonoBehaviour
{
    // Piksel pola dianggap "perlu dicat" jika alpha-nya di atas 0,1 (26 dari 255).
    private const byte AmbangAlphaPola = 25;

    [SerializeField] private Texture2D patternTexture;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Slider mistakeBar;
    [SerializeField] private Button NextButton;

    private int _lebar;
    private int _tinggi;

    private bool[] _maskPola;
    private bool[] _sudahTertutup;
    private bool[] _sudahSalah;

    private TextMeshProUGUI _labelProgress;
    private bool _perluPerbaruiTampilan;

    public int totalPatternPixel;
    public int coveredPatternPixel;
    public int mistakePixel;

    private void Start()
    {
        _lebar = patternTexture.width;
        _tinggi = patternTexture.height;

        // GetPixels32 mengambil seluruh piksel sekaligus (urutan: baris demi baris dari bawah), jauh lebih cepat dari GetPixel per piksel.
        Color32[] piksel = patternTexture.GetPixels32();

        _maskPola = new bool[piksel.Length];
        _sudahTertutup = new bool[piksel.Length];
        _sudahSalah = new bool[piksel.Length];

        for (int i = 0; i < piksel.Length; i++)
        {
            if (piksel[i].a > AmbangAlphaPola)
            {
                _maskPola[i] = true;
                totalPatternPixel++;
            }
        }

        if (progressBar != null)
        {
            progressBar.value = 0;

            _labelProgress = progressBar.GetComponentInChildren<TextMeshProUGUI>();
            if (_labelProgress != null) _labelProgress.text = "0%";
        }

        if (mistakeBar != null)
        {
            mistakeBar.value = 0;

            TextMeshProUGUI labelMistake = mistakeBar.GetComponentInChildren<TextMeshProUGUI>();
            if (labelMistake != null) labelMistake.text = "0%";
        }

        if (NextButton != null) NextButton.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!_perluPerbaruiTampilan) return;

        _perluPerbaruiTampilan = false;
        PerbaruiTampilan();
    }

    public void CheckPixel(int x, int y)
    {
        // Abaikan piksel di luar texture
        if (x < 0 || x >= _lebar || y < 0 || y >= _tinggi)
        {
            return;
        }

        int index = y * _lebar + x;

        if (_maskPola[index])
        {
            if (!_sudahTertutup[index])
            {
                _sudahTertutup[index] = true;
                coveredPatternPixel++;
                _perluPerbaruiTampilan = true;
            }
        }
        else
        {
            if (!_sudahSalah[index])
            {
                _sudahSalah[index] = true;
                mistakePixel++;
                _perluPerbaruiTampilan = true;
            }
        }
    }

    private void PerbaruiTampilan()
    {
        float coverage = GetCoverage();

        if (progressBar != null)
        {
            progressBar.value = coverage;

            if (_labelProgress != null) _labelProgress.text = $"{Mathf.RoundToInt(coverage * 100)}%";
        }

        if (mistakeBar != null && totalPatternPixel > 0)
        {
            mistakeBar.value = Mathf.Clamp01((float)(mistakePixel - coveredPatternPixel) / totalPatternPixel);
        }

        if (coverage >= 0.7f)
        {
            if (NextButton != null) NextButton.gameObject.SetActive(true);
        }
    }

    public float GetCoverage()
    {
        if (totalPatternPixel <= 0) return 0f;

        return Mathf.Clamp01(((float)coveredPatternPixel / totalPatternPixel) * 1.2f);
    }

    public int GetMistake()
    {
        return mistakePixel;
    }

    public float GetScore()
    {
        if (totalPatternPixel <= 0) return 0f;

        float coverage = (float)coveredPatternPixel / totalPatternPixel;

        float penalty = (float)mistakePixel / totalPatternPixel;

        return Mathf.Clamp01(coverage - penalty);
    }
}
