using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof (CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private CharacterController controller;
    private Vector3 playerVelocity;
    private Transform cameraTransform;

    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private float animationSmoothTime = 0.1f;

    private InputAction moveAction;

    private Animator animator;
    int moveXAnimationParameterId;
    int moveZAnimationParameterId;

    private AudioSource audioSource;
    public AudioClip walkingSound;

    Vector2 currentAnimationBlendVector;
    Vector2 animationVelocity;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;

        moveAction = playerInput.actions["Move"];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Animations
        animator = GetComponent<Animator>();
        moveXAnimationParameterId = Animator.StringToHash("MoveX");
        moveZAnimationParameterId = Animator.StringToHash("MoveZ");

        //SFX
        audioSource = GetComponent<AudioSource>();

    }

    void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, input, ref animationVelocity, animationSmoothTime);
        Vector3 move = new Vector3(currentAnimationBlendVector.x, 0, currentAnimationBlendVector.y);
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f;
        controller.Move(move * Time.deltaTime * playerSpeed);

        //Animation
        animator.SetFloat(moveXAnimationParameterId, currentAnimationBlendVector.x);
        animator.SetFloat(moveZAnimationParameterId, currentAnimationBlendVector.y);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

       controller.Move(playerVelocity * Time.deltaTime);

       //Rotate towards camera
       Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
       //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    }
}
