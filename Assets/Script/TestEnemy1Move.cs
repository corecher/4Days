using UnityEngine;

public class TestEnemy1Move : MonoBehaviour
{
    public float rotateSpeed = 50f;

    [Header("나선형 접근 설정")] // [추가됨]
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        baseY = transform.position.y;
        
        // 안전장치 추가
        GameObject playerObj = GameObject.FindWithTag("Player");
        if(playerObj) Player = playerObj.transform;

        // [추가됨] 랜덤 접근 속도 설정
        currentApproachSpeed = Random.Range(minApproachSpeed, maxApproachSpeed);
    }

    void Update()
    {
        if (!moveOn) return;
        switch (State)
        {
            case 0: MoveCircle(); break; // 평소: 돌면서 다가가기 + 출렁임
            case 1: Dash(); break;       // 공격: 플레이어에게 직선 돌진
            case 2: MoveCircle(); break; // 복귀: 돌면서 다가가기 + 고도 상승
        }
    }

    void MoveCircle()
    {
        // 1. 공전 (기존 기능)
        transform.RotateAround(Player.position, Vector3.up, rotateSpeed * Time.deltaTime);

        // 2. [추가됨] 센터 방향으로 천천히 접근 (나선 이동)
        if (Player != null)
        {
            Vector3 direction = (Player.position - transform.position);
            direction.y = 0; // 높이는 유지(Wave나 UpPlane이 제어하므로)

            // 센터와 너무 가깝지 않을 때만 접근
            if (direction.magnitude > 1.0f) 
            {
                transform.position += direction.normalized * currentApproachSpeed * Time.deltaTime;
            }
        }

        Vector3 pos = transform.position;

        // 3. 출렁임 및 기울임 (기존 기능 - State 2가 아닐 때만)
        if (State != 2)
        {
            float wave = Mathf.Sin(Time.time * waveSpeed);
            pos.y = baseY + wave * waveHeight;
            transform.position = pos; // Y축 변화 적용

            float tiltX = wave * tiltXAngle;
            float tiltZ = Mathf.Cos(Time.time * waveSpeed) * tiltZAngle;
            float currentY = transform.rotation.eulerAngles.y;

            transform.rotation = Quaternion.Euler(tiltX, currentY, tiltZ);
        }

        // 4. 상태 전환 (거리 체크)
        if (Player != null && Vector3.Distance(Player.position, transform.position) < 200 && State == 0)
        {
            State = 1;
        }

        // 5. 고도 상승 로직 (기존 기능)
        // 주의: UpPlane 함수가 transform.position을 덮어쓸 수 있음
        UpPlane(pos);
    }

    void Dash()
    {
        if (Player == null) return;
        Vector3 dir = (Player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);
        transform.position += dir * speed * Time.deltaTime;

        // 플레이어와 매우 가까워지면(공격 후) State 2로 전환 (고도 상승 및 복귀)
        if (Vector3.Distance(Player.position, transform.position) < 30) 
            State = 2;
    }

    void UpPlane(Vector3 pos)
    {
        // State 0이나 1에서 들어온 pos를 기반으로 처리
        // State가 2가 되어 이 함수가 주도적으로 호출될 때 상승 작용
        if (State == 2) // 명확하게 State 2일 때만 상승하도록 조건 강화 권장, 혹은 기존 로직 유지
        {
             // 현재 로직상 MoveCircle 내에서 호출되므로, Y가 100보다 낮으면 강제로 올림
            if (pos.y < 400f)
            {
                // Y축만 부드럽게 상승
                pos.y = Mathf.MoveTowards(pos.y, 300f, 50f * Time.deltaTime);
                transform.position = pos;
            }
            else
            {
                // 충분히 올라갔으면 다시 State 0으로 복귀
                State = 0;
                baseY = 400f; // 출렁임의 기준 높이를 현재 높이로 재설정 (안하면 다시 뚝 떨어짐)
            }
        }
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Damage();
        }
    }
}
