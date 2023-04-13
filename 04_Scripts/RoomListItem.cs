using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;//텍스트 메쉬 프로 기능 사용
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class RoomListItem : MonoBehaviourPunCallbacks//다른 포톤 반응 받아들이기 
{
    [SerializeField] TMP_Text RoomNameText;
    [SerializeField] TMP_Text RoomOwnerText;
    [SerializeField] TMP_Text PlayerCntText;

    [SerializeField] RawImage Lock;
    [SerializeField] Button InGameBtn;
    [SerializeField] Button IsFullBtn;

    public bool isGaming = false;

    // Start is called before the first frame update
    public RoomInfo info;

    public static RoomListItem Instance;//스크립트를 메서드로 사용하기 위해 선언

    void Awake()
    {
        Instance = this;//메서드로 사용
    }

    public void SetUp(RoomInfo _info)// 방정보 받아오기
    {
        info = _info;
        RoomNameText.text = _info.Name;
        RoomOwnerText.text = (string)_info.CustomProperties["Owner"];

        int playerCount = _info.PlayerCount;
        Debug.Log(playerCount);
        Debug.Log("room owner:" + _info.CustomProperties["Owner"]);
        Debug.Log("is secret:" + _info.CustomProperties["isSecret"]);
        Debug.Log("gaming state:" + _info.CustomProperties["isGaming"]);

        PlayerCntText.text = playerCount.ToString() + "/" + _info.MaxPlayers.ToString();

        if (playerCount == 1)
        {
            _info.CustomProperties["isOpen"] = "True";
            Debug.Log(info.CustomProperties["isOpen"].ToString());
            Debug.Log("Room Opened");
        }
        
        if (playerCount == 2)
        {
            _info.CustomProperties["isOpen"] = "False";
            Debug.Log(info.CustomProperties["isOpen"].ToString());
            Debug.Log("Room Closed");
        }


        if (_info.CustomProperties["isOpen"].ToString() == "False") //들어갈 수 없는 상태(full or in game)
        {
            if (_info.CustomProperties["isGaming"].ToString() == "True") // 게임 진행중
            {
                Debug.Log("room closed: in game");
                Lock.gameObject.SetActive(false);
                IsFullBtn.gameObject.SetActive(false);
                InGameBtn.gameObject.SetActive(true);
            }
            else // 2명이라 못 들어감
            {
                Debug.Log("room closed: is full");
                Lock.gameObject.SetActive(false);
                IsFullBtn.gameObject.SetActive(true);
                InGameBtn.gameObject.SetActive(false);
            }
            
        }
        else // 열린 상태
        {
            if ((string)_info.CustomProperties["isSecret"] == "True") // 비밀방
            {
                Debug.Log("room open: secret on");
                Lock.gameObject.SetActive(true);
                IsFullBtn.gameObject.SetActive(false);
                InGameBtn.gameObject.SetActive(false);
            }
            else //그저 열린방
            {
                Debug.Log("room open: secret off");
                Lock.gameObject.SetActive(false);
                IsFullBtn.gameObject.SetActive(false);
                InGameBtn.gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    public void OnClick()
    {
        LobbyManager.Instance.JoinRoom(info);//런처스크립트 메서드로 JoinRoom 실행
    }
}

