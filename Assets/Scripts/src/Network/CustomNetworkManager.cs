using Mirror;
using UnityEngine;

namespace src.Network
{
    /// <summary>
    /// 自定义玩家生成和断开连接时的逻辑
    /// </summary>
    public class CustomNetworkManager : NetworkManager
    {
        public Transform leftSpawnPoint;
        public Transform rightSpawnPoint;
        
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // --- 自定义角色生成位置逻辑 ---
            Transform startPos = numPlayers == 0 ? leftSpawnPoint : rightSpawnPoint;
            if (startPos is null)
            {
                Debug.LogError($"A spawn point is not set in NetworkManager. Player will spawn at default position.");
                startPos = this.transform;
            }
            
            // 实例化玩家预制体
            GameObject player = Instantiate(playerPrefab, startPos.position, startPos.rotation);
            
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