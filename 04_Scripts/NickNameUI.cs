using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

public class NickNameUI : MonoBehaviour, IPunObservable
{
    public TextMeshProUGUI txt;
    public PhotonView PV;
    private void Start()
    {
        Debug.Log("nicknameui--------" + PhotonNetwork.NickName);
        txt.text = PhotonNetwork.NickName;
        Debug.Log("´Ð³×ÀÓ : " + txt.text);
        PV = gameObject.GetComponent<PhotonView>();
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(txt.text);
        else txt.text = (string)stream.ReceiveNext();
    }
    private void Update()
    {
        
    }
}
