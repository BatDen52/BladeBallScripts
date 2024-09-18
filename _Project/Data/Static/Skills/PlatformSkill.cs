using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Data.Static.Skills
{
    [CreateAssetMenu(menuName = "Skills/PlatformSkill")]
    public class PlatformSkill : Skill
    {
        [SerializeField] private float _height;
        [SerializeField] private float _duration;
        [SerializeField] private bool _shouldChangeMaterial;
        [SerializeField] private GameObject _platformPrefab;
        [SerializeField] private LayerMask _surfaceLayer;

        private readonly RaycastHit[] _hits = new RaycastHit[2];
        
        private Character _character;
        private CoroutineHandle _coroutine;
        private GameObject _platform;
        private Vector3 _startPosition;

        public override bool Activate(MonoBehaviour target)
        {
            if (!base.Activate(target))
                return false;

            _startPosition = Vector3.zero;
            
            if (_character == null)
                _character = target.GetComponent<Character>();

            Array.Clear(_hits, 0, _hits.Length);
            
            int hitsCount = Physics.RaycastNonAlloc(target.transform.position, Vector3.down, _hits, 20f, _surfaceLayer);
            
            
            GameObject surface = null;
            
            for (int i = 0; i < hitsCount; i++)
            {
                surface = _hits[i].collider.gameObject;
                _startPosition = _hits[i].point;
                break;
            }

            
            if (surface == null)
            {
                return false;
            }

            _startPosition.y -= 0.01f;
            
            _platform = Instantiate(_platformPrefab, _startPosition, Quaternion.identity);
            
            if (_shouldChangeMaterial)
            {
                ChangeMaterial(surface);
            }

            _coroutine = Timing.RunCoroutine(Move(_height));

            return true;
        }
        
        private void ChangeMaterial(GameObject surface)
        {
            Mesh platformMesh = _platform.GetComponent<MeshFilter>().sharedMesh;
            Color[] vertexColors = new Color[platformMesh.vertexCount];
    
            for (int i = 0; i < vertexColors.Length; i++)
            {
                Color[] colors = surface.GetComponent<MeshFilter>().sharedMesh.colors;
                if (colors.Length == 0)
                {
                    vertexColors[i] = Color.white;
                }
                else
                {
                    vertexColors[i] = colors[0];
                }
                
            }
    
            platformMesh.colors = vertexColors;
            Material surfaceMaterial = surface.GetComponent<Renderer>().sharedMaterial;
            Material newMaterial = new Material(surfaceMaterial);
            _platform.GetComponent<Renderer>().sharedMaterial = newMaterial;
        }

        public override void Deactive(MonoBehaviour target)
        {
            Timing.KillCoroutines(_coroutine);
            _coroutine = Timing.RunCoroutine(Move(0f, () =>
            {
                Destroy(_platform);
                _platform = null;
            }));
        }

        public override void Cancel()
        {
            Timing.KillCoroutines(_coroutine);
        }

        private IEnumerator<float> Move(float height, Action onFinished = null)
        {
            yield return Timing.WaitForSeconds(0.1f);

            float elapsedTime = 0;
            Vector3 initialScale = _platform.transform.localScale;
            Vector3 targetScale = new Vector3(initialScale.x, height, initialScale.z);

            while (elapsedTime < _duration)
            {
                float t = elapsedTime / _duration;
                _platform.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

                elapsedTime += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }

            _platform.transform.localScale = targetScale;
            
            onFinished?.Invoke();
        }
    }
}