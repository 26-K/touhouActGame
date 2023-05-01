using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[SerializeField]
public class MoveData
{
    [LabelText("移動速度")] public Vector2 spd; //別で持たせるべき
    [LabelText("ジャンプ力")] public float jumpPow = 12.0f;
    public bool isLeft = false;
    public bool isGround = false;
    public bool isGravity = false;
    public float grav = 0.0f;
    public int jumpCount = 0;
    public float jumpTime = 0.0f;
}


public class Player : MonoBehaviour
{
    const string LeftAnimName = "Left";
    const string RightAnimName = "Right";
    const string LeftIdleAnimName = "LeftIdle";
    const string RightIdleAnimName = "RightIdle";

    [LabelText("アニメーション")] [SerializeField] Animator anim;
    [LabelText("拡大縮小アニメーション")] [SerializeField] Animator scaleAnim;
    [LabelText("移動速度")] [SerializeField] float moveSpd = 8.0f;
    [LabelText("ジャンプ力(上書き)")] [SerializeField] float OverridejumpPow = 16.0f;
    [LabelText("砂煙")] [SerializeField] ParticleSystem dust;
    [LabelText("先行カメラ位置")] [SerializeField] GameObject leadingCameraObj;
    [LabelText("先行カメラ範囲")] [SerializeField] Vector2 leadingCameraMargin;
    [LabelText("壁ジャン猶予時間")] [SerializeField] float wallJumpInterval = 0.2f;
    string nowAnimState = RightIdleAnimName;
    bool isTouchWall = false;
    float wallJumpTimer = 0.0f;

    Rigidbody2D rgd;
    public MoveData moveData = new MoveData();
    [SerializeField] Character_Jump jump;

    [SerializeField] GameObject testWallJumpObj;

