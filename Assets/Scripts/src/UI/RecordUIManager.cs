using System;
using System.Collections.Generic;
using src.Input_Layer;
using UnityEngine;

namespace src.UI
{
    public class RecordUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject recordPrefab;
        [SerializeField] private Transform player;
        [SerializeField] private Transform opponent;
        [Range(1, 99)] public int capacity = 20;
        public Character character;
        private InputLayer inputLayer;
        private InputBuffer<DirectionSignal> directionInput;
        private InputBuffer<AttackSignal> attackInput;
        private List<InputRecordUI> m_recordList;

        private DirectionSignal m_lastDir;
        private AttackSignal m_lastAtk;
        private int m_signalDuration;
        private int m_activeCount;
        [SerializeField] private bool m_isInLeft = true;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }
        private void Start()
        {
            inputLayer = character.inputLayer;
            if (inputLayer is null)
                throw new Exception("输入层为空");
            directionInput = inputLayer.directionInput;
            attackInput = inputLayer.attackInput;
            //Debug.Log($"Direction: {directionInput}\tAttack: {attackInput}");
            m_lastDir = null;
            m_lastAtk = null;
            m_signalDuration = 0;
            m_activeCount = 0;
            InitList();
        }

        private void Update()
        {
            m_isInLeft = character.context.isFacingRight;
            UpdateContent();
        }

        private void InitList()
        {
            if (capacity < 1)
                throw new Exception("Capacity 参数错误");

            m_recordList = new List<InputRecordUI>();
            for (int i = 0; i < capacity; i++)
            {
                GameObject recordObject = Instantiate(recordPrefab, transform);
                recordObject.name = $"Input Record {i + 1}";
                InputRecordUI record = recordObject.GetComponent<InputRecordUI>();
                // Debug.Log(record);
                // m_recordList[i] = record;
                m_recordList.Add(record);
                recordObject.SetActive(false);
            }
        }

        private void UpdateContent()
        {
            //  获取最新的输入信号，如果为空直接退出
            var direction = directionInput.Read(0);
            var attack = attackInput.Read(0);
            if (direction is null && attack is null)
                return;

            // 如果两个信号与上一帧完全相同，更改信号的持续时间
            if (m_lastDir is not null && m_lastAtk is not null
                                      && direction.direction == m_lastDir.direction && attack.attack == m_lastAtk.attack)
            {
                m_signalDuration = m_signalDuration < 99 ? m_signalDuration + 1 : 99;
                // m_signalDuration = direction.duration;
                m_recordList[0].SetDurationText(m_signalDuration);
            }

            // 如果 Record 还没全部启用，唤醒一个Record, 将原先的显示后移一位
            else
            {
                // 唤醒被禁用的, 索引最小的 Record
                if (m_activeCount < capacity)
                {
                    m_recordList[m_activeCount].gameObject.SetActive(true);
                    m_activeCount += 1;
                }
                // 后移 Record 数据
                if (m_activeCount > 1)
                {
                    for (int i = m_activeCount - 1; i > 0; i--)
                    {
                        m_recordList[i].Copy(m_recordList[i - 1]);
                    }
                }
                // 对第一个 Record 赋值最新数据
                m_signalDuration = 1;
                m_recordList[0].SetDurationText(m_signalDuration);
                SetDirectionIcon(m_recordList[0], direction);
                SetAttackIcon(m_recordList[0], attack);
            }

            // 记录当前帧的输入, 和下一帧比较
            m_lastDir = direction;
            m_lastAtk = attack;

        }
        private void SetDirectionIcon(InputRecordUI recordUI, DirectionSignal direction)
        {
            switch (direction.direction)
            {
                case Direction.Front:
                    recordUI.SetDirectionImage(m_isInLeft ? "Right" : "Left");
                    break;
                case Direction.Front | Direction.Down:
                    recordUI.SetDirectionImage(m_isInLeft ? "Right Down" : "Left Down");
                    break;
                case Direction.Back:
                    recordUI.SetDirectionImage(m_isInLeft ? "Left" : "Right");
                    break;
                case Direction.Back | Direction.Down:
                    recordUI.SetDirectionImage(m_isInLeft ? "Left Down" : "Right Down");
                    break;
                case Direction.Down:
                    recordUI.SetDirectionImage("Down");
                    break;
                case Direction.None:
                    recordUI.SetDirectionImage("None");
                    break;
                case Direction.Front | Direction.Up:
                    recordUI.SetDirectionImage(m_isInLeft ? "Right Up" : "Left Up");
                    break;
                case Direction.Back | Direction.Up:
                    recordUI.SetDirectionImage(m_isInLeft ? "Left Up" : "Right Up");
                    break;
                case Direction.Up:
                    recordUI.SetDirectionImage("Up");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void SetAttackIcon(InputRecordUI recordUI, AttackSignal attack)
        {
            recordUI.SetAttackIcon("LP", attack.Contains(Attack.LightPunch));

            recordUI.SetAttackIcon("MP", attack.Contains(Attack.MediumPunch));

            recordUI.SetAttackIcon("HP", attack.Contains(Attack.HeavyPunch));

            recordUI.SetAttackIcon("LK", attack.Contains(Attack.LightKick));

            recordUI.SetAttackIcon("MK", attack.Contains(Attack.MediumKick));

            recordUI.SetAttackIcon("HK", attack.Contains(Attack.HeavyKick));
        }
        private bool IsPlayerInLeft()
        {
            return player.position.x < opponent.position.x;
        }

    }
}
