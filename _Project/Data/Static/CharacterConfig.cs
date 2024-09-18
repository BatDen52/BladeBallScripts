using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "_Project/CharacterConfig", order = 0)]
    public class CharacterConfig : ScriptableObject
    {
        public List<Skin> Skins = new List<Skin>();
        public BlockConfig BlockConfig = new BlockConfig();
        //public DashConfig DashConfig = new DashConfig();
        public Material WhenTargetMaterial;
        [Header("Movement speed")]
        public float Acceleration = 16f;
        public float TurningDrag = 30f;
        public float MaxSpeed = 7.5f;
        public float Deceleration = 30f;
        public float RotationSpeed = 970f;
        public float Friction = 28f;
        [Header("Gravity")]
        public float GravityTopSpeed = 50f;
        public float Gravity = 49f;
        public float FallGravity = 65f;
        [Header("Jumps")]
        public int MaxJumps = 2;
        public float MaxJumpHeight = 19f;
        public float MinJumpHeight = 10f;
        public float AirAcceleration = 32f;
    }
    
    [Serializable]
    public class BlockConfig
    {
        public float Cooldown = 5f;
        public float Duration = 3f;
        public float ReturnWeaponToIdleAfter = 1f;
    }
    
    // [Serializable]
    // public class DashConfig
    // {
    //     public float Force = 25f;
    //     public float Duration = 0.3f;
    // }
}