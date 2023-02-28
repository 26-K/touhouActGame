using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Animator anim;
    const string LeftAnimName = "Left";
    const string RightAnimName = "Right";
    const string LeftIdleAnimName = "LeftIdle";
    const string RightIdleAnimName = "RightIdle";
    string nowAnimState = RightIdleAnimName;
    Rigidbody2D rgd;
    bool isLeft = false;

    void Start()
    {
        rgd = GetComponent<Rigidbody2D>();
    }

    public void Init()
    {

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rgd.velocity =(Vector2.up * 12.5f);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            rgd.AddForce(Vector2.up * 3.5f);
        }
    }

    private void AnimeUpdate()
    {
        if (rgd.velocity.x < 0)
        {
            isLeft = true;
            changeAnimeState(LeftAnimName);
        }
        else if (rgd.velocity.x > 0)
        {
            isLeft = false;
            changeAnimeState(RightAnimName);
        }
        else
        {
            if (isLeft)
            {
                changeAnimeState(LeftIdleAnimName);
            }
            else
            {
                changeAnimeState(RightIdleAnimName);
            }
        }
    }

    private void changeAnimeState(string name)
    {
        if (nowAnimState != name)
        {
            nowAnimState = name;
            anim.SetTrigger(name);
        }
    }
}
