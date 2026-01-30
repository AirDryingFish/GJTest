using UnityEngine;

// 使用说明：
// 1. Player：挂到角色物体上，需有 Rigidbody2D + Collider2D；在 Inspector 里把 Ground Layer 设为 "Ground"
// 2. 地面：Sprite2D 方块 + BoxCollider2D，物体 Layer 设为 "Ground"，只有踩在地面上才能起跳
// 3. 手感：已包含土狼时间、跳跃缓冲、空中控制、可变高度跳、下落加重
namespace Yzz
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [Tooltip("地面加减速，越大越跟手；约 400 可在一帧内到满速/刹停")]
        [SerializeField] private float groundAcceleration = 400f;
        [SerializeField] private float airControlFactor = 0.6f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private float gravityScale = 2.5f;
        [SerializeField] private float fallGravityMultiplier = 1.4f;
        [SerializeField] private float lowJumpMultiplier = 1.6f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.1f;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.1f);

        [Header("Sprint")]
        [Tooltip("移动速度的乘数，按住 Shift 时生效")]
        [SerializeField] private float sprintMultiplier = 1.5f;
        [Tooltip("按住 Shift 时的加速度乘数（地面/空中加速度都会乘以它）")]
        [SerializeField] private float sprintAccelerationMultiplier = 1.15f;

        [Header("Feel (Coyote & Buffer)")]
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.12f;

        private Rigidbody2D _rb;
        private Collider2D _col;
        private float _coyoteCounter;
        private float _jumpBufferCounter;
        private float _inputX;
        private bool _isSprinting;
        /// <summary> 每次着地只允许起跳一次，防止接地判定连续为 true 时重复加跳跃力 </summary>
        private bool _hasJumpedSinceGrounded = true;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _rb.gravityScale = gravityScale;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            _inputX = Input.GetAxisRaw("Horizontal");

            // Sprint: 按住 Shift 加速
            _isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // Jump buffer: remember jump input for a short time
            if (Input.GetKeyDown(KeyCode.Space))
                _jumpBufferCounter = jumpBufferTime;
            else if (_jumpBufferCounter > 0f)
                _jumpBufferCounter -= Time.deltaTime;

            // Coyote time: allow jump shortly after leaving ground
            if (IsGrounded())
            {
                _coyoteCounter = coyoteTime;
                _hasJumpedSinceGrounded = false; // 着地后允许跳一次
            }
            else if (_coyoteCounter > 0f)
                _coyoteCounter -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            bool grounded = IsGrounded();

            // Horizontal: 地面用大加速度跟手，空中略弱
            float currentMoveSpeed = moveSpeed * (_isSprinting ? sprintMultiplier : 1f);
            float targetVelX = _inputX * currentMoveSpeed;
            float baseAccel = grounded ? groundAcceleration : (groundAcceleration * airControlFactor);
            float accel = baseAccel * (_isSprinting ? sprintAccelerationMultiplier : 1f);
            float newVelX = Mathf.MoveTowards(_rb.velocity.x, targetVelX, accel * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(newVelX, _rb.velocity.y);

            // Jump: 每次着地只执行一次，避免重叠/误判时每帧加力
            bool canJump = !_hasJumpedSinceGrounded && (_coyoteCounter > 0f || grounded);
            if (_jumpBufferCounter > 0f && canJump)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
                _jumpBufferCounter = 0f;
                _coyoteCounter = 0f;
                _hasJumpedSinceGrounded = true;
            }

            // Variable jump height & fall gravity
            if (_rb.velocity.y < 0f)
                _rb.gravityScale = gravityScale * fallGravityMultiplier;
            else if (_rb.velocity.y > 0f && !Input.GetKey(KeyCode.Space))
                _rb.gravityScale = gravityScale * lowJumpMultiplier;
            else
                _rb.gravityScale = gravityScale;
        }

        private bool IsGrounded()
        {
            Vector2 center = (Vector2)transform.position + groundCheckOffset;
            Collider2D hit = Physics2D.OverlapCircle(center, groundCheckRadius, groundLayer);
            // 排除自身碰撞体：若 groundLayer 包含玩家层或选成 Everything，会一直“踩着自己”导致每帧加跳跃力，角色被顶飞
            if (hit == null) return false;
            if (_col != null && hit == _col) return false;
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);
        }
    }
}
