using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManagerTest : MonoBehaviour
{
    Player player;//포톤 리얼타임은 Player를 선언 할 수 있게 해준다.
    public static GameManagerTest instance;

    public bool isConnect = false;

    private string[] CharList = new string[] { "Muscle Bear", "Muscle Buffalo", "Muscle Cat", "Muscle Chick", "Muscle Chicken", 
        "Muscle Dog", "Muscle Duck", "Muscle Elephant", "Muscle Frog", "Muscle Monkey",
        "Muscle Pig", "Muscle Rabbit", "Muscle Rhino"};

    private Dictionary<int, (int, int)> Pos = new Dictionary<int, (int, int)>()
    {
        {0,(-8, 0) }, //방장 만들 위치, 카메라 기준 왼쪽
        {1,(8, 0) }, //게스트 카메라 기준 오른쪽
    };
    private int x;
    private int y;

    void Start()
    {
        isConnect = true;
        StartCoroutine(CreatePlayer());
    }

    IEnumerator CreatePlayer()
    {
        yield return new WaitUntil(() => isConnect);


        Debug.Log(PhotonNetwork.CurrentRoom.Name);
        Debug.Log(PhotonNetwork.NickName);
        Debug.Log(PhotonNetwork.IsMasterClient);
        Debug.Log(PhotonNetwork.LocalPlayer.NickName);

        if (PhotonNetwork.IsMasterClient) //방장인 플레이어
        {
            (x, y) = Pos[0];
            Vector3 pos = new Vector3(x * 1.0f, y * 1.0f, 0.0f);
            Debug.Log(pos);
            Debug.Log("setting master player");
            GameObject masterPlayer = PhotonNetwork.Instantiate(CharList[(int)PhotonNetwork.LocalPlayer.CustomProperties["charInt"]], pos, Quaternion.identity, 0);
            //masterPlayer.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        }
        else //게스트 플레이어
        {
            (x, y) = Pos[1];
            Vector3 pos = new Vector3(x * 1.0f, y * 1.0f, 0.0f);
            Debug.Log(pos);
            Debug.Log("setting guest player");
            GameObject guestPlayer = PhotonNetwork.Instantiate(CharList[(int)PhotonNetwork.LocalPlayer.CustomProperties["charInt"]], pos, Quaternion.identity, 0);
            //guestPlayer.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        }
    }
}
