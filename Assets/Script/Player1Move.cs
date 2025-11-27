using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FP_CubeController : NetworkBehaviour
{
    public Transform cam;               // 카메라(자식)
    public float mouseSensitivity = 300f;
    public float moveSpeed = 5f;
    float xRotation = 0f;
    public float jumpForce = 5f;        // 점프 힘
    private bool isGrounded = true;     // 땅에 닿아있는지 체크

    private Rigidbody rb;
    [SerializeField] private Transform tank;
    private Collider collider;
    public bool rideOn = false;
    [SerializeField] private Vector3 offset;
    [SerializeField] private TestAttack testAttack;
    [SerializeField] private List<GameObject> uis;
    [SerializeField]
    public NetworkVariable<int> haveMagazine = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private string walkSound = "grasswalk";
    void Start()
    {
        // 내 캐릭터일 때만 마우스 잠금 및 숨김
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        collider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        UiState();
    }

    void Update()
    {
        if (!IsOwner) return; // 오너만 이동
        Look();
        Move();
        Jump();
        
        if (Vector3.Distance(tank.position, transform.position) < 5)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                rideOn = rideOn ? false : true;
                UiState();
                collider.enabled = !rideOn;
                if (!rideOn)
                {
                    Vector3 pos = tank.position;
                    pos.x += 3f;
                    transform.position = pos + offset;

                }
            }
            if (Input.GetKeyDown(KeyCode.R) && haveMagazine.Value > 0)
            {
                ReloadServerRpc();
            }
        }
        if (rideOn) RideTank();
        if (transform.position.y < -5) transform.position += new Vector3(0, 300, 0);

    }
    [ServerRpc]
    void ReloadServerRpc(ServerRpcParams rpcParams = default)
    {
        // 서버에서만 처리됨
        if (haveMagazine.Value > 0)
        {
            haveMagazine.Value--;
            testAttack.ammunition.Value += 100;
        }
    }
    void Jump()
    {
        if (!isGrounded) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 카메라 X축 (상하)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 본체(Y축 회전)
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal"); // A, D
        float z = Input.GetAxis("Vertical");   // W, S
        if(x!=0||z!=0)
        SoundManager.Instance.PlayNetworkSound(walkSound,transform.position);
        Vector3 move = transform.right * x + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
    void RideTank()
    {
        transform.position = offset + tank.position;
    }
    void UiState()
    {
        uis[0].SetActive(rideOn);
        uis[1].SetActive(!rideOn);
    }
    private void OnTriggerEnter(Collider other)
    {
        // 1. 태그 확인
        if (other.CompareTag("BulletAdd"))
        {
            // 2. 서버 권한 확인
            if (!IsServer) return;

            // 3. 탄창 증가
            haveMagazine.Value += 1;
            Debug.Log($"탄창 획득! 현재 개수: {haveMagazine.Value}");

            // 4. 아이템 삭제
            if (other.TryGetComponent<NetworkObject>(out var netObj))
            {
                netObj.Despawn();
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        if (collision.gameObject.CompareTag("Storage"))
        {
            isGrounded = true;
            walkSound="concret";
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Storage"))
        {
            isGrounded = true;
            walkSound="grasswalk";
        }
    }

    // ------------------------------------------------------------------
    // [추가된 부분] 오브젝트가 파괴될 때(방 나감, 씬 이동) 마우스 복구
    // ------------------------------------------------------------------
    public override void OnDestroy()
    {
        // 1. 내 캐릭터(Owner)였던 경우에만 마우스를 복구합니다.
        // (다른 플레이어가 나갔을 때 내 마우스가 풀리면 안 되니까요)
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None; // 마우스 잠금 해제 (자유롭게 이동)
            Cursor.visible = true;                  // 마우스 보이게 하기
            Debug.Log("방을 나가서 마우스 커서를 복구했습니다.");
        }

        // 부모 클래스(NetworkBehaviour)의 정리 작업 실행
        base.OnDestroy();
    }
}