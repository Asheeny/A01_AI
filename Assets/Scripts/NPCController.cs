using A01.AgentShared;

namespace A01.NPC
{
    public class NPCController : AgentController
    {
        private InteractionController interactionController;
        
        protected override void Awake()
        {
            interactionController = GetComponent<InteractionController>();
            base.Awake();
        }

        protected override void Start()
        {
            interactionController.interactionEnabledEvent.AddListener(SwitchToIdle);
            anim.SetBool("Threatened", !interactionController.canInteract);
        }

        private void SwitchToIdle()
        {
            anim.SetBool("Threatened", false);
        }
    }
}
