using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 存档点管理器：单例，记录最近碰到的存档点位置。死亡时在 MulPlayerController 里用此位置复活玩家并把 mask 设到同位置。
    /// 场景里放一个空物体挂本脚本即可；各存档点挂 Checkpoint 组件（带 Trigger 的 Collider2D），碰到后自动记录。
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }

        private Vector3 _lastCheckpointPosition;
        private bool _hasCheckpoint;
        private int _lastCheckpointIndex = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary> 由 Checkpoint 触发时调用。若该存档点序号 ≥ 当前记录的序号才覆盖（实现先后顺序）。 </summary>
        public void SetCheckpoint(Vector3 worldPosition, int checkpointIndex = 0)
        {
            if (checkpointIndex >= _lastCheckpointIndex)
            {
                _lastCheckpointPosition = worldPosition;
                _lastCheckpointIndex = checkpointIndex;
                _hasCheckpoint = true;
            }
        }

        /// <summary> 是否已碰过至少一个存档点。 </summary>
        public bool HasCheckpoint()
        {
            return _hasCheckpoint;
        }

        /// <summary> 最近存档点的世界坐标（复活用）。 </summary>
        public Vector3 GetCheckpointPosition()
        {
            return _lastCheckpointPosition;
        }

        /// <summary> 清除存档（如新关卡开始时可选调用）。 </summary>
        public void ClearCheckpoint()
        {
            _hasCheckpoint = false;
            _lastCheckpointIndex = -1;
        }
    }
}
