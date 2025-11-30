using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeEffect : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("페이드 효과가 적용될 CanvasGroup")]
    public List<CanvasGroup> fadeCanvasGroup;
    
    [Tooltip("페이드 아웃에 걸리는 시간 (초)")]
    public List<float> fadeDuration;
    [SerializeField]public GameObject panel1;
    [SerializeField]public GameObject panel2;
    public bool State;
    [Header("UI 연결")]
    public Text textDisplay; // 글자가 나타날 텍스트 컴포넌트
    public string fullText; // 출력할 전체 문장
    public float typingSpeed = 0.1f; // 글자 간 시간 간격 (작을수록 빠름)
     void Start()
     {
        State=!SoundManager.Instance.state;
        textDisplay.gameObject.SetActive(false);
        if(!State)
        {
            FadeOut(0);
            PanelChange(1);
        }
        else
        {
            FadeIn(1);
            PanelChange(0);
            SoundManager.Instance.PlayBGM("Flyed");
        }
        
     }
     private void PanelChange(int i)
     {
        panel1.SetActive(i==0);
        panel2.SetActive(i!=0);
     }

    public void FadeOut(int i)
    {
        StartCoroutine(FadeOutRoutine(i));
    }

    public void FadeIn(int i)
    {
        StartCoroutine(FadeInRoutine(i));
    }

    // 페이드 아웃: 화면이 점점 어두워짐 (Alpha 0 -> 1)
    private IEnumerator FadeOutRoutine(int i)
    {
        float timer = 0f;
        
        while (timer < fadeDuration[i])
        {
            timer += Time.deltaTime;
            // Lerp를 사용하여 0에서 1로 부드럽게 변경
            fadeCanvasGroup[i].alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration[i]);
            yield return null;
        }

        // 확실하게 1로 설정
        fadeCanvasGroup[i].alpha = 1f;
        
        // 페이드 아웃이 끝나면 뒤의 버튼이 눌리지 않도록 레이캐스트 차단
        fadeCanvasGroup[i].blocksRaycasts = true; 
        StartCoroutine(SceneChange());
    }

    // 페이드 인: 화면이 점점 밝아짐 (Alpha 1 -> 0)
    private IEnumerator FadeInRoutine(int i)
    {
        float timer = 0f;

        while (timer < fadeDuration[i])
        {
            timer += Time.deltaTime;
            // Lerp를 사용하여 1에서 0으로 부드럽게 변경
            fadeCanvasGroup[i].alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration[i]);
            yield return null;
        }

        fadeCanvasGroup[i].alpha = 0f;
        
        // 페이드 인이 끝나면 뒤의 UI와 상호작용 할 수 있도록 레이캐스트 해제
        fadeCanvasGroup[i].blocksRaycasts = false;
        StartTyping(fullText);
    }
    private IEnumerator SceneChange()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Menu");
    }
    IEnumerator TypeText()
    {
        // 1. 텍스트를 비웁니다.
        textDisplay.text = "";

        // 2. 한 글자씩 루프를 돌며 출력합니다.
        for (int i = 0; i < fullText.Length; i++)
        {
            // 한 글자 추가
            textDisplay.text += fullText[i];

            // 설정한 시간만큼 대기 (이것 때문에 타자기처럼 보임)
            yield return new WaitForSeconds(typingSpeed);
        }
        
        Debug.Log("타이핑 끝!");
        SoundManager.Instance.StopBGM();
        StartCoroutine(SceneChange());
    }
    
    // 외부에서 텍스트를 바꿔서 실행하고 싶을 때 쓰는 함수
    public void StartTyping(string message)
    {
        textDisplay.gameObject.SetActive(true);
        fullText = message;
        StopAllCoroutines(); // 혹시 실행 중인 게 있으면 멈춤
        StartCoroutine(TypeText());
    }
}
