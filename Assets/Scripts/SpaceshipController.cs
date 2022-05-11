using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipController : MonoBehaviour
{
    [SerializeField]
    public float maxSpeed;
    [SerializeField]
    public float acceleration;
    [SerializeField]
    public float rotateSpeed;

    private Rigidbody2D rigidbody2d;

    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        float h = -Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector2 speed = -1*transform.up * (v * acceleration);
        rigidbody2d.AddForce(speed);
        transform.Rotate(Vector3.forward, h * rotateSpeed * Time.fixedDeltaTime);

        if (rigidbody2d.velocity.magnitude > maxSpeed)
        {
            rigidbody2d.velocity = rigidbody2d.velocity.normalized * maxSpeed;
        }
    }
}
