using UnityEngine;

namespace A01.AgentShared
{
    public class HealthController : MonoBehaviour
    {
        [Min(0)][SerializeField] private float maxHealth = 0f;
        [SerializeField] private GameObject hitParticlesPrefab = null;
        [SerializeField] private GameObject deathParticlesPrefab = null;

        public float currentHealth { get; private set; }

        private AgentController agentController = null;

        private void Awake()
        {
            agentController = GetComponent<AgentController>();  
        }

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damageToTake)
        {
            if (hitParticlesPrefab)
                Instantiate(hitParticlesPrefab, transform.position, Quaternion.identity);
            currentHealth -= damageToTake;
            if (currentHealth <= 0)
                StartDeath();
        }

        private void StartDeath()
        {
            if (deathParticlesPrefab)
                Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

            if(agentController)
                agentController.HandleDeath();
        }
    }
}


