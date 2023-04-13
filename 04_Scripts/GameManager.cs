using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("Timer References")]
    [SerializeField]
    private GameObject StartTimer;
    [SerializeField]
    private GameObject GameTimer;
    [Space(5f)]

    [Header("Castle References")]
    [SerializeField]
    private GameObject LeftCastle;
    [SerializeField]
    private GameObject RightCastle;
    [Space(5f)]

    [Header("Character Position References")]
    [SerializeField]
    private Transform LeftPosition;
    [SerializeField]
    private Transform RightPosition;
    [Space(5f)]

    [Header("Generator Position References")]
    [SerializeField]
    private Transform LeftGenPosition;
    [SerializeField]
    private Transform RightGenPosition;
    [Space(5f)]

    [Header("Game Result References")]
    [SerializeField]
    private GameObject GameResultPanel;
    [SerializeField]
    private TMP_Text WinText;
    [SerializeField]
    private TMP_Text LoseText;
    [SerializeField]
    private TMP_Text Player1NickText;
    [SerializeField]
    private TMP_Text Player2NickText;
    [SerializeField]
    private TMP_Text PlusText;
    [SerializeField]
    private TMP_Text MinusText;
    [SerializeField]
    private TMP_Text DrawText;
    [SerializeField]
    private TMP_Text Draw1Text;
    [SerializeField]
    private TMP_Text Draw2Text;
    [SerializeField]
    private Button GoToLobbyBtn;
    [Space(5f)]

    [Header("Words")]
    [SerializeField]
    private Words words;

    private CircularTimer circularStartTimer;
    private CircularTimer circularGameTimer;
    private Castle Left;
    private Castle Right;
    private float gameTime;
    private bool isStart;
    private bool isEnd;

    //게임전적용
    public string masterNick = "";
    public string masterUid = "";
    public string guestNick = "";
    public string guestUid = "";

    private string[] CharList = new string[] { "Muscle Bear", "Muscle Buffalo", "Muscle Cat", "Muscle Chicken", "Muscle Chick",
        "Muscle Dog", "Muscle Duck", "Muscle Elephant", "Muscle Frog", "Muscle Monkey",
        "Muscle Pig", "Muscle Rabbit", "Muscle Rhino"};

    Player player;//포톤 리얼타임은 Player를 선언 할 수 있게 해준다.

    private void Awake()
    {
        GoToLobbyBtn.onClick.AddListener(End);
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GamePlayerInit();
        circularStartTimer = StartTimer.GetComponent<CircularTimer>();
        circularGameTimer = GameTimer.GetComponent<CircularTimer>();
        isStart = false;
        isEnd = false;
        circularStartTimer.StartTimer();

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Player player in players)
        {
            if (player.IsMasterClient)
            {
                masterNick = player.CustomProperties["NICK"].ToString();
                masterUid = player.CustomProperties["UID"].ToString();
                Debug.Log(masterNick);
                Debug.Log(masterUid);
            }
            else
            {
                guestNick = player.CustomProperties["NICK"].ToString();
                guestUid = player.CustomProperties["UID"].ToString();
                Debug.Log(guestNick);
                Debug.Log(guestUid);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStart)
        {
            StartTimeCheck();
        }
        else
        {
            // 시간을 체크해 게임시간이 끝나거나 플레이어 둘 중 한 명이 죽으면 조건문 진입
            if (GameTimeCheck() || HealthCheck())
            {
                if (!isEnd)
                {
                    isEnd = true;
                    ActivateGameResult();
                }
            }
        }        
    }

    private void ActivateGameResult()
    {
        GameResultPanel.gameObject.SetActive(true);

        if (Left.GetHealth() > Right.GetHealth()) //방장이 이긴 상황
        {
            Player1NickText.text = masterNick;
            Player2NickText.text = guestNick;

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(PostGameData(masterUid, masterUid, guestUid, "true", "false"));
            }
            else
            {
                StartCoroutine(PostGameData(guestUid, guestUid, masterUid, "false", "true"));
            }

            WinText.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            LoseText.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
        }
        else if (Left.GetHealth() < Right.GetHealth()) //게스트 플레이어가 이긴 상황
        {
            Player1NickText.text = guestNick;
            Player2NickText.text = masterNick;

            if (PhotonNetwork.IsMasterClient)
            {
                LoseText.gameObject.SetActive(true);
                StartCoroutine(PostGameData(masterUid, masterUid, guestUid, "false", "true"));
            }
            else
            {
                WinText.gameObject.SetActive(true);
                StartCoroutine(PostGameData(guestUid, guestUid, masterUid, "true", "false"));
            }

            WinText.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
            LoseText.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }
        else if (Left.GetHealth() == Right.GetHealth()) //비김
        {
            Player1NickText.text = guestNick;
            Player2NickText.text = masterNick;

            WinText.gameObject.SetActive(false);
            LoseText.gameObject.SetActive(false);
            PlusText.gameObject.SetActive(false);
            MinusText.gameObject.SetActive(false);
            DrawText.gameObject.SetActive(true);
            Draw1Text.gameObject.SetActive(true);
            Draw2Text.gameObject.SetActive(true);
        }
    }

    private void End()
    {
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(2.0f);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        if(SceneManager.GetActiveScene().name == "GameScene")
        {
            PhotonNetwork.LoadLevel("Lobby");
        }
    }

    void GamePlayerInit()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject masterPlayer = PhotonNetwork.Instantiate(CharList[ReactController.userChar - 1],
                LeftPosition.position, Quaternion.identity, 0);
            masterPlayer.transform.GetChild(3).GetChild(0).gameObject.GetComponent<RectTransform>().rotation
                = Quaternion.Euler(new Vector3(0, 180, 0));
            masterPlayer.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            PhotonNetwork.Instantiate("UnitGenerator_Left", LeftGenPosition.position, Quaternion.identity, 0);
        }
        else
        {
            GameObject guestPlayer = PhotonNetwork.Instantiate(CharList[ReactController.userChar - 1],
                RightPosition.position, Quaternion.identity, 0);
            guestPlayer.transform.GetChild(3).GetChild(0).gameObject.GetComponent<RectTransform>().rotation
                = Quaternion.Euler(new Vector3(0, 180, 0));
            guestPlayer.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            PhotonNetwork.Instantiate("UnitGenerator_Right", RightGenPosition.position, Quaternion.identity, 0);
        }
        Left = LeftCastle.GetComponent<Castle>();
        Right = RightCastle.GetComponent<Castle>();
    }

    void StartTimeCheck()
    {
        gameTime = circularStartTimer.CurrentTime;
        if(circularStartTimer.duration - gameTime <= 0.0f)
        {
            gameTime = 0.0f;
            isStart = true;
            circularStartTimer.StopTimer();
            StartTimer.SetActive(false);
            circularGameTimer.StartTimer();

            //단어 보여줌
            words.GetRandomWords();
        }
    }

    bool GameTimeCheck()
    {
        gameTime = circularGameTimer.CurrentTime;
        // 게임 시간이 끝났을 경우        
        if (circularGameTimer.duration - gameTime <= 0.0f)
        {
            return true;
        }
        return false;
    }

    bool HealthCheck()
    {
        // 플레이어 두명의 체력을 체크
        if (Left.GetHealth() <= 0 || Right.GetHealth() <= 0)
        {
            return true;
        }
        return false;
    }

    public bool GetIsStart()
    {
        return isStart;
    }

    //  이긴 플레이어의 데이터를 서버로 보내기 위한 코루틴
    public IEnumerator PostGameData(string uid, string player1, string player2, string win, string lose)
    {
        WWWForm form = new WWWForm(); //임시로 만들 게임데이터(내가 1, 상대가 2이고 내가 이긴 상황)

        form.AddField("user_id", uid);
        form.AddField("player1", player1);
        form.AddField("player2", player2);
        form.AddField("win", win); //true or false
        form.AddField("lose", lose);

        Debug.Log(uid);
        Debug.Log(player1);
        Debug.Log(player2);
        Debug.Log(win);
        Debug.Log(lose);

        //  서버로 폼 데이터를 보내고 응답 받을 때까지 대기
        using (UnityWebRequest www = UnityWebRequest.Post("https://k6e204.p.ssafy.io/api/v1/game/gameResult", form))
        {
            www.SetRequestHeader("Authorization", ReactController.token);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Post Data Complete!");
            }
        }
    }
}
