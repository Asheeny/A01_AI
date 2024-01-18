using UnityEngine;
using UnityEngine.InputSystem;
using A01.AgentShared;
using Cinemachine;
using A01.Manager;

namespace A01.Player
{
    public class PlayerController : AgentController
    {
        [Header("Player")]
        //Note: animDampTime will affect player acceleration because root motion is in use
        [SerializeField] private float animDampTime = .5f;
        //This is set to "Player" to detect if the player is within range of an interactable, for some reason the int value would not work
        [SerializeField] private LayerMask interactMask = 0;
        [SerializeField] private CinemachineFreeLook freeLookCam;
        [SerializeField] private float camRotateSpeed = 120f;

        public bool hasDied { get; private set; }

        private PlayerInput playerInput;

        private InputAction move;
        private InputAction attack;
        private InputAction interact;
        private InputAction enableCamRotate;

        private Vector2 moveInputVector2;
        private Camera mainCam;
        private InteractionController currentInteractable;

        protected override void Awake()
        {
            base.Awake();
            playerInput = GetComponent<PlayerInput>();
            mainCam = Camera.main;

            move = playerInput.actions["Move"];
            attack = playerInput.actions["Attack"];
            interact = playerInput.actions["Interact"];
            enableCamRotate = playerInput.actions["ToggleCameraRotate"];
        }

        private void OnEnable()
        {
            move.performed += PerformMovement;
            move.canceled += PerformMovement;

            interact.performed += PerformInteract;

            attack.performed += PerformAttack;

            enableCamRotate.performed += PerformCamRotate;
            enableCamRotate.canceled += PerformCamRotate;
        }

        private void OnDisable()
        {
            move.performed -= PerformMovement;
            move.canceled -= PerformMovement;

            interact.performed -= PerformInteract;

            attack.performed -= PerformAttack;

            enableCamRotate.performed -= PerformCamRotate;
            enableCamRotate.canceled -= PerformCamRotate;
        }

        private void PerformCamRotate(InputAction.CallbackContext context)
        {
            freeLookCam.m_XAxis.m_MaxSpeed = context.ReadValueAsButton() ? camRotateSpeed : 0f;
        }

        private void PerformMovement(InputAction.CallbackContext context)
        {
            if (currentBehaviourTask == BehaviourTask.Interact)
                return;
            currentBehaviourTask = BehaviourTask.Move;
            moveInputVector2 = context.ReadValue<Vector2>();
            currentWeapon.CancelAttack();
            anim.SetBool("AttemptingAttack", false);
        }

        private void PerformAttack(InputAction.CallbackContext context)
        {
            if (currentBehaviourTask == BehaviourTask.Interact)
                return;

            //Check if our cursor is hovering over an interactable, if true then do not attack. This is to prevent playing the attack animation when attempting to interact
            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                if (hitInfo.transform.tag == "Interactable")
                    return;
            }

            currentBehaviourTask = BehaviourTask.Attack;
            currentWeapon.SetIsAttackingTrue();
        }

        private void PerformInteract(InputAction.CallbackContext context)
        {
            if (currentBehaviourTask == BehaviourTask.Interact)
                return;

            Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                if (hitInfo.transform.tag != "Interactable")
                    return;

                currentInteractable = hitInfo.transform.GetComponentInParent<InteractionController>();

                if (Physics.CheckSphere(currentInteractable.transform.position, currentInteractable.interactRange, interactMask))
                {
                    if (!currentInteractable.canInteract)
                        return;

                    currentBehaviourTask = BehaviourTask.Interact;
                    moveInputVector2 = Vector2.zero;
                    currentInteractable.SetInteractionState(true);
                }
            }
        }

        private void Update()
        {
            if (hasDied)
                return;

            CheckForDeath();
            HandleInteraction();
        }

        private void LateUpdate()
        {
            HandleRotation();
            HandleAnimator();
        }

        private void CheckForDeath()
        {
            if (currentBehaviourTask == BehaviourTask.Die)
            {
                hasDied = true;
                GameManager.instance.TriggerGameFlowEvent(false);
            }
        }

        private void HandleAnimator()
        {
            anim.SetBool("Die", currentBehaviourTask == BehaviourTask.Die);

            if (currentBehaviourTask == BehaviourTask.Die)
                return;

            //Damp for inputs so the blend tree works correctly i.e. doesn't go from 0 to 1 immediately
            anim.SetFloat("MagnitudeX", moveInputVector2.x, animDampTime, Time.deltaTime);
            anim.SetFloat("MagnitudeY", moveInputVector2.y, animDampTime, Time.deltaTime);

            anim.SetBool("AttemptingInteract", currentBehaviourTask == BehaviourTask.Interact);
            anim.SetBool("AttemptingAttack", currentWeapon.isAttacking);
        }

        private void HandleInteraction()
        {
            if (currentInteractable == null)
                return;
            if (currentBehaviourTask != BehaviourTask.Interact)
                return;

            currentBehaviourTask = currentInteractable.isInteracting ? BehaviourTask.Interact : currentBehaviourTask;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentInteractable.transform.position), Time.deltaTime * .05f);
        }

        private void HandleRotation()
        {
            if (currentBehaviourTask == BehaviourTask.Interact || currentBehaviourTask == BehaviourTask.Attack)
                return;
            if (moveInputVector2.magnitude <= 0f)
                return;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(mainCam.transform.forward), Time.deltaTime * rotateSpeed);
        }

        public void StopInteracting()
        {
            currentBehaviourTask = BehaviourTask.Idle;
        }

        /* 
        * #Animation Event in the movement animation(s)
        * This is referenced when the player moves, i.e. on each footstep
        */
        public void PlayFootstepSFX()
        {
            AudioManager.instance.PlaySFX(UnityEngine.Random.Range(0f, 1f) > .5f ? "Footstep1" : "Footstep2", 1f, UnityEngine.Random.Range(.9f, 1.1f));
            SpawnMoveParticles();
        }
    }
}
