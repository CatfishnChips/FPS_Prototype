using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    #region Serialized Variables

    [Header("Movement")]
    [SerializeField] private float _baseAcceleration;
    [SerializeField] private AnimationCurve _accelarationCurve;
    [SerializeField] private float _runningTreshold;
    [SerializeField] private float _stopDeceleration;
    [SerializeField] private float _slidingDeceleration;
    [SerializeField] private float _basicMovementSpeed;
    [SerializeField] private float _crouchingMovementSpeed;
    [SerializeField] private float _airborneMovementSpeed;
    [SerializeField] private float _forwardVelocityHardCap;
    [SerializeField] private float _forwardVelocitySoftCap;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _gravityMultiplier;
    [SerializeField] private float _maxGravity;
    [Space]
    [Header("Raycast")]
    [SerializeField] private float _slopeCheckDistance;
    [SerializeField] private float _groundCheckDistance;
    [SerializeField] private LayerMask _slopeLayer, _groundLayer;
    [SerializeField] private bool _isFacingObject;
    [SerializeField] private float _slopeAngle;
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
    [SerializeField] private PlayerStates _playerState = PlayerStates.Idle;

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
    private float _movementSpeed {
        get {
            if (_playerState == PlayerStates.Crouching) return _crouchingMovementSpeed;
            else if (!_isGrounded) return _airborneMovementSpeed;
            else if (_playerState == PlayerStates.Sliding) return 0f;
            else if (_playerState == PlayerStates.Stopping) return 0f;
            else return _basicMovementSpeed;
        }
    }
    private float _acceleration {
        get {
            if (_playerState == PlayerStates.Sliding) return 0f;
            else if (_playerState == PlayerStates.Crouching) return 0f;
            else if (_playerState == PlayerStates.Jumping) return 0f;
            else if (!_isGrounded) return 0f;
            else if (!_canAccelerate) return 0f;
            else return _baseAcceleration;
        }
    }
    private float _deceleration {
        get {
            if (_playerState == PlayerStates.Sliding) return _slidingDeceleration;
            else if (_inputDirection.z > 0) return 0f;
            else if (!_isGrounded) return 0f;
            else if (_playerState == PlayerStates.Jumping) return 0f;
            else {
                _playerState = PlayerStates.Stopping;
                return _stopDeceleration;
            } 
        }
    }
    private bool _canAccelerate {
        get {
            if (_playerState == PlayerStates.Sliding) return false;
            else if (_playerState == PlayerStates.Crouching) return false;
            else if (_playerState == PlayerStates.Jumping) return false;
            else if (!_isGrounded) return false;
            else if (_isFacingObject) return false;
            else return true;
        }
    }
    private Vector3 _characterControllerFeet {
        get {
            return transform.position + new Vector3(0f, -_characterController.height / 2, 0f);
        }
    }

    #endregion

    // private void HandlePlayerStates() {
    //     switch (_playerState) {
    //         case PlayerStates.Idle:
    //         break;
    //         case PlayerStates.Walking:
    //         break;
    //         case PlayerStates.Running:
    //         break;
    //         case PlayerStates.Crouching:
    //         break;
    //         case PlayerStates.Sliding:
    //         break;
    //         case PlayerStates.Jumping:
    //         break;
    //     }
    // }

    private void HandleBasicMovement() {
        _velocity.x = _inputDirection.x * _movementSpeed;
        _velocity.z = _inputDirectionZ();
        ForwardVelocityClamp();
    }

    private float HandleAcceleration() {
        // if (_playerState == PlayerStates.Jumping) return;
        // if (_playerState == PlayerStates.Sliding) return;
        // if (_playerState == PlayerStates.Crouching) return;
        // if (!_isGrounded) return;
        // if (_velocity.z < _basicMovementSpeed) return;
        //return _velocity.z += (AccelerationCurve() * _inputDirection.z * Time.deltaTime); 
        return _velocity.z + (AccelerationCurve() * _inputDirection.z * Time.deltaTime); 
    }

    private void HandleDeceleration() {
        if (_velocity.z > _basicMovementSpeed) {
            _velocity.z = Mathf.MoveTowards(_velocity.z, 0f, _deceleration * Time.deltaTime);
        }
    }

    private float _inputDirectionZ() {
        if (_velocity.z >= _runningTreshold) { 
            //Debug.Log("Accelerating");
            if (_playerState != PlayerStates.Crouching && _playerState != PlayerStates.Jumping && _playerState != PlayerStates.Sliding) _playerState = PlayerStates.Running;
            return HandleAcceleration(); }  
        else {
            //Debug.Log("Basic Moving");
            if (_playerState != PlayerStates.Crouching && _playerState != PlayerStates.Jumping && _playerState != PlayerStates.Sliding) _playerState = PlayerStates.Walking;
            return _inputDirection.z * _movementSpeed;
        } 
    }

    private void ForwardVelocityClamp() {
        _velocity.z = Mathf.Clamp(_velocity.z, -_basicMovementSpeed, _forwardVelocityHardCap);
    }
    
    private float AccelerationCurve() {
        float time = Mathf.Abs(_velocity.z - _baseAcceleration) / _forwardVelocitySoftCap;
        Debug.Log(time);
        return _accelarationCurve.Evaluate(time) * _acceleration;
    }

    private void Awake() {
        _characterController = GetComponent<CharacterController>();
        _gravity = Physics.gravity.y;
    }

    private void SlopeCheck() {
        Physics.Raycast(_characterControllerFeet, Vector3.down, out RaycastHit hitInfoGround, _groundCheckDistance, _groundLayer);
        Physics.Raycast(_characterControllerFeet, Vector3.down, out RaycastHit hitInfoSlope, _slopeCheckDistance, _slopeLayer);
        Debug.Log("Slope " + hitInfoSlope.collider + " " + "Ground " + hitInfoGround.collider);
        if (hitInfoSlope.transform != null) _isFacingObject = true;
        else _isFacingObject = false;
        _slopeAngle = Vector3.Angle(transform.up, hitInfoGround.normal);
        if (_slopeAngle >= _characterController.slopeLimit) HandleSlopeSlide();
    }

    private void HandleSlopeSlide() {

    }

    private void GroundCheck() {
        // if (Physics.Raycast(_characterControllerFeet, Vector3.down, _groundCheckDistance, _groundLayer)) {
        //     _isGrounded = true;
        //     if (_playerState == PlayerStates.Jumping) _playerState = PlayerStates.Idle;
        // }
        // else _isGrounded = false;
        _isGrounded = _characterController.isGrounded;
        if (_isGrounded) {
            if (_playerState == PlayerStates.Jumping) _playerState = PlayerStates.Idle;
        }
    }

    private void HandleMovement(Vector3 value) {
        _characterController.Move(value * Time.deltaTime);
    }

    private void HandleRotation() {
        if (_playerState == PlayerStates.Jumping) return;
        if (_playerState == PlayerStates.Sliding) return;
        if (_playerState == PlayerStates.Stopping) return;
        if (!_isGrounded) return;
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
        if (_playerState == PlayerStates.Stopping) return;
        if (!_isGrounded) return;
        float angle = Vector3.Angle(_cameraForward, transform.forward);
        if (angle >= _cameraAngleStandingRotation) transform.forward = Vector3.MoveTowards(transform.forward, _cameraForward, _cameraSpeedStandingRotation * Time.deltaTime);
        //Debug.Log(angle);
    }

    private void HandleSharpTurn() {
        if (_playerState == PlayerStates.Jumping) return;
        if (_playerState == PlayerStates.Sliding) return;
        if (_playerState == PlayerStates.Stopping) return;
        if (!_isGrounded) return;
        float angle = Vector3.Angle(_cameraForward, _cameraForwardLastFrame);     
        if (angle >= _cameraAngleSharpTurn) _velocity.z = 0f;  
        //Debug.Log(angle);
    }

    public void HandleJump() {
        if (!_isGrounded) return;
        _playerState = PlayerStates.Jumping;
        _velocity.y = Mathf.Sqrt(_jumpHeight * -3f * _gravity);
    }

    private void LastCameraForward() {
        if (_playerState == PlayerStates.Jumping) return;
        if (_playerState == PlayerStates.Sliding) return;
        if (_playerState == PlayerStates.Stopping) return;
        if (!_isGrounded) return;
        _cameraForwardLastFrame = _cameraForward;
    }

    public void HandleCrouch() {
        if (!_isGrounded) return;
        if (_velocity.z > _runningTreshold) {
            _playerState = PlayerStates.Sliding;
        }
        else {
            _playerState = PlayerStates.Crouching;
        }
    }

    private void FixedUpdate() {
        SlopeCheck();
    }

    private void Update() {
        MovingLastFrameCheck();

        GroundCheck();

        if (!_isGrounded){
            _velocity.y += _gravity * _gravityMultiplier * Time.deltaTime;
        } 
        else if (_isGrounded) {
            _velocity.y += _gravity * _gravityMultiplier * Time.deltaTime;
        }
        _velocity.y = Mathf.Clamp(_velocity.y, _maxGravity, 100f);

        HandleRotation();
        HandleStandingRotation();
 
        _isCameraLock = false;
        _moveDirection = _velocity.normalized;

        

        HandleBasicMovement();
        HandleDeceleration();
        HandleSharpTurn();
       
        Vector3 finalVector;
        if (_playerState == PlayerStates.Stopping || _playerState == PlayerStates.Jumping || _playerState == PlayerStates.Sliding || !_isGrounded) {
            finalVector = _velocity.x * transform.right + _velocity.z * transform.forward + Vector3.up * _velocity.y;
        }
        else {
            finalVector = _velocity.x * _cameraRight + _velocity.z * _cameraForward + Vector3.up * _velocity.y;
        }
        HandleMovement(finalVector);

        LastCameraForward();
        //HandleMovementStates();
        //HandlePlayerStates();

        //_playerState = PlayerStates.Idle;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawLine(_characterControllerFeet, Vector3.down * _groundCheckDistance + _characterControllerFeet);
        Gizmos.DrawLine(_characterControllerFeet, transform.forward * _slopeCheckDistance + _characterControllerFeet);
    }

    public void ChangePlayerState(PlayerStates state) { _playerState = state;}

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

    // private void HandleMovementStates() {
    //     if (_velocity.z == 0f) {
    //         _movementState = MovementStates.Standing;
    //     }
    //     else if (_velocity.z > 0f && _velocity.z < _basicMovementSpeed && _inputDirection.z > 0) {
    //         _movementState = MovementStates.BasicMove;
    //     }
    //     else if (_velocity.z >= _basicMovementSpeed && _inputDirection.z > 0) {
    //         _movementState = MovementStates.Accelerating;
    //     }
    //     else if (_velocity.z >= _basicMovementSpeed && _inputDirection.z <= 0) {
    //         _movementState = MovementStates.Decelerating;
    //     }
    // }

    // private Vector3 CombineVelocity() {
    //     Vector3 moveVector = new Vector3(_velocity.x, _velocity.y, _velocity.z + _forwardVelocity);
    //     return moveVector;
    // }

    //  private float HandleAdvancedMovement() {
    //     //return _velocity.z + (Acceleration() * _inputDirection.z * Time.deltaTime); 
    //     return _velocity.z + (AccelerationCurve() * _inputDirection.z * Time.deltaTime); 
    // }

    // private float ForwardDeceleration() {
    //     if (_playerState == PlayerStates.Sliding) return _slidingDeceleration; 
    //     else if (_inputDirection.z > 0) return 0f;
    //     else {
    //         //_isCameraLock = true;
    //         _playerState = PlayerStates.Stopping;
    //         return _stopDeceleration; 
    //     } 
    // }// private float ForwardDeceleration() {
    //     if (_playerState == PlayerStates.Sliding) return _slidingDeceleration; 
    //     else if (_inputDirection.z > 0) return 0f;
    //     else {
    //         //_isCameraLock = true;
    //         _playerState = PlayerStates.Stopping;
    //         return _stopDeceleration; 
    //     } 
    // }
}
