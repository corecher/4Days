using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    void Update()
    {
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
