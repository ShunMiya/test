using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    [Header("�t�F�[�h")] public FadeImage fade;
    [Header("�Q�[���X�^�[�g���ɖ炷SE")] public AudioClip startSE;

    private bool firstPush = false;
    private bool goNextScene = false;

    //�X�^�[�g�{�^���������ꂽ��Ă΂��
    public void PressStart()
    {
        Debug.Log("Press Start!");
        if(!firstPush)
        {
            GManager.instance.PlaySE(startSE);
            fade.StartFadeOut();
            firstPush = true;
        }
    }

    private void Update()
    {
        if(!goNextScene && fade.IsFadeOutComplete())
        {
            SceneManager.LoadScene("stage1");
            goNextScene = true;
        }
    }

}
