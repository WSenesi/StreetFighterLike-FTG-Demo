using Mirror;
using UnityEngine;

namespace src.Network
{
    /// <summary>
    /// 自定义玩家生成和断开连接时的逻辑
    /// </summary>
    public class CustomNetworkManager : NetworkManager
    {
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // --- 自定义角色生成位置逻辑 ---
            Transform startPos = GetStartPosition();
            
            // 如果有可用出生点, 则使用该出生点生成玩家; 否则在场景原点生成
            GameObject player = startPos is not null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);
            
            // 将生成的玩家对象与客户端连接关联起来
            NetworkServer.AddPlayerForConnection(conn, player);
            
            // --- 将角色注册到会话管理器 ---
            var newCharacter = conn.identity?.GetComponent<Character.Character>();
            if (newCharacter is not null && GameSessionManager.Instance is not null)
            {
                GameSessionManager.Instance.AddPlayer(newCharacter);
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            var characterToRemove = conn.identity?.GetComponent<Character.Character>();
            if (characterToRemove is not null && GameSessionManager.Instance is not null)
            {
                GameSessionManager.Instance.RemovePlayer(characterToRemove);
            }
            
            base.OnServerDisconnect(conn);
        }
    }
}