using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    PlayerLocomotion playerLocomotion;
    AnimatorManager animatorManager;

    private Vector2 movementInput;
    private Vector2 cameraInput;

    private float cameraInputX;
    private float cameraInputY;

    public float moveAmount;
    private float verticalInput;
    private float horizontalInput;

    private bool b_Input; // sprinting input
    private bool x_Input; // dodge input
    private bool jump_Input; // jumping input
    private bool roll_Input; // rolling input
    private bool slide_Input; // sliding input
    private bool crouch_Input; // crouch input

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            // Subscribe to input events for movement and camera
            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();

            // Subscribe to input events for player actions
            //Sprinting
            playerControls.PlayerActions.B.performed += i => b_Input = true;
            playerControls.PlayerActions.B.canceled += i => b_Input = false;

            //Rolling
            playerControls.PlayerActions.Roll.performed += i => roll_Input = true;

            //Sliding
            playerControls.PlayerActions.Slide.performed += i => slide_Input = true;

            //Dodge
            playerControls.PlayerActions.X.performed += i => x_Input = true;

            //Jump 
            playerControls.PlayerActions.Jump.performed += i => jump_Input = true;

            //Crouching
            playerControls.PlayerActions.Crouch.performed += i => crouch_Input = true;
            playerControls.PlayerActions.Crouch.canceled += i => crouch_Input = false;
        }

        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleSprintingInput();
        HandleJumpingInput();
        HandleDodgeInput();
        HandleRollingInput();
        HandleSlidingInput();
        HandleCrouchingInput();
    }

    #region Handle Inputs

    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputX = cameraInput.x;
        cameraInputY = cameraInput.y;

        // Calculate the total movement input amount
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        animatorManager.UpdateAnimatorValues(0, moveAmount, playerLocomotion.GetIsSprinting());
    }

    private void HandleSprintingInput()
    {
        //Keep sprinting only availabe if sprint button pressed and spped is above 0.5f
        if (b_Input && moveAmount > 0.5f)
        {
            playerLocomotion.SetIsSprinting(true); 
        }
        else
        {
            playerLocomotion.SetIsSprinting(false);
        }
    }

    private void HandleJumpingInput()
    {
        if (jump_Input)
        {
            jump_Input = false;
            playerLocomotion.HandleJumping();
        }
    }

    private void HandleDodgeInput()
    {
        if(x_Input)
        {
            x_Input = false;
            playerLocomotion.HandleDodge();
        }
    }

    private void HandleRollingInput()
    {
        if(roll_Input) 
        {
            roll_Input = false;
            playerLocomotion.HandleRoll();
        }
    }

    private void HandleSlidingInput()
    {
        if(slide_Input)
        {
            slide_Input = false;
            playerLocomotion.HandleSlide();
        }
    }

    private void HandleCrouchingInput()
    {
        if (crouch_Input /*&& moveAmount < 0.5f*/)
        {
            playerLocomotion.SetIsCrouching(true); 
        }
        else
        {
            playerLocomotion.SetIsCrouching(false);
        }
    }
    #endregion

    public float GetCameraInputX()
    {
        return cameraInputX;
    }
    public float GetCameraInputY()
    {
        return cameraInputY;
    }
    public float GetHorizontalInput()
    {
        return horizontalInput;
    }

    public float GetVerticalInput()
    {
        return verticalInput;
    }
}
