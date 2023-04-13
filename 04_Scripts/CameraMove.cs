using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraMove : MonoBehaviour
{
    private float moveSpeed = 10.0f;
    
    void Start()
    {
        //  로컬 플레이어가 방장인지의 여부에 따라 카메라 시작 위치를 바꿈
        if (PhotonNetwork.IsMasterClient)
        {
            transform.position = new Vector3(-10, 10, -10);
        }
        else
        {
            transform.position = new Vector3(20, 10, -10);
        }
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");// a,d 혹은 ←,→ 를 입력했을때 값을 받아옴        
        Vector3 dir = new Vector3(h, 0, 0);// 값을 3차원 벡터로 생성
        dir = dir.normalized;// 벡터의 정규화
        //  카메라의 이동 가능한 위치를 -10 <= x <= 20 으로 제한
        if((transform.position + dir * moveSpeed * Time.deltaTime).x >= -10 &&
            (transform.position + dir * moveSpeed * Time.deltaTime).x <= 20)
        {
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        
    }
}
