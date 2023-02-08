using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public Rigidbody2D rigidBody;

    public float driftFactor = 0.95f;
    public float accelerationFactor = 5.0f;
    public float turnFactor = 0.1f;

    float _rotationAngle = 0;

    float _forwardInput, _brakeInput, _turnAngle;

    Vector2 _engineForce, _brakeForce;

    void FixedUpdate ()
    {
        ApplyEngineForce();

        ApplyBrakeForce();

        ApplySteering();

        ReduceDriftForce();
    }

    private void OnDrawGizmos ()
    {
        if (_engineForce != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine( transform.position, (Vector2) transform.position + _engineForce );

            Handles.color = Color.red;
            Handles.Label( transform.position + transform.up * 2, Math.Round( _engineForce.magnitude, 1 ).ToString() );
        }

        if (_brakeForce != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine( transform.position, (Vector2) transform.position + _brakeForce );

            Handles.color = Color.black;
            Handles.Label( transform.position - transform.up * 2, Math.Round( _brakeForce.magnitude, 1 ).ToString() ) ;
        }
    }

    void ApplyEngineForce ()
    {
        if (_forwardInput == 0)
        {
            rigidBody.drag = Mathf.Lerp( rigidBody.drag, 3.0f, Time.fixedDeltaTime * 5);
        }
        else
        {
            rigidBody.drag = 0;
        }

        _engineForce = _forwardInput * accelerationFactor * transform.up;

        rigidBody.AddForce( _engineForce, ForceMode2D.Force );
    }

    void ApplyBrakeForce()
    {
        _brakeForce = _brakeInput * accelerationFactor * -transform.up;

        rigidBody.AddForce( _brakeForce, ForceMode2D.Force );
    }

    void ApplySteering ()
    {
        float minSpeedTurn = Mathf.Clamp01(rigidBody.velocity.magnitude / 8);

        _rotationAngle -= _turnAngle * turnFactor * minSpeedTurn;

        rigidBody.MoveRotation( _rotationAngle );
    }

    void ReduceDriftForce ()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot( rigidBody.velocity, transform.up );
        Vector2 rightVelocity = transform.right * Vector2.Dot( rigidBody.velocity, transform.right );

        rigidBody.velocity = forwardVelocity + rightVelocity * driftFactor;
    }

    public void Accelerate ( InputAction.CallbackContext a_context )
    {
        _forwardInput = a_context.ReadValue<float>();
    }

    public void Brake (InputAction.CallbackContext a_context)
    {
        _brakeInput = a_context.ReadValue<float>();
    }

    public void Turn ( InputAction.CallbackContext a_context )
    {
        _turnAngle = Mathf.Clamp( Vector2.SignedAngle( a_context.ReadValue<Vector2>(), Vector2.up ), -30, 30 );
    }
}