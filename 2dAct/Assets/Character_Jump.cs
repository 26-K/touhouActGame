using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Jump : MonoBehaviour
{
    Player owner;
    public void Init(Player owner)
    {
        this.owner = owner;
    }
    public void DoJump()
    {
        owner.moveData.isGravity = true;
        owner.moveData.grav = owner.moveData.jumpPow;
        owner.moveData.spd.y = owner.moveData.grav;
        owner.moveData.isGround = false;
        owner.moveData.jumpTime = 0.2f;
    }

    /// <summary>
    /// 壁ジャン、通常ジャンプに加えて進行方向の反対向きに力が加わる
    /// </summary>
    public void DoWallJump()
    {
        DoWallJump_AddGrav();
        if (owner.moveData.isLeft)
        {
            owner.moveData.isLeft = !owner.moveData.isLeft;
            owner.moveData.spd.x = owner.GetMaxMoveSpeed(); //てきとー
        }
        else
        {
            owner.moveData.isLeft = !owner.moveData.isLeft;
            owner.moveData.spd.x = -owner.GetMaxMoveSpeed(); //てきとー
        }

    }

    void DoWallJump_AddGrav()
    {
        owner.moveData.isGravity = true;
        owner.moveData.grav = owner.moveData.jumpPow;
        owner.moveData.spd.y = owner.moveData.grav;
        owner.moveData.isGround = false;
        owner.moveData.walljumpCoolTime = 0.3f;
        if (owner.moveData.isLeft)
        {
            owner.ChangeScaleAnim("LeftWallJumpStart");
        }
        else if (owner.moveData.isLeft == false)
        {
            owner.ChangeScaleAnim("RightWallJumpStart");
        }
    }
}
