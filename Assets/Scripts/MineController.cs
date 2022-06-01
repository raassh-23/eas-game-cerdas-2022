using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    public SpaceshipController dropper;
    public float mineDuration = 30;

    private float curDuration;

    private void Start() {
        curDuration = 0;
    }

    private void Update() {
        curDuration += Time.deltaTime;
        if (curDuration > mineDuration) {
            dropper.mineMissed++;
            DestroyMine();
        }
    }

    public void DestroyMine()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Spaceship")
        {
            var spaceship = other.gameObject.GetComponent<SpaceshipController>();
            if (dropper != spaceship)
            {
                dropper.shotHit++;
            }
            DestroyMine();
        }
    }
}
