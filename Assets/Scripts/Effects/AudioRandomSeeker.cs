using System.Collections;
using UnityEngine;

namespace Cc83.Effects
{
    public class AudioRandomSeeker : MonoBehaviour
    {
        private AkEvent _akAmbient;
        
        private void Awake()
        {
            _akAmbient = GetComponent<AkEvent>();
        }

        private void Start()
        {
            StartCoroutine(SeekPlaying(_akAmbient));
        }

        private static IEnumerator SeekPlaying(AkEvent ambient)
        {
            for (var i = 0; i < 300; i++)
            {
                yield return null;
                
                if (ambient.playingId != AkSoundEngine.AK_INVALID_PLAYING_ID)
                {
                    AkSoundEngine.SeekOnEvent(ambient.data.Id, ambient.gameObject, Random.value);
                    break;
                }
            }
        }
    }
}