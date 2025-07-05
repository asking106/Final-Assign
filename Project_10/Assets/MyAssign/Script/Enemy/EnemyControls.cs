using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControls : MonoBehaviour
{   
    private PhotonView photonview;
    public bool CanPartrol=true;
    public NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    public GameObject[] wayPointObj;
    public List<Vector3> wayPoint=new List<Vector3>();
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



    
    public List<Transform> attackList=new List<Transform>();
    private float nextAttack;
    public float attackRate;
    public float AttackRange;
    private bool isDead;
    public Transform targetPoint;
    public bool IsOnline;



 

    // Start is called before the first frame update
    void Start()
    {
        attackstart = false;
        photonview = GetComponent<PhotonView>();
        IsStartTime = false;
        IsChecking = false;
        Isattack = false;
        isSpeedTime = false;
        patrolState = transform.gameObject.AddComponent<PatrolState>();
        attackState=transform.gameObject.AddComponent<AttackState>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();    
        index = 0;
        if(PhotonNetwork.IsMasterClient)
        {
            if (CanPartrol)
            {
                TransitonToState(patrolState);
            }
        }
         

         
    }

    // Update is called once per frame
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
        {
            return;
        }

        if (!CanPartrol)
        {
            if (attackList.Count > 0 && !attackstart)
            {
                TransitonToState(attackState);
                attackstart = true;
            }
        }
      

        for (int i = attackList.Count - 1; i >= 0; i--)
        {
            Myplayer player = attackList[i].GetComponent<Myplayer>();
            if (player != null && player.isDead)
            {
                attackList.RemoveAt(i);  
            }
        }




        animator.SetBool("Run", isSpeedTime);
         
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("Scream"))
        {
            animator.applyRootMotion = true;
        }


       animator.SetBool("Checking",IsChecking);
        animator.SetBool("Scream",IsStartTime);
        if(isSpeedTime&&!Isattack)
        {
            agent.speed = 2f;
        }
        else if(!Isattack) 
        {
             agent.speed = 1.1f;
        }
     
        if(currentState!=null)
        {
            currentState.onupdate(this);
        }
             
        animator.SetInteger("State", animState);
       
    }
 


    public void MoveToTarget()
    {   if(animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1")||animator.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {   
            
            Isattack = true;
            animator.applyRootMotion = true;
            agent.speed = 0f;
        }
    else
        {
             Isattack=false;
            animator.applyRootMotion = false;
        }

        if (attackList.Count==0)
        {
            //Vector3 theway= Vector3.MoveTowards(transform.position, wayPoint[index],agent.speed*Time.deltaTime);
            agent.destination = wayPoint[index];
        }
        else
        {
            //Vector3 theway = Vector3.MoveTowards(transform.position, targetPoint.position,agent.speed*Time.deltaTime);
            agent.destination = targetPoint.position;
        }
      


    }
    public void loadPath(GameObject game)
    {   wayPoint.Clear();
        foreach(Transform t in game.transform)
        {
            wayPoint.Add(t.position);
        }
    }
    public void TransitonToState(EnemyBaseState state)
    {
        currentState = state;
        currentState.EnemyState(this);
    }
    public void Health(float damage,Vector3 bulletDirection, GameObject myplayers)
    {   
        if(photonview!=null)
        {   if(PhotonNetwork.IsMasterClient)
            {   if(!attackList.Contains(myplayers.transform))
                {
                    attackList.Add(myplayers.transform);
                }
                int viewID = myplayers.GetComponent<PhotonView>().ViewID;

                 photonview.RPC("RPCHealth", RpcTarget.AllBuffered, damage, bulletDirection, viewID);
             
            }
        
        }
        else
        {
           
        }
       


    }
    [PunRPC]
    public void RPCHealth(float damage,Vector3 bulletDirection,int viewID) 
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            PhotonView.Find(viewID).gameObject.GetComponent<Myplayer>().scores += 10;
            isdead = true;
            animator.SetTrigger("Death");
            foreach (Transform child in GetComponentsInChildren<Transform>(true)) // true 表示包括 inactive 物体
            {
                if (child.CompareTag("Impact"))
                {
                    Destroy(child.gameObject);
                }
            }
            Destroy(gameObject, 60f);


            if (agent != null)
            {
                agent.isStopped = true;       // 停止移动
                agent.updatePosition = false; // 不再更新位置
                agent.updateRotation = false; // 不再控制旋转
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
        if(Vector3.Distance(transform.position,targetPoint.position)<AttackRange)
        {
            if(Time.time>nextAttack)
            {
               if(PhotonNetwork.IsMasterClient)
                {
                    int s = Random.Range(1, 3);
                    photonview.RPC("attacks", RpcTarget.AllBuffered,s);
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
    {   if (PhotonNetwork.IsMasterClient)
        {
            

            if (!attackList.Contains(other.transform) && !isdead && other.CompareTag("Player"))
            {
                 
                
              
               
                    

                    attackList.Add(other.transform);

                    // 如果之前退出协程还在，取消它
                    if (exitCoroutines.ContainsKey(other.transform))
                    {
                        StopCoroutine(exitCoroutines[other.transform]);
                        exitCoroutines.Remove(other.transform);
                    }
                }
            
             }
    }

    public void OnExit(Collider other)
    {   if (PhotonNetwork.IsMasterClient)
        {


            if (!CanPartrol)
            {
                return;
            }
            if (attackList.Contains(other.transform))
            {
                Coroutine co = StartCoroutine(RemoveAfterDelay(other.transform));
                exitCoroutines[other.transform] = co;
            }

            // 启动一个延迟移除协程
        }
        
    }

    private IEnumerator RemoveAfterDelay(Transform target)
    {
        yield return new WaitForSeconds(5f);

        // 如果玩家在此期间没重新进入，则移除
        if (attackList.Contains(target))
        {
            attackList.Remove(target);
            Debug.Log("玩家离开超时，敌人停止追踪");
        }

        // 清理记录f
        exitCoroutines.Remove(target);
    }

}
