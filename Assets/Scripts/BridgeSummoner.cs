using UnityEngine;

namespace A01.Manager
{
    public class BridgeSummoner : GameFlowEventController
    {
        [SerializeField] private GameObject bridge;
        [SerializeField] private GameObject destructionParticles;

        private Animator anim;

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        public override void TriggerGameFlowEvent(bool value)
        {
            AudioManager.instance.PlaySFX("TinyWin");
            AudioManager.instance.PlaySFX("Splashing");
            bridge.SetActive(value);

            GameManager.instance.ReBuildNavMesh();
            if (anim)
                anim.SetBool("Destroy", value);

            Instantiate(destructionParticles, transform.position, Quaternion.identity);
            Destroy(this.gameObject, 1f);
        }
    }
}

