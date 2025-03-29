using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;

public class InputRecordUI : MonoBehaviour
{
    public SignalIconSO signalIcon;

    public TextMeshProUGUI durationText;
    public Image directionIcon;

    #region 攻击 Icon 控件
    [Header("攻击Icon对象获取")]
    public GameObject lp;
    public GameObject mp;
    public GameObject hp;
    public GameObject lk;
    public GameObject mk;
    public GameObject hk;
    #endregion

    // private RecordUIManager m_manager;

    private void Start()
    {
        
    }

    public void Init()
    {
        durationText.text = " 1";
        directionIcon.sprite = signalIcon.NoneInput;

        lp.SetActive(false);
        mp.SetActive(false);
        hp.SetActive(false);
        lk.SetActive(false);
        mk.SetActive(false);
        hk.SetActive(false);
    }

    public void Copy(InputRecordUI target)
    {
        durationText.text = target.durationText.text;
        directionIcon.sprite = target.directionIcon.sprite;
        lp.SetActive(target.lp.activeInHierarchy);
        mp.SetActive(target.mp.activeInHierarchy);
        hp.SetActive(target.hp.activeInHierarchy);
        lk.SetActive(target.lk.activeInHierarchy);
        mk.SetActive(target.mk.activeInHierarchy);
        hk.SetActive(target.hk.activeInHierarchy);
    }

    public void SetDurationText(int duration)
    {
        durationText.text = duration.ToString();
    }

    public void SetDirectionImage(string direction)
    {
        switch (direction)
        {
            case "Up":
                directionIcon.sprite = signalIcon.Up;
                break;
            case "Left Up":
                directionIcon.sprite = signalIcon.leftUp;
                break;
            case "Right Up":
                directionIcon.sprite = signalIcon.rightUp;
                break;
            case "Left":
                directionIcon.sprite = signalIcon.left;
                break;
            case "None":
                directionIcon.sprite = signalIcon.NoneInput;
                break;
            case "Right":
                directionIcon.sprite = signalIcon.right;
                break;
            case "Left Down":
                directionIcon.sprite = signalIcon.leftDown;
                break;
            case "Down":
                directionIcon.sprite = signalIcon.down;
                break;
            case "Right Down":
                directionIcon.sprite = signalIcon.rightDown;
                break;
            default:
                throw new System.Exception("InputRecordUI.SetDirection(): 参数错误");
        }
    }

    public void SetAttackIcon(string attack, bool isActive)
    {
        switch (attack)
        {
            case "LP":
                lp.SetActive(isActive);
                break;
            case "MP":
                mp.SetActive(isActive);
                break;
            case "HP":
                hp.SetActive(isActive);
                break;
            case "LK":
                lk.SetActive(isActive);
                break;
            case "MK":
                mk.SetActive(isActive);
                break;
            case "HK":
                hk.SetActive(isActive);
                break;
            default:
                throw new System.Exception("InputRecordUI.SetAttack(): 参数错误");
        }
    }
}
