using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomerSlot : MonoBehaviour
{
    private const float PpuDiam = 350f;
    private const float PpuJalan = 220f;

    [Header("Setting")]
    public float moveSpeed = 3f;
    public float animationSpeed = 0.25f;
    public bool isMoving = false;

    public Customer Customer { get; private set; }

    private SpriteRenderer spriteRenderer;

    private Coroutine animationCoroutine;
    private Coroutine gerakCoroutine;
    private Tween tweenGerak;
    private Vector3 targetTujuan;

    // Sprite hasil Sprite.Create disimpan supaya tidak dibuat ulang tiap frame animasi, dan dibersihkan saat slot dihancurkan.
    private readonly Dictionary<(Sprite, float), Sprite> cacheSprite = new();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    #region Animation

    void SetSprite(Sprite sprite, float ppu = PpuDiam)
    {
        if (sprite == null) return;

        if (!cacheSprite.TryGetValue((sprite, ppu), out Sprite hasil) || hasil == null)
        {
            hasil = Sprite.Create(sprite.texture, sprite.rect, sprite.pivot / sprite.rect.size, ppu);
            cacheSprite[(sprite, ppu)] = hasil;
        }

        spriteRenderer.sprite = hasil;
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
            });
    }

    public void Exit(Vector3 exitPosition)
    {
        BatalkanGerak();

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
