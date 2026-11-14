using System.Collections;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomerSlot : MonoBehaviour
{
    [Header("Setting")]
    public float moveSpeed = 3f;
    public float animationSpeed = 0.25f;
    public bool isMoving = false;

    public Customer Customer { get; private set; }

    private SpriteRenderer spriteRenderer;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    void SetSprite(Sprite sprite, float ppu = 350f)
    {
        if (sprite == null) return;

        spriteRenderer.sprite = Sprite.Create(sprite.texture, sprite.rect, sprite.pivot / sprite.rect.size, ppu);
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

        int index = 0;

        while (true)
        {
            SetSprite(Customer.customerWalkSprites[index], 220);

            index++;

            if (index >= Customer.customerWalkSprites.Length) index = 0;

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

    public void Move(Vector3 targetPosition, bool flip = false, float delay = 1.0f)
    {
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f) return;

        if (isMoving == true) return;

        StartCoroutine(HandleMove(targetPosition, flip, delay));
    }

    public IEnumerator HandleMove(Vector3 targetPosition, bool flip, float delay)
    {
        isMoving = true;

        float duration = Vector3.Distance(transform.position, targetPosition) / moveSpeed;

        CustomerManager.Instance.DelaySpawning(duration + 2f + delay);

        yield return new WaitForSeconds(delay);

        Stand();

        Walk(flip);

        transform.DOMove(targetPosition, duration).SetEase(Ease.Linear).OnComplete(() => { 
            Sit();
            isMoving = false;
        });
        
    }
    public void Exit(Vector3 exitPosition)
    {
        float duration = Vector3.Distance(transform.position, exitPosition) / moveSpeed;

        CustomerManager.Instance.DelaySpawning(duration + 2f);

        Stand();

        Walk(true);

        transform.DOMove(exitPosition, duration).SetEase(Ease.Linear).OnComplete(() => {
            Destroy(gameObject);
            SetCustomer(null);
        });
    }

    #endregion
}