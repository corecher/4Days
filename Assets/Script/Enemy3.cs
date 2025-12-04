using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Enemy3 : NetworkBehaviour
{
    [SerializeField] private Transform Player;

    [Header("상태 전환 설정")]
    public float dashRange = 10f;

    [Header("나선 이동 설정 (멀 때)")]
    public float rotateSpeed = 80f;
    public float minSpiralSpeed = 2.0f;
    public float maxSpiralSpeed = 4.0f;
    private float currentSpiralSpeed;

    [Header("돌진 설정 (가까울 때)")]
    public float dashSpeed = 15f;

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

    // ✅ 네트워크 스폰 이후 초기화
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        baseY = transform.position.y;
        hp = maxHp;

        if (Player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) Player = playerObj.transform;
        }

        currentSpiralSpeed = Random.Range(minSpiralSpeed, maxSpiralSpeed);

        StartCoroutine(SoundTimer());
    }

    void Update()
    {
        if (!IsServer) return;        // ✅ 서버만 로직 실행
        if (!moveOn || Player == null) return;

        float distance = Vector3.Distance(Player.position, transform.position);

        if (distance < dashRange)
        {
            Dash();
        }
        else
        {
            MoveSpiral();
        }
    }

    // 1️⃣ 멀리 있을 때: 나선 이동
    void MoveSpiral()
    {
        transform.RotateAround(Player.position, Vector3.up, rotateSpeed * Time.deltaTime);

        Vector3 direction = (Player.position - transform.position);
        direction.y = 0;
        transform.position += direction.normalized * currentSpiralSpeed * Time.deltaTime;

        Vector3 pos = transform.position;
        float wave = Mathf.Sin(Time.time * waveSpeed);
        pos.y = baseY + wave * waveHeight;
        transform.position = pos;

        float tiltX = wave * tiltXAngle;
        float tiltZ = Mathf.Cos(Time.time * waveSpeed) * tiltZAngle;

        Vector3 currentEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(tiltX, currentEuler.y, tiltZ);
    }

    // 2️⃣ 가까울 때: 돌진
    void Dash()
    {
        Vector3 dir = (Player.position - transform.position).normalized;

        transform.rotation = Quaternion.LookRotation(dir);
        transform.position += dir * dashSpeed * Time.deltaTime;
    }

    // ✅ 서버 전용 데미지
    void Damage()
    {
        if (!IsServer) return;

        hp--;

        if (hp <= 0)
        {
            rb.useGravity = true;
            moveOn = false;
        }
    }

    // ✅ 사운드 타이머는 서버에서만 실행 → 모든 클라이언트에 전달
    private IEnumerator SoundTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10f, 12f));
            PlaySoundClientRpc(transform.position);
        }
    }

    [ClientRpc]
    void PlaySoundClientRpc(Vector3 pos)
    {
        SoundManager.Instance.PlayNetworkSound("Fly", pos);
    }

    // ✅ 충돌 판정도 서버에서만 처리
    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Bullet"))
        {
            Damage();
        }
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}

