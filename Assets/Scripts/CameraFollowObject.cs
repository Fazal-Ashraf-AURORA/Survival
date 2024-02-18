using System.Collections;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipRotationTime = 0.5f;

    private Coroutine _turnCoroutine;

    private PlayerController _playerController;
    private bool _isFacingRight;

    private void Awake() {
        _playerController = _playerTransform.GetComponent<PlayerController>();

        _isFacingRight = _playerController.isFacingRight;
    }

    private void Update() {
        //make the camerafollowobject follow the player's position
        transform.position = _playerTransform.position;
    }

    public void CallTurn() {
        _turnCoroutine = StartCoroutine(FlipYLerp());
    }

    private IEnumerator FlipYLerp() {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < _flipRotationTime) {
            elapsedTime += Time.deltaTime;

            //lerp the y rotation
            yRotation = Mathf.Lerp(startRotation, endRotationAmount,(elapsedTime /  _flipRotationTime));
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null;
        }
    }

    private float DetermineEndRotation() {
        _isFacingRight = !_isFacingRight;

        if(_isFacingRight ) {
            return 0f;
        } else {
            return 180f;
        }
    }
}
