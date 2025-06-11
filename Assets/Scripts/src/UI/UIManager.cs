using TMPro;
using UnityEngine;

namespace src.UI
{
    /// <summary>
    /// 负责持有所有 UI 元素的引用, 并提供方法来将他们与游戏数据绑定
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
    
        [Header("Player 1 UI")]
        public TextMeshProUGUI p1NameText;
        public ProgressBarPro p1HealthBar;
        public RecordUIManager p1Record;
    
        [Header("Player 2 UI")]
        public TextMeshProUGUI p2NameText;
        public ProgressBarPro p2HealthBar;
        public RecordUIManager p2Record;
    
        [Header("Global UI")]
        public TextMeshProUGUI timerText;

        private Character.Character _player1, _player2;
        
        private void Awake()
        {
            if (Instance is null) Instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            Character.Character.OnCharacterInitialized += HandleCharacterInitialized;
        }

        private void OnDestroy()
        {
            Character.Character.OnCharacterInitialized -= HandleCharacterInitialized;
        }

        /// <summary>
        /// （废弃）有 GameSessionManager 在主机调用,并通过 Rpc 广播给所有客户端
        /// </summary>
        /// <param name="playerA"></param>
        /// <param name="playerB"></param>
        public void InitializeUIForPlayers(Character.Character playerA, Character.Character playerB)
        {
            var p1 = playerA.transform.position.x < playerB.transform.position.x ? playerA : playerB;
            var p2 = playerA.transform.position.x > playerB.transform.position.x ? playerA : playerB;
         
            // 绑定 P1 UI
            if (p1HealthBar is not null)
            {
                p1HealthBar.SetValue(p1.netHealth, p1.fullHealth, true);
                p1.healthBar = p1HealthBar;
            }
            if (p1NameText is not null) p1NameText.text = "Player 1";
            Debug.Log($"UIManager is initializing P1_RecordUI for char {p1.netId}. Checking its inputLayer: {(p1.inputLayer == null ? "NULL!" : "Exists.")}");
            if (p1Record is not null)
            {
                // 根据当前客户端视角决定开启哪个 输入记录
                if (p1.isLocalPlayer)
                {
                    p1Record.enabled = true;
                    p1Record.Initialize(p1);
                }
                else
                {
                    p1Record.enabled = false;
                }
            }
            // 绑定 P2 UI
            if (p2HealthBar is not null)
            {
                p2HealthBar.SetValue(p2.netHealth, p2.fullHealth, true);
                p2.healthBar = p2HealthBar;
            }
            if (p2NameText is not null) p2NameText.text = "Player 2";
            Debug.Log($"UIManager is initializing P2_RecordUI for char {p2.netId}. Checking its inputLayer: {(p2.inputLayer == null ? "NULL!" : "Exists.")}");
            if (p2Record is not null)
            {
                if (p2.isLocalPlayer)
                {
                    p2Record.enabled = true;
                    p2Record.Initialize(p2);
                }
                else
                {
                    p2Record.enabled = false;
                }
            }
        }

        /// <summary>
        /// 更新计时器
        /// </summary>
        /// <param name="timeRemaining"></param>
        public void UpdateTimerText(float timeRemaining)
        {
            if (timerText is null) return;
            timerText.text = Mathf.Ceil(timeRemaining).ToString();
        }
        
        /// <summary>
        /// 事件处理方法，记录注册的玩家，满员后初始化 UI
        /// </summary>
        /// <param name="character"></param>
        private void HandleCharacterInitialized(Character.Character character)
        {
            Debug.Log($"UI Manager received OnCharacterInitialized event from Character: {character.netId}");

            if (_player1 is null && _player2 is null)
            {
                _player1 = character;
            }
            else
            {
                if (_player1 is not null && _player2 is null)
                {
                    _player2 = character;
                }

                if (_player1 is not null && _player2 is not null)
                {
                    var p1Final = _player1.transform.position.x < _player2.transform.position.x ? _player1 : _player2;
                    var p2Final = _player2.transform.position.x < _player1.transform.position.x ? _player2 : _player1;
                    
                    InitializeAllPlayerUI(p1Final, p2Final);
                }
            }
        }

        // 由事件处理方法调用
        private void InitializeAllPlayerUI(Character.Character p1, Character.Character p2)
        {
            Debug.Log("UI Manager: Both characters are ready. Initializing all UI.");

            if (p1HealthBar is not null)
            {
                p1HealthBar.SetValue(p1.netHealth, p1.fullHealth, true);
                p1.healthBar = p1HealthBar;
            }
            if (p1NameText is not null) p1NameText.text = "Player 1";
            if (p1Record is not null)
            {
                // p1Record.enabled = true;
                p1Record.Initialize(p1);
            }

            if (p2HealthBar is not null)
            {
                p2HealthBar.SetValue(p2.netHealth, p2.fullHealth, true);
                p2.healthBar = p2HealthBar;
            }
            if (p2NameText is not null) p2NameText.text = "Player 2";
            if (p2Record is not null)
            {
                // p2Record.enabled = true;
                p2Record.Initialize(p2);
            }
            
        }
    }
}
