using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//Todo : Brake induced by drifting -> Limit max speed while difting

public class CarController : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    [SerializeField]
    UnityEvent _moveTrigger;

    [Header( "Engine" )]
    public float maxSpeed = 10;
    public float accelerationFactor = 5.0f;
    public float maxReverseSpeed = 5;
    public float reverseFactor = 2.0f;
    public float brakeFactor = 10;
    [SerializeField]
    float _currentSpeed;

    [Space()]
    [Header( "Steering" )]
    public float turnFactor = 0.1f;
    public float turnFactorWhileDrifting = 0.3f;
    public float carRotationSpeed = 1;

    [Space()]
    [Header( "Drifting" )]
    public float driftFactor = 0.95f;
    public float noneDriftFactor = 0.2f;
    public float driftBrakeFactor = 6;
    [SerializeField]
    bool _isDrifting;
    [SerializeField]
    float _currentDriftFactor;

    [Space()]
    [Header( "Wheels" )]
    public Transform[] directionalWheels;
    public float wheelsRotationSpeed = 0.5f;
    public TrailRenderer[] skidMarkTrails;

    public Vector2 Velocity { get => rigidBody.velocity; }

    float _rotationAngle = 0;

    float _forwardInput, _brakeInput, _turnAngle;

    Vector2 _engineForce, _brakeForce;

    void Start ()
    {
        _rotationAngle = rigidBody.rotation;
    }

    void Update ()
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

        if ( rigidBody.velocity.magnitude > 0 )
        {
            _moveTrigger?.Invoke();
        }

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
        rigidBody.velocity = Vector2.ClampMagnitude( rigidBody.velocity, IsGoingForward() ? maxSpeed : maxReverseSpeed );
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
        if ( IsGoingForward() )
        {
            _brakeForce = _brakeInput * brakeFactor * -transform.up;

            _brakeForce += ( _isDrifting ? 1 : 0 ) * driftBrakeFactor * -(Vector2)transform.up;
        }
        else
        {
            _brakeForce = _brakeInput * reverseFactor * -transform.up;
        }

        rigidBody.AddForce( _brakeForce, ForceMode2D.Force );
    }

    void ApplySteering ()
    {
        float minSpeedTurn = Mathf.Clamp01( rigidBody.velocity.magnitude / 8 );

        float turn = ( _isDrifting ) ? turnFactorWhileDrifting : turnFactor;

        if ( IsGoingForward() )
            _rotationAngle -= _turnAngle * turn * minSpeedTurn;
        else
            _rotationAngle -= -_turnAngle * turn * minSpeedTurn;

        rigidBody.MoveRotation( Mathf.LerpAngle( rigidBody.rotation, _rotationAngle, Time.fixedDeltaTime * carRotationSpeed ) );
    }

    void ReduceDriftForce ()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot( rigidBody.velocity, transform.up );
        Vector2 rightVelocity = transform.right * Vector2.Dot( rigidBody.velocity, transform.right );

        rigidBody.velocity = forwardVelocity + rightVelocity * _currentDriftFactor;
    }

    bool IsGoingForward ()
    {
        return Vector2.Dot( rigidBody.velocity.normalized, transform.up ) >= 0;
    }

    public void SetForwardInput ( float a_input )
    {
        _forwardInput = a_input;
    }

    public void SetBrakeInput ( float a_input )
    {
        _brakeInput = a_input;
    }

    public void SetTurnAngle ( float a_angle )
    {
        _turnAngle = Mathf.Clamp( a_angle, -30, 30 );
    }

    public void SetDriftInput ( bool a_input )
    {
        _isDrifting = a_input;
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