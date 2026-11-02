
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour {
    [Header("Ruch")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;
    
    [Header("Sprint")]
    [SerializeField] private float sprintSpeed = 8f;
    
    [Header("Kucanie")]
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchCameraHeight = 0.35f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    
    [Header("Walidacja kucania")]
    [SerializeField] private LayerMask crouchObstacleMask = ~0;
    [SerializeField] private float crouchCheckPadding = 0.02f;

    [Header("Kamera")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 0.1f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float cameraRotationX;
    
    private float standingHeight;
    private float standingCameraHeight;
    private bool isCrouching;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        
        standingHeight = characterController.height;
        standingCameraHeight = playerCamera.localPosition.y;
    }

    private void Start() {
        LockCursor();
    }

    private void Update() {
        HandleCursor();
        HandleCrouch();
        MovePlayer();
        RotateCamera();
    }

    private void HandleCrouch() {
        if (Keyboard.current == null) {
            return;
        }

        bool wantsToCrouch = Keyboard.current.leftCtrlKey.isPressed;

        if (wantsToCrouch) {
            isCrouching = true;
        }
        else if (isCrouching && CanStandUp()) {
            isCrouching = false;
        }

        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        float targetCameraHeight = isCrouching ? crouchCameraHeight : standingCameraHeight;

        characterController.height = Mathf.Lerp(characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        Vector3 cameraPosition = playerCamera.localPosition;
        cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetCameraHeight, crouchTransitionSpeed * Time.deltaTime);
        playerCamera.localPosition = cameraPosition;
    }


    private bool CanStandUp() {
        float checkRadius = characterController.radius - crouchCheckPadding;
        float checkHeight = (standingHeight - crouchHeight) / 2f;

        Vector3 capsuleCenter = transform.TransformPoint(characterController.center);

        Vector3 checkCenter = capsuleCenter + Vector3.up * ((standingHeight + crouchHeight) / 4f);
        Vector3 halfExtents = new Vector3(checkRadius, checkHeight / 2f, checkRadius);

        return !Physics.CheckBox(checkCenter, halfExtents, transform.rotation, crouchObstacleMask, QueryTriggerInteraction.Ignore);
    }


    private void HandleCursor() {
        if (Keyboard.current == null || Mouse.current == null) {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Cursor.lockState != CursorLockMode.Locked && Mouse.current.leftButton.wasPressedThisFrame) {
            LockCursor();
        }
    }


    private void LockCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void MovePlayer() {
        if (Keyboard.current == null) {
            return;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) input.y += 1f;
        if (Keyboard.current.sKey.isPressed) input.y -= 1f;
        if (Keyboard.current.dKey.isPressed) input.x += 1f;
        if (Keyboard.current.aKey.isPressed) input.x -= 1f;

        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;

        if (characterController.isGrounded && verticalVelocity < 0f) {
            verticalVelocity = -2f;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && characterController.isGrounded) {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        float currentSpeed = moveSpeed;

        if (isCrouching) {
            currentSpeed = crouchSpeed;
        }
        else if (Keyboard.current.leftShiftKey.isPressed)  {
            currentSpeed = sprintSpeed;
        }
        
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }


    private void RotateCamera() {
        if (Mouse.current == null || playerCamera == null || Cursor.lockState != CursorLockMode.Locked) {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraRotationX -= mouseY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -85f, 85f);

        playerCamera.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
    }

    private void OnDisable() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
