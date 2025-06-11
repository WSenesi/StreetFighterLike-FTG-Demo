using System.Collections.Generic;
using src.Input_Layer;
using UnityEngine;

namespace src.UI
{
    public class RecordUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject recordPrefab;
        [Range(1, 99)] public int capacity = 20;
        
        [SerializeField] private Character.Character character;
        private InputLayer _inputLayer;
        private ContextData _context;
        
        // The pool of UI elements, created once at the start.
        private readonly List<InputRecordUI> _recordUiPool = new List<InputRecordUI>();
        
        // The actual data for the input history. Using LinkedList for efficient addition/removal from the front.
        private readonly LinkedList<InputRecordData> _inputHistory = new LinkedList<InputRecordData>();
        
        // [SerializeField] private Transform player;
        // [SerializeField] private Transform opponent;
        //
        // private InputBuffer<DirectionSignal> _directionInput;
        // private InputBuffer<AttackSignal> _attackInput;
        // private List<InputRecordUI> _recordList;
        //
        // private DirectionSignal _lastDir;
        // private AttackSignal _lastAtk;
        // private int _signalDuration;
        // private int _activeCount;
        // [SerializeField] private bool isInLeft = true;

        private bool isInitialized = false;

        private void Awake()
        {
            // Pre-instantiate the UI objects to create a pool.
            for (int i = 0; i < capacity; i++)
            {
                GameObject recordObject = Instantiate(recordPrefab, transform);
                recordObject.name = $"Input Record {i}";
                InputRecordUI record = recordObject.GetComponent<InputRecordUI>();
                record.Clear(); // Start in a clean, disabled state.
                _recordUiPool.Add(record);
            }
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            ProcessInput();
            RenderInputHistory();
        }

        public void Initialize(Character.Character characterToTrack)
        {
            if (characterToTrack?.inputLayer is null || characterToTrack.context is null)
            {
                Debug.LogError("RecordUIManager: Initialization failed. Provided character or its modules are null.");
                this.enabled = false;
                return;
            }            
            Debug.Log($"Attempting to initialize RecordUIManager for character {characterToTrack.netId}. " +
                      $"InputLayer is {(characterToTrack.inputLayer == null ? "NULL" : "OK")}");
            character = characterToTrack;
            _inputLayer = character.inputLayer;
            _context = character.context;
            
            _inputHistory.Clear();
            isInitialized = true;
        }
        
        /// <summary>
        /// Processes the latest input and updates the _inputHistory data list.
        /// </summary>
        private void ProcessInput()
        {
            if (_inputLayer?.directionInput is null || _inputLayer.attackInput is null) return;
            
            DirectionSignal currentDirSignal = _inputLayer.directionInput.Read(0);
            AttackSignal currentAtkSignal = _inputLayer.attackInput.Read(0);

            if (currentDirSignal == null || currentAtkSignal == null) return;

            LinkedListNode<InputRecordData> firstNode = _inputHistory.First;

            // Check if the new input is the same as the last recorded input.
            if (firstNode != null && 
                firstNode.Value.Direction == currentDirSignal.direction && 
                firstNode.Value.Attack == currentAtkSignal.attack)
            {
                // If it is, just increment the duration.
                var duration = firstNode.Value.Duration;
                firstNode.Value.Duration = duration >= 99 ? duration : duration + 1;
            }
            else
            {
                // If it's a new input, add a new entry to the front of the list.
                _inputHistory.AddFirst(new InputRecordData(currentDirSignal.direction, currentAtkSignal.attack, 1));

                // If the list exceeds capacity, remove the oldest entry.
                if (_inputHistory.Count > capacity)
                {
                    _inputHistory.RemoveLast();
                }
            }
        }

        /// <summary>
        /// Renders the current state of _inputHistory to the pooled UI elements.
        /// </summary>
        private void RenderInputHistory()
        {
            int index = 0;
            bool isFacingRight = _context?.isFacingRight ?? true;

            // Update active UI elements based on the data
            foreach (var recordData in _inputHistory)
            {
                if (index < _recordUiPool.Count)
                {
                    InputRecordUI uiElement = _recordUiPool[index];
                    uiElement.gameObject.SetActive(true);
                    uiElement.UpdateDisplay(recordData, isFacingRight);
                    index++;
                }
                else
                {
                    // Should not happen if capacity is managed correctly, but good for safety.
                    break; 
                }
            }

            // Deactivate any remaining pooled UI elements that are not needed.
            for (int i = index; i < _recordUiPool.Count; i++)
            {
                if(_recordUiPool[i].gameObject.activeSelf)
                    _recordUiPool[i].gameObject.SetActive(false);
            }
        }
        
