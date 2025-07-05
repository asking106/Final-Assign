//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;
using System.Collections;

using Random = UnityEngine.Random;
using Unity.VisualScripting;
using Photon.Realtime;



public class Projectile : MonoBehaviour
    {   

        [Range(5, 100)]
        [Tooltip("After how long time should the bullet prefab be destroyed?")]
        public float destroyAfter;

        [Tooltip("If enabled the bullet destroys on impact")]
        public bool destroyOnImpact = false;

        [Tooltip("Minimum time after impact that the bullet is destroyed")]
        public float minDestroyTime;

        [Tooltip("Maximum time after impact that the bullet is destroyed")]
        public float maxDestroyTime;

        [Header("Impact Effect Prefabs")]
        public Transform[] bloodImpactPrefabs;
    public GameObject myplayers;
        public Transform[] metalImpactPrefabs;
        public Transform[] dirtImpactPrefabs;
        public Transform[] concreteImpactPrefabs;
    private Vector3 bulletDir;
    public float damage;

        private void Start()
        {


        bulletDir = GetComponent<Rigidbody>().velocity;
        StartCoroutine(DestroyAfter());
        }

    //If the bullet collides with anything
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.GetComponent<Projectile>() != null)
            return;



        if (!destroyOnImpact)
        {
            StartCoroutine(DestroyTimer());
        }
        else
        {
            Destroy(gameObject);
        }

        //If bullet collides with "Blood" tag
        if (collision.transform.tag == "Blood")
        {
            //Instantiate random impact prefab from array
            Transform bloodImpact=Instantiate(bloodImpactPrefabs[Random.Range
                    (0, bloodImpactPrefabs.Length)], transform.position,
                Quaternion.LookRotation(collision.contacts[0].normal));
            bloodImpact.SetParent(collision.transform);
            if (collision.transform.GetComponent<EnemyControls>() != null)
            {
                
                collision.transform.GetComponent<EnemyControls>().Health(damage,bulletDir, myplayers);
            }
            //Destroy bullet object
            Destroy(gameObject);
        }

        //If bullet collides with "Metal" tag
        if (collision.transform.tag == "Metal")
        {
            //Instantiate random impact prefab from array
            Instantiate(metalImpactPrefabs[Random.Range
                    (0, bloodImpactPrefabs.Length)], transform.position,
                Quaternion.LookRotation(collision.contacts[0].normal));
            //Destroy bullet object
            Destroy(gameObject);
        }

        //If bullet collides with "Dirt" tag
        if (collision.transform.tag == "Dirt")
        {
            //Instantiate random impact prefab from array
            Instantiate(dirtImpactPrefabs[Random.Range
                    (0, bloodImpactPrefabs.Length)], transform.position,
                Quaternion.LookRotation(collision.contacts[0].normal));
            //Destroy bullet object
            Destroy(gameObject);
        }

        //If bullet collides with "Concrete" tag
        if (collision.transform.tag == "Concrete")
        {
            //Instantiate random impact prefab from array
            Instantiate(concreteImpactPrefabs[Random.Range
                    (0, bloodImpactPrefabs.Length)], transform.position,
                Quaternion.LookRotation(bulletDir.normalized));
            //Destroy bullet object
            Destroy(gameObject);
        }



        if (collision.transform.tag == "Target")
        {
            //toggle "ishit" on target object
           
            collision.transform.gameObject.GetComponent<TargetScript>().isHit = true;
            //destroy bullet object
            Destroy(gameObject);
        }
    }

//If bullet collides with "ExplosiveBarrel" tag
//if (collision.transform.tag == "ExplosiveBarrel")
//    {
//        //Toggle "explode" on explosive barrel object
//        collision.transform.gameObject.GetComponent
//            <ExplosiveBarrelScript>().explode = true;
//        //Destroy bullet object
//        Destroy(gameObject);
//    }

//    //If bullet collides with "GasTank" tag
//    if (collision.transform.tag == "GasTank")
//    {
//        //Toggle "isHit" on gas tank object
//        collision.transform.gameObject.GetComponent
//            <GasTankScript>().isHit = true;
//        //Destroy bullet object
//        Destroy(gameObject);
//    }
//}

private IEnumerator DestroyTimer()
        {
            //Wait random time based on min and max values
            yield return new WaitForSeconds
                (Random.Range(minDestroyTime, maxDestroyTime));
            //Destroy bullet object
            Destroy(gameObject);
        }

        private IEnumerator DestroyAfter()
        {
            //Wait for set amount of time
            yield return new WaitForSeconds(destroyAfter);
            //Destroy bullet object
            Destroy(gameObject);
        }
    }
 