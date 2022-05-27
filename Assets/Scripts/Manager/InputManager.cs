using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private void GetInputAxis() {
        Vector2 _inputAxis;
        _inputAxis.x = Input.GetAxisRaw("Horizontal");
        _inputAxis.y = Input.GetAxisRaw("Vertical");

        //if (_inputAxis != Vector2.zero) EventManager.Instance.OnMovePerformed?.Invoke(_inputAxis);
        EventManager.Instance.OnMovePerformed?.Invoke(_inputAxis);  
    }

    private void GetMouseAxis() {
        Vector2 _mouseAxis;
        _mouseAxis.x = Input.GetAxisRaw("Mouse X");
        _mouseAxis.y = Input.GetAxisRaw("Mouse Y");

        EventManager.Instance.OnMousePerformed?.Invoke(_mouseAxis);
    }

    private void Update() {

        GetInputAxis();
        GetMouseAxis();

        if (Input.GetButtonDown("Jump")) EventManager.Instance.OnJumpPressed?.Invoke();

        if (Input.GetKey(KeyCode.LeftControl)) EventManager.Instance.OnCrouchHeld?.Invoke();
        if (Input.GetKeyUp(KeyCode.LeftControl)) EventManager.Instance.OnCrouchUp?.Invoke();
    }
    
}
