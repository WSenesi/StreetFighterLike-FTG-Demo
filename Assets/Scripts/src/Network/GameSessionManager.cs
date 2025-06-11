using System.Collections.Generic;
using Mirror;
using src.UI;
using UnityEngine;

namespace src.Network
{
    public class GameSessionManager : NetworkBehaviour
    {
        // 主机端单例
        public static GameSessionManager Instance { get; private set; }
    
        // 存储当前会话中所有玩家的 Character 实例
        private readonly List<Character.Character> players = new List<Character.Character>();

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
        
    }
}
