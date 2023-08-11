using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Cc83.Effects
{
    [RequireComponent(typeof(DecalProjector))]
    public class AutoReleaseDecal : MonoBehaviour
    {
        [Range(0.5f, 60f)]
        public float delay = 17;

        [Range(0.5f, 5f)]
        public float fadeDuration = 3;

        [SerializeField]
        private DecalProjector projector;

        private float fadeFactor;

        private float releaseTime;

        private void OnValidate()
        {
            if (projector == null)
            {
                projector = GetComponent<DecalProjector>();
            }
        }

        private void Awake()
        {
            fadeFactor = projector.fadeFactor;
        }

        private void OnEnable()
        {
            projector.fadeFactor = fadeFactor;
            releaseTime += Time.time + delay;
        }
        
        private void Update()
        {
            var currentTime = Time.time;
            if (currentTime > releaseTime)
            {
                var destroyTime = releaseTime + fadeDuration;
                
                if (currentTime > destroyTime)
                {
                    Destroy(gameObject);
                }
                else
                {
                    projector.fadeFactor = (destroyTime - currentTime) / fadeDuration;
                }
            }
        }
    }
}
