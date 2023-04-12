using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Castle : MonoBehaviour, IPunObservable, IPlayerInterface
{
    private int health;
    private GameObject castle100p;
    private GameObject castle70p;
    private GameObject castle30p;
    private bool destroyFlag;
    // Start is called before the first frame update
    void Start()
    {
        health = 100;
        castle30p = transform.GetChild(0).gameObject;
        castle70p = transform.GetChild(1).gameObject;
        castle100p = transform.GetChild(2).gameObject;
        destroyFlag = true;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(health);
        else this.health = (int)stream.ReceiveNext();
    }

    // Update is called once per frame
    void Update()
    {
        if (destroyFlag)
        {
            DestroyCastle();
        }
    }

    void DestroyCastle()
    {
        if (health <= 70 && castle100p != null)
        {
            Destroy(castle100p);
        }
        else if (health <= 30 && castle70p != null)
        {
            Destroy(castle70p);
            destroyFlag = false;
        }
    }

    public bool GetDamage(int damage)
    {
        health -= damage;
        return health <= 0 ? true : false;
    }

    public int GetHealth()
    {
        return health;
    }

}
