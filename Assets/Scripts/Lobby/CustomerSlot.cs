using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Satu customer yang terlihat di Lobby: berjalan masuk, duduk di kursi, dan berjalan keluar.
/// Customer yang boleh dipilih (paling depan antrean dan sudah duduk) menyala outline-nya saat kursor di atasnya,
/// dan memicu <see cref="Dipilih"/> saat diklik.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteHoverOutline))]
public class CustomerSlot : MonoBehaviour
{
    private const float PpuDiam = 350f;
    private const float PpuJalan = 220f;

    // Tampilan outline penanda "boleh dipilih" (lebar dalam texel texture)
    private static readonly Color WarnaOutlinePilihan = new Color(1f, 0.85f, 0.25f, 1f);
    private const float LebarOutlinePilihan = 24f;

    [Header("Setting")]
    public float moveSpeed = 3f;
    public float animationSpeed = 0.25f;
    public bool isMoving = false;

    public Customer Customer { get; private set; }

    /// <summary>True jika customer ini sedang boleh dipilih pemain.</summary>
    public bool BisaDipilih => _bisaDipilih;

    /// <summary>Dipicu ketika pemain mengklik customer ini saat boleh dipilih.</summary>
    public event Action<CustomerSlot> Dipilih;

    /// <summary>Dipicu ketika customer selesai berjalan dan duduk di kursinya.</summary>
    public event Action<CustomerSlot> SelesaiBergerak;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D _collider;
    private SpriteHoverOutline _outline;

    private Coroutine animationCoroutine;
    private Coroutine gerakCoroutine;
    private Tween tweenGerak;
    private Vector3 targetTujuan;

    private bool _bisaDipilih;
    private bool _outlineMenyala;

    // Sprite hasil Sprite.Create disimpan supaya tidak dibuat ulang tiap frame animasi, dan dibersihkan saat slot dihancurkan.
    private readonly Dictionary<(Sprite, float), Sprite> cacheSprite = new();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;

