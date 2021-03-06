using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    private void Update() {
        if (target != null) {
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
    }
}
