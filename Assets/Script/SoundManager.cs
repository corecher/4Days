using System;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : MonoBehaviour
{
   public static SoundManager Soundmanager;

   [Header("BGM Sound")]
    public AudioSource BgmSource;
    public AudioClip[] BGMClips;

    [Header("Effect Sound")]
    public AudioSource EffectSource;
    public AudioClip[] EffectClips;

    void Awake()
    {
        if (Soundmanager== null){
          Soundmanager = this;
          DontDestroyOnLoad(gameObject);
        }
        else
          Destroy(gameObject);
    }
    public void PlaySound(bool BGMorEffect,int index,float volume,bool loop,float playstarttime,float speed){
        if(BGMorEffect)
        PlayBGM(index,volume,loop,playstarttime,speed);
        else
        PlayEffect(index,volume,loop,playstarttime,speed);
    }
    void PlayBGM(int index,float volume,bool loop,float playstarttime,float speed)
    {
        if (index >= 0 && index < BGMClips.Length)
        {   
            BgmSource.volume=volume;
            BgmSource.clip = BGMClips[index];
            BgmSource.loop = loop;
            BgmSource.pitch= speed;
            BgmSource.Play();
        }
    }
    void PlayEffect(int index,float volume,bool loop,float playstarttime,float speed){
        if (index >= 0 && index < EffectClips.Length){
            EffectSource.volume=volume;
            EffectSource.loop = loop;
            EffectSource.pitch= speed;
            EffectSource.PlayOneShot(EffectClips[index]);
        }
            
        
    }
    public void StopBGM()
    {
        BgmSource.Stop();
    }
}
