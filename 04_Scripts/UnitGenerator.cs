using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class UnitGenerator : MonoBehaviour
{
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

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }
    
    public void gen(int lvl, int similarity)
    {
        //  로컬 플레이어의 경우에만
        if (PV.IsMine)
        {
            string prefabName = "";
            int randomNum;
            //  문장의 난이도에 따른 유닛을 랜덤하게 생성
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
                //  PhotonNetwork.IsMasterClient이 true인 경우 방장 소유로, false인 경우 방원 소유로 유닛을 생성한다.
                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("방장이 유닛을 생성");                    
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
