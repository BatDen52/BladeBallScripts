using _Project;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Data.Static.Skills
{

    [CreateAssetMenu(menuName = "Skills/QuadJumpSkill")]
    public class QuadJumpSkill : Skill
    {
        [field: SerializeField] public int MaxJumpsCount { get; private set; }

        public override bool Activate(MonoBehaviour target)
        {
            throw new System.InvalidOperationException("Passive skill can't be activated!");
        }

        override public int MaxJumps(CharacterConfig characterConfig)
        {
            return Mathf.Max(MaxJumpsCount, characterConfig.MaxJumps);
        }

        public override void Deactive(MonoBehaviour target)
        {
            throw new System.InvalidOperationException("Passive skill can't be deactivated!");
        }

        public override void Cancel()
        {
        }

    }
}