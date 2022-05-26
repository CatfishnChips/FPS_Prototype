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
        EventManager.Instance.OnJumpPressed += OnJump;
        EventManager.Instance.OnMousePerformed += OnAim;
    }

    private void UnassignEvents() {
        EventManager.Instance.OnMovePerformed -= OnMove;
        EventManager.Instance.OnJumpPressed -= OnJump;
        EventManager.Instance.OnMousePerformed -= OnAim;
    }

    private void OnMove(Vector2 inputAxis) {
        _movementController.SetInputValues(inputAxis);
        UIManager.Instance.SetVelocityInfo(_movementController.GetForwardVelocity());
    }

    private void OnJump() {

    }

    private void OnAim(Vector2 inputAxis) {
        _cameraController.SetInputAxis(inputAxis);
    }
}
