using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region References

    #region Managers

    #endregion

    #region Controllers

    [SerializeField] private MovementController _movementController;
    [SerializeField] private CameraController _cameraController;

    #endregion

    #endregion

    private void Awake() {
        _movementController = GetComponent<MovementController>();
        _cameraController = FindObjectOfType<CameraController>();
    }

    private void Start() {
        AssignEvents();
    }

    private void OnDisable() {
        UnassignEvents();
    }

    private void AssignEvents() {
        EventManager.Instance.OnMovePerformed += OnMove;
        EventManager.Instance.OnJumpPressed += OnJumpDown;
        EventManager.Instance.OnMousePerformed += OnAim;

        EventManager.Instance.OnCrouchHeld += OnCrouch;
        EventManager.Instance.OnCrouchUp += OnCrouchUp;
    }

    private void UnassignEvents() {
        EventManager.Instance.OnMovePerformed -= OnMove;
        EventManager.Instance.OnJumpPressed -= OnJumpDown;
        EventManager.Instance.OnMousePerformed -= OnAim;

        EventManager.Instance.OnCrouchHeld -= OnCrouch;
        EventManager.Instance.OnCrouchUp -= OnCrouchUp;
    }

    private void OnMove(Vector2 inputAxis) {
        _movementController.SetInputValues(inputAxis);
        UIManager.Instance.SetVelocityInfo(_movementController.GetForwardVelocity());
    }

    private void OnJumpDown() {
        // For the PlayerState to be Jumping, we need to be sure jump is actually performed.
        //_movementController.ChangePlayerState(PlayerStates.Jumping);
        
        _movementController.HandleJump();
    }

    private void OnAim(Vector2 inputAxis) {
        _cameraController.SetInputAxis(inputAxis);

        // No need to set this every frame. Maybe, instead of sending the Transform, directly send the forward and right.
        _movementController.SetCameraTransform(_cameraController.GetCameraTransform());
    }

    private void OnCrouch() {
        // HandleCrouch decides between Crouching and Sliding states.
        //_movementController.ChangePlayerState(PlayerStates.Crouching);

        _movementController.HandleCrouch();
    }

    private void OnCrouchUp() {
        _movementController.ChangePlayerState(PlayerStates.Idle);
    }
}
