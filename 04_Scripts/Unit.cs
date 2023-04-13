using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using InsaneSystems.HealthbarsKit.UI;

public class Unit : MonoBehaviour, IGetter, IAction, IPunObservable
{
    [SerializeField]
    private GameObject attackEffect;
    [SerializeField]
    private Transform effectTransform;
    [SerializeField]
    private AudioClip attackSound;

    delegate bool Damage(int damage);
    private STATE uState;
    private int[] HealthList = new int[] { 100, 150, 200, 250, 300 };
    private int[] PowerList = new int[] { 15, 20, 25, 30, 35 };
    private int maxHealth;
    private int health;
    private int power;
    private int index;
    private int similarity;
    private bool isCastle;
    private float attackTime;
    private float moveSpeed;
    private Animator anim;
    private AudioSource audioSource;
    private Damage targetDamage;
    private GameObject attackTarget;
    private Transform targetTransform;
    private Transform castleTargetTransform;
    private Transform LeftCastleTransform;
    private Transform RightCastleTransform;
    private PhotonView PV;
    private string[] TagList = new string[] { "tag1", "tag2", "Background" };

    public event HealthbarsController.HealthChangedAction healthChangedEvent;
    private void Awake()
    {        
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = attackSound;
        LeftCastleTransform = GameObject.Find("Left_Castle").transform.GetChild(3);
        RightCastleTransform = GameObject.Find("Right_Castle").transform.GetChild(3);
        PV = GetComponent<PhotonView>();
        uState = STATE.WALK;
        index = 0;
        power = PowerList[index];
        maxHealth = HealthList[index];
        health = maxHealth;
        attackTime = 1.0f;
        moveSpeed = 3.0f;
        isCastle = false;
    }

    void Start()
    {
        //  ���� �÷��̾�Լ���
        //  ����, ����� ���ο� ���� ������ ������(�±�) �׸��� �������� ����
        if (PV.IsMine)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                castleTargetTransform = RightCastleTransform;
                this.tag = TagList[0];
            }
            else
            {
                castleTargetTransform = LeftCastleTransform;
                this.tag = TagList[1];
            }
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                castleTargetTransform = LeftCastleTransform;
                this.tag = TagList[1];
            }
            else
            {
                castleTargetTransform = RightCastleTransform;
                this.tag = TagList[0];
            }
        }
        SetDest(castleTargetTransform);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(targetTransform);
        Fsm();
    }

    void Fsm()
    {
        switch (uState)
        {
            case STATE.WALK:
                {
                    Walk();
                }
                break;

            case STATE.ATTACK:
                {
                    Attack();
                }
                break;

            case STATE.DEAD:
                {
                    Dead();
                }
                break;
        }
        if (health <= 0)
        {
            uState = STATE.DEAD;
        }
    }

    //  ������ ������ �� ������ �������� ���� �̵��ϴ� ����
    public void Walk()
    {
        anim.SetInteger("animation", 1);
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, moveSpeed * Time.deltaTime);
    }

    //  ��� ���� Ȥ�� ��� ��(����)�� �����ϴ� ����
    public void Attack()
    {
        attackTime += Time.deltaTime;
        //  1�� �������� ���� ����
        if (attackTime >= 1.0f)
        {
            anim.SetInteger("animation", 6);
            audioSource.Play();
            Instantiate(attackEffect, effectTransform.position, Quaternion.identity);
            if (targetDamage(power))
            {
                targetDamage = null;
                uState = STATE.WALK;
                SetDest(castleTargetTransform);
            }
            attackTime = 0.0f;

            // ��� ���� �����ϴ� ��� �ѹ� ���� �� ������ ���
            if (isCastle)
            {
                uState = STATE.DEAD;
                GetDamage(health);
            }
        }

        // ���� �ִϸ��̼��� ������ Idle�� �����ϸ� Attack - Idle�� �ݺ�
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack")
            && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            anim.SetInteger("animation", 0);
        }
    }

    public void Dead()
    {
        this.tag = TagList[2];        
        anim.SetInteger("animation", 8);
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Die")
            && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && PV.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    //  ������ ������ ��, ������ ���̵��� ���絵 ������ ���� �ɷ�ġ�� ����
    public void SetAbility(int index, int similarity = 10)
    {
        this.index = index;
        this.similarity = similarity;
        // 100, 80 , 50 ,0
        if (this.index < HealthList.Length && this.index < PowerList.Length)
        {
            maxHealth = HealthList[this.index];
            maxHealth = (int)((float)HealthList[this.index] * ((float)similarity * 0.01f));
            health = maxHealth;
            power = (int)((float)PowerList[this.index] * ((float)similarity * 0.01f));
        }
    }

    public void SetDest(Transform dest)
    {        
        targetTransform = dest;
    }

    public int GetHealth()
    {
        return health;
    }

    public bool GetDamage(int damage)
    {
        health = Mathf.Clamp(health - damage, 0, maxHealth);
        //OnHealthChanged();
        return health <= 0? true : false;
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerFunc(other);
    }

    private void OnTriggerStay(Collider other)
    {
        triggerFunc(other);
    }

    //  ������ Ʈ���ſ� ������ �ݸ����� ��� �������� ���������� ���� ����
    private void triggerFunc(Collider other)
    {
        if (targetDamage == null)
        {
            if (other.tag != "Background" && other.tag != this.tag)
            {
                uState = STATE.ATTACK;

                if (other.tag == "castle")
                {
                    targetDamage = new Damage(other.GetComponent<Castle>().GetDamage);
                    attackTarget = other.gameObject;
                    isCastle = true;
                }
                else
                {
                    targetDamage = new Damage(other.GetComponent<Unit>().GetDamage);
                    attackTarget = other.gameObject;
                    SetDest(attackTarget.transform);
                }

            }
        }
    }

    //  ������ �̿��� ������ ���� ���°� �ƴ� ��쿡�� ���� ��ü�� ���� ��ü���� �����͸� ����ȭ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(this.uState != STATE.DEAD)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(this.tag);
                stream.SendNext(this.index);
                stream.SendNext(this.similarity);
            }
            else
            {
                string rtag = (string)stream.ReceiveNext();
                int rindex = (int)stream.ReceiveNext();
                int rsimilarity = (int)stream.ReceiveNext();
                if (this.tag != rtag) this.tag = rtag;
                if (this.index != rindex || this.similarity != rsimilarity)
                {
                    this.index = rindex;
                    this.similarity = rsimilarity;
                    SetAbility(this.index, this.similarity);
                }

            }
        }
    }
}
