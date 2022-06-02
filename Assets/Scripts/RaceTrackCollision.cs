using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RaceTrackCollision : MonoBehaviour
{
    private List<RaceTrackBorderController> raceTrackBorderControllers;
    PolygonCollider2D polygonCollider2D;

    [SerializeField]
    private bool isLooping;

    public UnityEvent onTrackCollisionFinished;

    private void Awake()
    {
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        raceTrackBorderControllers = new List<RaceTrackBorderController>();

        foreach (Transform child in transform)
        {
            RaceTrackBorderController raceTrackBorderController = child.GetComponent<RaceTrackBorderController>();

            if (raceTrackBorderController != null)
            {
                raceTrackBorderControllers.Add(raceTrackBorderController);
                raceTrackBorderController.lineController.isLooping = isLooping;
            }
        }
    }

    private void LateUpdate()
    {
        //Get all the positions from the line renderer
        Vector3[] positions1 = raceTrackBorderControllers[0].lineController.GetPositions();
        Vector3[] positions2 = raceTrackBorderControllers[1].lineController.GetPositions();

        if (positions1.Length != positions2.Length)
        {
            Debug.Log(positions1.Length);
            Debug.Log(positions2.Length);
            Debug.LogError("RaceTrackCollision: The number of points in the two lines is not equal");
            return;
        }

        if (positions1.Length >= 2)
        {
            int numberOfLines = positions1.Length - 1;

            polygonCollider2D.pathCount = numberOfLines;

            for (int i = 0; i < numberOfLines; i++)
            {
                List<Vector2> currentColliderPoints = new List<Vector2> {
                    positions1[i], positions2[i], positions2[i + 1], positions1[i + 1]
                };
                polygonCollider2D.SetPath(i, currentColliderPoints.ConvertAll(p => (Vector2)transform.InverseTransformPoint(p)));
            }

            if (isLooping)
            {
                polygonCollider2D.pathCount++;
                List<Vector2> currentColliderPoints = new List<Vector2> {
                    positions1[numberOfLines], positions2[numberOfLines], positions2[0], positions1[0]
                };
                polygonCollider2D.SetPath(numberOfLines, currentColliderPoints.ConvertAll(p => (Vector2)transform.InverseTransformPoint(p)));
            }

            onTrackCollisionFinished?.Invoke();
        }
        else
        {
            polygonCollider2D.pathCount = 0;
        }
    }
}
