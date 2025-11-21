using Unity.Netcode;
using UnityEngine;

public class Test : NetworkBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField]private FP_CubeController fP_CubeController;
    void Update()
    {
        if(!IsOwner) return;
        if(fP_CubeController.rideOn)
        TankMove();
    }
    void TankMove()
    {
        if(Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward*moveSpeed*Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back*moveSpeed*Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector2.up*-moveSpeed*Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector2.up*moveSpeed*Time.deltaTime);
        }
    }
}
