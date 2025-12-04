using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FP_CubeController : NetworkBehaviour
{
    public Transform cam;
    public float mouseSensitivity = 300f;
    public float moveSpeed = 5f;
    float xRotation = 0f;
    public float jumpForce = 5f;
    private bool isGrounded = true;
    private Rigidbody rb;
    [SerializeField] private Transform tank;
    private Collider collider;
    public bool rideOn = false;
    [SerializeField] private Vector3 offset;
    [SerializeField] private TestAttack testAttack;
    [SerializeField] private List<GameObject> uis;
    
    // 걷는 상태 동기화
    public NetworkVariable<bool> isWalking = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField]
    public NetworkVariable<int> haveMagazine = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private string walkSound = "grasswalk";
    public Animator animator;

    // 소리 쿨타임 변수
    private float soundTimer = 0f;
    private float soundInterval = 0.5f;
    [SerializeField]private Text bullet;
    private bool isWalkSoundRunning = false;

    void Start()
    {
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
        UpdateAnimation();

        if (!IsOwner) return;
        Textprint();
        Look();
        Move();
        Jump();
        
        if (Vector3.Distance(tank.position, transform.position) < 10)
        {
             if (Input.GetKeyDown(KeyCode.F))
            {
                rideOn = rideOn ? false : true;
                rb.useGravity=!rideOn;
                UiState();
                collider.enabled = !rideOn;
                if (!rideOn)
                {
                    Vector3 pos = tank.position;
                    pos.x += 5f;
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
    void Textprint()
    {
        bullet.text=$"{haveMagazine.Value}"+"/5";
    }
    void UpdateAnimation()
    {
        animator.SetBool("run", isWalking.Value);
    }

    [ServerRpc]
    void walkSoundServerRpc(ServerRpcParams rpcParams = default)
    {
        if(isWalkSoundRunning) return;
        StartCoroutine(WalkSoundControll());
    }
    private IEnumerator WalkSoundControll()
    {
        isWalkSoundRunning=true;
        SoundManager.Instance.PlayNetworkSound(walkSound, transform.position);
        yield return new WaitForSeconds(11f);
        isWalkSoundRunning=false;
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        bool isMovingInput = (x != 0 || z != 0);

        if (isWalking.Value != isMovingInput)
        {
            isWalking.Value = isMovingInput;
        }

        if (isMovingInput && isGrounded)
        {
            soundTimer += Time.deltaTime;
            if (soundTimer >= soundInterval)
            {
                walkSoundServerRpc();
                soundTimer = 0f;
            }
        }
        else
        {
            soundTimer = soundInterval;
        }

        Vector3 move = transform.right * x + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    void ReloadServerRpc(ServerRpcParams rpcParams = default)
    {
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

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
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
        if (other.CompareTag("BulletAdd"))
        {
            if (!IsServer) return;

            haveMagazine.Value += 1;
            Debug.Log($"탄창 획득! 현재 개수: {haveMagazine.Value}");

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

    // [완전히 수정된 부분]
    // override 키워드와 base.OnDestroy()를 완전히 삭제했습니다.
    public void OnDestroy()
    {
        // 1. 내 캐릭터(Owner)였던 경우에만 마우스를 복구합니다.
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.None; // 마우스 잠금 해제
            Cursor.visible = true;                  // 마우스 보이게 하기
            Debug.Log("방을 나가서 마우스 커서를 복구했습니다.");
        }
        
        // base.OnDestroy(); // 이 줄은 반드시 삭제되어야 합니다.
    }
}