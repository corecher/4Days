using Unity.Netcode;
using UnityEngine;

public class Player1Move : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 5f;

    void Update()
    {
        if (!IsOwner) return; // 오너만 이동
        move();        
    }
    void move()
    {
        
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(h, 0, v).normalized;
        
        transform.position += movement * moveSpeed * Time.deltaTime;

        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
        }
    }
}


