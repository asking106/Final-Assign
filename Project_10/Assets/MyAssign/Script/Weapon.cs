using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon :  MonoBehaviourPun
{
    public abstract void GunFire();
    public abstract void Reload();
    public abstract void ExpandingCrossUpdate(float expanDegree);

    public abstract void DoReloadAnimation();
    public abstract void AimIn();
    public abstract void AimOut();

}
