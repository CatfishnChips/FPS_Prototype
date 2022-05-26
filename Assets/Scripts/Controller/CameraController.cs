using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Serialized Variables

    [SerializeField] private Camera _camera;
    [SerializeField] private float _sensitivity;
    [SerializeField] private Transform _followTarget;
    [SerializeField] private float _cameraVerticalMinClamp, _cameraVerticalMaxClamp;
    
    #endregion

    #region Private Variables

    private Vector2 _inputAxis;
    private Vector2 _cameraRotation = Vector2.zero;

    #endregion  
 
    private void Awake() {
        _camera = GetComponent<Camera>();
        _followTarget = FindObjectOfType<PlayerManager>().transform;
    }

    private void HandleCameraRotation() {
        _cameraRotation.y += _inputAxis.y * _sensitivity * Time.deltaTime;
        _cameraRotation.x += _inputAxis.x * _sensitivity * Time.deltaTime;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, _cameraVerticalMinClamp, _cameraVerticalMaxClamp);
        
        Quaternion xQuat = Quaternion.AngleAxis(_cameraRotation.x, Vector3.up);
        Quaternion yQuat = Quaternion.AngleAxis(_cameraRotation.y, Vector3.left);
        _camera.transform.rotation = xQuat * yQuat;
    }

    private void HandleCameraMovement() {
        _camera.gameObject.transform.position = _followTarget.position + new Vector3(0f, 0.25f, 0f);
    }

    private void Update() {
        HandleCameraRotation();
    }

    private void LateUpdate() {
        HandleCameraMovement();
    }

    public void SetInputAxis(Vector2 inputAxis) {_inputAxis = inputAxis;}

    public Transform GetCameraTransform() {return _camera.transform;}

    //_camera.transform.rotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
    // _camera.transform.Rotate(Vector3.up * cameraHorizontalRotation);
    // _camera.transform.rotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
}
