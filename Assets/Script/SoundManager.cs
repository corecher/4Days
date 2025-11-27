using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;

    [System.Serializable]
    public struct SoundData
    {
        public string name;      
        public AudioClip clip;   
        [Range(0f, 1f)] public float volume; 
    }

    [Header("효과음(SFX) 목록")]
    [SerializeField] private List<SoundData> soundList;

    // [추가] BGM 목록 따로 관리
    [Header("배경음악(BGM) 목록")]
    [SerializeField] private List<SoundData> bgmList;

    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();
    private Dictionary<string, SoundData> bgmDictionary = new Dictionary<string, SoundData>();

    // [추가] BGM을 재생할 전용 스피커 (오디오 소스)
    private AudioSource bgmSource; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // SFX 딕셔너리
        foreach (var sound in soundList)
        {
            if (!soundDictionary.ContainsKey(sound.name)) soundDictionary.Add(sound.name, sound);
        }

        // [추가] BGM 딕셔너리 세팅
        foreach (var bgm in bgmList)
        {
            if (!bgmDictionary.ContainsKey(bgm.name)) bgmDictionary.Add(bgm.name, bgm);
        }

        // [추가] BGM 전용 AudioSource 생성 및 세팅
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;          // 무한 반복
        bgmSource.spatialBlend = 0f;    // 2D 사운드 (거리 영향 X)
        bgmSource.playOnAwake = false;
    }

    // ... (기존 PlayNetworkSound, PlayLocalSound 코드는 그대로 유지) ...

    // ==================================================================================
    // [추가] BGM 재생 함수 (로컬)
    // - 씬이 바뀔 때나 클라이언트가 시작할 때 호출
    // ==================================================================================
    public void PlayBGM(string bgmName)
    {
        if (bgmDictionary.TryGetValue(bgmName, out SoundData data))
        {
            // 이미 이 노래가 재생 중이면 다시 틀지 않음 (끊김 방지)
            if (bgmSource.clip == data.clip && bgmSource.isPlaying) return;

            bgmSource.clip = data.clip;
            bgmSource.volume = data.volume; // BGM 볼륨 설정
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM {bgmName}을(를) 찾을 수 없습니다!");
        }
    }

    // [추가] BGM 끄기
    public void StopBGM()
    {
        bgmSource.Stop();
    }
    
    // [추가] 서버에서 강제로 모두에게 BGM 변경 명령 (예: 보스 등장)
    public void PlayNetworkBGM(string bgmName)
    {
        if(IsServer)
        {
            PlayBGMClientRpc(bgmName);
        }
    }

    [ClientRpc]
    private void PlayBGMClientRpc(string bgmName)
    {
        PlayBGM(bgmName);
    }
    public void PlayNetworkSound(string soundName, Vector3 position)
    {
        // 서버만 이 명령을 내릴 수 있음
        if (!IsServer) return;

        PlaySoundClientRpc(soundName, position);
    }

    // ==================================================================================
    // 2. [클라이언트 실행] 실제 소리 재생 (RPC)
    // ==================================================================================
    [ClientRpc]
    private void PlaySoundClientRpc(string soundName, Vector3 position)
    {
        PlayLocalSound(soundName, position);
    }

    // ==================================================================================
    // 3. [로컬 실행] 실제 오디오 소스 생성 및 재생
    // ==================================================================================
    public void PlayLocalSound(string soundName, Vector3 position)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundData data))
        {
            // 임시 오디오 오브젝트 생성 (재생 후 자동 삭제됨)
            // PlayClipAtPoint는 간단하지만 볼륨 조절이 안 되므로 커스텀 로직 사용
            
            GameObject soundObj = new GameObject("TempAudio_" + soundName);
            soundObj.transform.position = position;
            
            AudioSource source = soundObj.AddComponent<AudioSource>();
            source.clip = data.clip;
            source.volume = data.volume;
            source.spatialBlend = 1f; // 3D 사운드 (거리에 따라 소리 작아짐)
            source.minDistance = 2f;  // 소리가 들리기 시작하는 거리
            source.maxDistance = 50f; // 소리가 안 들리는 최대 거리
            
            source.Play();

            // 재생 시간만큼 기다렸다가 오브젝트 삭제
            Destroy(soundObj, data.clip.length);
        }
        else
        {
            Debug.LogWarning($"사운드 {soundName}을(를) 찾을 수 없습니다!");
        }
    }
}
