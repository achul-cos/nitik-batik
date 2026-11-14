using UnityEngine;
using UnityEngine.EventSystems;

public class Canting : MonoBehaviour, IPointerClickHandler
{
    public int brushSize = 5;

    public Transform ujungCanting;

    public Vector2 originalPosition;

    public Vector2 CantingOffset = new Vector2(-500f, -200f);

    private DrawingManager _drawingManager;

    public void Start()
    {
        ujungCanting = transform.GetComponentInChildren<Transform>();

        originalPosition = transform.localPosition;

        _drawingManager = FindAnyObjectByType<DrawingManager>();
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (_drawingManager != null)
        {
            _drawingManager.SetCanting(this);
        }
    }
}
