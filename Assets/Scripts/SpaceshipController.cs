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

    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private Transform bulletSpawn;

    [SerializeField]
    private float bulletSpeed = 5;

    [SerializeField]
    private float bulletRange = 10;

    [SerializeField]
    private float shootCooldown = 1;

    public float ammo;

    private CheckpointController currentCheckpoint;
    public int nextCheckpoint { get; private set; }

    private Rigidbody2D rigidbody2d;

    public bool isInTrack;

    [SerializeField]
    private float outOfTrackTime;

    private float outOfTrackTimer;

    private float nextShootTime;

    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        currentCheckpoint = null;
        nextCheckpoint = 0;
        isInTrack = true;
        outOfTrackTimer = 0;
        nextShootTime = 0;
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

        if (Input.GetKey(KeyCode.Space)) {
            Shoot();
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

    public void Shoot()
    {
        if (Time.time > nextShootTime && ammo > 0)
        {
            nextShootTime = Time.time + shootCooldown;
            ammo--;
            BulletController bullet = Instantiate(bulletPrefab, bulletSpawn.position, transform.rotation)
                .GetComponent<BulletController>();
            bullet.bulletSpeed = bulletSpeed;
            bullet.bulletRange = bulletRange;
        }
    }
}
