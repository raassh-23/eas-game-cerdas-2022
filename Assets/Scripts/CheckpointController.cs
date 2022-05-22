using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public int order;

    public bool isLast;

    private SpaceshipController spaceshipController;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spaceshipController = GameObject.FindGameObjectWithTag("Spaceship").GetComponent<SpaceshipController>();
    }

    private void Update() {
        if (spaceshipController.nextCheckpoint == order) {
            spriteRenderer.color = new Color(1, 1, 1, 1f);
        } else {
            spriteRenderer.color = new Color(1, 1, 1, 0f);
        }
        
    }
}
