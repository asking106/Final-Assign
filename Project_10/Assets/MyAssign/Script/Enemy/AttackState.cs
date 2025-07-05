using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AttackState : EnemyBaseState
{
    private float SpeedUpTimer;
    private float detectTimer=5f;
    private float StartTimer;
     
    public override void EnemyState(EnemyControls enemy)
    {
        enemy.animState = 2;
        enemy.targetPoint=enemy.attackList[0];
        enemy.IsStartTime = true;
        StartTimer = 2f;
    }
    IEnumerator ContinueTime(EnemyControls enemy)
    {
        yield return new WaitForSeconds(enemy.SpeedUpTimes);
        SpeedUpTimer = 0;
        enemy.isSpeedTime = false;
        

    }
    public override void onupdate(EnemyControls enemy)
    {   
        if(enemy.IsStartTime==true)
        {
            detectTimer = 5f;
            StartTimer -=Time.deltaTime;
            enemy.agent.SetDestination(enemy.transform.position);
            if (StartTimer < 0)
            {
                enemy.IsStartTime=false;
            }
            return;
        }
        if (enemy.attackList.Count == 0)
        {   if(!enemy.CanPartrol)
            {
                enemy.animState = 0;
                enemy.agent.SetDestination(enemy.transform.position);
                return;
            }
            enemy.animState = 0;
            enemy.IsChecking = true;
            enemy.agent.SetDestination(enemy.transform.position);
            detectTimer -= Time.deltaTime;
            if(detectTimer < 0)
            {   enemy.IsChecking=false;
                enemy.currentState = enemy.patrolState;
            }
            return;
        }
        enemy.IsChecking = false;
        detectTimer = 5f;
            SpeedUpTimer += Time.deltaTime;
        if(SpeedUpTimer > enemy.SpeedUpWaitTime)
        {
            SpeedUpTimer = 0;
            
            enemy.isSpeedTime = true;
            StartCoroutine(ContinueTime(enemy));
        }
             
        if(enemy.attackList.Count > 1)
        {
            for(int i=0;i<enemy.attackList.Count;i++)
            {
                if(Vector3.Distance(enemy.transform.position, enemy.attackList[i].position)< Vector3.Distance(enemy.transform.position,enemy.targetPoint.position))
                {
                    enemy.targetPoint = enemy.attackList[i];
                 }

            }
        }
        else if(enemy.attackList.Count ==1)
        {
            enemy.targetPoint = enemy.attackList[0];
        }
        enemy.animState = 2;
        enemy.MoveToTarget();
        if (enemy.targetPoint.gameObject.tag=="Player")
        {
            enemy.AttackAction();
        }
    }
}
