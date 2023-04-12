using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using FrostweepGames.Plugins.Native;
using TMPro;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Runtime.InteropServices;


public class MicTest : MonoBehaviour
{
    public static MicTest Instance;

    private AudioClip _workingClip;

    public TMP_Text sendToDjangoStatusText;

    public TMP_Text randomWordsText;

    public TMP_Dropdown devicesDropdown;

    public AudioSource audioSource;

    public Button startRecordButton,
                stopRecordButton,
                playRecordedAudioButton,
                requestPermissionButton,
                refreshDevicesButton,
                goToLobbyButton;

    public List<AudioClip> recordedClips;

    public int frequency = 44100;

    public int recordingTime = 60;

    public static string selectedDevice;

    public bool makeCopy = false;

    public float averageVoiceLevel = 0f;

    public double voiceDetectionTreshold = 0.02d;

    public bool voiceDetectionEnabled = false;

    byte[] data = null;


    private void Awake()
    {
        Loading();
    }

    [DllImport("__Internal")]
    private static extern void Loading();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        startRecordButton.onClick.AddListener(StartRecord);
        stopRecordButton.onClick.AddListener(StopRecord);
        playRecordedAudioButton.onClick.AddListener(PlayRecordedAudio);
        requestPermissionButton.onClick.AddListener(RequestPermission);
        refreshDevicesButton.onClick.AddListener(RefreshMicrophoneDevicesButtonOnclickHandler);
        goToLobbyButton.onClick.AddListener(GoToLobby);

        devicesDropdown.onValueChanged.AddListener(DevicesDropdownValueChangedHandler);

        selectedDevice = string.Empty;
        CustomMicrophone.RefreshMicrophoneDevices();

        CustomMicrophone.RecordStreamDataEvent += RecordStreamDataEventHandler;
        CustomMicrophone.PermissionStateChangedEvent += PermissionStateChangedEventHandler;
        CustomMicrophone.RecordStartedEvent += RecordStartedEventHandler;
        CustomMicrophone.RecordEndedEvent += RecordEndedEventHandler;
    }

    private void OnDestroy()
    {
        CustomMicrophone.RecordStreamDataEvent -= RecordStreamDataEventHandler;
        CustomMicrophone.PermissionStateChangedEvent -= PermissionStateChangedEventHandler;
        CustomMicrophone.RecordStartedEvent -= RecordStartedEventHandler;
        CustomMicrophone.RecordEndedEvent -= RecordEndedEventHandler;
    }

    private void Update()
    {
        //permissionStatusText.text = $"Microphone permission: {selectedDevice} for '{ (CustomMicrophone.HasMicrophonePermission() ? "<color=green>권한 활성화</color>" : "<color=red>권한 없음</color>") }'";

        if (CustomMicrophone.HasConnectedMicrophoneDevices())
        {
            bool recording = CustomMicrophone.IsRecording(selectedDevice);

            if (recording)
            {
                startRecordButton.GetComponentInChildren<TMP_Text>().color = Color.black;
                //startRecordButton.GetComponent<TMP_Text>().color = new Color(241f/255f, 180f/255f, 87f/255f);
            }
            else
            {
                startRecordButton.GetComponentInChildren<TMP_Text>().color = Color.white;
                //startRecordButton.GetComponent<TMP_Text>().color = new Color(241f/255f, 180f/255f, 87f/255f);
            }
        }
    }

    /// <summary>
    /// Works only in WebGL
    /// </summary>
    /// <param name="samples"></param>
    private void RecordStreamDataEventHandler(float[] samples)
    {
        // handle streaming recording data
    }

    /// <summary>
    /// Works only in WebGL
    /// </summary>
    /// <param name="permissionGranted"></param>
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

    private void RefreshMicrophoneDevicesButtonOnclickHandler()
    {
        CustomMicrophone.RefreshMicrophoneDevices();

        if (!CustomMicrophone.HasConnectedMicrophoneDevices())
            return;

        devicesDropdown.ClearOptions();
        devicesDropdown.AddOptions(CustomMicrophone.devices.ToList());
        DevicesDropdownValueChangedHandler(0);
    }

    private void RequestPermission()
    {
        CustomMicrophone.RequestMicrophonePermission();
        CustomMicrophone.RefreshMicrophoneDevices();

        if (!CustomMicrophone.HasConnectedMicrophoneDevices())
            return;

        devicesDropdown.ClearOptions();
        devicesDropdown.AddOptions(CustomMicrophone.devices.ToList());
        DevicesDropdownValueChangedHandler(0);
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
        Debug.Log(recordedClips.Count);

        if (!CustomMicrophone.IsRecording(selectedDevice))
            return;

        // End recording is an async operation, so you have to provide callback or subscribe on RecordEndedEvent event
        CustomMicrophone.End(selectedDevice, () =>
        {
            if (makeCopy)
            {
                recordedClips.Add(CustomMicrophone.MakeCopy($"copy{recordedClips.Count}", recordingTime, frequency, _workingClip));
                audioSource.clip = recordedClips.Last();
            }
            else
            {
                audioSource.clip = _workingClip;
            }

            Debug.Log(SaveWavFile());
            //Debug.Log(data);
            //audioSource.Play();
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
        Debug.Log(randomWordsText.text);
        form.AddField("text", randomWordsText.text);
        form.AddField("uid", ReactController.userUid);

        using (UnityWebRequest www = UnityWebRequest.Post("https://k6e204.p.ssafy.io/api/v1/game/AI", form))
        {
            www.SetRequestHeader("headers", ReactController.token);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                sendToDjangoStatusText.text = www.error;
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Post Data Complete!");
                sendToDjangoStatusText.text = "점수: " + www.downloadHandler.text;
            }
        }
    }

    private void GetRandomWords()
    {
        StartCoroutine(Get());
    }

    IEnumerator Get()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://k6e204.p.ssafy.io/api/v1/game/words"))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                randomWordsText.text = www.error;
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Get Random Words Complete!");
                Debug.Log(www.downloadHandler.text);

                
                randomWordsText.text = www.downloadHandler.text;
            }
        }
    }

    private void PlayRecordedAudio()
    {
        if (_workingClip == null)
            return;

        audioSource.clip = _workingClip;
        audioSource.Play();
    }

    private void DevicesDropdownValueChangedHandler(int index)
    {
        if (index < CustomMicrophone.devices.Length)
        {
            selectedDevice = CustomMicrophone.devices[index];
        }
    }

    private void GoToLobby()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }

    private void GoToGame()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }
}
