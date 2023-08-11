using System;
using UnityEngine;

namespace Cc83.Interactable
{
    public class MaterialType : MonoBehaviour
    {

        public MaterialTypeEnum TypeOfMaterial = MaterialTypeEnum.Plaster;

        [Serializable]
        public enum MaterialTypeEnum
        {
            Plaster,
            Metall,
            Folliage,
            Rock,
            Wood,
            Brick,
            Concrete,
            Dirt,
            Glass,
            Water
        }
    }
}
