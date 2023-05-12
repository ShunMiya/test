using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    #region//�C���X�y�N�^�[�Őݒ肷��
    [Header("�ړ����x")] public float speed;
    [Header("�d��")] public float gravity;
    [Header("�W�����v���x")] public float jumpSpeed;
    [Header("�W�����v���鍂��")] public float jumpHeight;
    [Header("�W�����v���钷��")] public float jumpLimitTime;
    [Header("�ڒn����")] public GroundCheck ground;
    [Header("�V�䔻��")] public GroundCheck head;
    [Header("�_�b�V���̑����\��")] public AnimationCurve dashCurve;
    [Header("�W�����v�̑����\��")] public AnimationCurve jumpCurve;
    [Header("���݂�����̍����̊���(%)")] public float stepOnRate;
    [Header("�W�����v���鎞�ɖ炷SE")] public AudioClip jumpSE;
    [Header("���ꂽ���ɖ炷SE")] public AudioClip downSE;
    [Header("�R���e�B�j���[���ɖ炷SE")] public AudioClip continueSE;
    #endregion

    #region//�v���C�x�[�g�ϐ�
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
        //�R���|�[�l���g�̃C���X�^���X��߂܂���
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        capcol = GetComponent<CapsuleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isContinue)
        {
            //���Ł@���Ă���Ƃ��ɖ߂�
            if (blinkTime > 0.2f)
            {
                sr.enabled = true;
                blinkTime = 0.0f;
            }
            //���Ł@�����Ă���Ƃ�
            else if (blinkTime > 0.1f)
            {
                sr.enabled = false;
            }
            //���Ł@���Ă���Ƃ�
            else
            {
                sr.enabled = true;
            }

            //�P�b�������疾�ŏI���
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
            //�ڒn����𓾂�
            isGround = ground.IsGround();
            isHead = head.IsGround();

            //�e����W���̑��x�����߂�
            float xSpeed = GetXSpeed();
            float ySpeed = GetYSpeed();

            //�A�j���[�V������K�p
            SetAnimation();

            //�ړ����x��ݒ�
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
    /// Y�����ŕK�v�Ȍv�Z�����A���x��Ԃ��B
    /// </summary>
    /// <returns>Y���̑���</returns>
    private float GetYSpeed()
    {
        float verticalKey = Input.GetAxis("Vertical");
        float ySpeed = -gravity;

        //�����𓥂񂾍ۂ̃W�����v
        if (isOtherJump)
        {
�@�@�@      //���݂̍�������ׂ鍂����艺��
�@�@�@      bool canHeight = jumpPos + otherJumpHeight > transform.position.y;
            //�W�����v���Ԃ������Ȃ肷���ĂȂ���
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
        //�n�ʂɂ���Ƃ�
        else if (isGround)
        {
            if (verticalKey > 0)
            {
                if (!isJump)
                {
                    GManager.instance.PlaySE(jumpSE);
                }
                ySpeed = jumpSpeed;
                jumpPos = transform.position.y; //�W�����v�����ʒu���L�^����
                isJump = true;
                jumpTime = 0.0f;
            }
            else
            {
                isJump = false;
            }
        }

        //�W�����v��
        else if (isJump)
        {
            //������L�[�������Ă��邩
            bool pushUpKey = verticalKey > 0;
            //���݂̍�������ׂ鍂����艺��
            bool canHeight = jumpPos + jumpHeight > transform.position.y;
            //�W�����v���Ԃ������Ȃ肷���ĂȂ���
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
    /// X�����ŕK�v�Ȍv�Z�����A���x��Ԃ��B
    /// </summary>
    /// <returns>X���̑���</returns>
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

        //�O��̓��͂���_�b�V���̔��]�𔻒f���đ��x��ς���
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
    /// �A�j���[�V������ݒ肷��
    /// </summary>
    private void SetAnimation()
    {
        anim.SetBool("jump", isJump || isOtherJump);
        anim.SetBool("ground", isGround);
        anim.SetBool("run", isRun);
    }

    //���ꂽ���̏���
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

    #region//�ڐG����
    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool enemy = (collision.collider.tag == enemyTag);
        bool moveFloor = (collision.collider.tag == moveFloorTag);
        bool fallFloor = (collision.collider.tag == fallFloorTag);
        bool jumpStep = (collision.collider.tag == jumpStepTag);

        if(enemy || moveFloor || fallFloor || jumpStep)
        {
            //���݂�����ɂȂ鍂��
            float stepOnHeight = (capcol.size.y * (stepOnRate / 100f));
            //���݂�����̃��[���h���W
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
                                otherJumpHeight = o.boundHeight;    //����Â������̂��璵�˂鍂�����擾����
                                otherJumpSpeed = o.jumpSpeed;�@�@//�W�����v����X�s�[�h
                                o.playerStepOn = true;        //����Â������̂ɑ΂��ē���Â�������ʒm����
                                jumpPos = transform.position.y; //�W�����v�����ʒu���L�^���� 
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
                            Debug.Log("ObjectCollision���t���ĂȂ���I");
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
            //���������痣�ꂽ
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
    ///�R���e�B�j���[�ҋ@��Ԃ�
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

    //�_�E���A�j���[�V�������������Ă��邩�ǂ���
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
    ///�R���e�B�j���[����
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