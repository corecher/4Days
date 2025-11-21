using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FP_CubeController : NetworkBehaviour
{
    public Transform cam;               // 카메라(자식)
    public float mouseSensitivity = 300f;
    public float moveSpeed = 5f;
    float xRotation = 0f;
    [SerializeField]private Transform tank;
    private Collider collider;
    public bool rideOn=false;
    [SerializeField]private Vector3 offset;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        collider=GetComponent<Collider>();
    }

    void Update()
    {
        if (!IsOwner) return; // 오너만 이동
        Look();
        Move();
        Debug.Log(rideOn);
        if(Vector3.Distance(tank.position,transform.position)<5&&Input.GetKeyDown(KeyCode.F))
        {
            rideOn=rideOn?false:true;
            collider.enabled=!rideOn;
            if(!rideOn)
            {
                Vector3 pos=tank.position;
                pos.x+=3f;
                transform.position=pos;
            }
        }
        if(rideOn)RideTank();
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
}


