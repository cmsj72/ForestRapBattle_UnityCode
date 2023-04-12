using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using Photon.Pun;
using Photon.Realtime;

public class ReactController : MonoBehaviour
{
    int checkCnt = 1; //����Ʈ���� ���� ������ parsing�ϱ� ����

    public static ReactController Instance;

    Player player;//���� ����Ÿ���� Player�� ���� �� �� �ְ� ���ش�.

    public static string userNickname = "";
    public static int userUid = 0;
    public static int userChar = 0;
    public static int userWinPoint = 0;
    public static string userTier = "";
    public static string token = "";

    string[] tiers = new string[] { "Iron", "Bronze", "Silver", "Gold", "Platinum", "Diamond" };

    private void Awake()
    {
        LobbyStart();
        Instance = this;
    }

    private void Start()
    {
        //LobbyStart();
    }

    [DllImport("__Internal")]
    private static extern void LobbyStart();

    public static void Exit()
    {
        Application.Quit();
        ExitUnity();
    }

    [DllImport("__Internal")]
    private static extern void ExitUnity();

    public static void Change(int charInt)
    {
        userChar = charInt;
        ChangeChar(charInt);
    }

    [DllImport("__Internal")]
    private static extern void ChangeChar(int charInt);


    public void FromReact(string data)
    {
        Debug.Log("-----------------------------");
        Debug.Log("from react to unity!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Debug.Log(data); //'nickname,uid,charidx,winpoint,token'

        string nick_tmp = "";
        string uid_tmp = "";
        string char_tmp = "";
        string wp_tmp = "";
        string tk_tmp = "";

        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != ',') // ��ǥ�� �ƴϸ� ������ �ٿ���
            {
                if (checkCnt == 1)
                {
                    nick_tmp += data[i];
                }

                if (checkCnt == 2)
                {
                    uid_tmp += data[i];
                }

                if (checkCnt == 3)
                {
                    char_tmp += data[i];
                }

                if (checkCnt == 4)
                {
                    wp_tmp += data[i];
                }

                if (checkCnt == 5)
                {
                    tk_tmp += data[i];
                }
            }
            
            else //��ǥ�� ������ ī��Ʈ ����
            {
                checkCnt += 1;
            }
        }
        userNickname = nick_tmp; //�г����� ����ȯ ���� �״��

        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        hashtable.Add("NICK", userNickname);
        PhotonNetwork.SetPlayerCustomProperties(hashtable);

        token = tk_tmp;

            if (int.TryParse(uid_tmp, out int result)) // �Ʒ� ������ ���������� ��ȯ
            {
                userUid = result; 
            }
            if (int.TryParse(char_tmp, out int result2))
            {
                userChar = result2;
            }
            if (int.TryParse(wp_tmp, out int result3))
            {
                userWinPoint = result3;
            }
            //userUid = int.Parse(uid_tmp);
            //userChar = int.Parse(char_tmp);
            //userWinPoint = int.Parse(wp_tmp);

            Debug.Log(userNickname);
            Debug.Log(userUid);
            Debug.Log(userChar);
            Debug.Log(userWinPoint);
        Debug.Log(token);

        ExitGames.Client.Photon.Hashtable hashtable1 = new ExitGames.Client.Photon.Hashtable();
        hashtable1.Add("UID", userUid);
        PhotonNetwork.SetPlayerCustomProperties(hashtable1);
        //player.SetCustomProperties(hashtable);

        //winpoint �������� Ƽ�� �����ֱ�

        if (userWinPoint >= 0 && userWinPoint < 700)
            {
                userTier = tiers[0]; //���̾�
            }

            else if (userWinPoint >= 700 && userWinPoint < 900)
            {
                userTier = tiers[1]; //�����
            }

            else if (userWinPoint >= 900 && userWinPoint < 1100)
            {
                userTier = tiers[2]; //�ǹ�
            }

            else if (userWinPoint >= 1100 && userWinPoint < 1300)
            {
                userTier = tiers[3]; //���
            }

            else if (userWinPoint >= 1300 && userWinPoint < 1600)
            {
                userTier = tiers[4]; //�÷�Ƽ��
            }

            else if (userWinPoint >= 1600)
            {
                userTier = tiers[5]; //���̾� ���� ��� �������?
            }
    }
}
