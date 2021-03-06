using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrackController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] meteors;

    [SerializeField]
    private Transform meteorsGroup;

    [SerializeField]
    private int meteorsCount;

    [SerializeField]
    private GameObject powerUp;

    [SerializeField]
    private Transform powerUpsGroup;

    [SerializeField]
    private int powerUpsMaxCount;

    private int powerUpsCount;

    private PolygonCollider2D polygonCollider2D;

    private RaceTrackCollision raceTrackCollision;

    private bool hasSpawnedObjects;

    [SerializeField]
    private RaceTrackBorderController inner;

    [SerializeField]
    private RaceTrackBorderController outer;

    [SerializeField]
    private GameObject point;

    [SerializeField]
    private GameObject checkpoint;

    [SerializeField]
    private Transform checkpoints;

    [SerializeField]
    private Transform[] startPoints;

    [SerializeField]
    private SpaceshipController mainPlayer;

    private void Start() {
        polygonCollider2D = GetComponent<PolygonCollider2D>();

        hasSpawnedObjects = false;

        raceTrackCollision = GetComponent<RaceTrackCollision>();
        raceTrackCollision.onTrackCollisionFinished.AddListener(() =>{
            if (!hasSpawnedObjects) {
                hasSpawnedObjects = true;
                SpawnMeteors(meteorsCount);
                StartCoroutine(SpawnPowerUps());
            }
        });
    }

    public void SetupTrack(string trackName) {
        var track = TrackJsonReader.LoadTrack(trackName);

        inner.RemoveAllPoints();
        var innerPoints = new List<Transform>();
        for (int i = 0; i < track.inner.Count; i++) {
            var newPoint = Instantiate(point, inner.transform);
            newPoint.transform.localPosition = track.inner[i];
            innerPoints.Add(newPoint.transform);
        }
        inner.InitPoints(innerPoints);

        outer.RemoveAllPoints();
        var outerPoints = new List<Transform>();
        for (int i = 0; i < track.outer.Count; i++) {
            var newPoint = Instantiate(point, outer.transform);
            newPoint.transform.localPosition = track.outer[i];
            outerPoints.Add(newPoint.transform);
        }
        outer.InitPoints(outerPoints);

        CheckpointController.checkpoints = new List<CheckpointController>();
        foreach (Transform child in checkpoints) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < track.checkpoints.Count; i++) {
            var newCheckpoint = Instantiate(checkpoint, checkpoints).GetComponent<CheckpointController>();
            var cpData = track.checkpoints[i];
            newCheckpoint.transform.localPosition = cpData.point;
            newCheckpoint.transform.localRotation = Quaternion.Euler(0, 0, cpData.angle);
            newCheckpoint.transform.localScale = new Vector3(newCheckpoint.transform.localScale.x, cpData.length, 1);
            newCheckpoint.order = cpData.order;
            newCheckpoint.spaceshipController = mainPlayer;
            newCheckpoint.AddSelf();

            if (i == track.checkpoints.Count - 1) {
                newCheckpoint.isLast = true;
            } else {
                newCheckpoint.isLast = false;
            }
        }

        List<int> randomStartPoints = new List<int>();
        while (randomStartPoints.Count < track.starts.Count) {
            int index = Random.Range(0, track.starts.Count);
            if (!randomStartPoints.Contains(index)) {
                randomStartPoints.Add(index);
            }
        }

        for (int i = 0; i < randomStartPoints.Count; i++) {
            startPoints[i].localPosition = track.starts[randomStartPoints[i]].point;
            startPoints[i].localRotation = Quaternion.Euler(0, 0, track.starts[randomStartPoints[i]].angle);
        }
    }

    public void ResetObjects() {
        StopAllCoroutines();
        foreach (Transform child in meteorsGroup) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in powerUpsGroup) {
            Destroy(child.gameObject);
        }

        hasSpawnedObjects = false;
    }

    private IEnumerator SpawnPowerUps() {
        for (int i = 0; i < powerUpsMaxCount; i++) {
            GameObject powerUpInstance = Instantiate(powerUp, powerUpsGroup);
            powerUpInstance.transform.position = GetRandomPosition();
            powerUpsCount++;
        }

        while (true) {
            yield return new WaitForSeconds(Random.Range(5, 10));

            if (powerUpsCount < powerUpsMaxCount) {
                GameObject powerUpInstance = Instantiate(powerUp, powerUpsGroup);
                powerUpInstance.transform.position = GetRandomPosition();
                powerUpsCount++;
            }
        }
    }

    private void SpawnMeteors(int count) {
        count += Random.Range(0, count);

        for (int i = 0; i < count; i++) {
            int randomIndex = Random.Range(0, meteors.Length);
            GameObject meteorClone = Instantiate(meteors[randomIndex], meteorsGroup);
            meteorClone.transform.position = GetRandomPosition();
            meteorClone.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            meteorClone.transform.localScale = new Vector3(Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f), 1);
        }
    }

    private Vector3 GetRandomPosition() {
        Vector2 min = polygonCollider2D.bounds.min;
        Vector2 max = polygonCollider2D.bounds.max;

        Vector3 randomPoint = new Vector3();

        do
        {
            randomPoint = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), 0);
        } while (polygonCollider2D.OverlapPoint(randomPoint) == false);

        return randomPoint;
    }
}
