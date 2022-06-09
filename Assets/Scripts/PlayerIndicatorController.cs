using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIndicatorController : MonoBehaviour
{
    [SerializeField]
    private SpaceshipController spaceship;

    [SerializeField]
    private Text healthText;

    [SerializeField]
    private Text ammoText;

    [SerializeField]
    private Text mineText;

    private void Update() {
        healthText.text = spaceship.health.ToString("000");
        ammoText.text = spaceship.ammo.ToString("000");
        mineText.text = spaceship.mines.ToString("000");
    }
}
