using UnityEngine;

public class MovingPlatform : MonoBehaviour {
    public enum MovementType {
        Horizontal,
        Vertical
    }

    public MovementType movementType;
    public float speed = 2f;
    public float distance = 5f;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool movingTowardsTarget = true;

    void Start() {
        initialPosition = transform.position;
        if (movementType == MovementType.Horizontal) {
            targetPosition = initialPosition + Vector3.right * distance;
        } else if (movementType == MovementType.Vertical) {
            targetPosition = initialPosition + Vector3.up * distance;
        }
    }

    void Update() {
        if (movingTowardsTarget) {
            MovePlatform(targetPosition);
        } else {
            MovePlatform(initialPosition);
        }
    }

    void MovePlatform(Vector3 target) {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.01f) {
            movingTowardsTarget = !movingTowardsTarget;
        }
    }


    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(initialPosition, targetPosition);
    }
}
