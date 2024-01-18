using UnityEngine;

namespace A01.Helper
{
    //Destroys a particle gameobject once it has finished playing
    public class ParticleCleanup : MonoBehaviour
    {
        private ParticleSystem particles = null;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            if(!particles) { Destroy(gameObject); }
        }

        private void Update()
        {
            if (!particles.IsAlive())
                Destroy(gameObject, .2f);
        }
    }
}