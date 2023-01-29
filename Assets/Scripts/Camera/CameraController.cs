using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    public Rigidbody2D target;

    Vector3 _offset;
    Vector3 _velocity;

    public float smoothSpeed = 0.1f;

    private void Start()
    {
        _offset = new Vector3(0,0,-10);
        _velocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        SmoothFollow();
    }

    private void SmoothFollow()
    {
        Vector3 targetPos = target.transform.position + _offset;
        Vector3 smoothFollow = Vector3.SmoothDamp(transform.position,
        targetPos, ref _velocity, 0.3f);

        transform.position = smoothFollow;
    }
}