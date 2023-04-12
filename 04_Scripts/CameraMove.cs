using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraMove : MonoBehaviour
{
    private float moveSpeed = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            transform.position = new Vector3(-10, 10, -10);
        }
        else
        {
            transform.position = new Vector3(20, 10, -10);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");// a,d Ȥ�� ��,�� �� �Է������� ���� �޾ƿ�        
        Vector3 dir = new Vector3(h, 0, 0);// ���� 3���� ���ͷ� ����
        dir = dir.normalized;// ������ ����ȭ
        if((transform.position + dir * moveSpeed * Time.deltaTime).x >= -10 &&
            (transform.position + dir * moveSpeed * Time.deltaTime).x <= 20)
        {
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        
    }
}
