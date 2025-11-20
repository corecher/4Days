using UnityEngine;

public class SunOrbit : MonoBehaviour
{
    public Transform center;      // 회전 중심 (지구)
    public float radius = 300f;   // 태양 거리
    public float sunSize = 20f;   // 태양 크기

    [Header("Time Settings")]
    [Tooltip("시간에 따른 태양 위치 보정 (0이면 0시에 수평선, -90이면 0시에 발밑)")]
    public float angleOffset = -90f; // 보통 00:00(자정)은 태양이 가장 아래에 있어야 하므로 -90 추천

    void Start()
    {
        transform.localScale = Vector3.one * sunSize;

        if (center == null)
        {
            GameObject root = new GameObject("SunCenter");
            root.transform.position = Vector3.zero;
            center = root.transform;
        }
    }

    void Update()
    {
        if (Timer.Instance == null) return;

        float timePercent = Timer.Instance.gameTime / 86400f;

        float angle = (timePercent * 360f) + angleOffset;

        Quaternion rot = Quaternion.Euler(angle, 0f, 0f);
        Vector3 dir = Vector3.forward;

        transform.position = center.position + (rot * dir * radius);

        transform.LookAt(center);
    }
}