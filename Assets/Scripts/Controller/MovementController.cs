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
    [SerializeField] private float _slidingDeceleration;
    [SerializeField] private float _basicMovementSpeed;
    [SerializeField] private float _forwardVelocityHardCap;
    [SerializeField] private float _forwardVelocitySoftCap;
    [SerializeField] private float _jumpHeight;
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
    [SerializeField] private float _forwardVelocity;

    #endregion  

    #region Private Variables

    private float _gravity; 
    private Vector3 _inputDirection;
    private Vector3 _moveDirection;
    private bool _wasMovingLastFrame;
    private bool _isCameraLock;
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

    private void HandleBasicMovement() {
        if (_playerState == PlayerStates.Sliding) return;
        _velocity.x = _inputDirection.x * _basicMovementSpeed;
        _velocity.z = _inputDirection.z * _basicMovementSpeed;
    }

    private Vector3 CombineVelocity() {
        Vector3 moveVector = new Vector3(_velocity.x, _velocity.y, _velocity.z + _forwardVelocity);
        return moveVector;
    }

    private void HandleAcceleration() {
        if (_playerState == PlayerStates.Jumping) return;
        if (_playerState == PlayerStates.Sliding) return;
        if (_playerState == PlayerStates.Crouching) return;
        if (!_isGrounded) return;
        _forwardVelocity += (AccelerationCurve() * _inputDirection.z * Time.deltaTime); 
        ForwardVelocityClamp();

    }

    private void HandleDeceleration() {
        if (_playerState == PlayerStates.Jumping) return;
        if (!_isGrounded) return;
        if (_forwardVelocity > 0f) {
            _forwardVelocity = Mathf.MoveTowards(_forwardVelocity, 0f, ForwardDeceleration() * Time.deltaTime);
        }
    }

    private float ForwardDeceleration() {
        if (_inputDirection.z > 0) return 0f;
        else if (_playerState == PlayerStates.Sliding) return _slidingDeceleration; 
        else {
            _isCameraLock = true;
            return _stopDeceleration; 
        } 
    }

    private void ForwardVelocityClamp() {
        _forwardVelocity = Mathf.Clamp(_forwardVelocity, 0f, _forwardVelocityHardCap);
    }
    
    private float AccelerationCurve() {
        float time = _forwardVelocity / _forwardVelocitySoftCap;
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
        if (_playerState == PlayerStates.Jumping) return;
        if (_playerState == PlayerStates.Sliding) return;
        if (_isCameraLock) return;
        if (_inputDirection != Vector3.zero) transform.forward = _cameraForward;
    }

    private void MovingLastFrameCheck() {
        if (_moveDirection != Vector3.zero) {_wasMovingLastFrame = true;}
        else {_wasMovingLastFrame = false;}
    }

    private void HandleStandingRotation() {
        if (_isCameraLock) return;
        if (_playerState == PlayerStates.Jumping) return;
        if (_playerState == PlayerStates.Sliding) return;
        if (!_isGrounded) return;
        float angle = Vector3.Angle(_cameraForward, transform.forward);
        if (angle >= _cameraAngleStandingRotation) transform.forward = Vector3.MoveTowards(transform.forward, _cameraForward, _cameraSpeedStandingRotation * Time.deltaTime);
        //Debug.Log(angle);
    }

    private void HandleSharpTurn() {
        if (_playerState == PlayerStates.Jumping) return;
        if (_playerState == PlayerStates.Sliding) return;
        if (!_isGrounded) return;
        float angle = Vector3.Angle(_cameraForward, _cameraForwardLastFrame);     
        if (angle >= _cameraAngleSharpTurn) _forwardVelocity = 0f;  
        //Debug.Log(angle);
    }

    public void HandleJump() {
        if (!_isGrounded) return;
        _velocity.y = Mathf.Sqrt(_jumpHeight * -3f * _gravity);

        // While in air, disable deceleration OR apply very small amount of deceleration.
        // Also lock the ForwardVelocity acceleration. Only ifluence movement with Basic Movement, with lower values compared to the Grounded state.
        // Also pause the HandleSharpTurn while in air. Only after landing continue calculations. (So if you continue to look at your look direction, you keep your momentum but
        // if you look at a different direction it will go down to zero.)
    }

    private void LastCameraForward() {
        if (_playerState == PlayerStates.Jumping) return;
        if (_playerState == PlayerStates.Sliding) return;
        if (!_isGrounded) return;
        _cameraForwardLastFrame = _cameraForward;
    }

    public void HandleCrouch() {
        if (!_isGrounded) return;
        if (_forwardVelocity > 0) {
            _playerState = PlayerStates.Sliding;
        }
        else {
            _playerState = PlayerStates.Crouching;
        }
    }

    private void Update() {
        MovingLastFrameCheck();
        GroundCheck();

        HandleRotation();
        HandleStandingRotation();
 
        _isCameraLock = false;
        _moveDirection = _velocity.normalized;

        if (!_isGrounded){
            _velocity.y += _gravity * Time.deltaTime;
            _playerState = PlayerStates.Jumping;
        } 
        else if (_isGrounded) {
            _velocity.y = 0f;
            _playerState = PlayerStates.Walking;
        }

        HandleBasicMovement();
        HandleAcceleration();
        HandleDeceleration();
        HandleSharpTurn();
       
        Vector3 finalVector;
        if (_movementState == MovementStates.Decelerating || _playerState == PlayerStates.Jumping) {
            finalVector = CombineVelocity().x * transform.right + CombineVelocity().z * transform.forward + Vector3.up * CombineVelocity().y;
        }
        else {
            finalVector = CombineVelocity().x * _cameraRight + CombineVelocity().z * _cameraForward + Vector3.up * CombineVelocity().y;
        }
        HandleMovement(finalVector);

        LastCameraForward();
        HandleMovementStates();
        //HandlePlayerStates();

        //_playerState = PlayerStates.Idle;
    }

    public void SetInputValues(Vector2 inputValues) {_inputDirection = new Vector3(inputValues.x, 0, inputValues.y);}

    public float GetForwardVelocity() {return CombineVelocity().z;}

    public void SetCameraTransform(Transform camera) {_camera = camera;}

    //[SerializeField] private float _passiveDeceleration;

    // private float Acceleration() {
    //     if (_velocity.z >= _forwardVelocitySoftCap) return 0f;
    //     else {
    //         return _acceleration;
    //     } 
    // }
}
