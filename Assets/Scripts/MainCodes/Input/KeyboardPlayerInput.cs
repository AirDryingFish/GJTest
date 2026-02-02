using UnityEngine;

namespace Yzz
{
    public class KeyboardPlayerInput : MonoBehaviour, IPlayerInput
    {
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private KeyCode sprintKey1 = KeyCode.LeftShift;
        [SerializeField] private KeyCode sprintKey2 = KeyCode.RightShift;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;

        public float MoveX => Input.GetAxisRaw(horizontalAxis);
        public bool SprintHeld => Input.GetKey(sprintKey1) || Input.GetKey(sprintKey2);
        public bool JumpPressed => Input.GetKeyDown(jumpKey);
        public bool JumpHeld => Input.GetKey(jumpKey);
    }
}
