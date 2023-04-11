using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    const string LeftAnimName = "Left";
    const string RightAnimName = "Right";
    const string LeftIdleAnimName = "LeftIdle";
    const string RightIdleAnimName = "RightIdle";

    [LabelText("アニメーション")] [SerializeField] Animator anim;
    [LabelText("拡大縮小アニメーション")] [SerializeField] Animator scaleAnim;
    [LabelText("移動速度")] [SerializeField] Vector2 spd;
    [LabelText("移動速度")] [SerializeField] float moveSpd = 8.0f;
    [LabelText("ジャンプ力")] [SerializeField] float jumpPow = 12.0f;
    [LabelText("砂煙")] [SerializeField] ParticleSystem dust;
    [LabelText("先行カメラ位置")] [SerializeField] GameObject leadingCameraObj;
    [LabelText("先行カメラ範囲")] [SerializeField] Vector2 leadingCameraMargin;
    string nowAnimState = RightIdleAnimName;

    Rigidbody2D rgd;
    bool isLeft = false;
    bool isGround = false;
    bool isGravity = false;
    float grav = 0.0f;
    int jumpCount = 0;
    float jumpTime = 0.0f;

    void Start()
    {
        rgd = GetComponent<Rigidbody2D>();
    }

    public void Init()
    {

    }

    public void Update()
    {
        var a = CheckGroundStatus();
        jumpTime -= Time.deltaTime;
        Debug.Log(a.collider);
        if (a.collider != null)
        {
            if (a.collider.transform.tag == "Terrain" && jumpTime <= 0)
            {
                if (isGround == false)
                {
                    Debug.Log("接地");
                    scaleAnim.Play("Ground");
                    dust.Play();
                }
                isGround = true;
                spd.y = 0.0f;
                jumpCount = 1;
                grav = -3.0f;
            }
            else
            {
                isGround = false;
                jumpCount = 0;
            }
        }

        if (Input.GetKey(KeyCode.Space)) //スペースキーが押されている時はジャンプ用の重力に
        {
            if (isGround)
            {
                isGravity = true;
                grav = jumpPow;
                spd.y = grav;
                isGround = false;
                jumpTime = 0.2f;
                scaleAnim.Play("JumpStart");
                Debug.Log("ジャンプ");
                dust.Play();
            }
        }
        else //押されていない時は降下用重力に
        {
            if (isGravity == true)
            {
                isGravity = false;
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            spd.x = -moveSpd;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            spd.x = moveSpd;
        }
        else
        {
            spd.x = spd.x * 0.5f;
        }
        AnimeUpdate();
    }

    void FixedUpdate()
    {
        spd.y = grav;
        if (spd.y < -jumpPow)
        {
            spd.y = -jumpPow;
        }
        if (isGravity)
        {
            grav -= 0.75f;
        }
        else
        {
            grav -= 2.0f;
        }
        rgd.velocity = spd; //New
        if (rgd.velocity.x != 0)
        {
            float rate = (spd.x / moveSpd);
            Vector3 targetPos = Vector3.zero;
            targetPos.x = this.transform.position.x + rate * leadingCameraMargin.x;
            targetPos.y = this.transform.position.y;
            leadingCameraObj.transform.position = Vector3.Lerp(leadingCameraObj.transform.position, targetPos,0.25f);
        }
    }

    private void AnimeUpdate()
    {
        anim.SetFloat("MoveSpeed", Mathf.Abs(rgd.velocity.x * 0.5f));
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

    RaycastHit2D CheckGroundStatus()
    {
        Vector2 startPos = (Vector2)transform.position;

        Vector2 pos = new Vector2(0, -0.55f);
        Vector2 size = new Vector2(0.3f, 0.05f);
        float distance = 0.1f;

        return Physics2D.BoxCast(startPos + pos, size, 0, Vector2.down, distance);
    }
}
