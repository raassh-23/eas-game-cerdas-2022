using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

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

    public int currectCheckpointOrder
    {
        get
        {
            if (currentCheckpoint == null)
            {
                return -1;
            }

            return currentCheckpoint.order;
        }
    }

    private Rigidbody2D rigidbody2d;

    public bool isInTrack;

    private int pickedUpPowerup;

    [SerializeField]
    private float outOfTrackTime;

    private float outOfTrackTimer;

    private float nextShootTime;

    private float nextMineTime;

    public int currentLap;
    public int maxLap;

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

    public bool isCollidingTrackBorder;

    public bool isCollidingMeteor;

    public int currentPosition;

    public bool isPlayer = false;

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

    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();

        environmentManager = GetComponentInParent<EnvironmentManager>();

        if (environmentManager == null)
        {
            existentialReward = 1f / 20000;
            maxLap = 3;
            checkPointReward = 2f / 30;
            lapReward = 2f / 3;
        }
        else
        {
            existentialReward = 1f / environmentManager.maxStep;
            maxLap = environmentManager.maxLap;
            checkPointReward = 2f / (CheckpointController.checkpoints.Count * maxLap);
            lapReward = 2f / maxLap;
        }
    }

    private void Start()
    {
        init();
    }

    private void init()
    {
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
        currentPosition = 0;
        transform.rotation = startPosition.rotation;
        transform.position = startPosition.position;
        rigidbody2d.velocity = Vector2.zero;
        currentCheckpoint = null;
        nextCheckpoint = CheckpointController.getNextCheckpoint(currentCheckpoint);
        isCollidingTrackBorder = false;
        isCollidingMeteor = false;
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

        actionsOut.ContinuousActions.Array[0] = v;
        actionsOut.ContinuousActions.Array[1] = h;

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
        sensor.AddObservation(((transform.rotation.eulerAngles.z + 360) % 360) / 360);
        sensor.AddObservation(isCollidingTrackBorder);
        sensor.AddObservation(isCollidingMeteor);
        sensor.AddObservation((maxLap - currentLap) / maxLap);
        sensor.AddObservation(rigidbody2d.velocity);
        sensor.AddObservation(currentPosition / 5f);
        if (nextCheckpoint != null)
        {
            sensor.AddObservation(nextCheckpoint.transform.position - transform.position);
            sensor.AddObservation(((nextCheckpoint.transform.rotation.eulerAngles.z + 360) % 360) / 360);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(-1);
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        try
        {
            float forward = Mathf.Clamp(actions.ContinuousActions[0], -1, 1);
            float rotation = Mathf.Clamp(actions.ContinuousActions[1], -1, 1);

            MoveShip(rotation, forward);

            switch (actions.DiscreteActions[0])
            {
                case 1:
                    Shoot();
                    AddReward(-3 * existentialReward);
                    break;
                default:
                    break;
            }

            switch (actions.DiscreteActions[1])
            {
                case 1:
                    DropMine();
                    AddReward(-10 * existentialReward);
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

            if (isCollidingTrackBorder || isCollidingMeteor)
            {
                AddReward(-4 * existentialReward);
            }

            if (!isInTrack)
            {
                AddReward(-5 * existentialReward);
            }

            if (isReset)
            {
                AddReward(-20 * existentialReward);
                isReset = false;
            }

            if (damageTaken > 0)
            {
                AddReward(-5 * damageTaken * existentialReward);
                damageTaken = 0;
            }

            if (pickedUpPowerup > 0)
            {
                AddReward(10 * existentialReward);
                pickedUpPowerup = 0;
            }

        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }

        // if (shotHit > 0)
        // {
        //     AddReward(4 * shotHit * existentialReward);
        //     shotHit = 0;
        // }

        // if (mineHit > 0)
        // {
        //     AddReward(20 * mineHit * existentialReward);
        //     mineHit = 0;
        // }

        // if (shotMissed > 0)
        // {
        //     AddReward(-2 * shotMissed * existentialReward);
        //     shotMissed = 0;
        // }

        // if (mineMissed > 0)
        // {
        //     AddReward(-10 * mineMissed * existentialReward);
        //     mineMissed = 0;
        // }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        actionMask.SetActionEnabled(0, 1, canShoot);
        actionMask.SetActionEnabled(1, 1, canDropMine);
    }

    private void MoveShip(float rotation, float forward)
    {
        Vector2 force = -1 * transform.up * (forward * acceleration);
        rigidbody2d.AddForce(forward > 0 ? force : force * 0.33f);
        transform.Rotate(Vector3.forward, rotation * rotateSpeed * Time.fixedDeltaTime);

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

            if (isPlayer && !environmentManager.isTraining && !environmentManager.isGameOver)
            {
                AudioManager.Instance.PlayShootSFX();
            }
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

            if (isPlayer && !environmentManager.isTraining && !environmentManager.isGameOver)
            {
                AudioManager.Instance.PlayDropMineSFX();
            }
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

        if (isPlayer && !environmentManager.isTraining && !environmentManager.isGameOver)
        {
            AudioManager.Instance.PlayResetSFX();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Meteor")
            || other.gameObject.CompareTag("RaceTrackBorder"))
        {
            TakeDamage(1);
        }

        if (other.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(2);
        }

        if (other.gameObject.CompareTag("Meteor"))
        {
            isCollidingMeteor = true;
        }

        if (other.gameObject.CompareTag("RaceTrackBorder"))
        {
            isCollidingTrackBorder = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Meteor"))
        {
            isCollidingMeteor = false;
        }

        if (other.gameObject.CompareTag("RaceTrackBorder"))
        {
            isCollidingTrackBorder = false;
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
        else if (isPlayer && !environmentManager.isTraining && !environmentManager.isGameOver)
        {
            AudioManager.Instance.PlayDamagedSFX();
        }
    }

    private void PowerUp()
    {
        int randNum = Random.Range(1, 4);

        switch (randNum)
        {
            case 1:
                ammo += 20;
                break;
            case 2:
                mines += 3;
                break;
            default:
                health += 10;
                break;
        }

        if (isPlayer && !environmentManager.isTraining && !environmentManager.isGameOver)
        {
            AudioManager.Instance.PlayPowerUpSFX();
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
            nextCheckpoint = CheckpointController.getNextCheckpoint(currentCheckpoint);
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

            if (isPlayer && !environmentManager.isTraining && !environmentManager.isGameOver)
            {
                AudioManager.Instance.PlayCheckpointSFX();
            }
        }
    }

    public float GetDistanceToNextCheckpoint()
    {
        if (nextCheckpoint != null)
        {
            return Vector2.Distance(transform.position, nextCheckpoint.transform.position);
        }
        else
        {
            return -1;
        }
    }

    public void AddRelativeReward(float reward)
    {
        AddReward(reward * existentialReward);
    }
}
