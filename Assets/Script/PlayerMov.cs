using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FpsController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Camera PlayerCamera;
    public float walkSpeed = 5f;
    public float runSpeed = 13f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    [Header("Look Settings")]
    public float lookSpeed = 2f;
    public float lookXlimit = 45f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    public bool canMove = true;

    CharacterController characterController;
    Vector2 moveInput;
    Vector2 lookInput;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        // Enable all input actions
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        jumpAction?.action.Enable();
        sprintAction?.action.Enable();
    }

    void OnDisable()
    {
        // Disable all input actions
        moveAction?.action.Disable();
        lookAction?.action.Disable();
        jumpAction?.action.Disable();
        sprintAction?.action.Disable();
    }

    void Update()
    {
        // Read input values
        moveInput = moveAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;
        lookInput = lookAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;
        bool isRunning = sprintAction?.action?.IsPressed() ?? false;
        bool isJumping = jumpAction?.action?.WasPressedThisFrame() ?? false;

        // Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * moveInput.y : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * moveInput.x : 0;
        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jump
        if (isJumping && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Camera look
        if (canMove)
        {
            // Vertical look (mouse Y)
            rotationX += -lookInput.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXlimit, lookXlimit);
            PlayerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            // Horizontal look (mouse X)
            transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);
        }
    }
}