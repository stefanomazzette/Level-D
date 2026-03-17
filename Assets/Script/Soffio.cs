using UnityEngine;
using UnityEngine.InputSystem;

public class pushOject2 : MonoBehaviour
{
    [Header("Kick Settings")]
    public float kickForce = 20f;
    public float kickRange = 2f;
    public float upwardForce = 5f;
    public float kickCooldown = 1f;
    public float holdDuration = 0.5f; // How long to hold E before kicking

    [Header("Target")]
    public GameObject ball; // KICK THE ball drag
    public string ballTag = "Ball"; // found the bball

    [Header("Visuals")]
    public Transform kickPoint; // what point it need to get to kick it
    public ParticleSystem kickEffect;
    public AudioClip kickSound;

    private float lastKickTime;
    private Animator animator;
    private AudioSource audioSource;

    // Input System variables
    private PlayerInput playerInput;
    private InputAction kickAction;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool holdRequirementMet = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (ball == null && !string.IsNullOrEmpty(ballTag))
        {
            GameObject foundBall = GameObject.FindGameObjectWithTag(ballTag);
            if (foundBall != null) ball = foundBall;
        }

        // Auto create kick point
        if (kickPoint == null)
        {
            GameObject kickObj = new GameObject("KickPoint");
            kickObj.transform.SetParent(transform);
            kickObj.transform.localPosition = new Vector3(0, 0, 1f);
            kickPoint = kickObj.transform;
        }

        // Setup Input System
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
        }

        // Create the kick action with E key binding
        var actionMap = new InputActionMap("Kicking");
        kickAction = actionMap.AddAction("Kick", binding: "<Keyboard>/e");

        // Set up the hold behavior - FIXED: Removed the lambda parameters
        kickAction.started += OnKickStarted;
        kickAction.canceled += OnKickReleased;

        kickAction.Enable();
    }

    void OnDestroy()
    {
        if (kickAction != null)
        {
            // FIXED: Removed the lambda parameters here too
            kickAction.started -= OnKickStarted;
            kickAction.canceled -= OnKickReleased;
            kickAction.Disable();
        }
    }

    void Update()
    {
        if (ball == null) return;

        // Handle hold timer while E is pressed
        if (isHolding)
        {
            holdTimer += Time.deltaTime;

            // Check if hold duration requirement is met
            if (holdTimer >= holdDuration && !holdRequirementMet)
            {
                holdRequirementMet = true;
                Debug.Log("Hold requirement met - release E to kick!");

                // Optional: Visual/audio feedback that hold is ready
            }
        }
    }

    // FIXED: These methods now match the delegate signature (can accept CallbackContext parameter)
    void OnKickStarted(InputAction.CallbackContext context)
    {
        if (ball == null) return;

        // Start holding when E is pressed
        float distance = Vector3.Distance(kickPoint.position, ball.transform.position);
        if (distance <= kickRange && Time.time - lastKickTime > kickCooldown)
        {
            isHolding = true;
            holdTimer = 0f;
            holdRequirementMet = false;

            Debug.Log("Started holding E - keep holding...");
        }
    }

    void OnKickReleased(InputAction.CallbackContext context)
    {
        if (!isHolding) return; // Wasn't holding

        // Check if we held long enough AND ball is still in range
        if (holdRequirementMet)
        {
            float distance = Vector3.Distance(kickPoint.position, ball.transform.position);
            if (distance <= kickRange)
            {
                KickBall();
                lastKickTime = Time.time;
                Debug.Log($"Kicked after holding E for {holdTimer:F2} seconds!");
            }
            else
            {
                Debug.Log("Ball moved out of range - kick canceled");
            }
        }
        else
        {
            Debug.Log($"Released E too early (only held for {holdTimer:F2} seconds) - kick canceled");
        }

        // Reset hold state
        isHolding = false;
        holdRequirementMet = false;
        holdTimer = 0f;
    }

    void KickBall()
    {
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb == null) return;

        // Calculate direction to ball
        Vector3 direction = (ball.transform.position - transform.position).normalized;

        // Add upward component
        direction.y += upwardForce / kickForce;

        // Apply force
        ballRb.AddForce(direction * kickForce, ForceMode.Impulse);

        // Play effects
        PlayKickEffects();

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("Kick");
        }
    }

    void PlayKickEffects()
    {
        // Particle effects
        if (kickEffect != null)
        {
            kickEffect.transform.position = kickPoint.position;
            kickEffect.Play();
        }

        // Sound audio
        if (kickSound != null)
        {
            if (audioSource != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(kickSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(kickSound, kickPoint.position);
            }
        }
    }

    // Optional: Visual feedback in UI or with Gizmos
    public float GetHoldProgress()
    {
        if (!isHolding) return 0f;
        return Mathf.Clamp01(holdTimer / holdDuration);
    }

    public bool IsHoldRequirementMet()
    {
        return holdRequirementMet;
    }

    void OnDrawGizmosSelected()
    {
        if (kickPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(kickPoint.position, kickRange);

            if (ball != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(kickPoint.position, ball.transform.position);
            }
        }
    }
}