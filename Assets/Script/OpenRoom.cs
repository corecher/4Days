using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class OpenRoom : MonoBehaviour
{
    [SerializeField]private GameObject room;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) Open(false);
    }
    public void Open(bool state)
    {
        room.SetActive(state);
    }
}
