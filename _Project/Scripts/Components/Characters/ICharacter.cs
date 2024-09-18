using System.Collections.Generic;
using UnityEngine;

namespace _Project
{
    public interface ICharacter
    {
        void Die();
        bool IsBlockingByInput { get; }
        bool IsBlockingBySkill { get; }
        Vector3 GetPosition();
        Transform Transform { get; }
        ICharacter GetNextTarget(IEnumerable<ICharacter> availableTargets);
        void MarkAsTarget();
        void UnmarkAsTarget();
        bool IsLocalPlayer { get; set; }
        void Block();
        void React(float speed);
        void AddScorePoints(int points);
        int ScorePoints { get; set; }
    }
}