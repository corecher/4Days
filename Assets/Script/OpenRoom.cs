using UnityEngine;

public class OpenRoom : MonoBehaviour
{
    [SerializeField]private GameObject room;
    public void Open()
    {
        room.SetActive(true);
    }
}
