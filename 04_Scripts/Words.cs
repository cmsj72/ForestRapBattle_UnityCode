using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using FrostweepGames.Plugins.Native;
using TMPro;
using UnityEngine.Networking;
using MiniJSON;
using Photon.Pun;
using Photon.Realtime;

public class Words : MonoBehaviour
{
    private AudioClip _workingClip;
    public AudioSource audioSource;

    public GameObject randomWordsText;

    public TMP_Text ResultText,
                    recordingStatusText,
                    
                    word1LevelText,
                    word2LevelText,
                    word3LevelText,
                    word1Text,
                    word2Text,
                    word3Text;

    public List<AudioClip> recordedClips;

    public string selectedDevice; //테스트씬에서 고른 마이크

    //마이크 세팅
    public int frequency = 44100;
    public int recordingTime = 60;
    public bool makeCopy = false;
    public float averageVoiceLevel = 0f;
    public bool voiceDetectionEnabled = false;

    //녹음 중인지 여부
    public bool recording = false;
    [SerializeField] RawImage micOn,
                                micOff;

    public bool pressed = false;

    //음성데이터 담을 바이트
    byte[] data = null;

    //결과 점수값
    int score = 0;

    //단어전체
    string randomWords;

    //단어목록
    int word1level = 0;
    int word2level = 0;
    int word3level = 0;

    string word1 = "";
    string word2 = "";
    string word3 = "";

    //UnitGenerator
    private UnitGenerator leftUnitGenerator;
    private UnitGenerator rightUnitGenerator;

    // Start is called before the first frame update
    void Start()
    {
        selectedDevice = MicTest.selectedDevice;
        Debug.Log("선택한 장치" + selectedDevice);

        audioSource = GetComponent<AudioSource>();

        CustomMicrophone.RefreshMicrophoneDevices();

        CustomMicrophone.RecordStreamDataEvent += RecordStreamDataEventHandler;
        CustomMicrophone.PermissionStateChangedEvent += PermissionStateChangedEventHandler;
        CustomMicrophone.RecordStartedEvent += RecordStartedEventHandler;
        CustomMicrophone.RecordEndedEvent += RecordEndedEventHandler;

        if (PhotonNetwork.IsMasterClient) leftUnitGenerator = GameObject.Find("UnitGenerator_Left(Clone)").GetComponent<UnitGenerator>();
        else rightUnitGenerator = GameObject.Find("UnitGenerator_Right(Clone)").GetComponent<UnitGenerator>();

        //GetRandomWords();
    }

