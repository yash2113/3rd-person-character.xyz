using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private InputManager inputManager;

    public Transform targetTransform; // The object the camera will follow
    public Transform cameraPivot; // The object the camera uses to look up and down
    public Transform cameraTransform; //The transform of the actual camera object in the scene
    public LayerMask collisionLayers; //The layers we want our camera to collide with

    private float defaultPositon;

    private Vector3 cameraFollowVelocity = Vector3.zero;
    private Vector3 cameraVectorPosition;

    public float cameraCollisionOffset = 0.2f; // How much the camera will jump off of objects its colliding with
    public float minimumCollisionOffset = 0.2f;
    public float cameraCollisionRadius = 0.2f;
    public float cameraFollowSpeed = 0.2f;
    public float cameraLookSpeed = 2f;
    public float cameraPivotSpeed = 2f;

    public float lookAngle; //Camera looking Up and Down
    public float pivotAngle; //Camera looking left and right

    public float minimumPivotAngle = -35f;
    public float maximumPivotAngle = 35f;

    private void Awake()
    {
        inputManager = FindObjectOfType<InputManager>();
        targetTransform = FindObjectOfType<PlayerManager>().transform;
        cameraTransform = Camera.main.transform;
        defaultPositon = cameraTransform.localPosition.z;
    }

    public void HandleAllCameraMovement()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollisions();
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = Vector3.SmoothDamp
            (transform.position, targetTransform.position, ref cameraFollowVelocity, cameraFollowSpeed);

        transform.position = targetPosition;    
    }

    private void RotateCamera()
    {
        Vector3 rotation;
        Quaternion targetRotation;

        lookAngle = lookAngle + (inputManager.cameraInputX * cameraLookSpeed);
        pivotAngle = pivotAngle - (inputManager.cameraInputY * cameraPivotSpeed);
        pivotAngle = Mathf.Clamp(pivotAngle, minimumPivotAngle, maximumPivotAngle);

        rotation = Vector3.zero;
        rotation.y = lookAngle;
        targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;

        rotation = Vector3.zero;
        rotation.x = pivotAngle;
        targetRotation = Quaternion.Euler(rotation);
        cameraPivot.localRotation = targetRotation;
    }

    private void HandleCameraCollisions()
    {
        float targetPosition = defaultPositon;
        RaycastHit hit;
        Vector3 direction = cameraTransform.position - cameraPivot.position;
        direction.Normalize();

        if(Physics.SphereCast
            (cameraPivot.transform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetPosition),collisionLayers))
        {
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetPosition =- (distance - cameraCollisionOffset);
        }

        if(Mathf.Abs(targetPosition) < minimumCollisionOffset)
        {
            targetPosition = targetPosition - minimumCollisionOffset;
        }

        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.2f);
        cameraTransform.localPosition = cameraVectorPosition;

    }

}
