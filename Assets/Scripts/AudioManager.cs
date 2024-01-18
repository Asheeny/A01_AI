using A01.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace A01.Manager
{
    [Serializable]
    public class SFX
    {
        [field: SerializeField] public string name { get; private set; }
        [field: SerializeField] public AudioClip clip { get; private set; }
        [Range(0f, 1f)][SerializeField] private float volume = 1f;
        [Range(-3f, 3f)][SerializeField] private float pitch = 1f;
        [field: SerializeField] public bool loop { get; private set; }
        public float GetVolume() { return volume; }
        public float GetPitch() { return pitch; }
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        [SerializeField] private List<SFX> allSFX;
        [SerializeField][Range(0, 1)] private float masterVol;

        private List<GameObject> activeSFX = new List<GameObject>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySFX(string nameOfSound, float volumeVariation = 1f, float pitchVariation = 1f, bool dontDestroyOnLoad = false)
        {          
            SFX sfxToPlay = allSFX.Find(x => x.name == nameOfSound);

            if(sfxToPlay == null)
            {
                Debug.LogWarning("Sound: '" + nameOfSound +  "' not found!");
                return;
            }

            GameObject sound = new GameObject((sfxToPlay.loop ? "Music":"Sound") + ": " + sfxToPlay.name);
            if(dontDestroyOnLoad)
                DontDestroyOnLoad(sound);

            AudioSource source = sound.AddComponent<AudioSource>();
            
            if(!sfxToPlay.loop)
                sound.AddComponent<SoundCleanup>();

            activeSFX.Add(sound);

            source.clip = sfxToPlay.clip;

            //Max sfx volume on AudioSource is 100%. Min volume is 0%
            source.volume = Mathf.Clamp(sfxToPlay.GetVolume() * volumeVariation * masterVol, 0f, 1f);

            //Max pitch on AudioSouce is 3. Min pitch is -3
            source.pitch = Mathf.Clamp(sfxToPlay.GetPitch() * pitchVariation, -3f, 3f);
            
            source.loop = sfxToPlay.loop;
            source.Play();
        }
    }
}
