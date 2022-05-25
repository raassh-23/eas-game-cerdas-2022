using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public int order;

    public bool isLast;

    [SerializeField]
    private SpaceshipController spaceshipController;

    static public List<CheckpointController> checkpoints = new List<CheckpointController>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        checkpoints.Add(this);
    }

    private void Update() {
        if (spaceshipController.nextCheckpoint == this) {
            spriteRenderer.color = new Color(1, 1, 1, 1f);
        } else {
            spriteRenderer.color = new Color(1, 1, 1, 0f);
        }
    }

    static public CheckpointController getNextCheckpoint(CheckpointController currentCheckpoint) {
        if (currentCheckpoint == null || currentCheckpoint.isLast) {
            return CheckpointController.checkpoints.Find(cp => cp.order == 0);
        }

        return CheckpointController.checkpoints.Find(cp => cp.order == currentCheckpoint.order + 1);
    }
}
