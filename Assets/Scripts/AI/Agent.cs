using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Agent : MonoBehaviour
{
    public Transform target;
    public Rigidbody2D targetRb;
    public CarController controller;
    public float targetDistance = 5;
    public bool follow = false;

    float _turnAngle, _forwardInput;

    Vector2 _predictedTargetPosition, _lastTargetPosition;

    private void OnDrawGizmos ()
    {
#if UNITY_EDITOR
        if ( target )
        {
            Gizmos.DrawCube( _predictedTargetPosition, new Vector3( 2, 2 ) );
        }
#endif
    }

    void Start ()
    {
        if ( target )
        {
            _lastTargetPosition = target.position;
        }
    }

    void FixedUpdate ()
    {
        if ( target && follow )
        {
            _predictedTargetPosition = PredictTargetMovement();

            _lastTargetPosition = target.position;

            _turnAngle = CalulateSteeringInput();
            _forwardInput = CalculateThrottleOrBrake();

            controller.SetTurnAngle( _turnAngle );
            controller.SetForwardInput( _forwardInput );
        }
    }

    Vector2 PredictTargetMovement ()
    {
        Vector2 direction = (Vector2)target.position - _lastTargetPosition;

        float prediction;
        float distance = Vector2.Distance( target.position, transform.position );

        prediction = ( distance < targetDistance ) ? distance / targetDistance : 1;

        return (Vector2)target.position + prediction * targetRb.velocity.magnitude * direction.normalized;
    }

    float CalulateSteeringInput ()
    {
        Vector2 toTarget = ( _predictedTargetPosition - (Vector2)transform.position ).normalized;

        return -Vector2.SignedAngle( transform.up, toTarget );
    }

    float CalculateThrottleOrBrake ()
    {
        float distance = Vector2.Distance( _predictedTargetPosition, transform.position );

        return ( distance < targetDistance ) ? distance / targetDistance : 1;
    }
}
