using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableMask : MonoBehaviour
{
    public Camera cam;
    private Vector3 _grabOffsetWorld;
    private Vector3 _ori;
    [SerializeField]
    private Vector2 _delta=new();
    private Collider2D _collider;
    [SerializeField]
    private bool _isDragging = false;

    public Vector2 epsilon = new(0.001f,0.001f);
    public float deceleration = 12f;

    void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    void OnMouseDown()
    {
        _ori = Input.mousePosition;
    }

    void OnMouseDrag()
    {
        _isDragging = true;
        _delta = cam.ScreenToWorldPoint(Input.mousePosition) - cam.ScreenToWorldPoint(_ori);
        _ori = Input.mousePosition;

        transform.Translate(new Vector3(_delta.x, _delta.y, 0), Space.World);
    }

    void OnMouseUp() {
        _isDragging = false;
    }
    public TMP_Text temp;
    void Update()
    {
        
    }

    public bool isInMask(Vector2 worldPos)
    {
        temp.text = worldPos.ToString();
        return _collider.OverlapPoint(worldPos);
    }
}
