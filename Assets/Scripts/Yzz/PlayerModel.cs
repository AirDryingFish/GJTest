using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// Model 层：根据 PlayerController 的速度等状态驱动 Animator 参数。
    /// 挂到玩家物体上，指定 Animator 和 PlayerController；Animator 可在本物体或子物体上。在 Animator Controller 里建好对应参数（如 isWalking、jump）。
    /// </summary>
    public class PlayerModel : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("不指定则用同物体上的 PlayerController")]
        [SerializeField] private MulPlayerController mulPlayerController;
        [Tooltip("不指定则用同物体上的 Animator")]
        [SerializeField] private Animator animator;

        [Header("Animator Parameters (名字需和 Controller 里一致)")]
        [SerializeField] private string paramIsWalking = "isWalking";
        [SerializeField] private string paramSpeedX = "speedX";
        [SerializeField] private string paramSpeedY = "speedY";
        [SerializeField] private string paramIsGrounded = "isGrounded";
        [SerializeField] private string paramJump = "jump";

        [Header("Thresholds")]
        [Tooltip("水平速度超过此值视为在走路")]
        [SerializeField] private float walkSpeedThreshold = 0.1f;

        private void Awake()
        {
            // Controller 可能在父物体上（MulPlayerController 管理 players[] 时）
            if (mulPlayerController == null) mulPlayerController = GetComponent<MulPlayerController>();
            if (mulPlayerController == null) mulPlayerController = GetComponentInParent<MulPlayerController>();
            // Animator 常在子物体上（如 Visual/Model 子节点）
            if (animator == null) animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            // 与物理同帧更新，这样在 FixedUpdate 里调 TriggerJump() 时，本帧 Animator 就能看到 trigger，避免“到最高点才播”
            if (animator != null)
                animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        }

        private void Update()
        {
            if (mulPlayerController == null || animator == null) return;

            Vector2 v = mulPlayerController.Velocity;
            bool grounded = mulPlayerController.IsGroundedState;
            bool walking = grounded && Mathf.Abs(v.x) > walkSpeedThreshold;

            if (!string.IsNullOrEmpty(paramIsWalking))
                animator.SetBool(paramIsWalking, walking);
            if (!string.IsNullOrEmpty(paramSpeedX))
                animator.SetFloat(paramSpeedX, v.x);
            if (!string.IsNullOrEmpty(paramSpeedY))
                animator.SetFloat(paramSpeedY, v.y);
            if (!string.IsNullOrEmpty(paramIsGrounded))
                animator.SetBool(paramIsGrounded, grounded);
        }

        /// <summary> 由 MulPlayerController 在起跳时调用，触发 Animator 的 jump trigger。 </summary>
        public void TriggerJump()
        {
            print("TriggerJump: " + paramJump);
            if (animator == null || string.IsNullOrEmpty(paramJump)) return;
            animator.SetTrigger(paramJump);
        }
    }
}
