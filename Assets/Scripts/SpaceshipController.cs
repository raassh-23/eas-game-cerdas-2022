using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Events;

public class SpaceshipController : Agent
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
    private int initAmmo = 10;

    [SerializeField]
    private float shootCooldown = 1;

    [SerializeField]
    private GameObject minePrefab;

    [SerializeField]
    private float mineCooldown = 1;

    [SerializeField]
    private int initMines = 3;

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

    private int pickedUpPowerup;

    [SerializeField]
    private float outOfTrackTime;

    private float outOfTrackTimer;

    private float nextShootTime;

    private float nextMineTime;

    public int currentLap;
    private int maxLap;
    
    public Transform startPosition;

    private int checkPointSinceLastReward;
    private float checkPointReward;
    private int lapSinceLastReward;
    private float lapReward;

    private EnvironmentManager environmentManager;
    private float existentialReward;

    public UnityEvent<SpaceshipController> onFinishedRace;

    private float damageTaken;

    public int shotHit;
    public int mineHit;

    public int shotMissed;
    public int mineMissed;

    private bool isReset;

    private bool canShoot
    {
        get
        {
            return Time.time > nextShootTime && ammo > 0;
        }
    }

    private bool canDropMine
    {
        get
        {
            return Time.time > nextMineTime && mines > 0;
        }
    }

    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();

        environmentManager = GetComponentInParent<EnvironmentManager>();

        if (environmentManager == null)
        {
            existentialReward = 1f / 20000;
            maxLap = 3;
        }
        else
        {
            existentialReward = 1f / environmentManager.maxStep;
            maxLap = environmentManager.maxLap;
            checkPointReward = 2f / (CheckpointController.checkpoints.Count * maxLap);
            lapReward = 2f / maxLap;
        }

        init();
    }

    private void init()
    {
        currentCheckpoint = null;
        nextCheckpoint = CheckpointController.getNextCheckpoint(currentCheckpoint, environmentManager);
        isInTrack = true;
        isReset = false;
        outOfTrackTimer = 0;
        nextShootTime = 0;
        nextMineTime = 0;
        currentLap = -1;
        checkPointSinceLastReward = 0;
        lapSinceLastReward = 0;
        ammo = initAmmo;
        mines = initMines;
        health = initialHealth;
        damageTaken = 0;
        pickedUpPowerup = 0;
        shotHit = 0;
        mineHit = 0;
        transform.rotation = startPosition.rotation;
        transform.position = startPosition.position;
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
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode begin");
        init();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float h = -Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        actionsOut.ContinuousActions.Array[0] = h;
        actionsOut.ContinuousActions.Array[1] = v;

        if (Input.GetMouseButton(0))
        {
            actionsOut.DiscreteActions.Array[0] = 1;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            actionsOut.DiscreteActions.Array[1] = 1;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(health);
        sensor.AddObservation(ammo);
        sensor.AddObservation(mines);
        sensor.AddObservation(isInTrack);
        sensor.AddObservation(currentLap);
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);
        sensor.AddObservation(rigidbody2d.velocity);
        sensor.AddObservation(nextCheckpoint.transform.position - transform.position);
        sensor.AddObservation(nextCheckpoint.GetDirection());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float h = actions.ContinuousActions[0];
        float v = actions.ContinuousActions[1];

        MoveShip(h, v);

        switch (actions.DiscreteActions[0])
        {
            case 1:
                Shoot();
                break;
            default:
                break;
        }

        switch (actions.DiscreteActions[1])
        {
            case 1:
                DropMine();
                break;
            default:
                break;
        }

        AddReward(-1 * existentialReward);

        if (checkPointSinceLastReward > 0)
        {
            AddReward(checkPointSinceLastReward * checkPointReward);
            checkPointSinceLastReward = 0;
        }

        if (lapSinceLastReward > 0)
        {
            AddReward(lapSinceLastReward * lapReward);
            lapSinceLastReward = 0;
        }

        if (!isInTrack)
        {
            AddReward(-4 * existentialReward);
        }

        if (isReset)
        {
            AddReward(-20 * existentialReward);
            isReset = false;
        }

        if (damageTaken > 0)
        {
            AddReward(-6 * damageTaken * existentialReward);
            damageTaken = 0;
        }

        if (pickedUpPowerup > 0)
        {
            AddReward(6 * existentialReward);
            pickedUpPowerup = 0;
        }

        if (shotHit > 0)
        {
            AddReward(4 * shotHit * existentialReward);
            shotHit = 0;
        }

        if (mineHit > 0)
        {
            AddReward(20 * mineHit * existentialReward);
            mineHit = 0;
        }

        if (shotMissed > 0)
        {
            AddReward(-2 * shotMissed * existentialReward);
            shotMissed = 0;
        }

        if (mineMissed > 0)
        {
            AddReward(-10 * mineMissed * existentialReward);
            mineMissed = 0;
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        actionMask.SetActionEnabled(0, 1, canShoot);
        actionMask.SetActionEnabled(1, 1, canDropMine);
    }

    private void MoveShip(float h, float v)
    {
        Vector2 speed = -1 * transform.up * (v * acceleration);
        rigidbody2d.AddForce(v > 0 ? speed : speed * 0.3f);
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
        if (canShoot)
        {
            nextShootTime = Time.time + shootCooldown;
            ammo--;
            BulletController bullet = Instantiate(bulletPrefab, bulletSpawn.position, transform.rotation)
                .GetComponent<BulletController>();
            bullet.bulletRange = bulletRange;
            bullet.bulletSpeed = bulletSpeed;
            bullet.shooter = this;
        }
    }

    private void DropMine()
    {
        if (canDropMine)
        {
            nextMineTime = Time.time + mineCooldown;
            mines--;
            MineController mine = Instantiate(minePrefab, mineSpawn.position, transform.rotation)
                .GetComponent<MineController>();
            mine.dropper = this;
        }
    }

    private void ResetSpaceship()
    {
        if (currentCheckpoint != null)
        {
            transform.position = currentCheckpoint.transform.position;
            transform.rotation = Quaternion.Euler(0, 0, currentCheckpoint.transform.rotation.eulerAngles.z + 90);
        }
        else
        {
            transform.position = startPosition.position;
            transform.rotation = startPosition.rotation;
        }

        rigidbody2d.velocity = Vector2.zero;
        health = initialHealth;
        isInTrack = true;
        isReset = true;
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
        damageTaken += damage;
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

        pickedUpPowerup++;
    }

    private void CheckCheckPoint(GameObject other)
    {
        Debug.Log("Collide with checkpoint");

        CheckpointController cp = other.GetComponent<CheckpointController>();

        Vector2 dir = rigidbody2d.velocity.normalized;
        Vector2 cpDir = cp.GetDirection();

        bool isInRange = Vector2.Dot(dir, cpDir) > 0;

        if (isInRange && cp == nextCheckpoint)
        {
            currentCheckpoint = cp;
            nextCheckpoint = CheckpointController.getNextCheckpoint(currentCheckpoint, environmentManager);
            checkPointSinceLastReward++;

            Debug.Log("Checkpoint " + currentCheckpoint.order);
            Debug.Log("Next Checkpoint " + nextCheckpoint.order);

            if (cp.order == 0)
            {
                currentLap++;
                lapSinceLastReward++;

                if (currentLap >= maxLap)
                {
                    Debug.Log("You win!");
                    onFinishedRace?.Invoke(this);
                    return;
                }
            }
        }
    }
}