        // private void InitList()
        // {
        //     _recordList = new List<InputRecordUI>();
        //     for (int i = 0; i < capacity; i++)
        //     {
        //         GameObject recordObject = Instantiate(recordPrefab, transform);
        //         recordObject.name = $"Input Record {i + 1}";
        //         InputRecordUI record = recordObject.GetComponent<InputRecordUI>();
        //         _recordList.Add(record);
        //         recordObject.SetActive(false);
        //     }
        // }
        //
        // private void UpdateContent()
        // {
        //     if (_directionInput is null || _attackInput is null) return;
        //     
        //     //  获取最新的输入信号，如果为空直接退出
        //     var direction = _directionInput.Read(0);
        //     var attack = _attackInput.Read(0);
        //     if (direction is null && attack is null)
        //         return;
        //
        //     // 如果两个信号与上一帧完全相同，更改信号的持续时间
        //     if (_lastDir is not null && _lastAtk is not null
        //                               && direction.direction == _lastDir.direction && attack.attack == _lastAtk.attack)
        //     {
        //         _signalDuration = _signalDuration < 99 ? _signalDuration + 1 : 99;
        //         // m_signalDuration = direction.duration;
        //         _recordList[0].SetDurationText(_signalDuration);
        //     }
        //
        //     // 如果 Record 还没全部启用，唤醒一个Record, 将原先的显示后移一位
        //     else
        //     {
        //         // 唤醒被禁用的, 索引最小的 Record
        //         if (_activeCount < capacity)
        //         {
        //             _recordList[_activeCount].gameObject.SetActive(true);
        //             _activeCount += 1;
        //         }
        //         // 后移 Record 数据
        //         if (_activeCount > 1)
        //         {
        //             for (int i = _activeCount - 1; i > 0; i--)
        //             {
        //                 _recordList[i].Copy(_recordList[i - 1]);
        //             }
        //         }
        //         // 对第一个 Record 赋值最新数据
        //         _signalDuration = 1;
        //         _recordList[0].SetDurationText(_signalDuration);
        //         SetDirectionIcon(_recordList[0], direction);
        //         SetAttackIcon(_recordList[0], attack);
        //     }
        //
        //     // 记录当前帧的输入, 和下一帧比较
        //     _lastDir = direction;
        //     _lastAtk = attack;
        //
        // }
        // private void SetDirectionIcon(InputRecordUI recordUI, DirectionSignal direction)
        // {
        //     switch (direction.direction)
        //     {
        //         case Direction.Front:
        //             recordUI.SetDirectionImage(isInLeft ? "Right" : "Left");
        //             break;
        //         case Direction.Front | Direction.Down:
        //             recordUI.SetDirectionImage(isInLeft ? "Right Down" : "Left Down");
        //             break;
        //         case Direction.Back:
        //             recordUI.SetDirectionImage(isInLeft ? "Left" : "Right");
        //             break;
        //         case Direction.Back | Direction.Down:
        //             recordUI.SetDirectionImage(isInLeft ? "Left Down" : "Right Down");
        //             break;
        //         case Direction.Down:
        //             recordUI.SetDirectionImage("Down");
        //             break;
        //         case Direction.None:
        //             recordUI.SetDirectionImage("None");
        //             break;
        //         case Direction.Front | Direction.Up:
        //             recordUI.SetDirectionImage(isInLeft ? "Right Up" : "Left Up");
        //             break;
        //         case Direction.Back | Direction.Up:
        //             recordUI.SetDirectionImage(isInLeft ? "Left Up" : "Right Up");
        //             break;
        //         case Direction.Up:
        //             recordUI.SetDirectionImage("Up");
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // }
        // private void SetAttackIcon(InputRecordUI recordUI, AttackSignal attack)
        // {
        //     recordUI.SetAttackIcon("LP", attack.Contains(Attack.LightPunch));
        //
        //     recordUI.SetAttackIcon("MP", attack.Contains(Attack.MediumPunch));
        //
        //     recordUI.SetAttackIcon("HP", attack.Contains(Attack.HeavyPunch));
        //
        //     recordUI.SetAttackIcon("LK", attack.Contains(Attack.LightKick));
        //
        //     recordUI.SetAttackIcon("MK", attack.Contains(Attack.MediumKick));
        //
        //     recordUI.SetAttackIcon("HK", attack.Contains(Attack.HeavyKick));
        // }
        // private bool IsPlayerInLeft()
        // {
        //     return player.position.x < opponent.position.x;
        // }

    }
}
