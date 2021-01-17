using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ReddotUI : MonoBehaviour,IPointerClickHandler
{
    public string Path;

    private Text txt;


    private void Awake()
    {
        txt = GetComponentInChildren<Text>();
    }

    void Start()
    {
        TreeNode node = ReddotMananger.Instance.AddListener(Path, ReddotCallback);
        gameObject.name = node.FullPath;
    }

    private void ReddotCallback(int value)
    {
        Debug.Log("红点刷新，路径:" + Path + ",当前帧数:" + Time.frameCount + ",值:" + value);
        txt.text = value.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int value = ReddotMananger.Instance.GetValue(Path);

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ReddotMananger.Instance.ChangeValue(Path, value + 1);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ReddotMananger.Instance.ChangeValue(Path, Mathf.Clamp(value - 1,0, value));
        }
    }
}
