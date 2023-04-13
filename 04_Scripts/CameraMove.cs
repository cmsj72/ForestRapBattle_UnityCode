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
        //  ���� �÷��̾ ���������� ���ο� ���� ī�޶� ���� ��ġ�� �ٲ�
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
        float h = Input.GetAxis("Horizontal");// a,d Ȥ�� ��,�� �� �Է������� ���� �޾ƿ�        
        Vector3 dir = new Vector3(h, 0, 0);// ���� 3���� ���ͷ� ����
        dir = dir.normalized;// ������ ����ȭ
        //  ī�޶��� �̵� ������ ��ġ�� -10 <= x <= 20 ���� ����
        if((transform.position + dir * moveSpeed * Time.deltaTime).x >= -10 &&
            (transform.position + dir * moveSpeed * Time.deltaTime).x <= 20)
        {
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        
    }
}
