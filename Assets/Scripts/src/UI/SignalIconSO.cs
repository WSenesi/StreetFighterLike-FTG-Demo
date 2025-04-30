using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace src.UI
{
    [InlineEditor]
    [CreateAssetMenu(fileName = "InputSignalIcon", menuName = "UI/Signal Icon")]
    public class SignalIconSO : SerializedScriptableObject
    {
        #region 方向信号Icon配置
        //[TextArea] public const string describe = "根据 摇杆/小键盘 的方向, 配置各个方向输入信号的 Icon";

        [HorizontalGroup("Direction", Title = "Direction")]
        [VerticalGroup("Direction/column 1"), PreviewField(50)]
        public Sprite leftUp;
        [VerticalGroup("Direction/column 2"), PreviewField(50)]
        public Sprite up;
        [VerticalGroup("Direction/column 3"), PreviewField(50)]
        public Sprite rightUp;
        [VerticalGroup("Direction/column 1"), PreviewField(50)]
        public Sprite left;
        [FormerlySerializedAs("NoneInput")] [VerticalGroup("Direction/column 2"), PreviewField(50)]
        public Sprite noneInput;
        [VerticalGroup("Direction/column 3"), PreviewField(50)]
        public Sprite right;
        [VerticalGroup("Direction/column 1"), PreviewField(50)]
        public Sprite leftDown;
        [VerticalGroup("Direction/column 2"), PreviewField(50)]
        public Sprite down;
        [VerticalGroup("Direction/column 3"), PreviewField(50)]
        public Sprite rightDown;
        #endregion

        #region 攻击信号Icon配置
        [HorizontalGroup("Attack", Title = "Attack")]
        [VerticalGroup("Attack/column 1"), PreviewField(50)]
        public Sprite lightPunch;
        [VerticalGroup("Attack/column 1"), PreviewField(50)]
        public Sprite mediumPunch;
        [VerticalGroup("Attack/column 1"), PreviewField(50)]
        public Sprite hardPunch;
        [VerticalGroup("Attack/column 2"), PreviewField(50)]
        public Sprite lightKick;
        [VerticalGroup("Attack/column 2"), PreviewField(50)]
        public Sprite mediumKick;
        [VerticalGroup("Attack/column 2"), PreviewField(50)]
        public Sprite hardKick;
        #endregion

    }
}
