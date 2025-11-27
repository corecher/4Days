using UnityEngine;
using UnityEngine.UI; // TextMeshPro를 사용하기 위해 필요

public class TimerUi : MonoBehaviour
{
    // 에디터에서 TextMeshProUGUI 오브젝트를 연결할 변수
    [SerializeField] private Text timeText;

    void Update()
    {
        if (Timer.Instance != null)
        {
            timeText.text = Timer.Instance.GetTimeString();
        }
    }
}
