using TMPro;
using UnityEngine;

namespace Cc83.Utils
{
    public class FPSDisplay : MonoBehaviour
    {
        public static FPSDisplay Instance { get; private set; }
        
        public TMP_Text textComp;

        [Range(0.25f, 1.0f)]
        public float period;
        
        private float _lastTickTime;

        private int _currentFrames;

        private bool _pressed;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            _currentFrames++;
            _lastTickTime += Time.deltaTime;
            
            if (_lastTickTime >= period)
            {
                textComp.text = $"FPS: {Mathf.Round(_currentFrames / _lastTickTime)}";

                _lastTickTime = 0;
                _currentFrames = 0;
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
