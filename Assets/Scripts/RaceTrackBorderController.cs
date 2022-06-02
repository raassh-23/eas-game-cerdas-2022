using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrackBorderController : MonoBehaviour
{
    public List<Transform> points;

    public LineController lineController;

    public void RemoveAllPoints() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        lineController.ResetLine();
    }

    public void InitPoints(List<Transform> points) {
        this.points = points;

        lineController.SetUpLine(points.ToArray());
    }
}
