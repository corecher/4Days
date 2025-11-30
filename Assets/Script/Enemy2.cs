using UnityEngine;
using System.Collections;
public class Enemy2 : MonoBehaviour
{
    public Transform center;
    public float rotateSpeed = 50f;

    [Header("접근 속도 랜덤 설정")]
    public float minApproachSpeed = 0.5f; // 최소 속도 (이 값보다 느려지지 않음)
    public float maxApproachSpeed = 2.0f; // 최대 속도 (이 값보다 빨라지지 않음)
    private float currentApproachSpeed;   // 실제로 적용될 속도 (Start에서 결정됨)

    public float stopDistance = 0.5f;     // 멈추는 거리

    [Header("출렁임 설정")]
    public float waveHeight = 5f;
    public float waveSpeed = 2f;
    [Header("기울임 설정")]
    public float tiltXAngle = 30f;
    public float tiltZAngle = 20f;
    private float baseY;

    [SerializeField] private float maxHp;
    private float hp;
    private Rigidbody rb;
    private bool moveOn = true;
    private EnemyAttack1 enemyAttack1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        baseY = transform.position.y;
        enemyAttack1=GetComponent<EnemyAttack1>();
        // --- [변경점] 랜덤 속도 지정 ---
        // 최소값과 최대값 사이에서 랜덤하게 하나를 골라 현재 속도로 지정합니다.
        currentApproachSpeed = Random.Range(minApproachSpeed, maxApproachSpeed); 
        // ---------------------------

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
        if (moveOn && center != null)
            MoveCircle();
        
    }

    void MoveCircle()
    {
        // 1. 공전
        transform.RotateAround(center.position, Vector3.up, rotateSpeed * Time.deltaTime);

        // 2. 중심으로 다가가기
        Vector3 direction = (center.position - transform.position);
        direction.y = 0; 

        if (direction.magnitude > stopDistance)
        {
            // [변경점] approachSpeed 대신 위에서 랜덤으로 정한 currentApproachSpeed 사용
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
        hp--;
        if (hp <= 0)
        {
            rb.useGravity = true;
            moveOn = false;
            enemyAttack1.enabled=false;
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