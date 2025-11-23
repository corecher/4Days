using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;


public class TestAttack : NetworkBehaviour
{
    [SerializeField] float rotationSpeed;
    [SerializeField] float minX = -50f;
    [SerializeField] float maxX = 0f;
    [SerializeField] GameObject bullet;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private List<Transform> muzzleTransform;
    [SerializeField] private Transform cameraTransform;
    private int index = 0; 
    [SerializeField] private float bulletLifeTime = 5f;
    [SerializeField] private Transform body;
    [SerializeField] private Vector3 offset;
    [SerializeField] public NetworkVariable<int> ammunition = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public NetworkVariable<int> hp = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]private Image hpUi;
    void Start()
    {
        body=GameObject.FindWithTag("body").GetComponent<Transform>();
        if (IsServer)
        {
            hp.Value = 1000;
            ammunition.Value = 100;   // 다른 변수도 서버가 초기화하도록
        }
    }
    void Update()
    {
        FixBody();
        if (!IsOwner) return;
        Attack();
        hpUi.fillAmount=hp.Value/1000f;
    }

    void Attack()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            RotationClamp(-1);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            RotationClamp(1);
        }
        if (Input.GetKeyDown(KeyCode.F)&&ammunition.Value>0)
        {
            RequestFireServerRpc();
        }
    }
    [ServerRpc]
    void RequestFireServerRpc()
    {
        if (ammunition.Value <= 0) return;

        ammunition.Value--;

        index = index == 0 ? 1 : 0;

        Shot(index);
    }
    void FixBody()
    {
        transform.position=offset+body.position;
    }
    void RotationClamp(int input)
    {
        foreach (Transform child in transform)
        {
            Vector3 angles = child.localEulerAngles;
            float x = angles.x;
            if (x > 180f) x -= 360f;
            x += input * rotationSpeed * Time.deltaTime;
            x = Mathf.Clamp(x, minX, maxX);
            angles.x = x;
            child.localEulerAngles = angles;
        }
    }

    void Shot(int i)
    {
        Vector3 spawnPos;
        Vector3 forwardDir;
        if (muzzleTransform != null)
        {
            spawnPos = muzzleTransform[i].position;
            forwardDir = cameraTransform.forward;
        }
        else
        {
            spawnPos = cameraTransform.position + cameraTransform.forward * 0.6f;
            forwardDir = cameraTransform.forward;
        }
        GameObject bullets = Instantiate(bullet, spawnPos, Quaternion.LookRotation(forwardDir));
        Rigidbody rb = bullets.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = forwardDir.normalized * bulletSpeed;
        }
        Destroy(bullets, bulletLifeTime);
    }
}

