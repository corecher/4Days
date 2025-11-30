using UnityEngine;
using System.Collections;

public class Enemy3 : MonoBehaviour
{
    [SerializeField] private Transform Player;

    [Header("상태 전환 설정")]
    public float dashRange = 10f; // 이 거리보다 가까워지면 돌진(Dash) 시작 (기존 150은 너무 멀 수 있어 조절 필요)

    [Header("나선 이동 설정 (멀 때)")]
    public float rotateSpeed = 80f;
    public float minSpiralSpeed = 2.0f; // 빙글 돌 때 다가가는 최소 속도
    public float maxSpiralSpeed = 4.0f; // 빙글 돌 때 다가가는 최대 속도
    private float currentSpiralSpeed;

    [Header("돌진 설정 (가까울 때)")]
    public float dashSpeed = 15f; // 돌진 속도

    [Header("출렁임 (나선 이동 중)")]
    public float waveHeight = 2f;
    public float waveSpeed = 5f;
    public float tiltXAngle = 20f;
    public float tiltZAngle = 20f;
    private float baseY;

    [SerializeField] private float maxHp;
    private float hp;
    private Rigidbody rb;
    private bool moveOn = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        baseY = transform.position.y;
        hp = maxHp;

        if (Player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) Player = playerObj.transform;
        }

        // 나선 이동 시 다가갈 속도 랜덤 설정
        currentSpiralSpeed = Random.Range(minSpiralSpeed, maxSpiralSpeed);
        StartCoroutine(SoundTimer());
    }

    void Update()
    {
        if (!moveOn || Player == null) return;

        float distance = Vector3.Distance(Player.position, transform.position);

        // 거리가 dashRange보다 가까우면 돌진(Dash), 멀면 회전 접근(MoveSpiral)
        if (distance < dashRange)
        {
            Dash();
        }
        else
        {
            MoveSpiral();
        }
    }

    // 1. 멀리 있을 때: 플레이어 주변을 돌며 천천히 접근
    void MoveSpiral()
    {
        // 공전
        transform.RotateAround(Player.position, Vector3.up, rotateSpeed * Time.deltaTime);

        // 서서히 접근
        Vector3 direction = (Player.position - transform.position);
        direction.y = 0;
        transform.position += direction.normalized * currentSpiralSpeed * Time.deltaTime;

        // 출렁임 & 기울임 효과
        Vector3 pos = transform.position;
        float wave = Mathf.Sin(Time.time * waveSpeed);
        pos.y = baseY + wave * waveHeight;
        transform.position = pos;

        float tiltX = wave * tiltXAngle;
        float tiltZ = Mathf.Cos(Time.time * waveSpeed) * tiltZAngle;
        
        // 현재 Y축 회전 유지하며 흔들기
        Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(tiltX, currentEuler.y, tiltZ);
    }

    // 2. 가까이 있을 때: 직선으로 빠르게 돌진
    void Dash()
    {
        Vector3 dir = (Player.position - transform.position).normalized;
        
        // 플레이어를 바라보게 회전
        transform.rotation = Quaternion.LookRotation(dir);
        
        // 직선 이동
        transform.position += dir * dashSpeed * Time.deltaTime;
    }

    void Damage()
    {
        hp--;
        if (hp <= 0)
        {
            rb.useGravity = true;
            moveOn = false;
        }
    }
    private IEnumerator SoundTimer()
    {
        yield return new WaitForSeconds(Random.Range(12,10));
        SoundManager.Instance.PlayNetworkSound("Fly",transform.position);
        StartCoroutine(SoundTimer());
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Damage();
        }
    }
}
