using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineVirtualCamera[] _allVirtualCameras;

    [Header("Controls for lerping the Y Damping during player jump/fall")]
    //[SerializeField] private float _fallPanAmount = 0.25f;
    [SerializeField] private float _fallYPanTime = 0.35f;
    public float _fallSpeedYDampingChangeThreshold = -15f;

    public bool IsLerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private Coroutine _lerpYPanCoroutine;

    private CinemachineVirtualCamera _currentCamera;
    private CinemachineFramingTransposer _framingTransposer;

    private float _normYPanAmount = 2f;
    //private float _fallingYPanAmount = 0.25f;


    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        for (int i = 0; i < _allVirtualCameras.Length; i++) {

            if(_allVirtualCameras[i].enabled) {
                //set the current active camera
                _currentCamera = _allVirtualCameras[i];

                //set the framing transposer
                _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }

            // Attempt to initialize _normYPanAmount based on the framing transposer's YDamping
            if (_framingTransposer != null) {
                _normYPanAmount = _framingTransposer.m_YDamping;
            } else {
                Debug.LogWarning("Framing transposer not found or YDamping not set. Using default value for _normYPanAmount.");
            }
        }

        //set the YDamping amount so it's based on the inspector value
    }

    #region Lerp the Y Damping

    //public void LerpYDamping(bool isPlayerFalling) {
    //    _lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    //}

    //private IEnumerator LerpYAction(bool isPlayerFalling) {
    //    IsLerpingYDamping = true;

    //    //grab the starting damping amount
    //    float startDampAmount = _framingTransposer.m_YDamping;
    //    float endDampAmount = 0f;

    //    //determine the end damping amount
    //    if(isPlayerFalling) {
    //        endDampAmount = _fallPanAmount;
    //        LerpedFromPlayerFalling = true;
    //    } else {
    //        endDampAmount = _normYPanAmount;
    //    }

    //    //lerp the pan amount
    //    float elapsedTime = 0f;
    //    while(elapsedTime < _fallYPanTime) {
    //        elapsedTime += Time.deltaTime;

    //        float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / _fallYPanTime));
    //        _framingTransposer.m_YDamping = lerpedPanAmount;

    //        yield return null;
    //    }

    //    IsLerpingYDamping = false;
    //}

    public void ChangeYDamping(float newYDamping) {
        if (!IsLerpingYDamping) {
            _lerpYPanCoroutine = StartCoroutine(LerpYAction(newYDamping));
        }
    }

    private IEnumerator LerpYAction(float targetYDamping) {
        IsLerpingYDamping = true;

        // Grab the starting damping amount
        float startDampAmount = _framingTransposer.m_YDamping;
        float endDampAmount = targetYDamping;

        // Lerp the pan amount
        float elapsedTime = 0f;
        while (elapsedTime < _fallYPanTime) {
            elapsedTime += Time.deltaTime;

            // Calculate the lerp factor between 0 and 1 based on time
            float lerpFactor = Mathf.Clamp01(elapsedTime / _fallYPanTime);

            // Smoothly interpolate between start and end damping values
            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, lerpFactor);
            _framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        // Ensure that the final damping value is exactly the target value
        _framingTransposer.m_YDamping = endDampAmount;

        IsLerpingYDamping = false;
    }

    

    #endregion
}
