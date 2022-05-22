using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float bulletSpeed;
    public float bulletRange;
    public float curDistance;

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
        if (other.gameObject.tag == "Meteor" || other.gameObject.tag == "Spaceship") {
            Destroy(gameObject);
        }
    }
}
