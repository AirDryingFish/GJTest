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

    [Header("按 R 召唤到鼠标")]
    [Tooltip("召唤动画总时长（秒），用曲线缓入缓出")]
    [SerializeField] private float summonDuration = 0.4f;
    [Tooltip("自定义曲线：横轴 0~1 为进度，纵轴为插值系数；不填则用 SmoothStep 缓入缓出")]
    [SerializeField] private AnimationCurve summonCurve;
    private bool _summoningToMouse;
    private Vector2 _summonStartPos;
    private Vector2 _summonTarget;
    private float _summonProgress;

    public bool isInsideGround = false;

    public Vector3 initWorldPos;

    [Header("滚轮缩放")]
    [Tooltip("最小缩放倍数（相对初始 scale）")]
    [SerializeField] private float minScale = 0.3f;
    [Tooltip("最大缩放倍数")]
    [SerializeField] private float maxScale = 3f;
    [Tooltip("滚轮每格改变的缩放量")]
    [SerializeField] private float scrollScaleSpeed = 0.15f;
    private Vector3 _initialLocalScale;
    private float _scaleMultiplier = 1f;

    void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
        _initialLocalScale = transform.localScale;
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
            draggableDragging = false;
        }

        // 滚轮缩放：在初始 shape 上按倍数缩放，保持宽高比
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            _scaleMultiplier += scroll * scrollScaleSpeed;
            _scaleMultiplier = Mathf.Clamp(_scaleMultiplier, minScale, maxScale);
            transform.localScale = new Vector3(
                _initialLocalScale.x * _scaleMultiplier,
                _initialLocalScale.y * _scaleMultiplier,
                _initialLocalScale.z * _scaleMultiplier
            );
        }

        // 按 R 键用曲线平滑召唤 mask 到鼠标位置
        if (Input.GetKeyDown(KeyCode.R) && cam != null)
        {
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0f;
            _summonStartPos = _rb.position;
            _summonTarget = new Vector2(pos.x, pos.y);
            _summonProgress = 0f;
            _summoningToMouse = true;
        }
    }
    void FixedUpdate()
    {
        // 按 R 召唤：用曲线插值（缓入缓出）到鼠标位置
        if (_summoningToMouse)
        {
            float dur = Mathf.Max(0.001f, summonDuration);
            _summonProgress += Time.fixedDeltaTime / dur;
            float t = Mathf.Clamp01(_summonProgress);
            float curveT = summonCurve != null && summonCurve.keys.Length > 0
                ? summonCurve.Evaluate(t)
                : Mathf.SmoothStep(0f, 1f, t);
            Vector2 next = Vector2.Lerp(_summonStartPos, _summonTarget, curveT);
            _rb.MovePosition(next);
            if (_summonProgress >= 1f)
            {
                _rb.MovePosition(_summonTarget);
                _summoningToMouse = false;
            }
            return;
        }

        if (_isDragging)
        {
            if (isInsideGround)
            {
                Vector2 next = Vector2.MoveTowards(_rb.position, target, maxSpeed * Time.fixedDeltaTime);

                _rb.MovePosition(next);
            }
            else
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

    /// <summary> 将 mask 设到指定世界坐标（存档点复活时与玩家一致用）。 </summary>
    public void SetPosition(Vector2 worldPos)
    {
        _rb.position = worldPos;
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
