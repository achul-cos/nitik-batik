using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel dialog antara pemain dan customer di Lobby.
/// Menampilkan nama, foto, dan baris dialog (efek ketik) dan memberi tahu pendengar ketika semua baris sudah dibaca.
/// Panel ini diaktifkan oleh <see cref="Lobby"/>, jadi objeknya boleh nonaktif di scene.
/// </summary>
public class DialogPanel : MonoBehaviour
{
    [Header("Komponen UI")]
    public RawImage charaImage;
    public TextMeshProUGUI charaName;
    public TextMeshProUGUI charaDialog;
    public Button tombolLanjut;

    [Header("Pengaturan")]
    public float kecepatanKetik = 0.05f;

    /// <summary>Dipicu ketika tombol lanjut ditekan setelah baris dialog terakhir.</summary>
    public event Action DialogSelesai;

    private Customer _customer;
    private int _indexDialog;
    private Coroutine _ketikCoroutine;
    private Coroutine _jedaTombolCoroutine;
    private CanvasGroup _tombolCanvasGroup;
    private bool _sudahTerhubung;

    private void Awake()
    {
        Hubungkan();
    }

    private void OnDestroy()
    {
        if (tombolLanjut != null) tombolLanjut.onClick.RemoveListener(Lanjut);
        _sudahTerhubung = false;
    }

    private void Hubungkan()
    {
        if (_sudahTerhubung || tombolLanjut == null) return;

        tombolLanjut.onClick.AddListener(Lanjut);
        _tombolCanvasGroup = tombolLanjut.GetComponent<CanvasGroup>();
        _sudahTerhubung = true;
    }

    /// <summary>
    /// Membuka panel untuk customer dan langsung menampilkan baris dialog pertama.
    /// </summary>
    public void Tampilkan(Customer customer)
    {
        gameObject.SetActive(true);
        Hubungkan();

        _customer = customer;
        _indexDialog = 0;

        if (customer == null) return;

        if (charaName != null) charaName.text = customer.customerName;
        if (charaImage != null) charaImage.texture = customer.customerImage;

        Lanjut();
    }

    /// <summary>
    /// Menampilkan baris dialog berikutnya. Setelah baris terakhir, memicu <see cref="DialogSelesai"/>.
    /// </summary>
    public void Lanjut()
    {
        if (_customer == null) return;

        var daftarDialog = _customer.customerDialogList;

        if (daftarDialog == null || _indexDialog >= daftarDialog.Count)
        {
            _indexDialog = 0;
            DialogSelesai?.Invoke();
            return;
        }

        string baris = daftarDialog[_indexDialog];
        _indexDialog++;

        charaDialog.text = baris;

        if (_ketikCoroutine != null) StopCoroutine(_ketikCoroutine);
        _ketikCoroutine = StartCoroutine(KetikTeks(charaDialog));

        if (tombolLanjut != null)
        {
            if (_jedaTombolCoroutine != null) StopCoroutine(_jedaTombolCoroutine);
            _jedaTombolCoroutine = StartCoroutine(JedaTombolLanjut(baris));
        }
    }

    private IEnumerator KetikTeks(TextMeshProUGUI teks)
    {
        teks.ForceMeshUpdate();
        int jumlahKarakter = teks.textInfo.characterCount;

        teks.maxVisibleCharacters = 0;

        for (int i = 1; i <= jumlahKarakter; i++)
        {
            teks.maxVisibleCharacters = i;
            yield return new WaitForSeconds(kecepatanKetik);
        }

        _ketikCoroutine = null;
    }

    private IEnumerator JedaTombolLanjut(string baris)
    {
        tombolLanjut.interactable = false;
        if (_tombolCanvasGroup != null) _tombolCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutCubic).SetLink(tombolLanjut.gameObject);

        yield return new WaitForSeconds(kecepatanKetik * baris.Length);

        tombolLanjut.interactable = true;
        if (_tombolCanvasGroup != null) _tombolCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutCubic).SetLink(tombolLanjut.gameObject);

        _jedaTombolCoroutine = null;
    }
}
