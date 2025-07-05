using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watchscript : MonoBehaviour
{   public EnemyControls enemyControls;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private void OnTriggerEnter(Collider other)
    {
        enemyControls.OnEnter(other);
    }
    private void OnTriggerExit(Collider other)
    {
        enemyControls.OnExit(other);
    }
}
