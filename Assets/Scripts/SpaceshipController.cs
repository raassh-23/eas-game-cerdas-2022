using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipController : MonoBehaviour
{
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float rotateSpeed;

    private CheckpointController currentCheckpoint;
    private int nextCheckpoint = 0;

    private Rigidbody2D rigidbody2d;

    public bool isInTrack;

    [SerializeField]
    private float outOfTrackTime;

    private float outOfTrackTimer;

    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        currentCheckpoint = null;
        isInTrack = true;
    }

    private void Update()
    {
        if (!isInTrack)
        {
            outOfTrackTimer += Time.deltaTime;
            if (outOfTrackTimer >= outOfTrackTime)
            {
                transform.position = currentCheckpoint.transform.position;
                rigidbody2d.velocity = Vector2.zero;
                isInTrack = true;
            }
        }
    }

    private void FixedUpdate()
    {
        float h = -Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector2 speed = -1 * transform.up * (v * acceleration);
        rigidbody2d.AddForce(speed);
        transform.Rotate(Vector3.forward, h * rotateSpeed * Time.fixedDeltaTime);

        if (rigidbody2d.velocity.magnitude > maxSpeed)
        {
            rigidbody2d.velocity = rigidbody2d.velocity.normalized * maxSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            Debug.Log("Collide with checkpoint");

            CheckpointController cp = other.gameObject.GetComponent<CheckpointController>();

            if (cp.order == nextCheckpoint)
            {
                currentCheckpoint = cp;

                if (cp.isLast)
                {
                    nextCheckpoint = 0;
                }
                else
                {
                    nextCheckpoint = currentCheckpoint.order + 1;
                }

                Debug.Log("Checkpoint " + currentCheckpoint.order);
                Debug.Log("Next Checkpoint " + nextCheckpoint);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.CompareTag("RaceTrack"))
        {
            isInTrack = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("RaceTrack"))
        {
            isInTrack = false;
            outOfTrackTimer = 0;
        }
    }
}
