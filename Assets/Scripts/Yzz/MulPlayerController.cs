using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;

// 使用说明：
// 1. Player：挂到角色物体上，需有 Rigidbody2D + Collider2D；在 Inspector 里把 Ground Layer 设为 "Ground"
// 2. 地面：Sprite2D 方块 + BoxCollider2D，物体 Layer 设为 "Ground"，只有踩在地面上才能起跳
// 3. 手感：已包含土狼时间、跳跃缓冲、空中控制、可变高度跳、下落加重
namespace Yzz
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class MulPlayerController : MonoBehaviour
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
        // [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask[] groundLayers;
        [Tooltip("脚下射线检测长度，只检测正下方的地面")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [Tooltip("地面检测左右射线相对中心的偏移（占 collider 宽度比例），尖角/窄台用多射线更稳")]
        [Range(0f, 0.5f)]
        [SerializeField] private float groundCheckWidthFactor = 0.35f;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.05f);
        [Tooltip("表面法线 Y 至少为此值才算“踩在地面”（避免贴墙时把墙当地面）")]
        [Range(0.3f, 1f)]
        [SerializeField] private float minGroundNormalY = 0.5f;
        [Tooltip("尖角/陡坡：法线 Y 达到此值也允许起跳，纯墙(0)仍不算；应小于上面一项")]
        [Range(0.15f, 0.6f)]
        [SerializeField] private float minGroundNormalYCorner = 0.25f;
        [Tooltip("下尖角(V形底)：向下斜射线与竖直夹角(度)，0=不检测，约 25~40 可踩住 V 形底")]
        [Range(0f, 50f)]
        [SerializeField] private float groundCheckAngleDown = 30f;
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

        [Header("Stuck Push (卡住反推)")]
        [Tooltip("按住 AD 朝某方向走，超过此时间且水平位移几乎为 0 时，沿反方向推一点距离")]
        [SerializeField] private float stuckPushDuration = 0.35f;
        [Tooltip("这段时间内水平位移小于此值则判定为卡住")]
        [SerializeField] private float stuckDisplacementThreshold = 0.02f;
        [Tooltip("卡住时沿反方向推出的位移。建议 ≈ moveSpeed×0.02～0.03（如 moveSpeed=8 用 0.16～0.24），过小推不开仍不响应 AD")]
        [SerializeField] private float stuckPushDistance = 0.2f;
        [Tooltip("反推时同时给的瞬时水平速度（反方向），便于脱离贴墙后立刻响应 AD；0 则仅位移。建议约为 moveSpeed 的 0.2～0.5")]
        [SerializeField] private float stuckPushVelocity = 2.5f;

        [Header("Respawn")]
        [Tooltip("当角色的 Y 坐标低于此值时，会被传回初始出生点")]
        [SerializeField] private float respawnY = -1000f;

        [Header("MaskObject")]
        [Tooltip("The Mask Object to detect")]
        [SerializeField] private DraggableMask mask;

        [Header("Ground Colliders (按 curIndex 控制 isTrigger)")]
        [Tooltip("Layer 为 Ground 的物体上的 Collider2D 列表；curIndex=0 时为非 Trigger，curIndex=1 时为 Trigger")]
        [SerializeField] private Collider2D[] groundLayerColliders;
        [Tooltip("Layer 为 Ground2 的物体上的 Collider2D 列表；curIndex=1 时为非 Trigger，curIndex=0 时为 Trigger")]
        [SerializeField] private Collider2D[] ground2LayerColliders;

        private List<Collider2D> _groundLayerCollidersCached = new List<Collider2D>();
        private List<Collider2D> _ground2LayerCollidersCached = new List<Collider2D>();

        [Header("Sprite")]
        [Tooltip("不指定则用同物体上的 SpriteRenderer；向左走时 flipX = true")]
        [SerializeField] private SpriteRenderer[] spriteRenderers;

        // private Rigidbody2D _rb;
        // private Collider2D _col;
        private Rigidbody2D[] _rbs;
        private Collider2D[] _cols;

        public Transform judgePoint;
        public Transform judgePointM;

        public EdgeCollider2D maskEdgeCollider;
        private Vector3[] _offsets;

        public bool isAllSamePos = true;


        public GameObject[] players;
        [SerializeField] private int curIndex = 0;
        private Vector2 _spawnPosition;
        private float _coyoteCounter;

        private float _jumpBufferCounter;
        private float _inputX;
        private bool _isSprinting;
        /// <summary> 每次着地只允许起跳一次，防止接地判定连续为 true 时重复加跳跃力 </summary>
        private bool _hasJumpedSinceGrounded = true;
        /// <summary> judgePoint 是否在对应 Ground/Ground2 层内，用于 IsGrounded 时把 Mask 层也算地面 </summary>
        private bool _isInside;
        private float _stuckPushTimer;
        private float _stuckPushStartPosX;
        private int _stuckPushDirection; // 1 或 -1，0 表示未在计时

        /// <summary> Model 层 / 动画层可读：当前速度 </summary>
        public Vector2 Velocity => _rbs[curIndex] != null ? _rbs[curIndex].velocity : Vector2.zero;
        /// <summary> Model 层 / 动画层可读：是否在地面（起跳后短暂时间内强制为 false，避免 Jump 动画未播完就切回 Walk） </summary>
        public bool IsGroundedState => IsGrounded() && _animatorGroundedGraceRemaining <= 0f;
        /// <summary> Model 层 / 动画层可读：是否在冲刺（按住 Shift） </summary>
        public bool IsSprinting => _isSprinting;
        /// <summary> 起跳后在此时间内向动画层报告“未接地”，避免 Jump→Walk 条件在起跳瞬间成立 </summary>
        [SerializeField] private float animatorGroundedGraceAfterJump = 0.12f;
        private float _animatorGroundedGraceRemaining;

        public PlayerModel[] playerModels;


        public AudioSource jumpSound, warpSound;

        private void InitPlayers()
        {
            if (groundLayers.Count() != players.Count())
            {
                // Debug.LogError($"GroundLayers(count:{groundLayers.Count()}) has different count as players(count:{players.Count()}) ");
            }
            _rbs = new Rigidbody2D[players.Count()];
            _cols = new Collider2D[players.Count()];
            if (spriteRenderers == null)
            {

                spriteRenderers = new SpriteRenderer[players.Count()];
                for (int i = 0; i < players.Count(); i++)
                {
                    spriteRenderers[i] = players[i].GetComponentInChildren<SpriteRenderer>();
                }
            }

            for (int i = 0; i < players.Count(); i++)
            {
                if (!players[i].TryGetComponent(out _cols[i]))
                {
                    // Debug.LogError($"Failed to get Collider at players[{i}] ({players[i].name})");
                }
                if (!players[i].TryGetComponent(out _rbs[i]))
                {
                    // Debug.LogError($"Failed to get Rigidbody at players[{i}] ({players[i].name})");
                    continue;
                }
                _rbs[i].gravityScale = gravityScale;
                _rbs[i].interpolation = RigidbodyInterpolation2D.Interpolate;
                _rbs[i].collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                // 为每个玩家挂上转发器，碰撞/触发事件才会传到本控制器的 OnPlayerCollision* / OnPlayerTrigger*
                if (!players[i].TryGetComponent(out PlayerTriggerForwarder forwarder))
                    forwarder = players[i].AddComponent<PlayerTriggerForwarder>();
                forwarder.Setup(this, i);
            }

            InitTransformOffsets();
            ChangeCur(0);

        }

        private void InitTransformOffsets()
        {
            if (isAllSamePos)
            {
                return;
            }
            _offsets = new Vector3[players.Count()];
            for (int i = 0; i < players.Count(); ++i)
            {
                _offsets[i] = players[i].transform.position;
            }
        }

        public void ChangeCur(int c)
        {
            if (c >= players.Count() || c < 0)
            {
                // Debug.LogError($"{c} is not a valid index");
                return;
            }
            var v = _rbs[curIndex].velocity;
            var av = _rbs[curIndex].angularVelocity;
            // 非当前玩家：Kinematic + simulated = true，且 Collider isTrigger = true（切到 index=1 时 Collider2D[0] 也为 Trigger）
            for (int i = 0; i < _rbs.Length; i++)
            {
                _rbs[i].velocity = Vector2.zero;
                _rbs[i].angularVelocity = 0f;
                _rbs[i].bodyType = RigidbodyType2D.Kinematic;
                _rbs[i].simulated = true;
                if (_cols[i] != null) _cols[i].isTrigger = true;
            }
            curIndex = c;
            _rbs[curIndex].velocity = v;
            _rbs[curIndex].angularVelocity = av;
            _rbs[curIndex].bodyType = RigidbodyType2D.Dynamic;
            _rbs[curIndex].simulated = true;
            if (_cols[curIndex] != null) _cols[curIndex].isTrigger = false; // 当前玩家用物理碰撞
            // curIndex=0：Ground 层 Collider 非 Trigger，Ground2 层为 Trigger；curIndex=1：Ground 为 Trigger，Ground2 非 Trigger
            bool groundAsTrigger = c != 0;
            for (int i = 0; i < _groundLayerCollidersCached.Count; i++)
                _groundLayerCollidersCached[i].isTrigger = groundAsTrigger;
            bool ground2AsTrigger = c != 1;
            for (int i = 0; i < _ground2LayerCollidersCached.Count; i++)
                _ground2LayerCollidersCached[i].isTrigger = ground2AsTrigger;
        }
        private void SyncTransform()
        {

            for (int i = 0; i < players.Count(); i++)
            {
                if (i == curIndex)
                {
                    continue;
                }
                if (isAllSamePos)
                {
                    players[i].transform.position = players[curIndex].transform.position;
                }
                else
                {
                    players[i].transform.position = players[curIndex].transform.position + (_offsets[i] - _offsets[curIndex]);

                }
                players[i].transform.localScale = players[curIndex].transform.localScale;
                players[i].transform.rotation = players[curIndex].transform.rotation;
            }
        }

        private void Awake()
        {
            InitPlayers();

            // 保存初始出生点，跌落重生时回到此位置
            _spawnPosition = players[curIndex].transform.position;

            CacheGroundColliders();

            if (mask == null)
            {
                // Debug.LogError("Mask object is null");
            }

        }

        private void CacheGroundColliders()
        {
            _groundLayerCollidersCached.Clear();
            if (groundLayerColliders != null)
                for (int i = 0; i < groundLayerColliders.Length; i++)
                    if (groundLayerColliders[i] != null) _groundLayerCollidersCached.Add(groundLayerColliders[i]);
            _ground2LayerCollidersCached.Clear();
            if (ground2LayerColliders != null)
                for (int i = 0; i < ground2LayerColliders.Length; i++)
                    if (ground2LayerColliders[i] != null) _ground2LayerCollidersCached.Add(ground2LayerColliders[i]);
        }


        private void Update()
        {
            if (_animatorGroundedGraceRemaining > 0f)
                _animatorGroundedGraceRemaining -= Time.deltaTime;

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
            if (spriteRenderers != null)
            {


                foreach (var sr in spriteRenderers)
                {
                    if (_inputX < 0f)
                    {
                        sr.flipX = true;
                    }
                    else if (_inputX > 0f)
                    {
                        sr.flipX = false;
                    }
                }


            }

            SyncTransform();
            // print("curIndex: " + curIndex);

        }

        private bool wallStuckOne = false;
        private bool wallStuck = false;

        private void FixedUpdate()
        {
            var rb = _rbs[curIndex];
            float posX = rb.position.x;
            int inputSign = Mathf.Abs(_inputX) > 0.1f ? (int)Mathf.Sign(_inputX) : 0;

            // 卡住反推：按住 AD 一段时间内水平位移接近 0 则沿反方向推一点
            if (inputSign != 0)
            {
                if (_stuckPushDirection != inputSign)
                {
                    _stuckPushDirection = inputSign;
                    _stuckPushStartPosX = posX;
                    _stuckPushTimer = 0f;
                }
                _stuckPushTimer += Time.fixedDeltaTime;
                if (_stuckPushTimer >= stuckPushDuration)
                {
                    float displacement = posX - _stuckPushStartPosX;
                    if (Mathf.Abs(displacement) < stuckDisplacementThreshold)
                    {
                        int pushSign = -_stuckPushDirection;
                        // rb.position += Vector2.right * pushSign * stuckPushDistance;
                        rb.MovePosition(rb.position + pushSign * stuckPushDistance * Vector2.right);
                        if (stuckPushVelocity > 0f)
                            rb.velocity = new Vector2(pushSign * stuckPushVelocity, rb.velocity.y);
                        _stuckPushTimer = 0f;
                        _stuckPushStartPosX = rb.position.x;
                    }
                    else
                    {
                        _stuckPushStartPosX = posX;
                        _stuckPushTimer = 0f;
                    }
                }
            }
            else
            {
                _stuckPushDirection = 0;
                _stuckPushTimer = 0f;
            }

            if (IsGrounded() && Mathf.Abs(_inputX) > 0.1f)
            {
                // 被卡住：有输入但速度上不去（即时逃逸，与上面的“过一段时间”反推互补）
                if (Mathf.Abs(rb.velocity.x) < 0.05f && rb.velocity.y <= 0.01f)
                {
                    rb.position += Vector2.up * 0.03f + Vector2.right * (-Mathf.Sign(_inputX)) * 0.01f;
                }
            }
            if (mask.isInMask(judgePointM.position))
            {
                if (curIndex != 1)
                {
                    warpSound.Play();
                    ChangeCur(1);
                }

            }
            else
            {
                if (curIndex != 0)
                {
                    warpSound.Play();
                    ChangeCur(0);
                }
            }
            // 跌落重生：低于阈值则传回初始位置并重置速度/计时
            if (players[curIndex].transform.position.y < respawnY)
            {
                _rbs[curIndex].velocity = Vector2.zero;
                if (isAllSamePos)
                    players[curIndex].transform.position = _spawnPosition;
                else
                {
                    var dd = _offsets[curIndex] - _offsets[0];
                    players[curIndex].transform.position = _spawnPosition + (Vector2)dd;
                }
                mask.RespawnToInitPos();
                _coyoteCounter = coyoteTime;
                _jumpBufferCounter = 0f;
                _hasJumpedSinceGrounded = false;
                return;
            }

            RefreshMaskEdgeColliderFromJudgePoint();

            bool grounded = IsGrounded();
            bool wallLeft = CheckWall(-1);
            bool wallRight = CheckWall(1);

            // 空中贴墙时不再往墙里推，否则物理会把角色顶住/顶起，松键才下落
            float currentMoveSpeed = moveSpeed * (_isSprinting ? sprintMultiplier : 1f);
            float targetVelX = _inputX * currentMoveSpeed;
            // if (!grounded)
            // {
            //     if (_inputX > 0f && wallRight) targetVelX = 0f;
            //     if (_inputX < 0f && wallLeft) targetVelX = 0f;
            // }
            

            float baseAccel = grounded ? groundAcceleration : (groundAcceleration * airControlFactor);
            float accel = baseAccel * (_isSprinting ? sprintAccelerationMultiplier : 1f);
            float newVelX = Mathf.MoveTowards(_rbs[curIndex].velocity.x, targetVelX, accel * Time.fixedDeltaTime);
            var wallstop = (_inputX > 0f && wallRight) || (_inputX < 0f && wallLeft);
            {
                if (wallstop) newVelX = 0f;
            }
            wallStuck = !grounded && wallstop;
            // print(wallStuck+"wallgrou"+grounded);
            _rbs[curIndex].velocity = new Vector2(newVelX, _rbs[curIndex].velocity.y);
            if (wallStuck && !wallStuckOne)
            {
                wallStuckOne = true;
            }
            if (wallStuck)
            {
                
                _coyoteCounter = coyoteTime*2f;
                
            }
            // Jump: 每次着地只执行一次，避免重叠/误判时每帧加力
            bool canJump = (!_hasJumpedSinceGrounded || wallStuckOne) && (_coyoteCounter > 0f || grounded || wallStuck);
            if (_jumpBufferCounter > 0f && canJump)
            {
                _rbs[curIndex].velocity = new Vector2(_rbs[curIndex].velocity.x, jumpForce);
                if (wallStuck)
                {
                    _rbs[curIndex].velocity += new Vector2(-Mathf.Sign(_inputX) * 0.1f,0);
                }
                _jumpBufferCounter = 0f;
                _coyoteCounter = 0f;
                _hasJumpedSinceGrounded = true;
                wallStuckOne = false;
                _animatorGroundedGraceRemaining = animatorGroundedGraceAfterJump; // 起跳后短时间内对动画层不报“接地”，避免 Jump 未播完就切 Walk
                jumpSound.Play();
                foreach (var playerModel in playerModels)
                {
                    playerModel.TriggerJump();
                }
            }

            // Variable jump height & fall gravity
            if (_rbs[curIndex].velocity.y < 0f)
                _rbs[curIndex].gravityScale = gravityScale * fallGravityMultiplier;
            else if (_rbs[curIndex].velocity.y > 0f && !Input.GetKey(KeyCode.Space))
                _rbs[curIndex].gravityScale = gravityScale * lowJumpMultiplier;
            else
                _rbs[curIndex].gravityScale = gravityScale;


        }

        private bool IsGrounded()
        {
            Vector2 baseOrigin = (Vector2)players[curIndex].transform.position + groundCheckOffset;
            LayerMask layers = groundLayers[curIndex];
            if (_isInside)
                layers |= LayerMask.GetMask("Mask");
            float extX = _cols[curIndex] != null ? _cols[curIndex].bounds.extents.x * groundCheckWidthFactor : 0f;
            // 左、中、右三条竖直射线，站在尖角/窄台上时至少一条能命中“朝上”的地面
            Vector2[] offsets = { new Vector2(-extX, 0f), Vector2.zero, new Vector2(extX, 0f) };
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2 origin = baseOrigin + offsets[i];
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, layers);
                if (!hit) continue;
                if (_cols[curIndex] != null && hit.collider == _cols[curIndex]) continue;
                if (hit.normal.y >= minGroundNormalY || hit.normal.y >= minGroundNormalYCorner)
                {
                    return true;
                }
            }
            // 下尖角(V形底)：竖直射线易从尖端漏过，加两条向下斜射线打两侧斜面
            if (groundCheckAngleDown > 0.01f)
            {
                float rad = groundCheckAngleDown * Mathf.Deg2Rad;
                Vector2 downL = new Vector2(-Mathf.Sin(rad), -Mathf.Cos(rad));
                Vector2 downR = new Vector2(Mathf.Sin(rad), -Mathf.Cos(rad));
                RaycastHit2D hitL = Physics2D.Raycast(baseOrigin, downL, groundCheckDistance, layers);
                RaycastHit2D hitR = Physics2D.Raycast(baseOrigin, downR, groundCheckDistance, layers);
                if (hitL && hitL.collider != _cols[curIndex] && hitL.normal.y >= minGroundNormalYCorner){ 
                    // print($"{hitL.normal} {hitL.collider.name}");
                    return true; 
                    };
                if (hitR && hitR.collider != _cols[curIndex] && hitR.normal.y >= minGroundNormalYCorner){ 
                    // print($"{hitR.normal} {hitR.collider.name}");
                    return true; 
                    };
            }
            return false;
        }

        /// <summary> 检测左侧(-1)或右侧(1)是否有墙/障碍，用于空中贴墙时不往墙里推。站在尖角/下尖角时不算墙，避免卡住。 </summary>
        private bool CheckWall(int direction)
        {


            if (_cols[curIndex] == null) return false;
            float extX = _cols[curIndex].bounds.extents.x;
            Vector2 pos = (Vector2)judgePointM.position;
            Vector2 origin = pos + new Vector2(direction * extX, 0f);
            Vector2 dir = new Vector2(direction, 0f);
            LayerMask layers = groundLayers[curIndex];
            if (_isInside) layers |= LayerMask.GetMask("Mask");
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallCheckDistance, layers);
            if (!hit) return false;
            // 法线向上，说明是拐角或地面边缘，不当作墙
            if (hit.normal.y > 0.2f)
                return false;
            if (hit.collider == _cols[curIndex]) return false;
            // 下尖角/屋檐：命中面朝下(normal.y<0) 或 命中点在角色上方 → 是“头顶/身下”的斜面，不挡左右移动
            if (hit.normal.y < 0f || hit.point.y > pos.y + 0.05f)
                return false;
            // 若脚下射线打中的是同一碰撞体且法线朝上，说明是站在该平台（尖角/边缘），不当作阻挡墙
            Vector2 feetOrigin = pos + groundCheckOffset;
            RaycastHit2D feetHit = Physics2D.Raycast(feetOrigin, Vector2.down, groundCheckDistance, layers);
            if (feetHit && feetHit.collider == hit.collider && feetHit.normal.y >= minGroundNormalYCorner)
                return false;
            // print($"{hit.normal} {feetHit.normal}");
            return true;
        }

        // ----- 触发器回调（两个玩家 Collider2D 的触发事件会转发到这里） -----
        // 使用前请在对应玩家的 Collider2D 上勾选 Is Trigger；需要物理碰撞则保留一个非 Trigger 的 Collider，再另加一个 Trigger 的 Collider。

        /// <summary> 某玩家的触发器内有物体进入时调用。playerIndex 为 players 数组下标，other 为进入的碰撞体。 </summary>
        public virtual void OnPlayerTriggerEnter(int playerIndex, Collider2D other)
        {
            // 可在此处理：拾取物、机关、传送门等
            // 例：if (other.CompareTag("Coin")) { ... }
        }

        /// <summary> 某玩家的触发器内有物体停留时每帧调用。 </summary>
        public virtual void OnPlayerTriggerStay(int playerIndex, Collider2D other)
        {
        }

        /// <summary> 某玩家的触发器内有物体离开时调用。 </summary>
        public virtual void OnPlayerTriggerExit(int playerIndex, Collider2D other)
        {
        }

        // ----- 碰撞回调（两边都是非 Trigger 的 Collider2D 时：站在地面、贴墙等） -----
        // Kinematic 与 Dynamic 接触时也会触发，两个玩家的 Collider2D 都能收到。

        /// <summary> 某玩家与其它物体发生物理接触时调用。playerIndex 为 players 数组下标，collision 含接触点、法线等。 </summary>
        public virtual void OnPlayerCollisionEnter(int playerIndex, Collision2D collision)
        {
            // 例：if (collision.collider.CompareTag("Platform")) { ... }
        }

        /// <summary> maskEdgeCollider 的开关改由 Trigger 控制，不再用 CollisionStay。 </summary>
        public virtual void OnPlayerCollisionStay(int playerIndex, Collision2D collision)
        {
        }

        /// <summary> 某玩家与其它物体脱离接触时调用。 </summary>
        public virtual void OnPlayerCollisionExit(int playerIndex, Collision2D collision)
        {
        }

        /// <summary> 用 judgePoint + OverlapPoint(Ground/Ground2) 判断是否在对应层内；Default 层的碰撞体过滤掉，不参与判断。 </summary>
        private void RefreshMaskEdgeColliderFromJudgePoint()
        {
            if (maskEdgeCollider == null || judgePoint == null) return;
            Vector2 worldPoint = judgePoint.position;
            int defaultLayer = LayerMask.NameToLayer("Default");
            bool inside = false;
            string colInfo = "null";
            if (curIndex == 1)
            {
                LayerMask groundMask = LayerMask.GetMask("Ground");
                Collider2D col = Physics2D.OverlapPoint(worldPoint, groundMask);
                inside = col != null;
                colInfo = col != null ? $"{col.name}(layer={LayerMask.LayerToName(col.gameObject.layer)})" : "null";
                // if (Time.frameCount % 20 == 0)
                //     Debug.Log($"[isInside] curIndex=1 → 用 Ground 层 | worldPoint={worldPoint} | OverlapPoint 命中={col != null} | col={colInfo} → inside={inside}");
            }
            else if (curIndex == 0)
            {
                LayerMask ground2Mask = LayerMask.GetMask("Ground2");
                Collider2D col = Physics2D.OverlapPoint(worldPoint, ground2Mask);
                inside = col != null;
                colInfo = col != null ? $"{col.name}(layer={LayerMask.LayerToName(col.gameObject.layer)})" : "null";
                // if (Time.frameCount % 20 == 0)
                    // Debug.Log($"[isInside] curIndex=0 → 用 Ground2 层 | worldPoint={worldPoint} | OverlapPoint 命中={col != null} | col={colInfo} → inside={inside}");
            }
            // else if (Time.frameCount % 20 == 0)
            // {
            //     Debug.Log($"[isInside] curIndex={curIndex} 不在 0/1，未检测层 → inside 保持 false");
            // }
            _isInside = inside;
            mask.isInsideGround = inside;
            maskEdgeCollider.enabled = inside;
            // 限频调试：每 20 帧打一次；确认完可注释
            // if (Time.frameCount % 20 == 0)
            //     Debug.Log($"[MaskEdge] curIndex={curIndex}, worldPoint={worldPoint}, col={colInfo},  inside={inside}, enabled={maskEdgeCollider.enabled}");
        }

        // private void OnDrawGizmosSelected()
        // {
        //     if (players == null || curIndex >= players.Length) return;
        //     Vector2 origin = (Vector2)players[curIndex].transform.position + groundCheckOffset;
        //     Gizmos.color = IsGrounded() ? Color.green : Color.red;
        //     Gizmos.DrawLine(origin, origin + Vector2.down * groundCheckDistance);
        // }
    }
}
