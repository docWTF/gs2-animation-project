using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    public CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    public Transform cameraTransform;
    private Vector3 originalCenter;

    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float crouchSpeed = 1.0f;
    [SerializeField] private float crouchMoveHeight = 7.5f;

    private float jumpHeight = 1.5f;
    [SerializeField]
    private float gravityValue = -9.81f;
    //[SerializeField] private float rotationSpeed = 7.0f;
    [SerializeField] private float animationSmoothTime = 0.1f;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction crouchAction;

    private Animator animator;
    int moveXAnimationParameterId;
    int moveYAnimationParameterId;
    int jumpAnimationParameterId;

    private AudioSource audioSource;
    public AudioClip walkingSound;
    private bool isWalking = false;
    private bool isMoving = false;

    Vector2 currentAnimationBlendVector;
    Vector2 animationVelocity;

    public bool isCrouching = false;
    public float originalHeight;
    private Vector3 originalFeetPosition;

    

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        crouchAction = playerInput.actions["Crouch"];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalHeight = controller.height;
        originalCenter = controller.center;
        originalFeetPosition = transform.position - Vector3.up * (originalHeight / 2f);

        //Animations
        animator = GetComponent<Animator>();
        moveXAnimationParameterId = Animator.StringToHash("X_Velo");
        moveYAnimationParameterId = Animator.StringToHash("Y_Velo");
        jumpAnimationParameterId = Animator.StringToHash("IsJumping");
        originalCenter = controller.center;

        //SFX
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();
        input.Normalize();
        currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, input, ref animationVelocity, animationSmoothTime);
        Vector3 move = new Vector3(currentAnimationBlendVector.x, 0, currentAnimationBlendVector.y);
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f;
        animator.SetBool("Crouch", isCrouching);
        isMoving = input.magnitude > 0.1f;

        if (isCrouching)
        {
            controller.height = originalHeight / 1.8f;
            Vector3 newCenter = originalCenter / 1.8f;
            controller.Move(move * Time.deltaTime * crouchSpeed);
            controller.center = newCenter;
            if (isMoving)
            {
                // Crouch move
                controller.height = crouchMoveHeight;
                Vector3 updateCenter = newCenter * 1.2f;
                controller.center = updateCenter;
            }
            else
            {
                // Crouch idle
                controller.height = originalHeight / 1.8f;
            }
        }
        else
        {
            controller.height = originalHeight;
            controller.center = originalCenter;
            controller.Move(move * Time.deltaTime * playerSpeed);
        }

        // Animation
        animator.SetFloat(moveXAnimationParameterId, currentAnimationBlendVector.x);
        animator.SetFloat(moveYAnimationParameterId, currentAnimationBlendVector.y);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        // Jump
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            Debug.Log("Jump activated!");
            animator.SetTrigger("IsJumping");
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // Crouch
        if (crouchAction.triggered)
        {
            ToggleCrouch();
        }

        // Walking sound effect
        if (input.magnitude > 0 && !isWalking)
        {
            // Character started walking, play walking sound effect
            audioSource.clip = walkingSound;
            audioSource.loop = true;
            audioSource.Play();
            isWalking = true;
        }
        else if (input.magnitude == 0 && isWalking)
        {
            // Character stopped walking, stop playing walking sound effect
            audioSource.Stop();
            isWalking = false;
        }
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        audioSource.volume = isCrouching ? 0.3f : 0.8f; // Adjust volume based on crouching state
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
            SceneManager.LoadScene("You Died");
        }
    }
}
