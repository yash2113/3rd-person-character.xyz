using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    public Animator animator;
    PlayerManager playerManager;
    PlayerLocomotion playerLocomotion;

    int horizontal;
    int vertical;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerManager = GetComponent<PlayerManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void PlayargetAnimation(string targetAnimation, bool isInteracting, bool useRootMotion = false)
    {
        animator.SetBool("isInteracting", isInteracting);
        animator.SetBool("isUsingRootMotion", useRootMotion);
        animator.CrossFade(targetAnimation, 0.2f);
    }

    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement, bool isSprinting)
    {
        //Animation Snapping
        float snapperHorizontal;
        float snapperVertical;

        #region Snapped Horizontal
        if (horizontalMovement > 0 && horizontalMovement < 0.55f)
        {
            snapperHorizontal = 0.5f;
        }
        else if (horizontalMovement > 0.55f)
        {
            snapperHorizontal = 1;
        }
        else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
        {
            snapperHorizontal = -0.5f;
        }
        else if (horizontalMovement < -0.55f)
        {
            snapperHorizontal = -1;
        }
        else
        {
            snapperHorizontal = 0;
        }
        #endregion

        #region Snapped Vertical
        if (verticalMovement > 0 && verticalMovement < 0.55f)
        {
            snapperVertical = 0.5f;
        }
        else if (verticalMovement > 0.55f)
        {
            snapperVertical = 1;
        }
        else if (verticalMovement < 0 && verticalMovement > -0.55f)
        {
            snapperVertical = -0.5f;
        }
        else if (verticalMovement < -0.55f)
        {
            snapperVertical = -1;
        }
        else
        {
            snapperVertical = 0;
        }
        #endregion

        if (isSprinting)
        {
            snapperHorizontal = horizontalMovement;
            snapperVertical = 2;
        }

        animator.SetFloat(horizontal, snapperHorizontal, 0.1f, Time.deltaTime);
        animator.SetFloat(vertical, snapperVertical, 0.1f, Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        if(playerManager.isUsingRootMotion)
        {
            playerLocomotion.playerRigidBody.drag = 0;
            Vector3 deltaPosition = animator.deltaPosition;
            deltaPosition.y = 0;
            Vector3 velocity = deltaPosition / Time.deltaTime;
            playerLocomotion.playerRigidBody.velocity = velocity;
        }
    }

}
