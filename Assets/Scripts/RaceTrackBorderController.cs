using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrackBorderController : MonoBehaviour
{
    private List<Transform> points;

    public LineController lineController;

    public void InitPoints() {
        points = new List<Transform>();
        foreach (Transform child in transform) {
            points.Add(child);
        }

        lineController.SetUpLine(points.ToArray());
    }
}
