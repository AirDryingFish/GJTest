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
        [Tooltip("脚下射线检测长度，只检测正下方的地面")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.05f);
        [Tooltip("表面法线 Y 至少为此值才算“踩在地面”（避免贴墙时把墙当地面）")]
        [Range(0.3f, 1f)]
        [SerializeField] private float minGroundNormalY = 0.5f;
        [Tooltip("侧面射线长度，用于检测贴墙；空中贴墙时不往墙里推，避免卡住不下落")]
        [SerializeField] private float wallCheckDistance = 0.08f;

        [Header("Sprint")]
        [Tooltip("移动速度的乘数，按住 Shift 时生效")]
        [SerializeField] private float sprintMultiplier = 1.5f;
        [Tooltip("按住 Shift 时的加速度乘数（地面/空中加速度都会乘以它）")]
        [SerializeField] private float sprintAccelerationMultiplier = 1.15f;

        [Header("Feel (Coyote & Buffer)")]
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.12f;

        [Header("Respawn")]
        [Tooltip("当角色的 Y 坐标低于此值时，会被传回初始出生点")]
        [SerializeField] private float respawnY = -1000f;

        [Header("MaskObject")]
        [Tooltip("The Mask Object to detect")]
        [SerializeField] private DraggableMask mask;

        [Header("Sprite")]
        [Tooltip("不指定则用同物体上的 SpriteRenderer；向左走时 flipX = true")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D _rb;
        private Collider2D _col;
        private Vector2 _spawnPosition;
        private float _coyoteCounter;
        private float _jumpBufferCounter;
        private float _inputX;
        private bool _isSprinting;
        /// <summary> 每次着地只允许起跳一次，防止接地判定连续为 true 时重复加跳跃力 </summary>
        private bool _hasJumpedSinceGrounded = true;

        /// <summary> Model 层 / 动画层可读：当前速度 </summary>
        public Vector2 Velocity => _rb != null ? _rb.velocity : Vector2.zero;
        /// <summary> Model 层 / 动画层可读：是否在地面 </summary>
        public bool IsGroundedState => IsGrounded();

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            // 保存初始出生点，跌落重生时回到此位置
            _spawnPosition = transform.position;
            _rb.gravityScale = gravityScale;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (mask == null)
            {
                Debug.LogError("Mask object is null");
            }
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
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

            // 朝左走时 flipX，朝右走时不 flip
            if (spriteRenderer != null)
            {
                if (_inputX < 0f) spriteRenderer.flipX = true;
                else if (_inputX > 0f) spriteRenderer.flipX = false;
            }
        }

        private void FixedUpdate()
        {
            // 跌落重生：低于阈值则传回初始位置并重置速度/计时
            if (transform.position.y < respawnY)
            {
                _rb.velocity = Vector2.zero;
                transform.position = _spawnPosition;
                _coyoteCounter = coyoteTime;
                _jumpBufferCounter = 0f;
                _hasJumpedSinceGrounded = false;
                return;
            }

            bool grounded = IsGrounded();
            bool wallLeft = CheckWall(-1);
            bool wallRight = CheckWall(1);

            // 空中贴墙时不再往墙里推，否则物理会把角色顶住/顶起，松键才下落
            float currentMoveSpeed = moveSpeed * (_isSprinting ? sprintMultiplier : 1f);
            float targetVelX = _inputX * currentMoveSpeed;
            if (!grounded)
            {
                if (_inputX > 0f && wallRight) targetVelX = 0f;
                if (_inputX < 0f && wallLeft) targetVelX = 0f;
            }

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
            Vector2 origin = (Vector2)transform.position + groundCheckOffset;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
            if (!hit) return false;
            if (_col != null && hit.collider == _col) return false;
            // 只算“脚下方、表面朝上”的地面，贴墙时墙的法线是水平的，不会判成地面，避免卡墙不下落
            if (hit.normal.y < minGroundNormalY) return false;
            return true;
        }

        /// <summary> 检测左侧(-1)或右侧(1)是否有墙/障碍，用于空中贴墙时不往墙里推 </summary>
        private bool CheckWall(int direction)
        {
            if (_col == null) return false;
            float extX = _col.bounds.extents.x;
            Vector2 origin = (Vector2)transform.position + new Vector2(direction * extX, 0f);
            Vector2 dir = new Vector2(direction, 0f);
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallCheckDistance, groundLayer);
            if (!hit) return false;
            if (hit.collider == _col) return false;
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 origin = (Vector2)transform.position + groundCheckOffset;
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + Vector2.down * groundCheckDistance);
        }
    }
}
