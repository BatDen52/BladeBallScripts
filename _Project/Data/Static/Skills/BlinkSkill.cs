using _Project;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace _Project.Data.Static.Skills
{

    [CreateAssetMenu(menuName = "Skills/BlinkSkill")]
    public class BlinkSkill : Skill
    {
        [field: SerializeField] public float Distance { get; private set; } = 20;
        [field: SerializeField] public GameObject TeleportFrom { get; private set; }
        [field: SerializeField] public GameObject TeleportTo { get; private set; }

        private Character _character;
        private CoroutineHandle _coroutine;

        private Vector3 _lastTeleportFrom = Vector3.zero;
        private Vector3 _lastTeleportTo = Vector3.zero;

        public override bool Activate(MonoBehaviour target)
        {
            //Not ready to be activated
            if (_coroutine.IsRunning)
                return false;

            if (!base.Activate(target))
                return false;

            if (_character == null)
            {
                _character = target.GetComponent<Character>();
            }

            

            _coroutine = Timing.RunCoroutine(Teleport());

            GameObject.Instantiate(TeleportFrom, _character.transform.position, Quaternion.identity);

            return true;
        }


        private IEnumerator<float> Teleport()
        {
            var origin = _character.transform.position;
            var direction = _character.LateralVelocity.normalized;

            // Perform a single raycast using RaycastCommand and wait for it to complete
            // Setup the command and result buffers
            var results = new NativeArray<RaycastHit>(2, Allocator.TempJob);
            var commands = new NativeArray<RaycastCommand>(1, Allocator.TempJob);
            commands[0] = new RaycastCommand(origin, direction, QueryParameters.Default, Distance);
            // Schedule the batch of raycasts.
            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, 2, default(JobHandle));
            
            // Wait for the batch processing job to complete
            while (handle.IsCompleted == false)
            {
                yield return Timing.WaitForSeconds(Time.fixedDeltaTime);
            }

            handle.Complete();
            float minDistance = Distance;
            foreach(var r in results)
            {
                if (r.collider != null)
                    minDistance = Mathf.Min(minDistance, r.distance);
            }

            var movementVector = direction * minDistance;
            var newPosition = origin + movementVector;

            _lastTeleportFrom = origin;
            _lastTeleportTo = newPosition;

            _character.Controller.Move(movementVector);
            GameObject.Instantiate(TeleportTo, _character.transform);

            // Dispose the buffers
            results.Dispose();
            commands.Dispose();
        }

        public override void Deactive(MonoBehaviour target)
        {
        }

        public override void Cancel()
        {
        }

#if UNITY_EDITOR
        override public void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(_lastTeleportFrom, _lastTeleportTo);
        }
#endif

    }
}