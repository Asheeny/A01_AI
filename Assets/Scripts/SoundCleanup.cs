using UnityEngine;

namespace A01.Helper
{
    public class SoundCleanup : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();             
        }

        private void Start()
        {
            if(!audioSource) { Destroy(gameObject); }
        }

        void Update()
        {
            if(!audioSource.isPlaying && !audioSource.loop)
                Destroy(gameObject);
        }
    }
}