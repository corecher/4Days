using UnityEngine;

public class SunOrbit : MonoBehaviour
{
    public Transform center;      // ȸ�� �߽� (����)
    public float radius = 300f;   // �¾� �Ÿ�
    public float sunSize = 20f;   // �¾� ũ��

    [Header("Time Settings")]
    [Tooltip("�ð��� ���� �¾� ��ġ ���� (0�̸� 0�ÿ� ����, -90�̸� 0�ÿ� �߹�)")]
    public float angleOffset = -90f; // ���� 00:00(����)�� �¾��� ���� �Ʒ��� �־�� �ϹǷ� -90 ��õ

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

        float timePercent = Timer.Instance.netGameTime.Value / 86400f;

        float angle = (timePercent * 360f) + angleOffset;

        Quaternion rot = Quaternion.Euler(angle, 0f, 0f);
        Vector3 dir = Vector3.forward;

        transform.position = center.position + (rot * dir * radius);

        transform.LookAt(center);
    }
}