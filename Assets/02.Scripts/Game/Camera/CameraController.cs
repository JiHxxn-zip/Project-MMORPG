// Assets/Scripts/Game/Camera/CameraController.cs
using System.Collections;
using Cinemachine;
using UnityEngine;

namespace MMORPG.Game
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineFreeLook _freeLook;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 2f;
        [SerializeField] private float _minRadius = 3f;
        [SerializeField] private float _maxRadius = 15f;

        private Coroutine _shakeRoutine;
        private Coroutine _hitStopRoutine;

        private void Awake()
        {
            _freeLook.m_XAxis.m_InputAxisName = "";
            _freeLook.m_YAxis.m_InputAxisName = "";
        }

        private void OnEnable()  => CombatFeedbackBus.OnFeedback += OnFeedback;
        private void OnDisable() => CombatFeedbackBus.OnFeedback -= OnFeedback;

        private void Update()
        {
            HandleRotation();
            HandleZoom();
        }

        // ── 카메라 회전 ───────────────────────────────────────────────

        private void HandleRotation()
        {
            if (!Input.GetMouseButton(1)) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            _freeLook.m_XAxis.Value += mouseX * _freeLook.m_XAxis.m_MaxSpeed * Time.deltaTime;
            _freeLook.m_YAxis.Value -= mouseY * _freeLook.m_YAxis.m_MaxSpeed * Time.deltaTime;
            _freeLook.m_YAxis.Value  = Mathf.Clamp01(_freeLook.m_YAxis.Value);
        }

        // ── 줌 ───────────────────────────────────────────────────────

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Approximately(scroll, 0f)) return;

            float delta = -scroll * _zoomSpeed;
            for (int i = 0; i < 3; i++)
            {
                CinemachineFreeLook.Orbit orbit = _freeLook.m_Orbits[i];
                orbit.m_Radius = Mathf.Clamp(orbit.m_Radius + delta, _minRadius, _maxRadius);
                _freeLook.m_Orbits[i] = orbit;
            }
        }

        // ── 전투 피드백 ───────────────────────────────────────────────

        private void OnFeedback(CombatFeedbackEvent e)
        {
            if (e.ShakeDuration > 0f)
            {
                if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
                _shakeRoutine = StartCoroutine(ShakeRoutine(e.ShakeAmplitude, e.ShakeDuration));
            }

            if (e.HitStopDuration > 0f)
            {
                if (_hitStopRoutine != null) StopCoroutine(_hitStopRoutine);
                _hitStopRoutine = StartCoroutine(HitStopRoutine(e.HitStopDuration));
            }
        }

        private IEnumerator ShakeRoutine(float amplitude, float duration)
        {
            SetNoiseAmplitude(amplitude);
            yield return new WaitForSecondsRealtime(duration);  // timeScale 영향 없음
            SetNoiseAmplitude(0f);
            _shakeRoutine = null;
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            _hitStopRoutine = null;
        }

        private void SetNoiseAmplitude(float amplitude)
        {
            for (int i = 0; i < 3; i++)
            {
                var noise = _freeLook.GetRig(i)
                    .GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                if (noise != null)
                    noise.m_AmplitudeGain = amplitude;
                else
                    Debug.LogWarning($"[CameraController] Rig[{i}]에 CinemachineBasicMultiChannelPerlin 없음.");
            }
        }
    }
}
