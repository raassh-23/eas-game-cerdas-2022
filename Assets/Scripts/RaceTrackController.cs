using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrackController : MonoBehaviour
{
    [SerializeField]
    private GameObject meteor;

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
        for (int i = 0; i < count; i++) {
            GameObject meteorClone = Instantiate(meteor, meteorsGroup);
            meteorClone.transform.position = GetRandomPosition();
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
