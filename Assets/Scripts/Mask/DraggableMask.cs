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
    private Vector3 _ori_delta;
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
        _ori_delta = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        
    }

    void OnMouseDrag()
    {
        _isDragging = true;
        // _delta = cam.ScreenToWorldPoint(Input.mousePosition) - cam.ScreenToWorldPoint(_ori_delta);
        // _ori_delta = Input.mousePosition;

        // transform.Translate(new Vector3(_delta.x, _delta.y, 0), Space.World);
        transform.position = cam.ScreenToWorldPoint(Input.mousePosition) - _ori_delta;
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    void OnMouseUp() {
        _isDragging = false;
    }
    void Update()
    {
        
    }

    public bool isInMask(Vector2 worldPos)
    {
        return _collider.OverlapPoint(worldPos);
    }
}
