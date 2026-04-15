// Assets/Scripts/Game/Camera/CameraController.cs
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

        // FreeLook의 기본 InputAxisName을 비워두고 코드에서 직접 제어
        private void Awake()
        {
            // 기본 마우스 입력 비활성화 — 코드에서 직접 제어할 것
            _freeLook.m_XAxis.m_InputAxisName = "";
            _freeLook.m_YAxis.m_InputAxisName = "";
        }

        private void Update()
        {
            HandleRotation();
            HandleZoom();
        }

        private void HandleRotation()
        {
            // 우클릭 홀드 중에만 회전
            if (!Input.GetMouseButton(1)) return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // FreeLook Value에 직접 누적 (Invert는 인스펙터 체크로 처리)
            _freeLook.m_XAxis.Value += mouseX * _freeLook.m_XAxis.m_MaxSpeed * Time.deltaTime;
            _freeLook.m_YAxis.Value -= mouseY * _freeLook.m_YAxis.m_MaxSpeed * Time.deltaTime;

            // Y Axis clamp (0~1)
            _freeLook.m_YAxis.Value = Mathf.Clamp01(_freeLook.m_YAxis.Value);
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Approximately(scroll, 0f)) return;

            // 세 Rig의 Radius를 동시에 조정
            float delta = -scroll * _zoomSpeed;

            for (int i = 0; i < 3; i++)
            {
                CinemachineFreeLook.Orbit orbit = _freeLook.m_Orbits[i];
                orbit.m_Radius = Mathf.Clamp(orbit.m_Radius + delta, _minRadius, _maxRadius);
                _freeLook.m_Orbits[i] = orbit;
            }
        }
    }
}
