using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float bulletSpeed = 5;
    public float bulletRange = 10;
    public float curDistance;

    public SpaceshipController shooter;

    private void Start() {
        curDistance = 0;
    }

    private void FixedUpdate() {
        float dist = bulletSpeed * Time.fixedDeltaTime;
        transform.Translate(-1*Vector2.up * dist);
        curDistance += dist;
        if (curDistance > bulletRange) {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Spaceship") {
            var spaceship = other.gameObject.GetComponent<SpaceshipController>();
            if (shooter != spaceship) {
                shooter.shotHit++;
            }
            Destroy(gameObject);
        }

        if (other.gameObject.tag == "Meteor") {
            shooter.shotHit++;
            Destroy(gameObject);
        }
    }
}
