using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    #region//インスペクターで設定する
    [Header("移動速度")] public float speed;
    [Header("重力")] public float gravity;
    [Header("ジャンプ速度")] public float jumpSpeed;
    [Header("ジャンプする高さ")] public float jumpHeight;
    [Header("ジャンプする長さ")] public float jumpLimitTime;
    [Header("接地判定")] public GroundCheck ground;
    [Header("天井判定")] public GroundCheck head;
    [Header("ダッシュの速さ表現")] public AnimationCurve dashCurve;
    [Header("ジャンプの速さ表現")] public AnimationCurve jumpCurve;
    [Header("踏みつけ判定の高さの割合(%)")] public float stepOnRate;
    [Header("ジャンプする時に鳴らすSE")] public AudioClip jumpSE;
    [Header("やられた時に鳴らすSE")] public AudioClip downSE;
    [Header("コンティニュー時に鳴らすSE")] public AudioClip continueSE;
    #endregion

    #region//プライベート変数
    private Animator anim = null;
    private Rigidbody2D rb = null;
    private CapsuleCollider2D capcol = null;
    private MoveObject moveObj = null;
    private bool isGround = false;
    private bool isJump = false;
    private bool isRun = false;
    private bool isHead = false;
    private bool isDown = false;
    private bool isOtherJump = false;
    private bool isContinue = false;
    private SpriteRenderer sr = null;
    private bool nonDownAnim = false;
    private bool isClearMotion = false;
    private float continueTime = 0.0f;
    private float blinkTime = 0.0f;
    private float jumpPos = 0.0f;
    private float otherJumpHeight = 0.0f;
    private float otherJumpSpeed = 0.0f;
    private float dashTime = 0.0f;
    private float jumpTime = 0.0f;
    private float beforeKey = 0.0f;
    private string enemyTag = "Enemy";
    private string deadAreaTag = "DeadArea";
    private string hitAreaTag = "HitArea";
    private string moveFloorTag = "MoveFloor";
    private string fallFloorTag = "FallFloor";
    private string jumpStepTag = "JumpStep";
    #endregion

    void Start()
    {
        //コンポーネントのインスタンスを捕まえる
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        capcol = GetComponent<CapsuleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isContinue)
        {
            //明滅　ついているときに戻る
            if (blinkTime > 0.2f)
            {
                sr.enabled = true;
                blinkTime = 0.0f;
            }
            //明滅　消えているとき
            else if (blinkTime > 0.1f)
            {
                sr.enabled = false;
            }
            //明滅　ついているとき
            else
            {
                sr.enabled = true;
            }

            //１秒たったら明滅終わり
            if (continueTime > 1.0f)
            {
                isContinue = false;
                blinkTime = 0f;
                continueTime = 0f;
                sr.enabled = true;
            }
            else
            {
                blinkTime += Time.deltaTime;
                continueTime += Time.deltaTime;
            }
        }
    }

    void FixedUpdate()
    {
        if (!isDown && !GManager.instance.isGameOver && !GManager.instance.isStageClear)
        {
            //接地判定を得る
            isGround = ground.IsGround();
            isHead = head.IsGround();

            //各種座標軸の速度を求める
            float xSpeed = GetXSpeed();
            float ySpeed = GetYSpeed();

            //アニメーションを適用
            SetAnimation();

            //移動速度を設定
            Vector2 addVelocity = Vector2.zero;
            if (moveObj != null)
            {
                addVelocity = moveObj.GetVelocity();
            }
            rb.velocity = new Vector2(xSpeed, ySpeed) + addVelocity;
        }
        else
        {
            if(!isClearMotion && GManager.instance.isStageClear)
            {
                anim.Play("player_clear");
                isClearMotion = true;
            }
            rb.velocity = new Vector2(0, -gravity);
        }
    }

    /// <summary>
    /// Y成分で必要な計算をし、速度を返す。
    /// </summary>
    /// <returns>Y軸の速さ</returns>
    private float GetYSpeed()
    {
        float verticalKey = Input.GetAxis("Vertical");
        float ySpeed = -gravity;

        //何かを踏んだ際のジャンプ
        if (isOtherJump)
        {
　　　      //現在の高さが飛べる高さより下か
　　　      bool canHeight = jumpPos + otherJumpHeight > transform.position.y;
            //ジャンプ時間が長くなりすぎてないか
            bool canTime = jumpLimitTime > jumpTime;

            if (canHeight && canTime && !isHead)
            {
                ySpeed = otherJumpSpeed;
                jumpTime += Time.deltaTime;
            }
            else
            {
                isOtherJump = false;
                jumpTime = 0.0f;
            }
        }
        //地面にいるとき
        else if (isGround)
        {
            if (verticalKey > 0)
            {
                if (!isJump)
                {
                    GManager.instance.PlaySE(jumpSE);
                }
                ySpeed = jumpSpeed;
                jumpPos = transform.position.y; //ジャンプした位置を記録する
                isJump = true;
                jumpTime = 0.0f;
            }
            else
            {
                isJump = false;
            }
        }

        //ジャンプ中
        else if (isJump)
        {
            //上方向キーを押しているか
            bool pushUpKey = verticalKey > 0;
            //現在の高さが飛べる高さより下か
            bool canHeight = jumpPos + jumpHeight > transform.position.y;
            //ジャンプ時間が長くなりすぎてないか
            bool canTime = jumpLimitTime > jumpTime;

            if (pushUpKey && canHeight && canTime && !isHead)
            {
                ySpeed = jumpSpeed;
                jumpTime += Time.deltaTime;
            }
            else
            {
                isJump = false;
                jumpTime = 0.0f;
            }
        }

        if (isJump || isOtherJump)
        {
            ySpeed *= jumpCurve.Evaluate(jumpTime);
        }
        return ySpeed;
    }

    /// <summary>
    /// X成分で必要な計算をし、速度を返す。
    /// </summary>
    /// <returns>X軸の速さ</returns>
    private float GetXSpeed()
    {
        float horizontalKey = Input.GetAxis("Horizontal");
        float xSpeed = 0.0f;

        if (horizontalKey > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            isRun = true;
            dashTime += Time.deltaTime;
            xSpeed = speed;
        }
        else if (horizontalKey < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            isRun = true;
            dashTime += Time.deltaTime;
            xSpeed = -speed;
        }
        else
        {
            isRun = false;
            xSpeed = 0.0f;
            dashTime = 0.0f;
        }

        //前回の入力からダッシュの反転を判断して速度を変える
        if (horizontalKey > 0 && beforeKey < 0)
        {
            dashTime = 0.0f;
        }
        else if (horizontalKey < 0 && beforeKey > 0)
        {
            dashTime = 0.0f;
        }

        xSpeed *= dashCurve.Evaluate(dashTime);
        beforeKey = horizontalKey;
        return xSpeed;
    }

    /// <summary>
    /// アニメーションを設定する
    /// </summary>
    private void SetAnimation()
    {
        anim.SetBool("jump", isJump || isOtherJump);
        anim.SetBool("ground", isGround);
        anim.SetBool("run", isRun);
    }

    //やられた時の処理
    private void ReceiveDamage(bool downAnim)
    {
        if (isDown || GManager.instance.isStageClear)
        {
            return;
        }
        else
        {
            if (downAnim)
            {
                anim.Play("player_down");
            }
            else
            {
                nonDownAnim = true;
            }
            isDown = true;
            GManager.instance.PlaySE(downSE);
            GManager.instance.SubHeartNum();
        }
    }

    #region//接触判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool enemy = (collision.collider.tag == enemyTag);
        bool moveFloor = (collision.collider.tag == moveFloorTag);
        bool fallFloor = (collision.collider.tag == fallFloorTag);
        bool jumpStep = (collision.collider.tag == jumpStepTag);

        if(enemy || moveFloor || fallFloor || jumpStep)
        {
            //踏みつけ判定になる高さ
            float stepOnHeight = (capcol.size.y * (stepOnRate / 100f));
            //踏みつけ判定のワールド座標
            float judgePos = transform.position.y - (capcol.size.y / 2f) + stepOnHeight;

            foreach(ContactPoint2D p in collision.contacts)
            {
                if(p.point.y < judgePos)
                {
                    if(enemy || fallFloor || jumpStep)
                    {
                        ObjectCollision o = collision.gameObject.GetComponent<ObjectCollision>();
                        if(o != null)
                        {
                            if(enemy || jumpStep)
                            {
                                otherJumpHeight = o.boundHeight;    //踏んづけたものから跳ねる高さを取得する
                                otherJumpSpeed = o.jumpSpeed;　　//ジャンプするスピード
                                o.playerStepOn = true;        //踏んづけたものに対して踏んづけた事を通知する
                                jumpPos = transform.position.y; //ジャンプした位置を記録する 
                                isOtherJump = true;
                                isJump = false;
                                jumpTime = 0.0f;
                            }
                            else if(fallFloor)
                            {
                                o.playerStepOn = true;
                            }
                        }
                        else
                        {
                            Debug.Log("ObjectCollisionが付いてないよ！");
                        }
                    }
                    else if(moveFloor)
                    {
                        moveObj = collision.gameObject.GetComponent<MoveObject>();
                    }
                }
                else
                {
                    if(enemy)
                    {
                        ReceiveDamage(true);
                        break;
                    }
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.collider.tag == moveFloorTag)
        {
            //動く床から離れた
            moveObj = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == deadAreaTag)
        {
            ReceiveDamage(false);
        }
        else if (collision.tag == hitAreaTag)
        {
            ReceiveDamage(true);
        }
    }
    #endregion

    ///<summary>
    ///コンティニュー待機状態か
    ///</summary>
    ///<returns></returns>
    public bool IsContinueWaiting()
    {
        if (GManager.instance.isGameOver)
        {
            return false;
        }
        else
        {
            return IsDownAnimEnd() || nonDownAnim;
        }
    }

    //ダウンアニメーションが完了しているかどうか
    private bool IsDownAnimEnd()
    {
        if(isDown && anim != null)
        {
            AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);
            if(currentState.IsName("player_down"))
            {
                if(currentState.normalizedTime >= 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    ///<summary>
    ///コンティニューする
    ///</summary>
    public void ContinuePlayer()
    {
        GManager.instance.PlaySE(continueSE);
        isDown = false;
        anim.Play("player_stand");
        isJump = false;
        isOtherJump = false;
        isRun = false;
        isContinue = true;
        nonDownAnim = false;
    }
}