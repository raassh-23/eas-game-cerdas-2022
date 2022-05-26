using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineController : MonoBehaviour
{
    public SpaceshipController dropper;

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
