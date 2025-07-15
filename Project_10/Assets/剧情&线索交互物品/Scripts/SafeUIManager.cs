using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeUIManager : MonoBehaviour
{
    public GameObject paperContentFloor1UI;
    public GameObject paperContentFloor2_1UI;
    public GameObject paperContentFloor2_2UI;
    public GameObject diaryContentUI;
    public GameObject passwordF1_1UI;
    public GameObject passwordF1_2UI;
    public GameObject passwordF2_1UI;
    public GameObject passwordF2_2UI;

    void Update()
    {
        // ���ȣ�����Ƿ����κ�һ��UI����Ǽ���״̬
        if (paperContentFloor1UI.activeSelf || paperContentFloor2_1UI.activeSelf || paperContentFloor2_2UI.activeSelf || diaryContentUI.activeSelf)
        {
            // �����UI�Ǽ���ģ��ټ���Ƿ��������������
            if (Input.GetMouseButtonDown(0))
            {
                // ��������ˣ��͵��������Ѿ�д�õĹر�����UI�ķ���
                HideAllUI();
            }
        }
    }

    public void ShowFloor1PaperContent()
    {
        paperContentFloor1UI.SetActive(true);
    }

    public void ShowFloor2_1PaperContent()
    {
        paperContentFloor2_1UI.SetActive(true);
    }

    public void ShowFloor2_2PaperContent()
    {
        paperContentFloor2_2UI.SetActive(true);
    }

    public void ShowDiaryContent()
    {
        diaryContentUI.SetActive(true);
    }

    public void ShowPasswordF1_1()
    {
        passwordF1_1UI.SetActive(true);
    }

    public void ShowPasswordF1_2()
    {
        passwordF1_2UI.SetActive(true);
    }

    public void ShowPasswordF2_1()
    {
        passwordF2_1UI.SetActive(true);
    }

    public void ShowPasswordF2_2()
    {
        passwordF2_2UI.SetActive(true);
    }

    public void HideAllUI()
    {
        paperContentFloor1UI.SetActive(false);
        paperContentFloor2_1UI.SetActive(false);
        paperContentFloor2_2UI.SetActive(false);
        diaryContentUI.SetActive(false);
        passwordF1_1UI.SetActive(false);
        passwordF1_2UI.SetActive(false);
        passwordF2_1UI.SetActive(false);
        passwordF2_2UI.SetActive(false);
    }
}
