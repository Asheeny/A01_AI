using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace A01.AgentShared
{
    public enum BehaviourTask
    {
        HaltInteract,
        Idle,
        RandomPatrol,
        Move,
        Aggression,
        Attack,
        Interact,
        Die
    }

    public class AgentController : MonoBehaviour
    {
        [Header("Random Patrol")]
        [SerializeField] private protected Transform patrolRegionCenterTransform = null;
        [SerializeField] private protected float patrolRegionRadius = 10f;
        [SerializeField][Min(0f)] private protected float minPatrolWait = 1.5f;
        [SerializeField][Min(0f)] private protected float maxPatrolWait = 4f;

        [Header("Aggression")]
        [SerializeField] private protected float aggressionRegionRadius = 5f;
        [SerializeField] private protected Transform attackDirectionTransform = null;
        [SerializeField] private protected float attackRangeRadius;

        //[Header("Interaction")]
        //Should this Agent be involved in toggling another agents interaction state i.e. move the target agent from "HaltInteract" to "Interact"
        [field: SerializeField] public InteractionController toggleInteractionOf { get; private set; }

        [Header("General")]
        [SerializeField] private protected ParticleSystem moveParticlesPrefab = null;
        [SerializeField] private protected Transform moveParticlesPos = null;
        [SerializeField] private protected float rotateSpeed = 1.2f;
        private protected Animator anim;
        private protected WeaponController currentWeapon;
        private protected HealthController health;
        private protected NavMeshAgent navMeshAgent;
        private protected BehaviourTask currentBehaviourTask;

        protected virtual void Awake()
        {
            anim = GetComponent<Animator>();
            currentWeapon = GetComponent<WeaponController>();
            health = GetComponent<HealthController>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        protected virtual void Start()
        {
            currentBehaviourTask = BehaviourTask.Idle;
        }

        private protected IEnumerator GetRandomPatrolPos()
        {
            Vector3 positionToMoveto;
            Vector3 randomPatrolPoint = patrolRegionCenterTransform.position + Random.insideUnitSphere * patrolRegionRadius;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPatrolPoint, out hit, 1f, NavMesh.AllAreas))
            {
                positionToMoveto = hit.position;
                currentBehaviourTask = BehaviourTask.Idle;
                yield return new WaitForSeconds(Random.Range(minPatrolWait, maxPatrolWait));
                navMeshAgent.SetDestination(positionToMoveto);
            }
            else
            {
                positionToMoveto = Vector3.zero;
                yield return null;
            }
        }

        internal void HandleDeath()
        {
            currentBehaviourTask = BehaviourTask.Die;
        }

        /* 
        * #Animation Event in the movement animation(s)
        * This is referenced when an agent moves, i.e. on each footstep
        */
        public void SpawnMoveParticles()
        {
            if (moveParticlesPrefab)
                Instantiate(moveParticlesPrefab, moveParticlesPos.position, Quaternion.identity);
        }

        private void OnDrawGizmos()
        {
            if (aggressionRegionRadius > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, aggressionRegionRadius);
            }

            if (patrolRegionCenterTransform)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(patrolRegionCenterTransform.position, patrolRegionRadius);
            }

            if (attackDirectionTransform)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(attackDirectionTransform.position, attackRangeRadius);
            }
        }
    }
}
