using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Object data yang mendefinisikan objek customer di dalam game,
/// yang menyimpan data-data variabel dari customer.
/// </summary>
[CreateAssetMenu(fileName = "Customer", menuName = "AksaraBatik/Customer")]
public class Customer : ScriptableObject
{
    // Id atau kode identifier dari customer
    public string customerId;

    // Nama dari customer
    public string customerName;

    // Deskripsi dari customer
    [TextArea(3, 12)] public string customerDesc = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque sit amet orci venenatis, blandit tellus sed, pretium nisl. Aenean semper, neque in ultricies mattis, quam sapien sodales lectus, sit amet sodales tellus tortor ac est.";

    // Dialog secara utuh dari customer
    public string customerDialog;

    // List dialog dari customer
    public List<string> customerDialogList = new List<string>()
    {
        "Ini adalah contoh dari baris pertama dari dialog",
        "Selanjutnya ini adalah baris kedua dari dialog karakter ini",
        "Ketigalah ini baris dialog sekarang",
        "Empat adalah baris dialog yang biasanya menjadi sebelum dialog penutup",
        "Dan ini baris terakhir dari dialog placehoder dan ini baris kelima."
    };

    // foto potrait dari customer
    [ShowAssetPreview(1024, 1024)] public Texture2D customerImage;

    // sprite dari customer
    [ShowAssetPreview(1024, 1024)] public Sprite customerSprite;

    // foto potrait dari customer pose side
    [ShowAssetPreview(1024, 1024)] public Texture2D customerSideImage;

    // sprite dari customer pose side
    [ShowAssetPreview(1024, 1024)] public Sprite customerSideSprite;

    [ShowAssetPreview(1024, 1024)] public Texture2D customerSitImage;

    [ShowAssetPreview(1024, 1024)] public Sprite customerSitSprite;

    [ShowAssetPreview(1024, 1024)] public Texture2D customerHappyEkspresImage;

    [ShowAssetPreview(1024, 1024)] public Sprite customerHappyEkspresSprite;

    [ShowAssetPreview(1024, 1024)] public Texture2D customerSadEkspresImage;

    [ShowAssetPreview(1024, 1024)] public Sprite customerSadEkspresSprite;

    [ShowAssetPreview(1024, 1024)] public Sprite[] customerWalkSprites;

    // Kain yang dinginkan oleh customer
    public Fabric customerFabric;

    // Batik yang diinginkan oleh customer
    public Batik customerBatik;

    // Tipe customer berdasarkan kekayaan
    public WealthType customerWealth;

    // Tipe customer berdasarkan perfeksionis
    public PerfectType customerPerfect;

    // Warna batik yang ia inginkan
    public BatikcolorType customerBatikColor;
}