using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCollision : MonoBehaviour
{
    //The Line Manager Class
    LineController lineController;

    //The collider for the line
    PolygonCollider2D polygonCollider2D;

    //The points to draw a collision shape between
    List<Vector2> colliderPoints = new List<Vector2>();

    void Start()
    {
        lineController = GetComponent<LineController>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    private void LateUpdate()
    {
        Vector3[] positions = lineController.GetPositions();

        if (positions.Length >= 2)
        {
            int numberOfLines = positions.Length - 1;

            polygonCollider2D.pathCount = numberOfLines;

            for (int i = 0; i < numberOfLines; i++)
            {
                SetCollider(positions[i], positions[i + 1], i);
            }

            if (lineController.isLooping)
            {
                polygonCollider2D.pathCount++;
                SetCollider(positions[positions.Length - 1], positions[0], numberOfLines);
            }
        }
        else
        {

            polygonCollider2D.pathCount = 0;
        }
    }

    private void SetCollider(Vector3 pointA, Vector3 pointB, int index)
    {
        List<Vector2> currentPositions = new List<Vector2> { pointA, pointB };

        List<Vector2> currentColliderPoints = CalculateColliderPoints(currentPositions);
        polygonCollider2D.SetPath(index, currentColliderPoints.ConvertAll(p => (Vector2)transform.InverseTransformPoint(p)));
    }

    private List<Vector2> CalculateColliderPoints(List<Vector2> positions)
    {
        float width = lineController.GetWidth();

        float m = (positions[1].y - positions[0].y) / (positions[1].x - positions[0].x);
        float deltaX = (width / 2f) * (m / Mathf.Pow(m * m + 1, 0.5f));
        float deltaY = (width / 2f) * (1 / Mathf.Pow(1 + m * m, 0.5f));

        Vector2[] offsets = new Vector2[2];
        offsets[0] = new Vector2(-deltaX, deltaY);
        offsets[1] = new Vector2(deltaX, -deltaY);

        List<Vector2> colliderPoints = new List<Vector2> {
            positions[0] + offsets[0],
            positions[1] + offsets[0],
            positions[1] + offsets[1],
            positions[0] + offsets[1]
        };

        return colliderPoints;
    }
}
