using UnityEngine;
using UnityEngine.Localization;

namespace _Project.Data.Static.Skills
{
    public abstract class Skill : ScriptableObject
    {
        protected IGameFactory GameFactory;

        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public SkillShopType ShopType { get; private set; }
        [field: SerializeField] public int Price { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public float CooldownTime { get; private set; }
        [field: SerializeField] public float ActiveTime { get; private set; }
        [field: SerializeField] public int ActivationsCount { get; private set; } = 1;
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] public LocalizedString Description { get; private set; }
        
        [field: SerializeField] public bool IsPassive { get; private set; }

        public Sound ActivateSound;
        public Sound DeactivateSound;

        private int _activationsCount = 0;

        public virtual bool Activate(MonoBehaviour target)
        {
            _activationsCount += 1;
            return true;
        }

        public abstract void Deactive(MonoBehaviour target);
        public virtual void CooldownEnded(MonoBehaviour target)
        {
            _activationsCount -= 1;
        }

        public abstract void Cancel();

        public void Init(IGameFactory gameFactory)
        {
            GameFactory = gameFactory;
        }

        virtual public int MaxJumps(CharacterConfig characterConfig)
        {
            return characterConfig.MaxJumps;
        }

        virtual public float MaxJumpHeight(CharacterConfig characterConfig)
        {
            return characterConfig.MaxJumpHeight;
        }

        public int ActivatedTimesCount()
        {
            return _activationsCount;
        }

#if UNITY_EDITOR
        virtual public void OnDrawGizmos()
        {

        }
#endif

    }

    public enum SkillShopType
    {
        Purchasable,
        FortuneWheel
    }
}