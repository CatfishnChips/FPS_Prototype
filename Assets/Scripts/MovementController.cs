using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Vector3 _velocity;
    private Vector3 _direction;
    private float _speed;
    private float _mass;
    private float _gravity;

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float _weight;

    private void Awake() {
        _gravity = Physics.gravity.y;
    }

    // Velocity = Speed * Direction
    private Vector3 CalculateVelocity() {
        _velocity = _speed * _direction;
        return _velocity;
    }

    // Mass = Weight * Gravity
    private float CalculateMass() {
        _mass = _gravity * _weight;
        return _mass;
    }

    private void CalculateGravity() {
        _velocity.y -= _gravity * Time.deltaTime;
    }

    // Momentum = Mass * Velocity
    private void HandleMovement() {
        _characterController.Move(_mass * _velocity * Time.deltaTime);
    }

    private void Update() {
        CalculateVelocity();
        CalculateGravity();
        CalculateMass();
        HandleMovement();
    }
}
