using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 存档点：挂到带 Trigger 的 Collider2D 物体上。当 Tag 为 "Player" 的物体进入时，通知 CheckpointManager 记录当前位置。
    /// 需在场景中有 CheckpointManager（单例）。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Tooltip("存档点位置（不填则用本物体 position）")]
        [SerializeField] private Transform savePosition;
        [Tooltip("先后顺序：序号更大的存档点会覆盖序号小的。都填 0 则后碰到的覆盖先碰到的。")]
        [SerializeField] private int checkpointIndex = 0;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            Vector3 pos = savePosition != null ? savePosition.position : transform.position;
            if (CheckpointManager.Instance != null)
                CheckpointManager.Instance.SetCheckpoint(pos, checkpointIndex);
        }
    }
}
