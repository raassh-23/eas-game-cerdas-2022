using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapNumberController : MonoBehaviour
{
    [SerializeField]
    private Text current;

    [SerializeField]
    private Text max;

    [SerializeField]
    private SpaceshipController spaceship;

    private void LateUpdate() {
        current.text = Mathf.Clamp(spaceship.currentLap + 1, 1, spaceship.maxLap).ToString("0");
        max.text = "/" + spaceship.maxLap;

    }
}