    void Start()
    {
        if (rgd == null) //地形のアタッチ忘れ対策
        {
            rgd = GetComponent<Rigidbody2D>();
        }
        moveData.jumpPow = OverridejumpPow;
        Init();
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Init()
    {
        jump.Init(this);
    }

    public void Update()
    {
        //テスト用、壁ジャンが可能な時はオブジェクトを表示する

        testWallJumpObj.SetActive(wallJumpTimer > 0.0f);
        var a = CheckGroundStatus();
        DecTimer();
        Debug.Log(a.collider);
        if (a.collider != null)
        {
            if (a.collider.transform.tag == "Terrain" && moveData.jumpTime <= 0) //床系統に触れている場合
            {
                if (moveData.isGround == false)
                {
                    scaleAnim.Play("Ground");
                    dust.Play();
                }
                moveData.isGround = true;
                moveData.spd.y = 0.0f;
                moveData.jumpCount = 1;
                moveData.grav = -3.0f;
            }
            else
            {
                moveData.isGround = false;
                moveData.jumpCount = 0;
            }
        }


        isTouchWall = false;
        if (moveData.isGround == false)
        {
            var touchWall = CheckTouchWall();
            if (touchWall.collider != null)
            {
                if (touchWall.collider.transform.tag == "Terrain" && moveData.jumpTime <= 0)//床系統に触れている場合
                {
                    wallJumpTimer = wallJumpInterval;
                    isTouchWall = true;
                }
            }
        }

        if (wallJumpTimer > 0.0f && Input.GetKeyDown(KeyCode.Space)) //スペースが押された時壁ジャンチェック
        {
            jump.DoWallJump(); //test
        }

        if (Input.GetKey(KeyCode.Space)) //スペースキーが押されている時はジャンプ用の重力に
        {
            if (moveData.isGround)
            {
                DoJump();
            }
        }
        else //押されていない時は降下用重力に
        {
            if (moveData.isGravity == true)
            {
                moveData.isGravity = false;
            }
        }
        TryMove();
        AnimeUpdate();
    }

    private void DecTimer()
    {
        moveData.jumpTime -= Time.deltaTime;
        wallJumpTimer -= Time.deltaTime;
    }

    private void TryMove()
    {
        if (Input.GetKey(KeyCode.A))
        {
            moveData.spd.x = -moveSpd;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveData.spd.x = moveSpd;
        }
        else //キーを入力していない間の減速
        {
            float speedDecRatio = 0.95f;
            moveData.spd.x = moveData.spd.x * speedDecRatio;
        }
    }

    private void DoJump()
    {
        jump.DoJump();
        scaleAnim.Play("JumpStart");
        dust.Play();
    }

    void FixedUpdate()
    {
        moveData.spd.y = moveData.grav;
        if (moveData.spd.y < -moveData.jumpPow)
        {
            moveData.spd.y = -moveData.jumpPow;
        }
        if (moveData.isGravity)
        {
            moveData.grav -= 0.75f;
        }
        else
        {
            moveData.grav -= 2.0f;
        }
        if (isTouchWall) //壁ずさり
        {
            moveData.grav = Mathf.Max(moveData.grav, -4.0f);
        }
        rgd.velocity = moveData.spd; //New
        if (rgd.velocity.x != 0)
        {
            float rate = (moveData.spd.x / moveSpd);
            Vector3 targetPos = Vector3.zero;
            targetPos.x = this.transform.position.x + rate * leadingCameraMargin.x;
            targetPos.y = this.transform.position.y;
            //先行カメラが気持ち悪いので一旦止める :Fix
            //leadingCameraObj.transform.position = Vector3.Lerp(leadingCameraObj.transform.position, targetPos,0.25f);
        }
    }

    private void AnimeUpdate()
    {
        anim.SetFloat("MoveSpeed", Mathf.Abs(rgd.velocity.x * 0.5f));
        if (rgd.velocity.x < 0)
        {
            moveData.isLeft = true;
            changeAnimeState(LeftAnimName);
        }
        else if (rgd.velocity.x > 0)
        {
            moveData.isLeft = false;
            changeAnimeState(RightAnimName);
        }
        else
        {
            if (moveData.isLeft)
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

    /// <summary>
    /// 下方向に僅かなrayを飛ばして接地しているか判定する
    /// </summary>
    /// <returns></returns>
    RaycastHit2D CheckGroundStatus()
    {
        Vector2 startPos = (Vector2)transform.position;

        Vector2 pos = new Vector2(0, -0.55f);
        Vector2 size = new Vector2(0.3f, 0.05f);
        float distance = 0.1f;
        return Physics2D.BoxCast(startPos + pos, size, 0, Vector2.down, distance);
    }


    /// <summary>
    /// 向いている方向に僅かなrayを飛ばして壁にくっついてるか判定する。
    /// </summary>
    /// <returns></returns>
    RaycastHit2D CheckTouchWall()
    {
        Vector2 startPos = (Vector2)transform.position;

        Vector2 pos = new Vector2(0, -0.55f);
        Vector2 size = new Vector2(0.3f, 0.05f);
        float distance = 0.1f;
        Vector2 targetDir = Vector2.right;
        if (moveData.isLeft)
        {
            targetDir = Vector2.left;
        }
        return Physics2D.BoxCast(startPos + pos, size, 0, targetDir, distance);
    }

    #region 坂道チェックの名残だったもの
    //void Test ()
    //{
    //    //rayの設定
    //    Vector2 startPos = (Vector2)transform.position;
    //    Vector2 pos = new Vector2(0, -0.55f);
    //    Vector2 size = new Vector2(0.3f, 0.05f);
    //    float distance = 0.1f;

    //    //rayを飛ばして命中した左端と右端を取得する。
    //    Vector2 leftVec = new Vector2(-0.3f, 0); //仮
    //    var b = startPos + pos + leftVec;
    //    var a = Physics2D.BoxCast(b, size, 0, Vector2.down, distance); //開始地点から少し左にずれた地形を取得する…?
    //    Debug.Log($"LeftHit::{a.normal}");
    //    var c = startPos + pos + (leftVec * -1.0f);
    //    a = Physics2D.BoxCast(c, size, 0, Vector2.down, distance); //開始地点から少し→にずれた地形を取得する…?
    //    Debug.Log($"RightHit::{a.normal}");

    //}

    //    /// <summary>
    //    /// 坂道チェック用
    //    /// 登るのが遅かったり勝手に下るのを修正
    //    /// </summary>
    //    /// <param name="startPos">対象位置</param>
    //    /// <param name="dir">方向</param>
    //    /// <returns></returns>
    //    Vector2 SlopeCheck(Vector2 startPos, Vector2 dir)
    //    {
    //        Vector2 slopeVec = Vector2.right;
    //        float Checkdistance = 1.0f;
    //        RaycastHit2D rayHit2D = Physics2D.Raycast(startPos, Vector2.down, Checkdistance);

    //        if (rayHit2D.collider != null)
    //        {
    //            //当たった地形の法線ベクトルを取る
    //            Vector2 normal = rayHit2D.normal; //命中したrayの法線ベクトル

    //        }
    //    }
    #endregion
}
