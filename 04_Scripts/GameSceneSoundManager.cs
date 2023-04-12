using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneSoundManager : MonoBehaviour
{
    bool flag = false;
    //public AudioSource gameAudio;

    // Start is called before the first frame update
    void Start()
    {
        //this.gameAudio = this.gameObject.AddComponent<AudioSource>();

        if(!SettingStore.instance.getBackgroundMusicFlag())
        {
            flag = SettingStore.instance.getBackgroundMusicFlag();
            Debug.Log("if 게임씬 flag" + flag+"노래 재생!");
            Debug.Log("바꾸기전" + AudioListener.pause);
            AudioListener.pause = false;
            Debug.Log("바꾼 후" + AudioListener.pause);

        }
        else
        {
            flag = SettingStore.instance.getBackgroundMusicFlag();
            Debug.Log("else 게임씬 flag" + flag + "노래 멈춰어!");
            Debug.Log("바꾸기전" + AudioListener.pause);
            AudioListener.pause = true;
            Debug.Log("바꾼 후" + AudioListener.pause);
        }
    }

}
