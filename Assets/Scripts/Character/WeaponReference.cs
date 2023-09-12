using Cc83.Interactable;
using UnityEngine;

namespace Cc83.Character
{
    public class WeaponReference : MonoBehaviour
    {
        public WeaponRifle weapon;

        public void DropDown()
        {
            if (weapon)
            {
                weapon.DropDown();
                weapon = null;
            }
        }
        
        public void DropDown(Vector3 force)
        {
            if (weapon)
            {
                weapon.DropDown(force);
                weapon = null;
            }
        }
    }
}
