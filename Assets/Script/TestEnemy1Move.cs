using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class TestEnemy1Move : NetworkBehaviour
{
    public float rotateSpeed = 50f;

    [Header("나선형 접근 설정")]
    public float minApproachSpeed = 1.0f;
    public float maxApproachSpeed = 3.0f;
    private float currentApproachSpeed;

    [Header("출렁임 설정")]
    public float waveHeight = 5f;
    public float waveSpeed = 2f;

    [Header("기울임 설정")]
    public float tiltXAngle = 30f;
    public float tiltZAngle = 20f;
    private float baseY;

    [SerializeField] private Transform Player;
    [SerializeField] private float speed; // Dash 속도

    public int State = 0;

    [SerializeField] private float maxHp;
    private float hp;

    private Rigidbody rb;
    private bool moveOn = true;

    [SerializeField] private GameObject bullet; // ✅ 반드시 NetworkObject 포함 프리팹

    // ✅ 네트워크 스폰 이후 서버에서만 초기화
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        baseY = transform.position.y;
        hp = maxHp;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj) Player = playerObj.transform;

        currentApproachSpeed = Random.Range(minApproachSpeed, maxApproachSpeed);

        StartCoroutine(SoundTimer());
    }

    void Update()
    {
        if (!IsServer) return;     // ✅ 서버만 이동 처리
        if (!moveOn) return;

        switch (State)
        {
            case 0: MoveCircle(); break;
            case 1: Dash(); break;
            case 2: MoveCircle(); break;
        }
    }

    void MoveCircle()
    {
        if (Player == null) return;

        // 1. 공전
        transform.RotateAround(Player.position, Vector3.up, rotateSpeed * Time.deltaTime);

        // 2. 나선 접근
        Vector3 direction = (Player.position - transform.position);
        direction.y = 0;

        if (direction.magnitude > 1.0f)
        {
            transform.position += direction.normalized * currentApproachSpeed * Time.deltaTime;
        }

        Vector3 pos = transform.position;

        // 3. 출렁임 + 기울임
        if (State != 2)
        {
            float wave = Mathf.Sin(Time.time * waveSpeed);
            pos.y = baseY + wave * waveHeight;
            transform.position = pos;

            float tiltX = wave * tiltXAngle;
            float tiltZ = Mathf.Cos(Time.time * waveSpeed) * tiltZAngle;
            float currentY = transform.rotation.eulerAngles.y;

            transform.rotation = Quaternion.Euler(tiltX, currentY, tiltZ);
        }

        // 4. 상태 전환
        if (Vector3.Distance(Player.position, transform.position) < 200 && State == 0)
        {
            State = 1;
        }

        // 5. 고도 상승
        UpPlane(pos);
    }

    void Dash()
    {
        if (Player == null) return;

        Vector3 dir = (Player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(Player.position, transform.position) < 30)
        {
            State = 2;

            // ✅ 서버에서만 총알 생성 + 네트워크 스폰
            GameObject b = Instantiate(bullet, transform.position, transform.rotation);
            NetworkObject n = b.GetComponent<NetworkObject>();
            if (n != null)
                n.Spawn();
        }
    }

    void UpPlane(Vector3 pos)
    {
        if (State == 2)
        {
            if (pos.y < 400f)
            {
                pos.y = Mathf.MoveTowards(pos.y, 400f, 50f * Time.deltaTime);
                transform.position = pos;
            }
            else
            {
                State = 0;
                baseY = 400f;
            }
        }
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

    // ✅ 서버 타이머 → 전체 클라이언트 사운드 재생
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

    // ✅ 충돌 판정도 서버에서만
    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Bullet"))
        {
            Damage();
        }
    }
}