        // Outline dikendalikan oleh slot ini (hanya menyala jika boleh dipilih), bukan oleh hover bawaan
        _outline = GetComponent<SpriteHoverOutline>();
        _outline.hoverOutline = false;
        _outline.SetOutlineColor(WarnaOutlinePilihan);
        _outline.SetOutlineWidth(LebarOutlinePilihan);
    }

    private void OnDestroy()
    {
        tweenGerak?.Kill();

        foreach (Sprite sprite in cacheSprite.Values)
        {
            if (sprite != null) Destroy(sprite);
        }

        cacheSprite.Clear();
    }

    #region Setup

    public void SetCustomer(Customer customer)
    {
        Customer = customer;

        if (customer == null)
        {
            spriteRenderer.sprite = null;
            return;
        }

        SetSprite(customer.customerSitSprite);
    }

    #endregion

    #region Memilih customer

    /// <summary>
    /// Mengatur apakah customer ini boleh dipilih. Jika tidak lagi boleh, outline langsung dimatikan.
    /// </summary>
    public void SetBisaDipilih(bool bisa)
    {
        if (_bisaDipilih == bisa) return;

        _bisaDipilih = bisa;

        if (!bisa) AturOutline(false);
    }

    // Dipanggil Unity setiap frame selama kursor berada di atas collider
    private void OnMouseOver()
    {
        if (!_bisaDipilih) return;

        AturOutline(!KursorDiAtasUI());
    }

    private void OnMouseExit()
    {
        AturOutline(false);
    }

    private void OnMouseDown()
    {
        if (!_bisaDipilih || KursorDiAtasUI()) return;

        Dipilih?.Invoke(this);
    }

    private void AturOutline(bool menyala)
    {
        if (_outlineMenyala == menyala) return;

        _outlineMenyala = menyala;
        _outline.SetOutlineLangsung(menyala);
    }

    // Event mouse fisika tidak terhalang UI, jadi kita periksa sendiri agar klik pada panel tidak "tembus" ke customer.
    private static bool KursorDiAtasUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    #endregion

    #region Animation

    void SetSprite(Sprite sprite, float ppu = PpuDiam)
    {
        if (sprite == null) return;

        if (!cacheSprite.TryGetValue((sprite, ppu), out Sprite hasil) || hasil == null)
        {
            // FullRect: mesh sprite berbentuk persegi penuh supaya outline bisa tergambar di luar tepi gambar
            hasil = Sprite.Create(sprite.texture, sprite.rect, sprite.pivot / sprite.rect.size, ppu, 0, SpriteMeshType.FullRect);
            cacheSprite[(sprite, ppu)] = hasil;
        }

        spriteRenderer.sprite = hasil;

        // Area klik mengikuti ukuran sprite yang sedang tampil
        _collider.size = hasil.bounds.size;
        _collider.offset = hasil.bounds.center;
    }

    public void Stand()
    {
        if (Customer == null) return;

        StopAnimation();

        SetSprite(Customer.customerSprite);
    }

    public void Sit()
    {
        if (Customer == null) return;

        StopAnimation();

        SetSprite(Customer.customerSitSprite);
    }

    public void Walk(bool flip = false)
    {
        if (Customer == null) return;

        StopAnimation();

        animationCoroutine = StartCoroutine(HandleWalk(flip));
    }

    IEnumerator HandleWalk(bool flip)
    {
        spriteRenderer.flipX = flip;

        Sprite[] spriteJalan = Customer.customerWalkSprites;

        // Tanpa sprite jalan, tampilkan sprite berdiri saja.
        if (spriteJalan == null || spriteJalan.Length == 0)
        {
            SetSprite(Customer.customerSprite);
            yield break;
        }

        int index = 0;

        while (true)
        {
            SetSprite(spriteJalan[index], PpuJalan);

            index++;

            if (index >= spriteJalan.Length) index = 0;

            yield return new WaitForSeconds(animationSpeed);
        }
    }

    void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    #endregion

    #region Movement

    public void Enter(Vector3 spawnPosition, Vector3 targetPosition)
    {
        transform.position = spawnPosition;

        Move(targetPosition, false);
    }

    /// <summary>
    /// Berjalan ke posisi target. Jika sedang menuju target lain, gerakan lama dibatalkan dan diganti.
    /// Jika sudah sampai atau sedang menuju target yang sama, tidak melakukan apa-apa.
    /// </summary>
    public void Move(Vector3 targetPosition, bool flip = false, float delay = 1.0f)
    {
        if (isMoving)
        {
            if (Vector3.Distance(targetTujuan, targetPosition) < 0.01f) return;
        }
        else if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            return;
        }

        BatalkanGerak();

        gerakCoroutine = StartCoroutine(HandleMove(targetPosition, flip, delay));
    }

    public IEnumerator HandleMove(Vector3 targetPosition, bool flip, float delay)
    {
        isMoving = true;
        targetTujuan = targetPosition;

        float duration = Vector3.Distance(transform.position, targetPosition) / moveSpeed;

        if (CustomerManager.Instance != null)
            CustomerManager.Instance.DelaySpawning(duration + 2f + delay);

        yield return new WaitForSeconds(delay);

        Stand();

        Walk(flip);

        // Durasi dihitung ulang karena jarak bisa berubah setelah jeda.
        duration = Vector3.Distance(transform.position, targetPosition) / moveSpeed;

        tweenGerak = transform.DOMove(targetPosition, duration)
            .SetEase(Ease.Linear)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                Sit();
                isMoving = false;
                gerakCoroutine = null;

                SelesaiBergerak?.Invoke(this);
            });
    }

    public void Exit(Vector3 exitPosition)
    {
        BatalkanGerak();

        SetBisaDipilih(false);

        float duration = Vector3.Distance(transform.position, exitPosition) / moveSpeed;

        if (CustomerManager.Instance != null)
            CustomerManager.Instance.DelaySpawning(duration + 2f);

        Stand();

        Walk(true);

        tweenGerak = transform.DOMove(exitPosition, duration)
            .SetEase(Ease.Linear)
            .SetLink(gameObject)
            .OnComplete(() => Destroy(gameObject));
    }

    private void BatalkanGerak()
    {
        if (gerakCoroutine != null)
        {
            StopCoroutine(gerakCoroutine);
            gerakCoroutine = null;
        }

        tweenGerak?.Kill();
        tweenGerak = null;
        isMoving = false;
    }

    #endregion
}
