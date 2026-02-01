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
    private Vector2 _delta = new();
    private Collider2D _collider;
    [SerializeField]
    private bool _isDragging = false;
    private Rigidbody2D _rb;

    public LayerMask draggableMask;
    public Vector3 draggableOffset;
    public bool draggableDragging;


    public Vector2 epsilon = new(0.001f, 0.001f);
    public float maxSpeed = 25f;
    [SerializeField]
    private Vector2 target;

    public bool isInsideGround = false;

    public Vector3 initWorldPos;

    void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
    }

    

    void OnMouseDownD()
    {
        _ori_delta = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;

    }

    void OnMouseDragD()
    {
        _isDragging = true;
        var pos = cam.ScreenToWorldPoint(Input.mousePosition) - cam.ScreenToWorldPoint(_ori_delta);
        // _ori_delta = Input.mousePosition;

        // transform.Translate(new Vector3(_delta.x, _delta.y, 0), Space.World);
        // target = cam.ScreenToWorldPoint(Input.mousePosition) - _ori_delta;
        if (!isInsideGround)
        {
            // transform.position = new Vector3(pos.x, pos.y, 0);
        }
        // _rb.MovePosition(new Vector3(pos.x, pos.y, 0));
    }

    void OnMouseUpD()
    {
        _isDragging = false;
    }
    void Update()
    {
        target = cam.ScreenToWorldPoint(Input.mousePosition) - _ori_delta;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            mouseWorld.z = 0f;
            var hit = Physics2D.Raycast(mouseWorld, Vector2.zero, 0f, draggableMask);
            if (hit.collider)
            {
                draggableOffset = hit.collider.transform.position - mouseWorld;
                draggableDragging = true;

                OnMouseDownD();
            }
        }
        if (draggableDragging && Input.GetMouseButtonDown(0))
        {
            OnMouseDragD();
        }
        if (draggableDragging && Input.GetMouseButtonUp(0))
        {
            OnMouseUpD();
            draggableDragging=false;
        }
    }
    void FixedUpdate()
    {
        if (_isDragging)
        {
            if (isInsideGround)
            {
                Vector2 next = Vector2.MoveTowards(_rb.position, target, maxSpeed * Time.fixedDeltaTime);

                _rb.MovePosition(next);
            } else
            {
                _rb.MovePosition(target);

                
            }
        }
    }

    public bool isInMask(Vector2 worldPos)
    {
        return _collider.OverlapPoint(worldPos);
    }

    public void RespawnToInitPos()
    {
        _rb.MovePosition(initWorldPos);
    }
}

// using UnityEngine;

// [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
// public class DraggableMask : MonoBehaviour
// {
//     private Camera _cam;
//     private Rigidbody2D _rb;
//     private Vector2 _targetPos;
//     private bool _dragging;
//     private Vector2 _offset;
//     private Collider2D _collider;

//     private void Awake()
//     {
//         _cam = Camera.main;
//         _rb = GetComponent<Rigidbody2D>();
//         _collider = GetComponent<Collider2D>();
//     }

//     private void OnMouseDown()
//     {
//         _dragging = true;
//         Vector2 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
//         _offset = (Vector2)transform.position - mouseWorld;
//     }

//     private void OnMouseUp()
//     {
//         _dragging = false;
//     }

//     private void Update()
//     {
//         if (!_dragging) return;
//         Vector2 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
//         _targetPos = mouseWorld + _offset;
//     }

//     private void FixedUpdate()
//     {
//         if (!_dragging) return;
//         _rb.MovePosition(_targetPos);
//     }

//     public bool isInMask(Vector2 worldPos)
//     {
//         return _collider.OverlapPoint(worldPos);
//     }
// }
