using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Spaceship")) {
            Debug.Log("PowerUp collected");
            Destroy(gameObject);
        }
    }
}
