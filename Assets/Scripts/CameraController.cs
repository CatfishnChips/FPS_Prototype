using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _sensitivity;

    [SerializeField] private Transform _followTarget;

    [SerializeField] private float _cameraVerticalMinClamp, _cameraVerticalMaxClamp;
    private Vector2 _inputAxis;
 
    private void Awake() {
        _camera = GetComponent<Camera>();
        _followTarget = FindObjectOfType<PlayerManager>().transform;
    }

    private void HandleCameraRotation() {
        float cameraVerticalRotation = -_inputAxis.y * _sensitivity * Time.deltaTime;
        float cameraHorizontalRotation = _inputAxis.x * _sensitivity * Time.deltaTime;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, _cameraVerticalMinClamp, _cameraVerticalMaxClamp);
        
        //_camera.transform.rotation = Quaternion.Euler(cameraVerticalRotation, 0f, 0f);
        _camera.transform.Rotate(Vector3.up * cameraHorizontalRotation);

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
}
