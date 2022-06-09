using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public int order;

    public bool isLast;

    public SpaceshipController spaceshipController;

    private EnvironmentManager environmentManager;

    static public List<CheckpointController> checkpoints = new List<CheckpointController>();

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void AddSelf() {
        checkpoints.Add(this);
    }

    private void Update() {
        if (spaceshipController.nextCheckpoint == this) {
            spriteRenderer.color = new Color(136, 129, 132, 0.25f);
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

    public Vector2 GetDirection() {
        return new Vector2(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad),
            Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
    }
}
