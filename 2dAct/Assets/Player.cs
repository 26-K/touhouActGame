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
    public float walljumpCoolTime = 0.0f;
}


public class Player : MonoBehaviour
{
    const string LeftAnimName = "Left";
    const string RightAnimName = "Right";
    const string LeftIdleAnimName = "LeftIdle";
    const string RightIdleAnimName = "RightIdle";
    const string LeftJumpAnimName = "LeftJump";
    const string RightJumpAnimName = "RightJump";

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
    [SerializeField] LineRenderer linerend;

    Rigidbody2D rgd;
    public MoveData moveData = new MoveData();
    [SerializeField] Character_Jump jump;

    [SerializeField] GameObject testWallJumpObj;
    bool isInputMove = false;

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
        Application.targetFrameRate = 60;
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
                DoLandingGround(); //着地チェック

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
            wallJumpTimer = 0.0f;
            jump.DoWallJump(); //test
            //if (moveData.isLeft)
            //{
            //    changeAnimeState(LeftAnimName);
            //}
            //else
            //{
            //    changeAnimeState(RightAnimName);
            //}
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

    /// <summary>
    /// 着地
    /// </summary>
    private void DoLandingGround()
    {
        if (moveData.isGround == false) //着地
        {
            scaleAnim.Play("Ground");
            dust.Play();
        }
        moveData.isGround = true;
        moveData.spd.y = 0.0f;
        moveData.jumpCount = 1;
        moveData.grav = -3.0f;
    }

    /// <summary>
    /// タイマーの減算
    /// </summary>
    private void DecTimer()
    {
        moveData.jumpTime -= Time.deltaTime;
        wallJumpTimer -= Time.deltaTime;
        moveData.walljumpCoolTime -= Time.deltaTime;
    }

