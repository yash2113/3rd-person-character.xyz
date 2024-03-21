using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerLocomotion : MonoBehaviour
{
    InputManager inputManager;
    AnimatorManager animatorManager;
    PlayerManager playerManager;

    Vector3 moveDirection;
    Transform cameraObject;
    public Rigidbody playerRigidBody;

    [Header("Falling")]
    [SerializeField] private float inAirTimer;
    [SerializeField] private float leapingVelocity;
    [SerializeField] private float fallingVelocity;
    [SerializeField] private float rayCastHeightOffset = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Rolling")]
    [SerializeField] private float rollSpeed = 3f;
    [SerializeField] private float rollDistance = 4f;

    [Header("Movement Flags")]
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isCrouching;

    [Header("Movement Speeds")]
    [SerializeField] private float walkingSpeed = 1.5f;
    [SerializeField] private float runningSpeed = 5;
    [SerializeField] private float sprintingSpeed = 7;
    [SerializeField] private float rotationSpeed = 15;

    [Header("Jump Speeds")]
    [SerializeField] private float jumpHeight = 3;
    [SerializeField] private float gravityIntensity = -15;

    [Header("Sliding Speeds")]
    [SerializeField] private float slideSpeed = 5;
    [SerializeField] private float slideDistance = 5;

    [Header("Crouching Speeds")]
    [SerializeField] private float crouchingSpeedMultiplier = 0.5f;


    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        playerRigidBody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();

        if (playerManager.isInteracting)
        {
            return;
        }

        HandleMovement();
        HandleRotation();
        HandleCrouching();
    }

    #region Handle Movement
    private void HandleMovement()
    {

        if (isJumping)
        {
            return;
        }

        // Calculate the move direction based on player input and camera orientation
        moveDirection = cameraObject.forward * inputManager.GetVerticalInput();
        moveDirection = moveDirection + cameraObject.right * inputManager.GetHorizontalInput();
        moveDirection.Normalize();
        moveDirection.y = 0;

        //Change speed multiplier based on the input
        if (isSprinting)
        {
            moveDirection = moveDirection * sprintingSpeed;
        }
        else if(isCrouching)
        {
            Debug.Log("Moving at crouching");
            moveDirection = moveDirection * (walkingSpeed * crouchingSpeedMultiplier);
        }
        else
        {
            if (inputManager.moveAmount >= 0.5f)
            {
                moveDirection = moveDirection * runningSpeed;
            }
            else
            {
                moveDirection = moveDirection * walkingSpeed;
            }
        }

        // Apply movement velocity to the Rigidbody
        Vector3 movementVelocity = moveDirection;
        playerRigidBody.velocity = movementVelocity;
    }
    #endregion

    #region Handle Rotation
    private void HandleRotation()
    {
        if (isJumping)
        {
            return;
        }

        // Calculate the target direction for rotation
        Vector3 targetDirection = Vector3.zero;

        targetDirection = cameraObject.forward * inputManager.GetVerticalInput();
        targetDirection = targetDirection + cameraObject.right * inputManager.GetHorizontalInput();
        targetDirection.Normalize();
        targetDirection.y = 0;

        // Set the player's rotation to face the target direction
        if (targetDirection == Vector3.zero)
        {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }
    #endregion

    #region Handle Falling and landing
    private void HandleFallingAndLanding()
    {
        // Perform a raycast to check if the player is grounded
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;
        Vector3 targetPosition;
        rayCastOrigin.y = rayCastOrigin.y + rayCastHeightOffset;//Adjust raycast origin to perform walk on stairs and slopes
        targetPosition = transform.position;

        if (!isGrounded && !isJumping)
        {
            if (!playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Falling", true);
            }

            // Disable root motion for the animator to allow physics-based movement
            animatorManager.animator.SetBool("isUsingRootMotion", false);   
            inAirTimer = inAirTimer + Time.deltaTime;
            // Apply forces to simulate leaping and falling
            playerRigidBody.AddForce(transform.forward * leapingVelocity);
            playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        }

        if (Physics.SphereCast(rayCastOrigin, 0.2f, -Vector3.up, out hit, groundLayer))
        {
            if (!isGrounded && !playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Landing", true);
            }

            // Adjust the target position to align with the ground
            Vector3 rayCastHitPoint = hit.point;
            targetPosition.y = rayCastHitPoint.y;

            //Reset values
            inAirTimer = 0;
            isGrounded = true;
        }
        
        else
        {
            isGrounded = false;
        }

        // If the player is grounded and not jumping, smoothly interpolate the player's position to the target position
        if (isGrounded && !isJumping)
        {
            if (playerManager.isInteracting || inputManager.moveAmount > 0)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime / 0.1f);
            }
            else
            {
                transform.position = targetPosition;
            }
        }

    }
    #endregion

    #region Handle Jumping
    public void HandleJumping()
    {
        if (isGrounded)
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);

            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpingVelocity;
            playerRigidBody.velocity = playerVelocity;

        }
    }
    #endregion

    #region Handle Dodge
    public void HandleDodge()
    {
        if(playerManager.isInteracting)
        {
            return;
        }
        animatorManager.PlayTargetAnimation("Dodge", true, true);
    }
    #endregion

    #region Handle Roll
    public void HandleRoll()
    {
        StartCoroutine(HandleRollCoroutine());
    }

    public IEnumerator HandleRollCoroutine()
    {
        if (playerManager.isInteracting)
        {
            yield break;
        }

        Vector3 rollDirection = moveDirection;
        rollDirection.y = 0f;
        rollDirection.Normalize();

        // Calculate the target position for the roll
        Vector3 targetPosition = transform.position + (rollDirection * rollDistance);

        // Calculate the distance to cover
        float distanceToCover = Vector3.Distance(transform.position, targetPosition);
        float distanceCovered = 0f;

        // Calculate the time it will take to cover the distance
        float rollTime = distanceToCover / rollSpeed;

        animatorManager.PlayTargetAnimation("Roll", true, true);

        // Move the player smoothly to the target position
        while (distanceCovered < distanceToCover)
        {
            float moveDistance = rollSpeed * Time.deltaTime;
            transform.position += rollDirection * moveDistance;
            distanceCovered += moveDistance;

            yield return null; // Wait for the next frame
        }

        // Ensure the player ends up exactly at the target position
        transform.position = targetPosition;
    }
    #endregion

    #region Handle Slide
    public void HandleSlide()
    {
        // Calculate the target position for the slide based on the moveDirection and slide distance
        Vector3 targetPosition = transform.position + (moveDirection * slideDistance);

        StartCoroutine(SlideToPosition(targetPosition, slideSpeed));
    }

    public IEnumerator SlideToPosition(Vector3 targetPosition, float speed)
    {
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float time = 0;

        animatorManager.PlayTargetAnimation("Slide", true, true);

        while (time < distance / speed)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / distance);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Ensure the final position is exactly the target
    }

    #endregion

    #region Handle Crouching
    private void HandleCrouching()
    {
        if (isCrouching)
        {
            animatorManager.PlayTargetAnimation("Crouch", true);
        }
    }
    #endregion

    public bool GetIsSprinting()
    {
        return isSprinting;
    }
    public void SetIsSprinting(bool _isSprinting)
    {
        isSprinting = _isSprinting;
    }
    public bool GetIsGrounded()
    {
        return isGrounded;
    }
    public bool GetIsJumping()
    {
        return isJumping;
    }
    public void SetIsJumping(bool _isJumping)
    {
        isJumping = _isJumping;
    }
    public bool GetIsCrouching()
    {
        return isCrouching;
    }
    public void SetIsCrouching(bool _isCrouching)
    {
        isCrouching = _isCrouching;
    }
}
