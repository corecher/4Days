using UnityEngine;

public class LobbyCursorSetter : MonoBehaviour
{
    void Start()
    {
        // 이 씬이 시작되면 무조건 마우스를 보이게 하고 잠금을 풉니다.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM("MenuBGM");
        }
    }
}
