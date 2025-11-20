using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    public Transform center;
    public float rotateSpeed = 50f;
    [Header("출렁임 설정")]
    public float waveHeight = 5f;
    public float waveSpeed = 2f;
    [Header("기울임 설정")]
    public float tiltXAngle = 30f;
    public float tiltZAngle = 20f;
    private float baseY;
    void Start()
    {
        baseY=transform.position.y;
        center = GameObject.FindWithTag("center").GetComponent<Transform>();
    }
    void Update()
    {
        MoveCircle();
    }
    void MoveCircle()
    {
        transform.RotateAround(center.position, Vector3.up, rotateSpeed * Time.deltaTime);
        Vector3 pos = transform.position;
        float wave = Mathf.Sin(Time.time * waveSpeed);
        pos.y = baseY + wave * waveHeight;
        transform.position = pos;
        float tiltX = wave * tiltXAngle;
        float tiltZ = Mathf.Cos(Time.time * waveSpeed) * tiltZAngle;
        float currentY = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(tiltX, currentY, tiltZ);
    }
}
