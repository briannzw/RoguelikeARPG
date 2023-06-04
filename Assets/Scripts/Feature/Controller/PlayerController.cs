using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Controller
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        public Animator Animator;

        private PlayerAction playerControls;
        private CharacterController controller;

        [Header("Movement")]
        public float moveSpeed = 5f;
        public float sprintSpeed = 8f;
        public float gravity = -9.8f;
        public float turnSmoothTime = 0.1f;

        Vector3 rawInputMovement = Vector3.zero;
        Vector3 velocity;
        Vector3 moveDirection;
        Vector3 initialPos;

        // Movement Parameters
        float speed;
        float turnSmoothVelocity = 0;

        public float Speed => speed;
        public float SpeedMultiplier => speedMultiplier;

        bool canMove = true;
        float tmpGravity = 0f;

        public float transitionSpeed = 2f;

        [Header("Environment")]
        public float pushForce;

        [Header("Modifiers")]
        [Range(.1f, 10), SerializeField] float speedMultiplier = 1;

        private AudioSource _audioSource = default;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            _audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            initialPos = transform.position;
            speed = sprintSpeed;
            playerControls = InputManager.playerAction;
            RegisterInputCallbacks();

            playerControls.Gameplay.Move.Disable();
            tmpGravity = gravity;
            gravity = 0f;
            GameManager.Instance.DungeonNavMesh.OnDungeonNavMeshBuilt += EnableControl;
        }

        void EnableControl()
        {
            gravity = tmpGravity;
            playerControls.Gameplay.Move.Enable();
        }

        private void OnEnable()
        {
            RegisterInputCallbacks();
        }

        private void OnDisable()
        {
            UnregisterInputCallbacks();
        }

        void Update()
        {
            canMove = Animator.GetBool("canMove");

            moveDirection = GetMovementInputDirection();
            velocity = new Vector3(moveDirection.x * speed * speedMultiplier, velocity.y, moveDirection.z * speed * speedMultiplier);

            // Gravity
            if (controller.isGrounded)
            {
                if (velocity.y < 0f)
                    velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            CheckOutOfBound();

            Animator.SetFloat("Movement", Mathf.MoveTowards(Animator.GetFloat("Movement"), (speed / sprintSpeed) * moveDirection.magnitude, Time.deltaTime * transitionSpeed));
        }

        #region Callbacks
        private void RegisterInputCallbacks()
        {
            if (playerControls == null) return;

            playerControls.Gameplay.Move.performed += OnMove;
            playerControls.Gameplay.Move.canceled += OnMoveCanceled;
            playerControls.Gameplay.Sprint.performed += OnSprint;
            playerControls.Gameplay.Sprint.canceled += OnSprintCanceled;
        }
        private void UnregisterInputCallbacks()
        {
            if (playerControls == null) return;

            playerControls.Gameplay.Move.performed -= OnMove;
            playerControls.Gameplay.Move.canceled -= OnMoveCanceled;
            playerControls.Gameplay.Sprint.performed -= OnSprint;
            playerControls.Gameplay.Sprint.canceled -= OnSprintCanceled;
        }
        #endregion

        #region Movement
        // Return Vector3 Move Input Direction
        private Vector3 GetMovementInputDirection()
        {
            if (!canMove) return Vector3.zero;
            if (rawInputMovement.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(rawInputMovement.x, rawInputMovement.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                return moveDirection;
            }

            return Vector3.zero;
        }
        #endregion

        #region Callback Functions
        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 inputMovement = context.ReadValue<Vector2>();
            rawInputMovement = new Vector3(inputMovement.x, 0, inputMovement.y);
        }

        public void OnMoveCanceled(InputAction.CallbackContext context)
        {
            rawInputMovement = Vector3.zero;
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            speed = moveSpeed;
        }

        public void OnSprintCanceled(InputAction.CallbackContext context)
        {
            speed = sprintSpeed;
        }
        #endregion

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb && !rb.isKinematic)
            {
                rb.velocity = hit.moveDirection * pushForce * (speed / moveSpeed);
            }
        }

        private void CheckOutOfBound()
        {
            if(transform.position.y < -5f)
            {
                velocity = Vector3.zero;
                transform.position = initialPos;
            }
        }

        public void PlayFootStepSfx()
        {
            _audioSource.Play();
        }
    }
}