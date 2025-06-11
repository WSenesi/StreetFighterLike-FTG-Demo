using src.Input_Layer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.UI
{
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

        /// <summary>
        /// Updates the entire UI component based on a given data object.
        /// This is the primary method for controlling this UI element's state.
        /// </summary>
        /// <param name="data">The data object containing the input state to display.</param>
        /// <param name="isFacingRight">The player's current orientation to correctly display directional input.</param>
        public void UpdateDisplay(InputRecordData data, bool isFacingRight)
        {
            SetDurationText(data.Duration);
            SetDirectionImage(data.Direction, isFacingRight);
            SetAttackIcons(data.Attack);
        }

        /// <summary>
        /// Resets the UI to a default, inactive state.
        /// </summary>
        public void Clear()
        {
            durationText.text = "";
            directionIcon.sprite = signalIcon.noneInput;
            lp.SetActive(false);
            mp.SetActive(false);
            hp.SetActive(false);
            lk.SetActive(false);
            mk.SetActive(false);
            hk.SetActive(false);
            gameObject.SetActive(false);
        }

        private void SetDirectionImage(Direction direction, bool isFacingRight)
        {
            // Remap direction based on player facing to get the correct icon
            Direction iconDirection = RemapDirectionForDisplay(direction, isFacingRight);
            
            switch (iconDirection)
            {
                case Direction.Up:
                    directionIcon.sprite = signalIcon.up;
                    break;
                case Direction.Up | Direction.Front: // Up-Right
                    directionIcon.sprite = signalIcon.rightUp;
                    break;
                case Direction.Up | Direction.Back: // Up-Left
                    directionIcon.sprite = signalIcon.leftUp;
                    break;
                case Direction.Front: // Right
                    directionIcon.sprite = signalIcon.right;
                    break;
                case Direction.Back: // Left
                    directionIcon.sprite = signalIcon.left;
                    break;
                case Direction.Down | Direction.Front: // Down-Right
                    directionIcon.sprite = signalIcon.rightDown;
                    break;
                case Direction.Down | Direction.Back: // Down-Left
                    directionIcon.sprite = signalIcon.leftDown;
                    break;
                case Direction.Down:
                    directionIcon.sprite = signalIcon.down;
                    break;
                case Direction.None:
                default:
                    directionIcon.sprite = signalIcon.noneInput;
                    break;
            }
        }
        
        private Direction RemapDirectionForDisplay(Direction logicalDirection, bool isFacingRight)
        {
            if (isFacingRight)
            {
                // If facing right, Front is Right, Back is Left. No change needed.
                return logicalDirection;
            }

            // If facing left, the logical 'Front' and 'Back' need to be swapped for display.
            Direction displayDirection = logicalDirection;
            if (logicalDirection.HasFlag(Direction.Front))
            {
                displayDirection &= ~Direction.Front;
                displayDirection |= Direction.Back;
            }
            else if (logicalDirection.HasFlag(Direction.Back))
            {
                displayDirection &= ~Direction.Back;
                displayDirection |= Direction.Front;
            }
            return displayDirection;
        }

        private void SetAttackIcons(Attack attack)
        {
            lp.SetActive(attack.HasFlag(Attack.LightPunch));
            mp.SetActive(attack.HasFlag(Attack.MediumPunch));
            hp.SetActive(attack.HasFlag(Attack.HeavyPunch));
            lk.SetActive(attack.HasFlag(Attack.LightKick));
            mk.SetActive(attack.HasFlag(Attack.MediumKick));
            hk.SetActive(attack.HasFlag(Attack.HeavyKick));
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

        private void SetDurationText(int duration)
        {
            durationText.text = duration >= 1 ? duration.ToString() : "";
        }

    }
}
