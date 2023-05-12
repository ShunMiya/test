using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearEffect : MonoBehaviour
{
    [Header("�g��k���̃A�j���[�V�����J�[�u")] public AnimationCurve curve;
    [Header("�X�e�[�W�R���g���[���[")] public StageCtrl ctrl;
    
    private bool comp = false;
    private float timer;

    // Start is called before the first frame update
    private void Start()
    {
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        if(!comp)
        {
            if (timer < 1.0f)
            {
                transform.localScale = Vector3.one * curve.Evaluate(timer);
                timer += Time.deltaTime;
            }
            else
            {
                transform.localScale = Vector3.one;
                ctrl.ChangeScene(GManager.instance.stageNum + 1);
                comp = true;
            }
        }
    }
}
