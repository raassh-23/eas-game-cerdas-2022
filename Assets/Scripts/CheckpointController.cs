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

    private EnvironmentManager environmentManager;

    static public List<CheckpointController> checkpoints = new List<CheckpointController>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        environmentManager = GetComponentInParent<EnvironmentManager>();

        if (environmentManager == null) {
            Debug.Log("EnvironmentManager not found");
        } else {
            Debug.Log("EnvironmentManager found " + environmentManager);
        }

        checkpoints.Add(this);
    }

    private void Update() {
        if (spaceshipController.nextCheckpoint == this) {
            spriteRenderer.color = new Color(1, 1, 1, 1f);
        } else {
            spriteRenderer.color = new Color(1, 1, 1, 0f);
        }
    }

    static public CheckpointController getNextCheckpoint(CheckpointController currentCheckpoint, EnvironmentManager environmentManager) {
        if (currentCheckpoint == null || currentCheckpoint.isLast) {
            return CheckpointController.checkpoints.Find(cp => cp.order == 0 && cp.environmentManager == environmentManager);
        }

        return CheckpointController.checkpoints.Find(cp => cp.order == currentCheckpoint.order + 1 && cp.environmentManager == environmentManager);
    }

    public Vector2 GetDirection() {
        return new Vector2(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad),
            Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
    }
}
