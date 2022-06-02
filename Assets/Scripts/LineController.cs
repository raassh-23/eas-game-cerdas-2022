using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;
    private Transform[] points;

    public bool isLooping;

    private void Update()
    {
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(points[i].position.x, points[i].position.y, 0));
        }
    }

    public void ResetLine() {
        lineRenderer.positionCount = 0;
        points = new Transform[]{};
    }

    public void SetUpLine(Transform[] points)
    {
        lineRenderer.loop = isLooping;
        lineRenderer.positionCount = points.Length;
        this.points = points;
    }

    public Vector3[] GetPositions()
    {
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);
        return positions;
    }

    public float GetWidth()
    {
        return lineRenderer.startWidth;
    }
}