    private void OnDestroy()
    {
        CustomMicrophone.RecordStreamDataEvent -= RecordStreamDataEventHandler;
        CustomMicrophone.PermissionStateChangedEvent -= PermissionStateChangedEventHandler;
        CustomMicrophone.RecordStartedEvent -= RecordStartedEventHandler;
        CustomMicrophone.RecordEndedEvent -= RecordEndedEventHandler;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GetIsStart())
        {
            if (Input.GetKey(KeyCode.Space) && pressed == false)
            {
                recording = true;
                micOn.gameObject.SetActive(true);
                micOff.gameObject.SetActive(false);
                StartRecord();
                pressed = true;
                recordingStatusText.text = "recording...";
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                if (recording == true)
                {
                    recording = false;
                    micOn.gameObject.SetActive(false);
                    micOff.gameObject.SetActive(true);
                    StopRecord();
                    Debug.Log("1");
                    recordingStatusText.text = "";
                    pressed = false;
                }
            }
        }
    }

    private void StartRecord()
    {
        if (!CustomMicrophone.HasConnectedMicrophoneDevices())
        {
            Debug.Log("No connected devices found. Refreshing...");
            CustomMicrophone.RefreshMicrophoneDevices();
            return;
        }

        _workingClip = CustomMicrophone.Start(selectedDevice, false, recordingTime, frequency);
    }

    private void StopRecord()
    {
        Debug.Log("2");
        Debug.Log(CustomMicrophone.IsRecording(selectedDevice));

        if (!CustomMicrophone.IsRecording(selectedDevice))
            return;

        // End recording is an async operation, so you have to provide callback or subscribe on RecordEndedEvent event
        CustomMicrophone.End(selectedDevice, () =>
        {
            Debug.Log("3");
            if (makeCopy)
            {
                recordedClips.Add(CustomMicrophone.MakeCopy($"copy{recordedClips.Count}", recordingTime, frequency, _workingClip));
                audioSource.clip = recordedClips.Last();
            }
            else
            {
                audioSource.clip = _workingClip;
                Debug.Log("4");
            }
            audioSource.Play();
            SendAudioClip();
            Debug.Log("5");

            randomWordsText.gameObject.SetActive(false);
        });
    }

    public string SaveWavFile()
    {
        string filepath;
        data = WavUtility.FromAudioClip(_workingClip, out filepath, true);
        Debug.Log(filepath[1]);
        Debug.Log(data);
        Debug.Log(_workingClip);
        return filepath;
    }

    private void SendAudioClip()
    {
        StartCoroutine(Send());
    }

    IEnumerator Send()
    {
        byte[] audioByte = File.ReadAllBytes(SaveWavFile());
        Debug.Log(audioByte.Length);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioByte, "sample.wav");
        Debug.Log(randomWords);
        form.AddField("text", randomWords);
        form.AddField("uid", ReactController.userUid);

        /*using (UnityWebRequest www = UnityWebRequest.Post("https://127.0.0.1:8000/api/v1/game/AI", form))*/
        using (UnityWebRequest www = UnityWebRequest.Post("https://k6e204.p.ssafy.io/api/v1/game/AI", form))
        {
            www.SetRequestHeader("Authorization", ReactController.token);
            ResultText.gameObject.SetActive(true);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                ResultText.text = www.error;
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Post Data Complete!");
                string raw = www.downloadHandler.text;
                Debug.Log(raw);
                //string raw = "{'level' : 1,'similarity' : 100}";
                Dictionary<string, object> response = Json.Deserialize(raw) as Dictionary<string, object>;
                Debug.Log(response);
                Debug.Log("사전 카운트 : " + response.Count);
                string level = response["level"].ToString();
                string similarity = response["similarity"].ToString();

                Debug.Log(level);
                Debug.Log(similarity);

                if (similarity == "10")
                {
                    ResultText.text = "Weak...";                    
                }
                else if (similarity == "50")
                {
                    ResultText.text = "Good!";
                }
                else if (similarity == "80")
                {
                    ResultText.text = "Great!";
                }
                else if (similarity == "100")
                {
                    ResultText.text = "Perfect!";
                }
                if (int.TryParse(similarity, out int sresult) 
                    && int.TryParse(level, out int lresult))
                {
                    if (PhotonNetwork.IsMasterClient) leftUnitGenerator.gen(lresult,sresult);
                    else rightUnitGenerator.gen(lresult, sresult);
                }
                StartCoroutine(ChangePanel());
            }
        }
    }

    IEnumerator ChangePanel() //결과창과 단어창을 바꾸면서 1초의 간격을 둠
    {
        yield return new WaitForSeconds(1);

        Debug.Log("1초 지남");
        ResultText.text = "";
        ResultText.gameObject.SetActive(false);

        randomWordsText.gameObject.SetActive(true);
        GetRandomWords();
    }

    public void GetRandomWords()
    {
        //게임 시작 카운트다운 후 게임매니저에서 실행하도록
        //public 으로 바꿈
        StartCoroutine(Get());
    }

    IEnumerator Get()
    {
        /*using (UnityWebRequest www = UnityWebRequest.Get("https://127.0.0.1:8000/api/v1/game/words"))*/
        using (UnityWebRequest www = UnityWebRequest.Get("https://k6e204.p.ssafy.io/api/v1/game/words"))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Get Random Words Complete!");
                Debug.Log(www.downloadHandler.text);

                randomWords = www.downloadHandler.text;
                string fromDjango = www.downloadHandler.text; //[{"word_level":3,"word":"강변북로 강변북로 강변북로"},{"word_level":4,"word":"깐 콩깍지나 안 깐 콩깍지나 다 콩깍지"},{"word_level":5,"word":"베타갈락토시다아제"}]

                int cnt = 0;

                string word_tmp = "";

                for (int i = 0; i < fromDjango.Length; i++)
                {
                    if (fromDjango[i].ToString() == "\"")
                    {
                        cnt += 1;
                    }
                    if (cnt == 2)
                    {
                        if (int.TryParse(fromDjango[i + 2].ToString(), out int result))
                        {
                            word1level = result;
                        }
                        cnt += 1;
                        continue;
                    }

                    if (cnt == 6 && fromDjango[i].ToString() != "\"")
                    {
                        word_tmp += fromDjango[i].ToString();
                        continue;
                    }

                    if (cnt == 7)
                    {
                        word1 = word_tmp;
                        word_tmp = "";
                        cnt += 1;
                        continue;
                    }

                    if (cnt == 10)
                    {
                        if (int.TryParse(fromDjango[i + 2].ToString(), out int result))
                        {
                            word2level = result;
                        }
                        cnt += 1;
                        continue;
                    }

                    if (cnt == 14 && fromDjango[i].ToString() != "\"")
                    {
                        word_tmp += fromDjango[i].ToString();
                        continue;
                    }

                    if (cnt == 15)
                    {
                        word2 = word_tmp;
                        word_tmp = "";
                        cnt += 1;
                        continue;
                    }

                    if (cnt == 18)
                    {
                        if (int.TryParse(fromDjango[i + 2].ToString(), out int result))
                        {
                            word3level = result;
                        }
                        cnt += 1;
                        continue;
                    }

                    if (cnt == 22 && fromDjango[i].ToString() != "\"")
                    {
                        word_tmp += fromDjango[i].ToString();
                        continue;
                    }

                    if (cnt == 23)
                    {
                        word3 = word_tmp;
                        word_tmp = "";
                        cnt += 1;
                        continue;
                    }
                }
                randomWordsText.gameObject.SetActive(true);
                word1LevelText.text = word1level.ToString();
                word1Text.text = word1;
                word2LevelText.text = word2level.ToString();
                word2Text.text = word2;
                word3LevelText.text = word3level.ToString();
                word3Text.text = word3;
            }
        }
    }

    private void RecordStreamDataEventHandler(float[] samples)
    {
        // handle streaming recording data
    }

    private void PermissionStateChangedEventHandler(bool permissionGranted)
    {
        // handle current permission status

        Debug.Log($"Permission state changed on: {permissionGranted}");
    }

    private void RecordEndedEventHandler()
    {
        // handle record ended event

        Debug.Log("Record ended");
    }

    private void RecordStartedEventHandler()
    {
        // handle record started event

        Debug.Log("Record started");
    }
}
