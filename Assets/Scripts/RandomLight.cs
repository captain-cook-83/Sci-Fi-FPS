using System;
using UnityEngine;

namespace Cc83
{
    public class RandomLight : MonoBehaviour
    {
        private void Start()
        {
            if (new DateTime().Millisecond % 2 == 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
