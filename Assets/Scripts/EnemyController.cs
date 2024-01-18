using UnityEngine;
using A01.AgentShared;

namespace A01.Enemy
{
    public class EnemyController : AgentController
    {
        [Header("Enemy")]
        [SerializeField] private LayerMask aggressionLayer = 0;
        [SerializeField] private Vector3 attackLookAtOffset;

        private GameObject player;
        private bool isPlayerWithinAttackRange;


        protected override void Awake()
        {
            base.Awake();
            player = GameObject.FindGameObjectWithTag("Player");
        }

        protected override void Start()
        {
            currentBehaviourTask = BehaviourTask.RandomPatrol;
        }

        private void Update()
        {
            switch (currentBehaviourTask)
            {
                case BehaviourTask.RandomPatrol:
                    anim.SetBool("isMoving", true);
                    anim.SetBool("isAttacking", false);

                    if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                        StartCoroutine(GetRandomPatrolPos());
                    break;

                case BehaviourTask.Aggression:
                    anim.SetBool("isMoving", true);
                    anim.SetBool("isAttacking", false);

                    navMeshAgent.SetDestination(player.transform.position);

                    if (isPlayerWithinAttackRange)
                        currentBehaviourTask = BehaviourTask.Attack;
                    break;

                case BehaviourTask.Attack:
                    anim.SetBool("isAttacking", currentWeapon.isAttacking);
                    anim.SetBool("isMoving", false);

                    transform.LookAt(player.transform.position + attackLookAtOffset);      
                    currentWeapon.SetIsAttackingTrue();
                    break;

                case BehaviourTask.Die:
                    anim.SetBool("Die", true);

                    if (toggleInteractionOf)
                        toggleInteractionOf.StartCoroutine(toggleInteractionOf.SetCanInteract(true));
                    break;

                case BehaviourTask.Idle:
                    anim.SetBool("isAttacking", false);
                    anim.SetBool("isMoving", false);
                    break;

                default:
                    break;
            }
        }

        private void FixedUpdate()
        {
            CheckAggression();
        }

        private void CheckAggression()
        {
            if (currentBehaviourTask == BehaviourTask.Die)
                return;
            if (currentBehaviourTask == BehaviourTask.Aggression || currentBehaviourTask == BehaviourTask.Attack)
                isPlayerWithinAttackRange = Physics.CheckSphere(attackDirectionTransform.position, attackRangeRadius, aggressionLayer);

            if (Physics.CheckSphere(transform.position, aggressionRegionRadius, aggressionLayer))
            {
                if (isPlayerWithinAttackRange)
                    return;
                currentBehaviourTask = BehaviourTask.Aggression;
            }
            else
            {
                currentBehaviourTask = BehaviourTask.RandomPatrol;
            }
        }

        /*
         * #Animation Event        
         * Referenced at the end of the death animation
        */
        public void DestroyDelay() 
        {
            if (currentBehaviourTask != BehaviourTask.Die)
                return;
            Destroy(gameObject);
        }
    }
}
