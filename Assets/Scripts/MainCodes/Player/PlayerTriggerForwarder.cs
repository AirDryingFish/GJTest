using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 挂在每个玩家子物体上，把该物体的 Collider2D 的触发/碰撞事件转发给 MulPlayerController。
    /// 由 MulPlayerController.InitPlayers() 自动添加并设置 playerIndex，无需手动配置。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerTriggerForwarder : MonoBehaviour
    {
        [Tooltip("由 MulPlayerController 在运行时设置，表示是 players 数组中的第几个")]
        [SerializeField] private int playerIndex;
        [SerializeField] private MulPlayerController controller;

        public void Setup(MulPlayerController ctrl, int index)
        {
            controller = ctrl;
            playerIndex = index;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            controller?.OnPlayerTriggerEnter(playerIndex, other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            controller?.OnPlayerTriggerStay(playerIndex, other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            controller?.OnPlayerTriggerExit(playerIndex, other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Debug.Log($"[PlayerCollision] Enter: playerIndex={playerIndex}, other={collision.collider.name}", collision.collider);
            controller?.OnPlayerCollisionEnter(playerIndex, collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            controller?.OnPlayerCollisionStay(playerIndex, collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            // Debug.Log($"[PlayerCollision] Exit: playerIndex={playerIndex}, other={collision.collider.name}", collision.collider);
            controller?.OnPlayerCollisionExit(playerIndex, collision);
        }
    }
}
