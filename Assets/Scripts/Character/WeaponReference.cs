using Cc83.Interactable;
using UnityEngine;

namespace Cc83.Character
{
    public class WeaponReference : MonoBehaviour
    {
        public WeaponRifle weapon;

        public void DropDown(ref Vector3 direction)
        {
            weapon.DropDown(ref direction);
        }
    }
}
