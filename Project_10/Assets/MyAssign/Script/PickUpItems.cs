using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

public class PickUpItems : MonoBehaviour
{
    public float rotateSpeed;
    public string itemName;
    private Transform weaponmodel;
    private GameObject gameObjects;
    private bool IsIn;
    private PhotonView photon;

    void Start()
    {
        photon = GetComponent<PhotonView>();
    }

    void Update()
    {
        // 自转
        transform.eulerAngles += new Vector3(0, rotateSpeed * Time.deltaTime, 0);
        Debug.Log(gameObjects);

        if (IsIn && Input.GetKeyDown(KeyCode.E))
        {
            PhotonView playerView = gameObjects.GetComponent<PhotonView>();
            
            if (playerView.IsMine) // 只有本地玩家可触发拾取
            {
                // 发起 RPC 拾取请求（同步到所有人）
                photon.RPC("PickWeaponRPC", RpcTarget.AllBuffered, playerView.ViewID, itemName);
            }
        }
    }

    [PunRPC]
    public void PickWeaponRPC(int playerViewID, string weaponName)
    {
        GameObject playerObj = PhotonView.Find(playerViewID)?.gameObject;
        if (playerObj == null)
        {
            Debug.LogWarning("无法找到 player 对象 ViewID: " + playerViewID);
            return;
        }

        Myplayer player = playerObj.GetComponent<Myplayer>();
        if (player == null)
        {
            Debug.LogWarning("player 对象上未找到 Myplayer 脚本");
            return;
        }

        weaponmodel = player.inventory.transform.Find(weaponName);
        if (weaponmodel != null)
        {
            player.pickUpWeapon(weaponmodel.gameObject);
        }
        else
        {
            Debug.LogWarning("未找到名为 " + weaponName + " 的子物体");
        }

        // 同步销毁拾取物体
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsIn = true;
            gameObjects = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsIn = false;
        }
    }
}
