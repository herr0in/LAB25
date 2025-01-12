﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance
    {
        get
        {
            if (instance != null)
                return instance;
            instance = FindObjectOfType<WeaponManager>();
            return instance;
        }
    }

    private static WeaponManager instance;

    public RuntimeAnimatorController[] weaponCtrls;
    public Animator anim;

    public int weaponNum;
    public GameObject playerObj;
    private int curWeaponNum = 0;
    public int stage = 0;

    private readonly string akIdleStr = "Idle(AK)";
    private readonly string sciIdleStr = "Idle(Scifi)";

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2) && stage >= 4)
        {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
 
            if (anim.runtimeAnimatorController == weaponCtrls[2] && info.IsName(akIdleStr))
                anim.runtimeAnimatorController = weaponCtrls[3];
            else if(anim.runtimeAnimatorController == weaponCtrls[3] && info.IsName(sciIdleStr))
                anim.runtimeAnimatorController = weaponCtrls[2];
        }
        else if (Input.GetMouseButtonUp(2))
            UIManager.Instance.TextUpdate();
    }

    public void ChangeStartAxe()
    {
        playerObj.SetActive(true);
        anim.runtimeAnimatorController = weaponCtrls[4];
    }

    public void ChangeStartAK()
    {
        playerObj.SetActive(true);
        anim.runtimeAnimatorController = weaponCtrls[0];
    }

    public void ChangeStartScifi()
    {
        if( anim.runtimeAnimatorController.Equals(weaponCtrls[0]))
            anim.runtimeAnimatorController = weaponCtrls[1];
        else if (anim.runtimeAnimatorController.Equals(weaponCtrls[2]))
            anim.runtimeAnimatorController = weaponCtrls[3];
    }

    public void ChangeStartIron()
    {
        if (anim.runtimeAnimatorController.Equals(weaponCtrls[0]))
            anim.runtimeAnimatorController = weaponCtrls[2];
        else if (anim.runtimeAnimatorController.Equals(weaponCtrls[1]))
            anim.runtimeAnimatorController = weaponCtrls[6];
    }
}
