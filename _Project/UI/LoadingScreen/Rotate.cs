using UnityEngine;

namespace _Project.UI
{
    public class Rotate : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed;
        [SerializeField] private Vector3 _axis;

        private void Update()
        {
            transform.Rotate(_axis, _rotationSpeed * Time.deltaTime);
        }
    }
}