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
            Debug.Log("if ���Ӿ� flag" + flag+"�뷡 ���!");
            Debug.Log("�ٲٱ���" + AudioListener.pause);
            AudioListener.pause = false;
            Debug.Log("�ٲ� ��" + AudioListener.pause);

        }
        else
        {
            flag = SettingStore.instance.getBackgroundMusicFlag();
            Debug.Log("else ���Ӿ� flag" + flag + "�뷡 �����!");
            Debug.Log("�ٲٱ���" + AudioListener.pause);
            AudioListener.pause = true;
            Debug.Log("�ٲ� ��" + AudioListener.pause);
        }
    }

}
