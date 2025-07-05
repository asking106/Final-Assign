using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControls : MonoBehaviour
{
    private PhotonView photonview;
    public bool CanPartrol = true;
    public NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    public GameObject[] wayPointObj;
    public List<Vector3> wayPoint = new List<Vector3>();
    public int index;
    public int animState; // 0 idle,1 run,2 attack;
    public EnemyBaseState currentState;
    public float currentHealth;
    public float maxHealth;
    private bool isdead;
    public float SpeedUpWaitTime;
    public float SpeedUpTimes;
    public bool isSpeedTime;
    private bool Isattack;
    public bool IsChecking;
    public bool IsStartTime;
    private bool attackstart;
    private Dictionary<Transform, Coroutine> exitCoroutines = new Dictionary<Transform, Coroutine>();

    public PatrolState patrolState;
    public AttackState attackState;

    public List<Transform> attackList = new List<Transform>();
    private float nextAttack;
    public float attackRate;
    public float AttackRange;
    private bool isDead;
    public Transform targetPoint;
    public bool IsOnline;

    //潜行系统参数
    public float baseViewAngle = 120f;
    public float crouchViewAngle = 45f;
    public float baseViewDistance = 10f;
    public float crouchViewDistance = 3f;
    public float minDetectionDistance = 2f;

    void Start()
    {
        attackstart = false;
        photonview = GetComponent<PhotonView>();
        IsStartTime = false;
        IsChecking = false;
        Isattack = false;
        isSpeedTime = false;
        patrolState = transform.gameObject.AddComponent<PatrolState>();
        attackState = transform.gameObject.AddComponent<AttackState>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        index = 0;

        if (PhotonNetwork.IsMasterClient && CanPartrol)
        {
            TransitonToState(patrolState);
        }
    }

    void Update()
    {
        if (isdead)
        {
            animator.applyRootMotion = true;
            agent.SetDestination(transform.position);
            GetComponent<CapsuleCollider>().enabled = false;
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!CanPartrol && attackList.Count > 0 && !attackstart)
        {
            TransitonToState(attackState);
            attackstart = true;
        }

        // 清理死亡玩家
        for (int i = attackList.Count - 1; i >= 0; i--)
        {
            Myplayer player = attackList[i].GetComponent<Myplayer>();
            if (player != null && player.isDead)
            {
                attackList.RemoveAt(i);
            }
        }

        animator.SetBool("Run", isSpeedTime);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Scream"))
            animator.applyRootMotion = true;

        animator.SetBool("Checking", IsChecking);
        animator.SetBool("Scream", IsStartTime);

        if (isSpeedTime && !Isattack)
            agent.speed = 2f;
        else if (!Isattack)
            agent.speed = 1.1f;

        if (currentState != null)
            currentState.onupdate(this);

        animator.SetInteger("State", animState);

        // 主动搜索附近玩家
        Collider[] players = Physics.OverlapSphere(transform.position, baseViewDistance);
        foreach (var col in players)
        {
            if (col.CompareTag("Player") && !attackList.Contains(col.transform))
            {
                Myplayer p = col.GetComponent<Myplayer>();
                if (p != null && !p.isDead && CanSeePlayer(p.transform))
                {
                    attackList.Add(p.transform);
                }
            }
        }


    }


    public void MoveToTarget()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1") || animator.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            Isattack = true;
            animator.applyRootMotion = true;
            agent.speed = 0f;
        }
        else
        {
            Isattack = false;
            animator.applyRootMotion = false;
        }

        if (attackList.Count == 0)
            agent.destination = wayPoint[index];
        else
            agent.destination = targetPoint.position;
    }

    public void loadPath(GameObject game)
    {
        wayPoint.Clear();
        foreach (Transform t in game.transform)
        {
            wayPoint.Add(t.position);
        }
    }

    public void TransitonToState(EnemyBaseState state)
    {
        currentState = state;
        currentState.EnemyState(this);
    }

    public void Health(float damage, Vector3 bulletDirection, GameObject myplayers)
    {
        if (photonview != null && PhotonNetwork.IsMasterClient)
        {
            if (!attackList.Contains(myplayers.transform))
            {
                attackList.Add(myplayers.transform);
            }
            int viewID = myplayers.GetComponent<PhotonView>().ViewID;
            photonview.RPC("RPCHealth", RpcTarget.AllBuffered, damage, bulletDirection, viewID);
        }
    }

    [PunRPC]
    public void RPCHealth(float damage, Vector3 bulletDirection, int viewID)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            PhotonView.Find(viewID).gameObject.GetComponent<Myplayer>().scores += 10;
            isdead = true;
            animator.SetTrigger("Death");

            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child.CompareTag("Impact"))
                    Destroy(child.gameObject);
            }

            Destroy(gameObject, 60f);

            if (agent != null)
            {
                agent.isStopped = true;
                agent.updatePosition = false;
                agent.updateRotation = false;
            }

            Vector3 lookDir = -bulletDirection;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    public void attackSounds(AudioClip attackSound)
    {
        audioSource.PlayOneShot(attackSound);
    }

    public void AttackAction()
    {
        if (Vector3.Distance(transform.position, targetPoint.position) < AttackRange)
        {
            if (Time.time > nextAttack)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    int s = Random.Range(1, 3);
                    photonview.RPC("attacks", RpcTarget.AllBuffered, s);
                }
                nextAttack = Time.time + attackRate;
            }
        }
    }

    [PunRPC]
    public void attacks(int s)
    {
        animator.SetTrigger("Attack" + s);
    }

    public void OnEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient && other.CompareTag("Player") && !isdead)
        {
            if (!attackList.Contains(other.transform) && CanSeePlayer(other.transform))
            {
                attackList.Add(other.transform);

                if (exitCoroutines.ContainsKey(other.transform))
                {
                    StopCoroutine(exitCoroutines[other.transform]);
                    exitCoroutines.Remove(other.transform);
                }
            }
        }
    }

    public void OnExit(Collider other)
    {
        if (PhotonNetwork.IsMasterClient && CanPartrol)
        {
            if (attackList.Contains(other.transform))
            {
                Coroutine co = StartCoroutine(RemoveAfterDelay(other.transform));
                exitCoroutines[other.transform] = co;
            }
        }
    }

    private IEnumerator RemoveAfterDelay(Transform target)
    {
        yield return new WaitForSeconds(5f);

        if (attackList.Contains(target))
        {
            attackList.Remove(target);
            Debug.Log("玩家离开超时，敌人停止追踪");
        }

        exitCoroutines.Remove(target);
    }

    //  敌人视线感知函数
    public bool CanSeePlayer(Transform player)
    {
        Vector3 dirToPlayer = player.position - transform.position;
        float distance = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer.normalized);

        bool isCrouching = player.GetComponent<Myplayer>().isCrouching;

        float currentFOV = isCrouching ? crouchViewAngle : baseViewAngle;
        float currentDistance = isCrouching ? crouchViewDistance : baseViewDistance;

        if (distance <= minDetectionDistance)
            return true;

        if (angle <= currentFOV / 2 && distance <= currentDistance)
            return true;

        return false;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, baseViewDistance);
    }


}
