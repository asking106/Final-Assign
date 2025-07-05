using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBaseState : MonoBehaviour
{

    public abstract void EnemyState(EnemyControls enemy);
    public abstract void onupdate(EnemyControls enemy);
     
}
