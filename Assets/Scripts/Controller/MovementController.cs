using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    #region Serialized Variables

    [Header("Movement")]
    [SerializeField] private float _acceleration;
    [SerializeField] private AnimationCurve _accelarationCurve;
    [SerializeField] private float _stopDeceleration;
    [SerializeField] private float _basicMovementSpeed;
    [SerializeField] private float _forwardVelocityHardCap;
    [SerializeField] private float _forwardVelocitySoftCap;
    [Space]
    [Header("Camera")]
    [SerializeField] private float _cameraAngleStandingRotation;
    [SerializeField] private float _cameraSpeedStandingRotation;
    [SerializeField] private float _cameraAngleSharpTurn;
    [Space]
    [Space]
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _camera;
    [Space]
    [Space]
    [Header("Control Values")]
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private MovementStates _movementState;
    [SerializeField] private PlayerStates _playerState;

    #endregion  

    #region Private Variables

    private float _gravity; 
    private Vector3 _inputDirection;
    private Vector3 _moveDirection;
    private bool _wasMovingLastFrame;
    private bool _isCameraLock;
    private float _forwardVelocity;
    private Vector3 _cameraForwardLastFrame;
    private Vector3 _cameraRight {
        get {
            return _camera.right;
        }
    }
    private Vector3 _cameraForward {
        get { 
            return Vector3.ProjectOnPlane(_camera.forward, Vector3.up).normalized;
        }
    }

    #endregion

    private void HandleMovementStates() {
        if (_velocity.z == 0f) {
            _movementState = MovementStates.Standing;
        }
        else if (_velocity.z > 0f && _velocity.z < _basicMovementSpeed && _inputDirection.z > 0) {
            _movementState = MovementStates.BasicMove;
        }
        else if (_velocity.z >= _basicMovementSpeed && _inputDirection.z > 0) {
            _movementState = MovementStates.Accelerating;
        }
        else if (_velocity.z >= _basicMovementSpeed && _inputDirection.z <= 0) {
            _movementState = MovementStates.Decelerating;
        }
    }

    private void HandlePlayerStates() {

    }

    private float _inputDirectionZ() {
        if (_velocity.z >= _basicMovementSpeed) {
            return HandleAdvancedMovement();         
        }
        else 
        return _inputDirection.z * _basicMovementSpeed;
    }

    private void HandleBasicMovement() {
        _velocity.x = _inputDirection.x * _basicMovementSpeed;
        _velocity.z = _inputDirectionZ();
        ForwardVelocityClamp();
    }

    private float HandleAdvancedMovement() {
        //return _velocity.z + (Acceleration() * _inputDirection.z * Time.deltaTime); 
        return _velocity.z + (AccelerationCurve() * _inputDirection.z * Time.deltaTime); 
    }

    private void HandleDeceleration() {
        if (_velocity.z > _basicMovementSpeed) {
            _velocity.z = Mathf.MoveTowards(_velocity.z, 0f, ForwardDeceleration() * Time.deltaTime);
        }
    }

    private float ForwardDeceleration() {
        if (_inputDirection.z > 0) return 0f;
        else if (_inputDirection.z < 0) {
            _isCameraLock = true;
            return _stopDeceleration;  
        }  
        else {
            _isCameraLock = true;
            return _stopDeceleration; 
        } 
    }

    private void ForwardVelocityClamp() {
        _velocity.z = Mathf.Clamp(_velocity.z, -_basicMovementSpeed, _forwardVelocityHardCap);
    }
    
    private float AccelerationCurve() {
        float time = _velocity.z / _forwardVelocitySoftCap;
        return _accelarationCurve.Evaluate(time) * _acceleration;
    }

    private void Awake() {
        _characterController = GetComponent<CharacterController>();
        _gravity = Physics.gravity.y;
    }

    private void GroundCheck() {
        _isGrounded = _characterController.isGrounded;
    }

    private void HandleMovement(Vector3 value) {
        _characterController.Move(value * Time.deltaTime);
    }

    private void HandleRotation() {
        if (_isCameraLock) return;
        if (_inputDirection != Vector3.zero) transform.forward = _cameraForward;
    }

    private void MovingLastFrameCheck() {
        if (_moveDirection != Vector3.zero) {_wasMovingLastFrame = true;}
        else {_wasMovingLastFrame = false;}
    }

    private void HandleStandingRotation() {
        if (_isCameraLock) return;
        float angle = Vector3.Angle(_cameraForward, transform.forward);
        if (angle >= _cameraAngleStandingRotation) transform.forward = Vector3.MoveTowards(transform.forward, _cameraForward, _cameraSpeedStandingRotation * Time.deltaTime);
        //Debug.Log(angle);
    }

    private void HandleSharpTurn() {
        float angle = Vector3.Angle(_cameraForward, _cameraForwardLastFrame);     
        if (angle >= _cameraAngleSharpTurn) _velocity.z = 0f;  
        //Debug.Log(angle);
    }

    private void Update() {
        MovingLastFrameCheck();
        GroundCheck();

        HandleRotation();
        HandleStandingRotation();
 
        _isCameraLock = false;
        _moveDirection = _velocity.normalized;

        if (!_isGrounded) _velocity.y += _gravity;
        else if (_isGrounded) _velocity.y = 0f;

        HandleBasicMovement();
        HandleDeceleration();
        HandleSharpTurn();

        Vector3 finalVector;
        if (_movementState == MovementStates.Decelerating) {
            finalVector = _velocity.x * transform.right + _velocity.z * transform.forward + Vector3.up * _velocity.y;
        }
        else {
            finalVector = _velocity.x * _cameraRight + _velocity.z * _cameraForward + Vector3.up * _velocity.y;
        }
        HandleMovement(finalVector);

        _cameraForwardLastFrame = _cameraForward;
        HandleMovementStates();
        //HandlePlayerStates();
    }

    public void SetInputValues(Vector2 inputValues) {_inputDirection = new Vector3(inputValues.x, 0, inputValues.y);}

    public float GetForwardVelocity() {return _velocity.z;}

    public void SetCameraTransform(Transform camera) {_camera = camera;}

    //[SerializeField] private float _passiveDeceleration;

    // private float Acceleration() {
    //     if (_velocity.z >= _forwardVelocitySoftCap) return 0f;
    //     else {
    //         return _acceleration;
    //     } 
    // }
}
