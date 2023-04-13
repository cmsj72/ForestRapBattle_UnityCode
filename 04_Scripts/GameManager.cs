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

    //����������
    public string masterNick = "";
    public string masterUid = "";
    public string guestNick = "";
    public string guestUid = "";

    private string[] CharList = new string[] { "Muscle Bear", "Muscle Buffalo", "Muscle Cat", "Muscle Chicken", "Muscle Chick",
        "Muscle Dog", "Muscle Duck", "Muscle Elephant", "Muscle Frog", "Muscle Monkey",
        "Muscle Pig", "Muscle Rabbit", "Muscle Rhino"};

    Player player;//���� ����Ÿ���� Player�� ���� �� �� �ְ� ���ش�.

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
            // �ð��� üũ�� ���ӽð��� �����ų� �÷��̾� �� �� �� ���� ������ ���ǹ� ����
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

        if (Left.GetHealth() > Right.GetHealth()) //������ �̱� ��Ȳ
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
        else if (Left.GetHealth() < Right.GetHealth()) //�Խ�Ʈ �÷��̾ �̱� ��Ȳ
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
        else if (Left.GetHealth() == Right.GetHealth()) //���
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

            //�ܾ� ������
            words.GetRandomWords();
        }
    }

    bool GameTimeCheck()
    {
        gameTime = circularGameTimer.CurrentTime;
        // ���� �ð��� ������ ���        
        if (circularGameTimer.duration - gameTime <= 0.0f)
        {
            return true;
        }
        return false;
    }

    bool HealthCheck()
    {
        // �÷��̾� �θ��� ü���� üũ
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

    //  �̱� �÷��̾��� �����͸� ������ ������ ���� �ڷ�ƾ
    public IEnumerator PostGameData(string uid, string player1, string player2, string win, string lose)
    {
        WWWForm form = new WWWForm(); //�ӽ÷� ���� ���ӵ�����(���� 1, ��밡 2�̰� ���� �̱� ��Ȳ)

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

        //  ������ �� �����͸� ������ ���� ���� ������ ���
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
