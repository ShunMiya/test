using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpObject : MonoBehaviour
{
    [Header("”ò‚ñ‚¾‚É–Â‚ç‚·SE")] public AudioClip jumpSE;


    private ObjectCollision oc;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        oc = GetComponent<ObjectCollision>();
        anim = GetComponent<Animator>();
        if(oc == null || anim == null)
        {
            Debug.Log("ƒWƒƒƒ“ƒv‘ä‚Ìİ’è‚ª‘«‚è‚Ä‚¢‚Ü‚¹‚ñ");
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(oc.playerStepOn)
        {
            anim.SetTrigger("on");
            GManager.instance.PlaySE(jumpSE);

            oc.playerStepOn = false;
        }
    }
}
