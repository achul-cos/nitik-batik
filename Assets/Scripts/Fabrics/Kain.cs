using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Kain : MonoBehaviour
{
    public bool OutlineEffect = true;
    public bool IsDraggable = true;
    public bool IsHover = true;
    public Fabric fabric;

    private DragableObject drag;
    private SpriteHoverOutline outline;
    private Vector3 initialPosition;
    private tatakan tatakanRef;

    private void Awake()
    {
        initialPosition = gameObject.transform.position;
        outline = gameObject.GetComponent<SpriteHoverOutline>() ?? gameObject.AddComponent<SpriteHoverOutline>();
        outline.hoverOutline = OutlineEffect;
        drag = gameObject.GetComponent<DragableObject>() ?? gameObject.AddComponent<DragableObject>();
        drag.isDragging = IsDraggable;
        drag.isHover = IsHover;
    }

    // Start is called before the first frame update
    void Start()
    {
        tatakanRef = FindAnyObjectByType<tatakan>();

        drag.onPointerEnterEvent.AddListener(MauDitarokKeTatakan);
        drag.onDragEvent.AddListener(LagiDigeser);
        drag.onEndDragEvent.AddListener(LagiDitaruh);
        drag.onPointerExitEvent.AddListener(GakJadiAmbil);
    }
    public void MauDitarokKeTatakan()
    {
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
    }

    public void GakJadiAmbil()
    {
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
    }

    public void LagiDigeser()
    {
        if (tatakanRef == null) return;

        float jarak = Vector3.Distance(tatakanRef.transform.position, transform.position);

        if (jarak <= 4)
        {
            tatakanRef.transform.DOMove(tatakanRef.readyPostion, 0.5f).SetEase(Ease.OutQuint);
        }
        else if (Vector3.Distance(tatakanRef.transform.position, tatakanRef.idlePosition) >= 1f)
        {
            tatakanRef.transform.DOMove(tatakanRef.idlePosition, 0.5f).SetEase(Ease.OutQuint);
        }
    }

    public void LagiDitaruh()
    {
        if (tatakanRef == null) return;

        float jarak = Vector3.Distance(tatakanRef.transform.position, transform.position);

        if (jarak <= 4)
        {
            if (tatakanRef.kainTatakan != null)
            {
                tatakanRef.kainTatakan.GetComponent<SpriteRenderer>().sprite = fabric.fabricSprite;

                NextBatik();

                CustomerManager.Instance.curretChooseFabric = fabric;

                Destroy(gameObject);
            }
        }
        else
        {
            transform.DOMove(initialPosition, 1.0f).SetEase(Ease.OutQuint);
            GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
    }

    public void NextBatik()
    {
        if (tatakanRef == null) return;

        if (tatakanRef.kainTatakan.GetComponent<SpriteRenderer>().sprite != null)
        {
            tatakanRef.transform.DOMove(tatakanRef.initialPosition, 1.0f).SetEase(Ease.InQuint);

            GameManager.Instance.NextBatik();
        }
    }
}
