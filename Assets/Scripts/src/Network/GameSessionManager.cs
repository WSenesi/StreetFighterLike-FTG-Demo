using System.Collections.Generic;
using System.Linq;
using Mirror;
using src.Input_Layer;
using src.UI;
using UnityEngine;

namespace src.Network
{
    public struct PlayerInputForTick
    {
        public uint playerNetId;
        public NetworkInputData inputData;
    }
    public class GameSessionManager : NetworkBehaviour
    {
        // 主机端单例
        public static GameSessionManager Instance { get; private set; }
    
        // 存储当前会话中所有玩家的 Character 实例
        private readonly List<Character.Character> players = new ();

        // 保留多少历史帧输入，这个值需要大于预期的最大网络延迟+处理延迟所对应的帧数，以确保正在处理的帧不会被以外清除
        [Tooltip("决定服务器保留多少帧的输入历史记录")] 
        [SerializeField] private uint _historyTicksToKeep = 200;
        // --- 输入缓冲 ---
        private readonly SortedDictionary<uint, List<PlayerInputForTick>> _tickInputBuffer = new ();
        
        // 服务器的逻辑帧计数器
        private uint _serverTick = 0;

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (Instance is null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning($"Multiple GameSessionManager instances detected. Destroying duplicate.");
                Destroy(this);
            }
        }

        private void FixedUpdate()
        {
            if (!isServer) return;
            _serverTick++;
            
            // ProcessTick();

            CleanupOldInputs();
        }

        // 当一个网络化的玩家被添加到服务器，有 CustomNetworkManager 调用
        public void AddPlayer(Character.Character newPlayer)
        {
            // 仅在主机端执行
            if (!isServer) return;

            if (!players.Contains(newPlayer))
            {
                players.Add(newPlayer);
                Debug.Log($"Player {newPlayer.netId} added. Total players: {players.Count}");

                if (players.Count == 2)
                {
                    AssignOpponents();
                }
            }
        }
    
        // 当一个玩家断开连接, 由 CustomNetworkManager 调用
        public void RemovePlayer(Character.Character playerToRemove)
        {
            if (!isServer) return;

            if (players.Contains(playerToRemove))
            {
                players.Remove(playerToRemove);
                Debug.Log($"Player {playerToRemove.netId} removed. Total players: {players.Count}");
                // TODO: Handle game over logic if a player disconnects mid-game.
            }
        }

        private void AssignOpponents()
        {
            if (players.Count != 2) return;
        
            var player1 = players[0];
            var player2 = players[1];
            Debug.Log($"Assigning opponents: P1 = {player1.netId}, P2 = {player2.netId}");
        
            // 主机端权威设置, 并初始化主机上的角色逻辑层
            player1.opponent = player2.transform;
            player2.opponent = player1.transform;
            player1.InitializeForGameplay();
            player2.InitializeForGameplay();
        
            // --- 同步给客户端 ---
            // 不在此处直接赋值, 而是通知客户端它的对手是谁, 自行获取
            player1.opponentNetId = player2.netId;
            player2.opponentNetId = player1.netId;
        
            // TODO: 可考虑添加 - Rpc通知客户端服务器已完成准备
        }

        /// <summary>
        /// （废弃）使用 Rpc 绑定角色UI引用
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        [ClientRpc]
        private void RpcInitializeUI(Character.Character player1, Character.Character player2)
        {
            // 这个方法将在所有客户端上执行
            if (UIManager.Instance is not null)
            {
                // 因为UIManager是场景中的单例，所有客户端都能找到它
                UIManager.Instance.InitializeUIForPlayers(player1, player2);
            }
            else
            {
                Debug.LogError("UIManager instance not found on client!");
            }
        }

        /// <summary>
        /// （仅在服务器端调用）接收由 Character 的 Command 转发来的玩家输入，并将其存储缓冲区
        /// </summary>
        /// <param name="playerNetId">提交输入的玩家网络ID</param>
        /// <param name="input">提交的输入数据</param>
        public void ReceivePlayerInput(uint playerNetId, NetworkInputData input)
        {
            if (!isServer) return;
            
            var tick = input.tick;
            
            // 如果字典中没有这个tick的记录
            if (!_tickInputBuffer.ContainsKey(tick))
            {
                _tickInputBuffer.Add(tick, new List<PlayerInputForTick>());
            }
            // 创建新的输入记录
            var inputRecord = new PlayerInputForTick()
            {
                playerNetId = playerNetId,
                inputData = input,
            };
            
            _tickInputBuffer[tick].Add(inputRecord);
            
            Debug.Log($"Host Buffered input for Player {playerNetId} at Client Tick: {tick}." +
                      $"Dir: {input.dirInput}, Atk: {input.atkInput}");
        }

        /// <summary>
        /// 清理过于陈旧的输入缓存，防止内存占用无限增长
        /// </summary>
        private void CleanupOldInputs()
        {
            // 服务器Tick还未超过预期最大网络延迟+处理延迟，或缓冲区为空，则无需清理
            if (_serverTick < _historyTicksToKeep || _tickInputBuffer.Count == 0) return;
            
            // 要保留的数据中最早的 Tick 号
            var oldestAllowedTick = _serverTick - _historyTicksToKeep;
            // 找到所有需要移除的过期缓存
            var keysToRemove = _tickInputBuffer.Keys.Where(key => key < oldestAllowedTick).ToList();
            if (keysToRemove.Any())
            {
                foreach (var key in keysToRemove)
                {
                    _tickInputBuffer.Remove(key);
                }
                Debug.Log($"Cleaned up {keysToRemove.Count} input ticks older than {oldestAllowedTick}." +
                          $"Buffer size is {_tickInputBuffer.Count} now.");
            }
        }
    }
}
