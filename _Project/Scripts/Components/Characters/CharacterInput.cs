using System;
using UnityEngine;

namespace _Project
{
    public abstract class CharacterInput : MonoBehaviour
    {
        public abstract Vector2 Move { get; set; }
        public abstract Vector3 MoveDirection { get; set; }
        public abstract Vector3 LookDirection { get; set; }
        public abstract event Action Block;
        public abstract event Action UseSkill;
        public abstract event Action<bool> RotationLock;
        public abstract bool GetJumpDown();
        public abstract bool GetJumpUp();
    }
}