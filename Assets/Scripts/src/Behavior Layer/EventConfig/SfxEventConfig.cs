using UnityEngine;

namespace src.Behavior_Layer.EventConfig
{
    public class SfxEventConfig : EventConfigBase
    {
        public string soundId;           // 音效资源的唯一标识符或路径
        public AudioClip clip;  // 音效资源引用
        public float volume = 1f;       // 播放音量
        public bool loop = false;        // 是否循环播放 (若循环，durationFrames可用于控制停止时机)
        public string attachmentPointName; // 声音源跟随的角色骨骼挂点名称 (可选)
        // public float pitch = 1f;        // 音高 (可选)
        
    }
}