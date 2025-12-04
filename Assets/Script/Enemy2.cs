using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Enemy2 : NetworkBehaviour
{
    public Transform center;
    public float rotateSpeed = 50f;

    [Header("접근 속도 랜덤 설정")]
    public float minApproachSpeed = 0.5f;
    public float maxApproachSpeed = 2.0f;
    private float currentApproachSpeed;

    public float stopDistance = 0.5f;

    [Header("출렁임 설정")]
    public float waveHeight = 5f;
    public float waveSpeed = 2f;

    [Header("기울임 설정")]
    public float tiltXAngle = 30f;
    public float tiltZAngle = 20f;

    private float baseY;

    [SerializeField] private float maxHp = 5f;
    private float hp;

    private Rigidbody rb;
    private bool moveOn = true;
    private EnemyAttack1 enemyAttack1;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;   // ✅ 서버만 적을 움직이게 함

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        baseY = transform.position.y;
        enemyAttack1 = GetComponent<EnemyAttack1>();

        hp = maxHp;

        currentApproachSpeed = Random.Range(minApproachSpeed, maxApproachSpeed);

        GameObject centerObj = GameObject.FindWithTag("Player");
        if (centerObj != null)
        {
            center = centerObj.transform;
        }
        else
        {
            moveOn = false;
        }

        StartCoroutine(SoundTimer());
    }

    void Update()
    {
        if (!IsServer) return;   // ✅ 서버만 이동 처리
        if (moveOn && center != null)
            MoveCircle();
    }

    void MoveCircle()
    {
        // 1. 공전
        transform.RotateAround(center.position, Vector3.up, rotateSpeed * Time.deltaTime);

        // 2. 중심으로 접근
        Vector3 direction = center.position - transform.position;
        direction.y = 0;

        if (direction.magnitude > stopDistance)
        {
            transform.position += direction.normalized * currentApproachSpeed * Time.deltaTime;
        }

        // 3. 출렁임
        Vector3 pos = transform.position;
        float wave = Mathf.Sin(Time.time * waveSpeed);
        pos.y = baseY + wave * waveHeight;
        transform.position = pos;

        // 4. 기울임
        float tiltX = wave * tiltXAngle;
        float tiltZ = Mathf.Cos(Time.time * waveSpeed) * tiltZAngle;
        float currentY = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(tiltX, currentY, tiltZ);
    }

    void Damage()
    {
        if (!IsServer) return;

        hp--;

        if (hp <= 0)
        {
            rb.useGravity = true;
            moveOn = false;
            enemyAttack1.enabled = false;
        }
    }

    private IEnumerator SoundTimer()
    {
        yield return new WaitForSeconds(Random.Range(10f, 12f));
        PlaySoundClientRpc(transform.position);
        StartCoroutine(SoundTimer());
    }

    [ClientRpc]
    void PlaySoundClientRpc(Vector3 pos)
    {
        SoundManager.Instance.PlayNetworkSound("Fly", pos);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Bullet"))
        {
            Damage();
        }
    }
}
