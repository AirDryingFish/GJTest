using UnityEngine;

namespace Yzz
{
    public interface IPlayerInput
    {
        float MoveX { get; }
        bool SprintHeld { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
    }
}
