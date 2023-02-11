using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;


// TODO: 
// -Add small brake when drifting
// -Add inertia related to a mass value (maybe juste change mass of rigidbody)
// -Add max reverse speed

public class CarController : MonoBehaviour
{
    public Rigidbody2D rigidBody;

    [Header( "Engine" )]
    public float maxSpeed = 10;
    public float accelerationFactor = 5.0f;
    [SerializeField]
    float _currentSpeed;

    [Space()]
    [Header( "Steering" )]
    public float turnFactor = 0.1f;
    public float turnFactorWhileDrifting = 0.3f;

    [Space()]
    [Header( "Drifting" )]
    public float driftFactor = 0.95f;
    public float noneDriftFactor = 0.2f;
    [SerializeField]
    bool _isDrifting;
    [SerializeField]
    float _currentDriftFactor;

    [Space()]
    [Header( "Wheels" )]
    public Transform[] directionalWheels;
    public float wheelsRotationSpeed = 0.5f;
    public TrailRenderer[] skidMarkTrails;

    float _rotationAngle = 0;

    float _forwardInput, _brakeInput, _turnAngle;

    Vector2 _engineForce, _brakeForce;

    private void Update ()
    {
        ApplyTurnAngleToWheels();
    }

    void FixedUpdate ()
    {
        ApplyEngineForce();

        ApplyBrakeForce();

        ApplySteering();

        ApplyDrag();

        ReduceDriftForce();

        LimitMaxVelocity();

        _currentDriftFactor = Mathf.Lerp( _currentDriftFactor, ( _isDrifting ) ? driftFactor : noneDriftFactor, 0.1f );

        _currentSpeed = rigidBody.velocity.magnitude;
    }

    private void OnDrawGizmos ()
    {
#if UNITY_EDITOR
        if ( _engineForce != null )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine( transform.position, (Vector2)transform.position + _engineForce );

            Handles.color = Color.red;
            Handles.Label( transform.position + transform.up * 2, Math.Round( _engineForce.magnitude, 1 ).ToString() );
        }

        if ( _brakeForce != null )
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine( transform.position, (Vector2)transform.position + _brakeForce );

            Handles.color = Color.black;
            Handles.Label( transform.position - transform.up * 2, Math.Round( _brakeForce.magnitude, 1 ).ToString() );
        }
#endif
    }

    void ApplyTurnAngleToWheels ()
    {
        Quaternion wheelAngle = Quaternion.AngleAxis( -_turnAngle, transform.forward );
        wheelAngle *= transform.rotation;

        for ( int i = 0 ; i < directionalWheels.Length ; i++ )
        {
            directionalWheels[ i ].rotation = Quaternion.Lerp( directionalWheels[ i ].rotation, wheelAngle, Time.deltaTime * wheelsRotationSpeed );
        }
    }

    void LimitMaxVelocity ()
    {
        rigidBody.velocity = Vector2.ClampMagnitude( rigidBody.velocity, maxSpeed );
    }

    void ApplyDrag ()
    {
        if ( _forwardInput == 0 && _brakeInput == 0 )
        {
            rigidBody.drag = Mathf.Lerp( rigidBody.drag, 3.0f, Time.fixedDeltaTime * 0.5f );
        }
        else
        {
            rigidBody.drag = 0;
        }
    }

    void ApplyEngineForce ()
    {
        _engineForce = _forwardInput * accelerationFactor * transform.up;

        rigidBody.AddForce( _engineForce, ForceMode2D.Force );
    }

    void ApplyBrakeForce ()
    {
        _brakeForce = _brakeInput * accelerationFactor * -transform.up;

        rigidBody.AddForce( _brakeForce, ForceMode2D.Force );
    }

    void ApplySteering ()
    {
        float minSpeedTurn = Mathf.Clamp01( rigidBody.velocity.magnitude / 8 );

        float turn = ( _isDrifting ) ? turnFactorWhileDrifting : turnFactor;

        if ( Vector2.Dot( rigidBody.velocity.normalized, transform.up ) < 0 )
            _rotationAngle -= -_turnAngle * turn * minSpeedTurn;
        else
            _rotationAngle -= _turnAngle * turn * minSpeedTurn;

        rigidBody.MoveRotation( _rotationAngle );
    }

    void ReduceDriftForce ()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot( rigidBody.velocity, transform.up );
        Vector2 rightVelocity = transform.right * Vector2.Dot( rigidBody.velocity, transform.right );

        rigidBody.velocity = forwardVelocity + rightVelocity * _currentDriftFactor;
    }

    public void Accelerate ( InputAction.CallbackContext a_context )
    {
        _forwardInput = a_context.ReadValue<float>();
    }

    public void Brake ( InputAction.CallbackContext a_context )
    {
        _brakeInput = a_context.ReadValue<float>();
    }

    public void Turn ( InputAction.CallbackContext a_context )
    {
        _turnAngle = Mathf.Clamp( Vector2.SignedAngle( a_context.ReadValue<Vector2>(), Vector2.up ), -30, 30 );
    }

    public void Drift ( InputAction.CallbackContext a_context )
    {
        _isDrifting = a_context.ReadValue<float>() == 1;

        for ( int i = 0 ; i < skidMarkTrails.Length ; i++ )
        {
            skidMarkTrails[ i ].emitting = _isDrifting;
        }
    }
}