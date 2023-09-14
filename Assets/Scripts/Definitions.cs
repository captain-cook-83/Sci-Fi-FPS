using UnityEngine;

namespace Cc83
{
    public static class Definitions
    {
        public static readonly int PhysicsIgnoreLayer = LayerMask.NameToLayer("PhysicsIgnore");
        
        public static readonly int UILayer = LayerMask.NameToLayer("UI");
        
        public static readonly int DefaultLayer = LayerMask.NameToLayer("Default");
        
        public static readonly int DynamicLayer = LayerMask.NameToLayer("Dynamic");
        
        public static readonly int CharacterLayer = LayerMask.NameToLayer("Character");
        
        public static readonly int WeaponLayer = LayerMask.NameToLayer("Weapon");
        
        public static readonly LayerMask ShootTargetLayerMask = (1 << DefaultLayer) | (1 << DynamicLayer) | (1 << CharacterLayer) | (1 << WeaponLayer);
    }
}
