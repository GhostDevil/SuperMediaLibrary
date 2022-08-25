using System;

namespace SuperMedia.FFmpegHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class SuperFFPlayerMode
    {
        #region 委托

        /// <summary>
        /// 播放状态改变委托
        /// </summary>
        /// <param name="playState"></param>
        /// <param name="msg"></param>
        public delegate void StateChange(PlayState playState, string msg);
        /// <summary>
        /// 播放进度改变委托
        /// </summary>
        /// <param name="playState"></param>
        /// <param name="position"></param>
        public delegate void ProgressChange(PlayState playState, TimeSpan position);
        ///// <summary>
        ///// 播放进度改变委托
        ///// </summary>
        ///// <param name="playState"></param>
        ///// <param name="position"></param>
        //public delegate void ProgressSeekChange(PlayState playState, TimeSpan position);
        /// <summary>
        /// 音量改变委托
        /// </summary>
        /// <param name="volState"></param>
        /// <param name="position"></param>
        public delegate void VolumeSeekChange(VolumeStae volState, double position);
        /// <summary>
        /// 输出流事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="playState"></param>
        /// <param name="data"></param>
        public delegate void DataReceivedHandler(object obj, PlayState playState, string data);

        /// <summary>
        /// 操作动作事件
        /// </summary>
        /// <param name="appEvent"></param>
        /// <param name="value"></param>
        public delegate void AppEventHandler(AppEvent appEvent, string value);
        #endregion

        #region 播放器exe操作类型
        /// <summary>
        /// 播放器操作
        /// </summary>
        public enum AppEvent
        {
            /// <summary>
            /// 鼠标进入窗口内
            /// </summary>
            Mouse_in = 200,
            /// <summary>
            /// 左键按下
            /// </summary>
            Mouse_LeftDown = 201,
            /// <summary>
            /// 左键双击
            /// </summary>
            Mouse_LeftDownTwo = 203,
            /// <summary>
            /// 右键按下
            /// </summary>
            Mouse_RightDown = 204,
            /// <summary>
            /// 鼠标离开窗口外
            /// </summary>
            Mouse_Leave = 210,

            /// <summary>
            /// 光标显示
            /// </summary>
            Cursor_Show = 101,
            /// <summary>
            /// 光标隐藏
            /// </summary>
            Cursor_Hide = 100,

            /// <summary> 
            /// 播放操作 播放 暂停 停止
            /// </summary>
            Play_State = 300,
            /// <summary>
            /// 按下了esc或者q，程序要退出
            /// </summary>
            Play_Close = 303,

            /// <summary>
            /// 音量操作M  开启声音0 开启静音1
            /// </summary>
            Mute_Switch = 500,
            /// <summary>
            /// 声音加
            /// </summary>
            Mute_Up = 503,
            /// <summary>
            /// 声音减
            /// </summary>
            Mute_Down = 504,

            /// <summary>
            /// 屏幕操作F 全屏 初始大小
            /// </summary>            
            Screen_Switch = 600,

            /// <summary>
            /// 显示模式切换W 视频 音频波形 音频频带
            /// </summary>
            Video_Mode = 400,
            /// <summary>
            /// 逐帧S
            /// </summary>
            Video_Frame = 404
        }
       
        /// <summary>
        /// 播放状态
        /// </summary>
        public enum PlayState
        {
            Null,
            PlayingSelect,
            Select,
            Open,
            Load,
            Ready,
            Playing,
            Pasue,
            //Stop,
            End,
            MuteTrue,
            MuteFalse
        }
        public enum VolumeStae
        {
            Up,
            Down

        }
        #endregion

        #region 配置
        /// <summary>
        /// 播放参数
        /// </summary>
        [Serializable]
        public class PlayInfo
        {
            /// <summary>
            /// 播放地址
            /// </summary>
            public string Url { get; set; }
            /// <summary>
            /// 播放位置
            /// </summary>
            public TimeSpan Position { get; set; } = new TimeSpan(0);
            /// <summary>
            /// 视频宽度
            /// </summary>
            public int VideoWidth { get; set; } = 1;
            /// <summary>
            /// 视频高度
            /// </summary>
            public int VideoHeight { get; set; } = 1;
            /// <summary>
            ///  是否直播流
            /// </summary>
            public bool IsLiveStream { get; set; } = false;
            /// <summary>
            ///  是否自动开始
            /// </summary>
            public bool IsAutoPlay { get; set; } = false;
            /// <summary>
            ///  是否静音
            /// </summary>
            public bool IsMute { get; set; } = false;

        }
        /// <summary>
        /// 播放配置
        /// </summary>
        public class PlayConfig
        {
            /// <summary>
            /// 是否异步回调处理
            /// </summary>
            public bool IsAsync { get; set; } = true;
            /// <summary>
            /// 线程速度
            /// </summary>
            public int ThreadTike { get; set; } = 23;

            /// <summary>
            /// 是否使用回调事件
            /// </summary>
            public bool IsUseEvent { get; set; } = true;
            /// <summary>
            /// 播放库目录
            /// </summary>
            public string DllDirectory { get; set; } = "";
            /// <summary>
            /// 程序工作目录
            /// </summary>
            public string WorkDirectory { get; set; } = "";
        }
        #endregion
    }
}
