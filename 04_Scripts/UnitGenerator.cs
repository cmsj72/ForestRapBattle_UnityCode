using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class UnitGenerator : MonoBehaviour
{
    //안쓰는거라 지워도 될듯
    /*[Header("Castle Position References")]
    [SerializeField]
    private Transform LeftCastlePoint;
    [SerializeField]
    private Transform RightCastlePoint;
    [Space(5f)]*/

    [Header("Gen Position References")]
    [SerializeField]
    private Transform LeftGenPosition;
    [SerializeField]
    private Transform RightGenPosition;
    [Space(5f)]

    private PhotonView PV;
    private bool isMaster = false;
    private string[] LV1_UnitList = new string[] { "Chick", "Duck", "DuckB", "Rabbit", "RabbitB"};
    private string[] LV2_UnitList = new string[] { "Cat", "CatB", "Dog", "DogB"};
    private string[] LV3_UnitList = new string[] { "GoatA", "GoatB" , "GoatC", "Pig"};
    private string[] LV4_UnitList = new string[] { "Alpaca", "AlpacaB", "Sheep", "SheepB", "SheepC"};
    private string[] LV5_UnitList = new string[] { "Cow", "Horse", "HorseB"};
    /*private Transform UnitGenPositon;*/

    // Start is called before the first frame update
    void Start()
    {
        //안쓰는거라 지워도 될듯
        /*LeftCastlePoint = GameObject.Find("Left_Castle").transform.GetChild(3);
        RightCastlePoint = GameObject.Find("Right_Castle").transform.GetChild(3);*/
        PV = GetComponent<PhotonView>();
        /*UnitGenPositon = PhotonNetwork.IsMasterClient ? LeftGenPosition : RightGenPosition;*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void gen(int lvl, int similarity)
    {
        if (PV.IsMine)
        {
            string prefabName = "";
            int randomNum;
            switch (lvl)
            {
                case 1:
                    randomNum = Random.Range(0, LV1_UnitList.Length);
                    prefabName = LV1_UnitList[randomNum];
                    break;
                case 2:
                    randomNum = Random.Range(0, LV2_UnitList.Length);
                    prefabName = LV2_UnitList[randomNum];
                    break;
                case 3:
                    randomNum = Random.Range(0, LV3_UnitList.Length);
                    prefabName = LV3_UnitList[randomNum];
                    break;
                case 4:
                    randomNum = Random.Range(0, LV4_UnitList.Length);
                    prefabName = LV4_UnitList[randomNum];
                    break;
                case 5:
                    randomNum = Random.Range(0, LV5_UnitList.Length);
                    prefabName = LV5_UnitList[randomNum];
                    break;
            }
            if (GameManager.Instance.GetIsStart())
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("방장이 유닛을 생성");
                    /*PhotonNetwork.Instantiate(LV5_UnitList[0], transform.position, Quaternion.identity, 0).GetComponent<Unit>().SetAbility(1, 80);*/
                    PhotonNetwork.Instantiate(prefabName, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity, 0).GetComponent<Unit>().SetAbility(lvl, similarity);
                }
                else
                {
                    Debug.Log("방원이 유닛을 생성");
                    PhotonNetwork.Instantiate(prefabName, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity, 0).GetComponent<Unit>().SetAbility(lvl, similarity);
                }
            }
        }  
    }

    public void SetIsMaster(bool flag)
    {
        isMaster = flag;
    }
}
