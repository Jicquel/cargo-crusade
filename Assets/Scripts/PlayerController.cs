using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rigidBody;
    public PlayerInput playerInput;

    public float maxMagnitude = 60;
    public float acceleration = 1;
    public float deceleration = 1f;

    private Vector2 input = Vector2.zero;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
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
        float forwardSpeed = input.y * acceleration;

        if (forwardSpeed == 0)
        {
            if (Mathf.Abs(rigidBody.velocity.magnitude) < deceleration)
            {
                rigidBody.velocity = Vector2.zero;
            }
            else
            {
                rigidBody.velocity -= Vector2.ClampMagnitude(rigidBody.velocity, deceleration);
            }
        }
        else
        {
            rigidBody.velocity = Vector2.ClampMagnitude(rigidBody.velocity + (Vector2)transform.up * forwardSpeed, maxMagnitude);
        }

        if (rigidBody.velocity.magnitude > 0) {
            float steeringAngle = input.x * -10f;
            transform.Rotate(new Vector3(0, 0, steeringAngle));
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(rigidBody.position, rigidBody.position+rigidBody.velocity);    
    }

    public void Move(InputAction.CallbackContext callbackContext)
    {
        input = callbackContext.ReadValue<Vector2>();
    }
}