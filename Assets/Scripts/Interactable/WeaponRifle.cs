using Cc83.Character;
using UnityEngine;

namespace Cc83.Interactable
{
    public class WeaponRifle : MonoBehaviour
    {
        public Transform primaryAnchor;
        public Transform secondaryAnchor;

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
            
            gameObject.GetComponent<EnemyShootController>().enabled = false;
            gameObject.GetComponent<Collider>().enabled = true;
            gameObject.AddComponent<Rigidbody>();
        }
    }
}
