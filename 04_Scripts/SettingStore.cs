using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingStore : MonoBehaviour
{

    public static SettingStore instance;
    private bool backgroundMusicFlag;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }

        backgroundMusicFlag = false;
    }
    public void setBackgroundMusicFlag(bool flag)
    {
        Debug.Log("flag πŸ≤„¡‡" + flag);
        backgroundMusicFlag = flag;
    }
    
    public bool getBackgroundMusicFlag()
    {
        return backgroundMusicFlag;
    }

}
