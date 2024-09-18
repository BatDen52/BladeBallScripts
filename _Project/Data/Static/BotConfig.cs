using System;
using System.Collections.Generic;
using _Project.Data.Static.Skills;
using UnityEngine;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "BotConfig", menuName = "_Project/BotConfig", order = 0)]
    public class BotConfig : ScriptableObject
    {
        public List<Skin> Skins = new List<Skin>();
        public List<Skill> Skills = new List<Skill>();
        public FloatRange DecisionsDelay = new FloatRange { Min = 0f, Max = 10f };
        
        [Header("BladeBall")]
        public AnimationCurve BlockProbabilityCurve;
        
        [Header("Movement")]
        public FloatRange TendencyToMove = new FloatRange { Min = 0f, Max = 1f };
        public FloatRange MoveDecisionInterval = new FloatRange { Min = 1f, Max = 5f };
        public float DestinationReachedDistance = 3f;
        public float NextDestinationRadius = 40f;
        public FloatRange PreferredDistanceFromBall = new FloatRange { Min = 1f, Max = 100f };

        [Header("Look")]
        public FloatRange TendencyToChangeTarget = new FloatRange { Min = 0f, Max = 1f };
        public FloatRange ChangeTargetInterval = new FloatRange { Min = 1f, Max = 5f };
        public FloatRange TendencyToLockRotation = new FloatRange { Min = 0f, Max = 1f };
        public FloatRange LockRotationInterval = new FloatRange { Min = 1f, Max = 5f };
        
        [Header("Jumps")] 
        public FloatRange JumpInterval = new FloatRange{ Min = 5f, Max = 5f};
        public FloatRange JumpProbability = new FloatRange{ Min = 0.4f, Max = 0.4f};
        public FloatRange DoubleJumpProbability = new FloatRange{ Min = 0.3f, Max = 0.3f};
        public DoubleJumpTimings JumpTimings = new DoubleJumpTimings();
        
        [Header("Skills")]
        public FloatRange UseSkillInterval = new FloatRange{ Min = 5f, Max = 5f};
        public FloatRange UseSkillProbability = new FloatRange{ Min = 0.4f, Max = 0.4f};

        [Serializable]
        public class DoubleJumpTimings
        {
            public float FirstPress = 0.01f;
            public float Pause = 0.1f;
            public float SecondPress = 0.5f;
        }
        
        [Serializable]
        public class FloatRange
        {
            public float Min;
            public float Max;
        }
    }

    

    
}