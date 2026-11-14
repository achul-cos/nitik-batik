using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class SpriteHoverOutline : MonoBehaviour
{
    public const float MaxOutlineWidth = 40f;

    [Header("Outline Settings")]
    [SerializeField]
    private Color outlineColor = Color.white;

    // Lebar dihitung dalam texel texture, jadi sprite beresolusi tinggi butuh nilai lebih besar agar terlihat tebal di layar.
    [SerializeField]
    [Range(0f, MaxOutlineWidth)]
    private float outlineWidth = 10f;

    [SerializeField]
    [Range(0f, 1f)]
    private float alphaThreshold = 0.51f;


    [Header("Initial State")]
    [SerializeField]
    private bool outlineEnabled = false;


    private SpriteRenderer spriteRenderer;

    private MaterialPropertyBlock propertyBlock;


    // Shader property IDs
    private static readonly int OutlineColor =
        Shader.PropertyToID("_OutlineColor");

    private static readonly int OutlineWidth =
        Shader.PropertyToID("_OutlineWidth");

    private static readonly int OutlineEnabled =
        Shader.PropertyToID("_OutlineEnabled");

    private static readonly int AlphaThreshold =
        Shader.PropertyToID("_AlphaThreshold");


    public Material materialOutline;

    public bool hoverOutline = true;

    private void Awake()
    {
        if (materialOutline == null)
        {
            materialOutline = Resources.Load<Material>("SpriteHoverOutline_Material");
        }

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        propertyBlock =
            new MaterialPropertyBlock();

        ApplyShaderProperties();
    }

    private void Start()
    {
        // [RequireComponent] menjamin SpriteRenderer dan BoxCollider2D sudah ada.
        // sharedMaterial dipakai (bukan material) agar tidak membuat salinan material untuk setiap sprite;
        // nilai outline per sprite sudah diatur lewat MaterialPropertyBlock.
        if (materialOutline != null) spriteRenderer.sharedMaterial = materialOutline;

        BoxCollider2D cl = GetComponent<BoxCollider2D>();
        cl.isTrigger = true;

        if (spriteRenderer.sprite != null) cl.size = spriteRenderer.sprite.bounds.size;
    }

    private void OnMouseEnter()
    {
        SetOutline(true);
    }


    private void OnMouseExit()
    {
        SetOutline(false);
    }


    private void SetOutline(bool enabled)
    {
        if (hoverOutline == true)
        {
            outlineEnabled = enabled;

            ApplyShaderProperties();
        }

    }


    private void ApplyShaderProperties()
    {
        if (spriteRenderer == null)
            return;


        // Ambil PropertyBlock yang sedang digunakan
        spriteRenderer.GetPropertyBlock(
            propertyBlock
        );


        // Outline Color
        propertyBlock.SetColor(
            OutlineColor,
            outlineColor
        );


        // Outline Width
        propertyBlock.SetFloat(
            OutlineWidth,
            outlineWidth
        );


        // Alpha Threshold
        propertyBlock.SetFloat(
            AlphaThreshold,
            alphaThreshold
        );


        // Outline Enabled
        propertyBlock.SetFloat(
            OutlineEnabled,
            outlineEnabled ? 1f : 0f
        );


        // Terapkan PropertyBlock
        spriteRenderer.SetPropertyBlock(
            propertyBlock
        );
    }


    // =========================================================
    // PUBLIC METHODS
    // Bisa dipanggil oleh script lain
    // =========================================================

    public void ShowOutline()
    {
        SetOutline(true);
    }


    public void HideOutline()
    {
        SetOutline(false);
    }

    /// <summary>
    /// Menyalakan/mematikan outline secara langsung, tanpa memedulikan <see cref="hoverOutline"/>.
    /// Dipakai skrip yang mengatur sendiri kapan outline boleh tampil (misalnya hanya untuk objek yang boleh dipilih).
    /// </summary>
    public void SetOutlineLangsung(bool aktif)
    {
        outlineEnabled = aktif;

        ApplyShaderProperties();
    }


    public void SetOutlineColor(Color color)
    {
        outlineColor = color;

        ApplyShaderProperties();
    }


    public void SetOutlineWidth(float width)
    {
        outlineWidth =
            Mathf.Clamp(
                width,
                0f,
                MaxOutlineWidth
            );

        ApplyShaderProperties();
    }


    public void SetAlphaThreshold(float threshold)
    {
        alphaThreshold =
            Mathf.Clamp01(
                threshold
            );

        ApplyShaderProperties();
    }


    // =========================================================
    // OPTIONAL
    // Untuk mengambil status outline
    // =========================================================

    public bool IsOutlineEnabled()
    {
        return outlineEnabled;
    }
}