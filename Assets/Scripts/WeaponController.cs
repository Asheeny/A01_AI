using A01.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace A01.AgentShared
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private float damageRadius = 0f;
        [SerializeField] private Transform damageTransformCenter = null;
        [field: SerializeField] public float damageValue { get; private set; } = 1f;
        [SerializeField] private float attackCooldown = .5f;
        [SerializeField] private LayerMask layerToDamage = 0;
        [SerializeField] private string weaponAttackSFX;
        [SerializeField] private string weaponHitSFX;
        public bool isAttacking { get; private set; }

        private float attackTimer;
        private bool weaponDamageActive;

        private List<GameObject> hitsThisAtatck;

        /*
         * #Animation Event        
         * Referenced at the first point in an attack animation when damage can reasonably be expected form the animation
        */
        public void BeginAttack()
        {
            if (attackTimer > 0)
                return;

            weaponDamageActive = true;
            hitsThisAtatck = new List<GameObject>();
            attackTimer = attackCooldown;


            if (!weaponAttackSFX.Equals(string.Empty))
                AudioManager.instance.PlaySFX(weaponAttackSFX, Random.Range(.9f, 1.1f), Random.Range(.9f, 1.1f));
        }

        /*
         * #Animation Event  
         * Referenced in animation when the weapon is not in a reasonable position to do damage (i.e. behind players head) 
        */
        public void ConcludeAttack()
        {
            weaponDamageActive = false;
            hitsThisAtatck.Clear();
        }

        /* 
         * #Animation Event
         * This is referenced at the end of an attack animation. Useful so an attack does not need an exit time and can be interrupted but we still know when to flag attacking as false.
        */
        public void SetIsAttackingFalse() { isAttacking = false; }
        public void SetIsAttackingTrue() { isAttacking = true; }

        //Used to interrupt atack for whatever reason (i.e. player begins moving)
        public void CancelAttack()
        {
            attackTimer = 0;
            isAttacking = false;
        }

        private void Update()
        {
            if (attackTimer >= 0)
                attackTimer -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            HandleAttack();
        }

        //Look into nonalloc for overlap
        private void HandleAttack()
        {
            if (!weaponDamageActive)
                return;

            Collider[] hits = Physics.OverlapSphere(damageTransformCenter.position, damageRadius, layerToDamage, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hits)
            {
                if (hitsThisAtatck.Contains(hit.gameObject))
                    continue;

                HealthController hitHealthController = hit.GetComponent<HealthController>();
                if (!hitHealthController || hitHealthController.currentHealth <= 0)
                    continue;

                hitsThisAtatck.Add(hit.gameObject);
                hitHealthController.TakeDamage(damageValue);

                if (!weaponHitSFX.Equals(string.Empty))
                    AudioManager.instance.PlaySFX(weaponHitSFX, Random.Range(.9f, 1.1f), Random.Range(.9f, 1.1f));
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(damageTransformCenter.position, damageRadius);
        }
    }
}
