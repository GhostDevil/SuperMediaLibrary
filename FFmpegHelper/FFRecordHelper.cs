using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SuperMedia.FFmpegHelper
{
    public class FFRecordHelper
    {
        #region 模拟控制台信号需要使用的api

        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        #endregion

        // ffmpeg进程
        static Process p = new Process();

        // ffmpeg.exe实体文件路径
        static string ffmpegPath = AppDomain.CurrentDomain.BaseDirectory + "ffmpeg\\ffmpeg.exe";

        ///// <summary>
        ///// 获取声音输入设备列表
        ///// </summary>
        ///// <returns>声音输入设备列表</returns>
        //public static CaptureDevicesCollection GetAudioList()
        //{
        //    CaptureDevicesCollection collection = new CaptureDevicesCollection();

        //    return collection;
        //}

        /// <summary>
        /// 开始声音录制
        /// </summary>
        /// <param name="audioDevice">音频捕获设备</param>
        /// <param name="outFilePath">文件输出地址</param>
        public static void Start(string audioDevice, string outFilePath)
        {
            if (File.Exists(outFilePath))
            {
                File.Delete(outFilePath);
            }

            /*转码，视频录制设备：gdigrab；录制对象：桌面；
             * 音频录制方式：dshow；
             * 视频编码格式：h.264；*/
            ProcessStartInfo startInfo = new ProcessStartInfo(ffmpegPath);
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.Arguments = "-f gdigrab -framerate 15 -i desktop -f dshow -i audio=\"" + audioDevice + "\" -vcodec libx264 -preset:v ultrafast -tune:v zerolatency -acodec libmp3lame \"" + outFilePath + "\"";

            p.StartInfo = startInfo;

            p.Start();
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public static void Stop()
        {
            AttachConsole(p.Id);
            SetConsoleCtrlHandler(IntPtr.Zero, true);
            GenerateConsoleCtrlEvent(0, 0);
            FreeConsole();
        }
    }
}
