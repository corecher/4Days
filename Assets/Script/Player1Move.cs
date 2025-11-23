using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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
    [SerializeField]private Transform tank;
    private Collider collider;
    public bool rideOn=false;
    [SerializeField]private Vector3 offset;
    [SerializeField]private TestAttack testAttack;
    [SerializeField]private List<GameObject> uis;
    [SerializeField] public NetworkVariable<int> haveMagazine = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        collider=GetComponent<Collider>();
        rb = GetComponent<Rigidbody>(); 
        UiState();
    }

    void Update()
    {
        if (!IsOwner) return; // 오너만 이동
        Look();
        Move();
        Jump();
        Debug.Log(rideOn);
        if(Vector3.Distance(tank.position,transform.position)<5)
        {
            if(Input.GetKeyDown(KeyCode.F))
            {
                rideOn=rideOn?false:true;
                UiState();
                collider.enabled=!rideOn;
                if(!rideOn)
                {
                    Vector3 pos=tank.position;
                    pos.x+=3f;
                    transform.position=pos+offset;
                    
                }
            }
            if(Input.GetKeyDown(KeyCode.R)&&haveMagazine.Value>0)
            {
                ReloadServerRpc();
            }
        }
        if(rideOn)RideTank();
        
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

        Vector3 move = transform.right * x + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
    void RideTank()
    {
        transform.position=offset+tank.position;
    }
    void UiState()
    {
        uis[0].SetActive(rideOn);
        uis[1].SetActive(!rideOn);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;     // ★ 땅에 닿으면 점프 가능
        }
    }
}


