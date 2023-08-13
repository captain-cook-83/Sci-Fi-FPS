using UnityEngine;
using UnityEngine.InputSystem;

namespace Cc83
{
    public class RandomLight : MonoBehaviour
    {
        public InputActionReference menuAction;
        
        private void Awake()
        {
            if (menuAction)
            {
                menuAction.action.performed += OnMenuAction;
            }
        }

        private void OnDestroy()
        {
            if (menuAction)
            {
                menuAction.action.performed -= OnMenuAction;
            }
        }

        private void OnMenuAction(InputAction.CallbackContext context)
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
