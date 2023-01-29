using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using static UnityEngine.InputSystem.InputAction;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerController : MonoBehaviour
{
    public float maxMagnitude = 5;
    public float acceleration = 0.5f;
    public float deceleration = 0.3f;
    public float turnCoeff = 0.4f;
    public float forwardSpeed = 0f;
    public float backwardSpeed = 0f;

    Rigidbody2D _rigidBody;
    float _turnAngle = 0f;

    [SerializeField] 
    UnityEvent _moveTrigger; 

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (forwardSpeed == 0)
        {
            if (Mathf.Abs(_rigidBody.velocity.magnitude) < deceleration)
            {
                _rigidBody.velocity = Vector2.zero;
            }
            else
            {
                _rigidBody.velocity -= Vector2.ClampMagnitude(_rigidBody.velocity, deceleration);
            }
        }
        else
        {
            _rigidBody.velocity = Vector2.ClampMagnitude(_rigidBody.velocity + (Vector2)transform.up * forwardSpeed * acceleration, maxMagnitude);
        }

        if (_rigidBody.velocity.magnitude > 0) {
            Vector2 steering = _rigidBody.velocity;
            float steeringAngle = _turnAngle * turnCoeff;
            steering = Vector2Extensions.RotateRet(steering, -Mathf.Deg2Rad * steeringAngle); 

            _rigidBody.velocity = Vector2.ClampMagnitude(_rigidBody.velocity + steering, _rigidBody.velocity.magnitude);
            _rigidBody.transform.up = _rigidBody.velocity;
            _moveTrigger.Invoke();
        }

    }

    private void OnDrawGizmos()
    {
        //if (EditorApplication.isPlaying)
        //{
        //    Gizmos.DrawLine(_rigidBody.position, _rigidBody.position + _rigidBody.velocity);
        //}
    }

    public void Accelerate(InputAction.CallbackContext callbackContext)
    {
        forwardSpeed = callbackContext.ReadValue<float>(); 
    }

    public void Decelerate(InputAction.CallbackContext callbackContext)
    {
        //Do not brake or go backward if player is already accelerating
        if (forwardSpeed == 0)
        {
            backwardSpeed = callbackContext.ReadValue<float>();
        }
    }

    public void Turn(InputAction.CallbackContext callbackContext)
    {
        _turnAngle = Mathf.Clamp(Vector2.SignedAngle(callbackContext.ReadValue<Vector2>(), Vector2.up), -30, +30);
    }
}