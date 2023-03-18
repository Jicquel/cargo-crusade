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

[RequireComponent(typeof(BoxCollider2D))]
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
    PlayerInputActions _playerInputActions;

    Collider2D _collider;
    List<Collider2D> _collidedObjects = new List<Collider2D>();
    [SerializeField]
    ContactFilter2D _filter;

    Interactable _mostCloseInteractable;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.interpolation = RigidbodyInterpolation2D.Interpolate;

        _playerInputActions = new PlayerInputActions();
    }

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider2D>();
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

            UpdateMostCloseInteractable();
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
                                      
    private Interactable GetMostCloseInteractableInRange()
    {
        _collider.OverlapCollider(_filter, _collidedObjects);
        float distMin = float.MaxValue;
        Interactable interactable = null;

        foreach (var collider in _collidedObjects)
        {
            Interactable collidedInteractable = collider.gameObject.GetComponent<Interactable>();

            if (collidedInteractable)
            {
                float dist = Vector2.Distance(collider.transform.position, _collider.transform.position);
                if (dist < distMin)
                {
                    distMin = dist;
                    interactable = collidedInteractable;
                }
            }
        }
        return interactable;
    }

    private void UpdateMostCloseInteractable()
    {
        Interactable mostCloseInteractable = GetMostCloseInteractableInRange();

        if(mostCloseInteractable != _mostCloseInteractable) {
            _mostCloseInteractable?.DisableInteractIcon();
            _mostCloseInteractable = mostCloseInteractable;
            _mostCloseInteractable?.EnableInteractIcon();
        }
    }

    public void Interact(InputAction.CallbackContext callbackContext) {
        if(_mostCloseInteractable != null)
        {
            _mostCloseInteractable = _mostCloseInteractable.Interact();
        }
    }


    protected void OnEnable()
    {
        _playerInputActions.Player.Accelerate.performed += Accelerate;
        _playerInputActions.Player.Accelerate.canceled += Accelerate;
        _playerInputActions.Player.Accelerate.Enable();

        _playerInputActions.Player.Decelerate.performed += Decelerate;
        _playerInputActions.Player.Decelerate.canceled += Decelerate;
        _playerInputActions.Player.Decelerate.Enable();

        _playerInputActions.Player.Turn.performed += Turn;
        _playerInputActions.Player.Turn.canceled += Turn;
        _playerInputActions.Player.Turn.Enable();

        _playerInputActions.Player.Interact.started += Interact;
        _playerInputActions.Player.Interact.Enable();
    }

    protected void OnDisable()
    {
        _playerInputActions.Player.Accelerate.Disable();
        _playerInputActions.Player.Decelerate.Disable();
        _playerInputActions.Player.Turn.Disable();
        _playerInputActions.Player.Interact.Disable();

    }
}