    /// <summary>
    /// 移動しようとする
    /// </summary>
    private void TryMove()
    {
        isInputMove = false;
        if (moveData.walljumpCoolTime <= 0.0f)
        {

            if (Input.GetKey(KeyCode.A))
            {
                isInputMove = true;
                if (moveData.isGround)
                {
                    moveData.spd.x = -moveSpd;

                    moveData.isLeft = true;
                }
                else
                {
                    if (moveData.jumpTime <= 0.0f)
                    {
                        moveData.isLeft = true;
                        moveData.spd.x -= moveSpd * 0.7f;
                        moveData.spd.x = Mathf.Max(moveData.spd.x, -moveSpd);
                    }
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                isInputMove = true;
                if (moveData.isGround)
                {
                    moveData.spd.x = moveSpd;

                    moveData.isLeft = false;
                }
                else
                {
                    if (moveData.jumpTime <= 0.0f)
                    {
                        moveData.isLeft = false;
                        moveData.spd.x += moveSpd * 0.7f;
                        moveData.spd.x = Mathf.Min(moveData.spd.x, moveSpd);
                    }
                }
            }
            else //キーを入力していない間の減速
            {
                float speedDecRatio = 1.0f;
                if (moveData.isGround)
                {
                    speedDecRatio = 0.8f;
                }
                moveData.spd.x = moveData.spd.x * speedDecRatio;
            }
        }
    }

    public float GetMaxMoveSpeed()
    {
        return moveSpd;
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
        if (moveData.isGround == true) //地面にいる
        {
            if (SlopeCheck(false))
            {
                Debug.Log("坂");
                if (isInputMove) //移動中の場合Plを押し上げて坂道を移動しやすく
                {
                    rgd.gravityScale = -5;
                    moveData.grav = 1.5f;
                    moveData.spd.y = 0.0f;
                }
                else
                {
                    moveData.spd.y = 0.0f;
                    moveData.spd.x = 0.0f;
                }
            }
            else
            {
                rgd.gravityScale = 0;
            }
            if (SlopeCheck(true) && isInputMove == false) //坂で停止中
            {
                moveData.spd.y = 0.0f;
                moveData.spd.x = 0.0f;
            }
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
        if (moveData.isGround)
        {
            if (rgd.velocity.x < -0.05f)
            {
                changeAnimeState(LeftAnimName);
            }
            else if (rgd.velocity.x > 0.05f)
            {
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
        else
        {
            if (rgd.velocity.x < 0)
            {
                changeAnimeState(LeftJumpAnimName);
            }
            else
            {
                changeAnimeState(RightJumpAnimName);
            }
        }
    }

    public void ChangeScaleAnim(string str)
    {
        scaleAnim.Play(str);
    }

    void changeAnimeState(string name)
    {
        if (nowAnimState != name || anim.GetCurrentAnimatorStateInfo(0).IsName(name) == false)
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
        Vector2 pos = new Vector2(0, 0.0f);
        Vector2 size = new Vector2(0.3f, 0.05f);
        float distance = 0.2f;
        LayerMask mask = LayerMask.GetMask("Platforms");
        return Physics2D.BoxCast(startPos + pos, size, 0, Vector2.down, distance, mask);
    }


    /// <summary>
    /// 向いている方向に僅かなrayを飛ばして壁にくっついてるか判定する。
    /// </summary>
    /// <returns></returns>
    RaycastHit2D CheckTouchWall()
    {
        Vector2 startPos = (Vector2)transform.position;

        Vector2 pos = new Vector2(0.4f, 0.3f);
        Vector2 size = new Vector2(0.3f, 0.05f);
        float distance = 0.2f;
        Vector2 targetDir = Vector2.right;
        if (moveData.isLeft)
        {
            targetDir = Vector2.left;
            pos.x = pos.x * -1;
        }
        LayerMask mask = LayerMask.GetMask("Platforms");
        return Physics2D.BoxCast(startPos + pos, size, 0, targetDir, distance, mask);
    }

    #region 坂道チェックの名残だったもの
    bool SlopeCheck(bool isReverse)
    {
        //rayの設定
        Vector2 startPos = (Vector2)transform.position;
        Vector2 pos = new Vector2(0.45f, 0.1f);
        Vector2 pos_2 = new Vector2(0.45f, 0.4f);
        Vector2 size = new Vector2(0.3f, 0.05f);
        Vector2 targetDir = Vector2.right;
        bool dir = (moveData.isLeft == true);
        if (isReverse)
        {
            dir = !dir;
        }
        if (dir)
        {
            targetDir *= -1;
            pos.x *= -1;
            pos_2.x *= -1;
        }
        LayerMask mask = LayerMask.GetMask("Platforms");
        float distance = 0.1f;

        //rayを飛ばして坂道チェック
        var a = Physics2D.BoxCast(startPos + pos, size, 0, targetDir, distance, mask);
        var b = Physics2D.BoxCast(startPos + pos_2, size, 0, targetDir, distance, mask);
        DrawRayLine(startPos + pos, targetDir * distance);
        return (a.collider != null && b.collider == null);
        //if (a.collider != null && b.collider == null)
        //{
        //    Debug.Log("坂道");
        //}

    }

    /// <summary>
    /// 坂道チェック用
    /// 登るのが遅かったり勝手に下るのを修正
    /// </summary>
    /// <param name="startPos">対象位置</param>
    /// <param name="dir">方向</param>
    /// <returns></returns>
    //Vector2 SlopeCheck(Vector2 startPos, Vector2 dir)
    //{
    //    Vector2 slopeVec = Vector2.right;
    //    float Checkdistance = 1.0f;
    //    RaycastHit2D rayHit2D = Physics2D.Raycast(startPos, Vector2.down, Checkdistance);

    //    if (rayHit2D.collider != null)
    //    {
    //        //当たった地形の法線ベクトルを取る
    //        Vector2 normal = rayHit2D.normal; //命中したrayの法線ベクトル

    //    }
    //}
    #endregion


    //引数はorigin（始点）と方向（direction）
    private void DrawRayLine(Vector3 start, Vector3 direction)
    {
        //LineRendererコンポーネントの取得

#if UNITY_EDITOR
#else
        if (linerend)
        {
        linerend.enabled = false;
        }
#endif
        //線の太さを設定
        linerend.startWidth = 0.04f;
        linerend.endWidth = 0.04f;

        //始点, 終点を設定し, 描画
        linerend.SetPosition(0, start);
        linerend.SetPosition(1, start + direction);
    }
}
