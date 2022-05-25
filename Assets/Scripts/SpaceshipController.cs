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

    [SerializeField]
    private GameObject minePrefab;

    [SerializeField]
    private float mineCooldown = 1;

    [SerializeField]
    private Transform mineSpawn;

    public float ammo;

    public float mines;

    [SerializeField]
    private float initialHealth = 10;

    public float health;

    private CheckpointController currentCheckpoint;
    public CheckpointController nextCheckpoint { get; private set; }

    private Rigidbody2D rigidbody2d;

    public bool isInTrack;

    [SerializeField]
    private float outOfTrackTime;

    private float outOfTrackTimer;

    private float nextShootTime;

    private float nextMineTime;

    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        init();
    }

    private void init()
    {
        currentCheckpoint = null;
        nextCheckpoint = CheckpointController.getNextCheckpoint(currentCheckpoint);
        isInTrack = true;
        outOfTrackTimer = 0;
        nextShootTime = 0;
        nextMineTime = 0;
        health = initialHealth;
    }

    private void Update()
    {
        if (!isInTrack)
        {
            outOfTrackTimer += Time.deltaTime;
            if (outOfTrackTimer >= outOfTrackTime)
            {
                ResetSpaceship();
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            DropMine();
        }

        if (Input.GetMouseButton(0))
        {
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
            CheckCheckPoint(other.gameObject);
        }

        if (other.gameObject.CompareTag("Mine"))
        {
            ResetSpaceship();
        }

        if (other.gameObject.CompareTag("PowerUp"))
        {
            PowerUp();
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
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

    private void DropMine()
    {
        if (Time.time > nextMineTime && mines > 0)
        {
            nextMineTime = Time.time + mineCooldown;
            mines--;
            Instantiate(minePrefab, mineSpawn.position, transform.rotation);
        }
    }

    private void ResetSpaceship()
    {
        transform.position = currentCheckpoint.transform.position;
        rigidbody2d.velocity = Vector2.zero;
        isInTrack = true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Bullet") || other.gameObject.CompareTag("Meteor"))
        {
            TakeDamage(1);
        }
    }

    private void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            ResetSpaceship();
        }
    }

    private void PowerUp()
    {
        int randNum = Random.Range(1, 4);

        switch (randNum)
        {
            case 1:
                ammo += 10;
                break;
            case 2:
                mines += 3;
                break;
            case 3:
                health += 5;
                break;
        }
    }

    private void CheckCheckPoint(GameObject other)
    {
        Debug.Log("Collide with checkpoint");

        CheckpointController cp = other.GetComponent<CheckpointController>();

        Vector2 dir = rigidbody2d.velocity;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion dirAngle = Quaternion.AngleAxis(angle, Vector3.forward);

        bool isInRange = cp.transform.eulerAngles.z >= dirAngle.eulerAngles.z - 90 && cp.transform.eulerAngles.z <= dirAngle.eulerAngles.z + 90;

        if (isInRange && cp == nextCheckpoint)
        {
            currentCheckpoint = cp;

            nextCheckpoint = CheckpointController.getNextCheckpoint(currentCheckpoint);

            Debug.Log("Checkpoint " + currentCheckpoint.order);
            Debug.Log("Next Checkpoint " + nextCheckpoint.order);
        }
    }
}
