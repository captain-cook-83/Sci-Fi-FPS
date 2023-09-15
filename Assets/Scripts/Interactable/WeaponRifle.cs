using UnityEngine;

namespace Cc83.Interactable
{
    public class WeaponRifle : MonoBehaviour
    {
        public Transform primaryAnchor;
        public Transform secondaryAnchor;

        private EnemyShootController _shootController;
        private Collider _collider;
        
        private Rigidbody _rigidbody;

        public void RefreshPrimaryAnchor(Vector3 localPosition, Quaternion localRotation)
        {
            primaryAnchor.SetLocalPositionAndRotation(localPosition, localRotation);
        }

        public void RefreshSecondaryAnchor(Vector3 localPosition, Quaternion localRotation)
        {
            secondaryAnchor.SetLocalPositionAndRotation(localPosition, localRotation);
        }

        public void DropDown()
        {
            transform.SetParent(null);
            
            _shootController.enabled = false;
            _collider.enabled = true;
            
            if (!_rigidbody)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
            }
        }
        
        public void DropDown(Vector3 force)
        {
            if (!_rigidbody)
            {
                DropDown();
            }
            
            _rigidbody.AddForce(force);
        }

        private void Awake()
        {
            _shootController = GetComponent<EnemyShootController>();
            _collider = GetComponent<Collider>();
        }
    }
}
