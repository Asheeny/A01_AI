using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace A01.AgentShared
{
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem interactableParticles;
        [SerializeField] private GameObject interactionUI;
        [SerializeField] private bool requiresTrigger;
        [SerializeField] private float interactionEnableDelay;
        [field: SerializeField] public float interactRange { get; private set; }

        public bool isInteracting { get; private set; }
        public bool canInteract { get; private set; }

        public UnityEvent interactionEnabledEvent { get; private set; }

        private void OnEnable()
        {
            if (interactionEnabledEvent == null)
                interactionEnabledEvent = new UnityEvent();
            canInteract = !requiresTrigger;
        }

        private void Start()
        {
            if(canInteract)
                interactableParticles.Play();
        }

        //Will set the UI gameobject to active / inactive
        public void SetInteractionState(bool value)
        {
            if (!canInteract)
                return;
            interactionUI.SetActive(value);
            isInteracting = value;  
        }

        public IEnumerator SetCanInteract(bool value)
        {
            yield return new WaitForSeconds(interactionEnableDelay);
            //AudioManager.instance.PlaySFX("TinyWin");
            canInteract = value;
            interactableParticles.Play();
            interactionEnabledEvent.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (interactRange > 0)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, interactRange);
            }
        }
    }
}