using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class PatrolState : EnemyBaseState
{
    private float waitTime = 2f;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    public override void EnemyState(EnemyControls enemy)
    {
        enemy.animState = 0;
        enemy.loadPath(enemy.wayPointObj[0]);
    }

    public override void onupdate(EnemyControls enemy)
    {
        if (enemy.attackList.Count > 0)
        {
            enemy.TransitonToState(enemy.attackState);
        }
        if (isWaiting)
        {
            enemy.animState = 0;
            waitTimer -= Time.deltaTime;
            enemy.agent.SetDestination(enemy.transform.position);
            if (waitTimer < 0)
            {
                isWaiting = false;
                waitTimer = 0f;

            }
        }
        else
        {


            enemy.animState = 1;
            enemy.MoveToTarget();
            float distance = Vector3.Distance(enemy.transform.position, enemy.wayPoint[enemy.index]);
            if (distance <= 0.5f)
            {
                isWaiting = true;
                waitTimer = waitTime;
                enemy.index++;

                if (enemy.index == enemy.wayPoint.Count)
                {
                    enemy.index = 0;
                }
            }
        }
         
    }
}
