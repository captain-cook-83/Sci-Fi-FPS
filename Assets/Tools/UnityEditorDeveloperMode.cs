using UnityEngine;

namespace Cc83.Tools
{
    public class UnityEditorDeveloperMode : MonoBehaviour
    {
#if UNITY_EDITOR
        [ContextMenu("Set Editor Developer Mode")]
        private void SetEditorDeveloperMode()
        {
            UnityEditor.EditorPrefs.SetBool("DeveloperMode", true);
        }

        [ContextMenu("Clear Editor Developer Mode")]
        private void ClearEditorDeveloperMode()
        {
            UnityEditor.EditorPrefs.SetBool("DeveloperMode", false);
        }
#endif
    }
}
