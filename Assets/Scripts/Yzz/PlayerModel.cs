using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// Model 层：根据 PlayerController 的速度等状态驱动 Animator 参数。
    /// 挂到玩家物体上，指定 Animator 和 PlayerController；在 Animator Controller 里建好对应参数（如 isWalking）。
    /// </summary>
    [RequireComponent(typeof(Animator))]
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

        [Header("Thresholds")]
        [Tooltip("水平速度超过此值视为在走路")]
        [SerializeField] private float walkSpeedThreshold = 0.1f;

        private void Awake()
        {
            if (mulPlayerController == null) mulPlayerController = GetComponent<MulPlayerController>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (mulPlayerController == null || animator == null) return;

            Vector2 v = mulPlayerController.Velocity;
            bool grounded = mulPlayerController.IsGroundedState;
            bool walking = grounded && Mathf.Abs(v.x) > walkSpeedThreshold;

            if (!string.IsNullOrEmpty(paramIsWalking))
                animator.SetBool(paramIsWalking, walking);
            // if (!string.IsNullOrEmpty(paramSpeedX))
            //     animator.SetFloat(paramSpeedX, v.x);
            // if (!string.IsNullOrEmpty(paramSpeedY))
            //     animator.SetFloat(paramSpeedY, v.y);
            // if (!string.IsNullOrEmpty(paramIsGrounded))
            //     animator.SetBool(paramIsGrounded, grounded);
        }
    }
}
