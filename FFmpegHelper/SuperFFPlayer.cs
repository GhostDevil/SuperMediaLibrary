

using SuperFramework.WindowsAPI;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static SuperMedia.FFmpegHelper.SuperFFHelper;
using static SuperMedia.FFmpegHelper.SuperFFPlayerMode;

namespace SuperMedia.FFmpegHelper
{
    /// <summary>
    /// FFPlay的一个子工具，它具有强大的音视频解码播放能力
    /// </summary>
    public class SuperFFPlayer
    {
        #region 变量
        /// <summary>
        /// 播放状态
        /// </summary>
        public PlayState playState { get; private set; } = PlayState.Null;
        /// <summary>
        /// 是否静音
        /// </summary>
        public bool isMute = false;
        /// <summary>
        /// 当前音量
        /// </summary>
        public double currentVolume = 41;
        /// <summary>
        /// 是否具有边框
        /// </summary>
        public bool isShowBorder = true;
        
        /// <summary>
        /// 视频信息
        /// </summary>
        public FFVideoInfo videoInfo = null;
        /// <summary>
        /// 是否在播放
        /// </summary>
        public bool isPlaying = false;
        /// <summary>
        /// 播放配置
        /// </summary>
        public PlayConfig playConfig = null;
        /// <summary>
        /// 播放参数
        /// </summary>
        public PlayInfo playInfo = null;
        /// <summary>
        /// 当前进度
        /// </summary>
        public TimeSpan currentDuration = new TimeSpan(0);
        /// <summary>
        /// 播放进程对象
        /// </summary>
        Process playProcess = null;
        /// <summary>
        /// 播放地址
        /// </summary>
        string RtmpUrl = "";
        /// <summary>
        /// 播放器句柄
        /// </summary>
        public IntPtr playIntPtr;
        
        #endregion

        #region 事件委托
       
        /// <summary>
        /// 音量改变事件
        /// </summary>
        public event VolumeSeekChange VolumeChange;
        /// <summary>
        /// 播放进度改变事件
        /// </summary>
        public event ProgressChange PlayProgressSeekChange;
        /// <summary>
        /// 播放进度改变事件
        /// </summary>
        public event ProgressChange PlayProgressChange;
        /// <summary>
        /// 播放状态改变事件
        /// </summary>
        public event StateChange PlayStateChange;
        /// <summary>
        /// 操作动作事件
        /// </summary>
        public event AppEventHandler AppEventReceived;
        /// <summary>
        /// 输出内容事件
        /// </summary>
        public event DataReceivedHandler DataErrorReceived;
        /// <summary>
        /// 输出内容事件
        /// </summary>
        public event DataReceivedHandler DataReceived;
        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        public SuperFFPlayer()
        {
            InitProcess();
        }

        #region 方法
        /// <summary>
        /// 设置播放参数
        /// </summary>
        /// <param name="config"></param>
        public void SetConfig(PlayConfig config)
        {
            playConfig = config;
        }
        private void InitProcess()
        {
            if (playProcess == null)
            {
                playProcess = new Process
                {
                    EnableRaisingEvents = true,

                };

                playProcess.OutputDataReceived += (s, _e) =>
                {
                    if (playConfig.IsUseEvent)
                    {
                        DataReceivedEvent(s, _e);
                    }
                    DataReceived?.Invoke(s, playState, _e.Data);
                    Console.WriteLine(_e.Data);
                };
                playProcess.ErrorDataReceived += (s, _e) =>
                {
                    //new Thread(() =>
                    //{
                    if (playConfig.IsUseEvent)
                    {
                        DataErrorReceivedEvent(s, _e);
                    }
                    DataErrorReceived?.Invoke(s, playState, _e.Data);
                    //dataErrorReceivedEventHandler?.Invoke(s, _e);
                    Console.WriteLine(_e.Data);
                    //})
                    //{ IsBackground = true,Priority= ThreadPriority.Highest }.Start();

                };

                playProcess.Exited += async (s, o) =>
                {

                    await Task.Delay(100);
                    Process p1 = s as Process;
                    PlayEnd();
                    //handler?.Invoke(s, o);
                    //p1.Dispose();
                };

            }
        }
        /// <summary>
        /// 加载
        /// </summary>
        ///<param name="playParm">播放参数</param>
        /// <returns>播放进程</returns>
        public Process PlayLoad(PlayInfo playParm)
        {
            if (playParm == null)
                return null;
            isMute = false;
            playInfo = playParm;
            playState = PlayState.Load;
            PlayStateChange?.Invoke(PlayState.Load, "Load");
            //Process p = new Process();
            InitProcess();
            RtmpUrl = playParm.Url;
            //string fileName;
            currentDuration = new TimeSpan(0);
            //string dir = Environment.CurrentDirectory;
            videoInfo = GetVideoInfo(playConfig.DllDirectory + "/ffmpeg.exe", RtmpUrl);
            //if (videoInfo.Duration == new TimeSpan(0))
            //    isUseEvent = false;

            ProcessStartInfo startInfo = new ProcessStartInfo(playConfig.DllDirectory + "/ffplay.exe");//(@"cmd.exe");(/dllDirectory+ "/ffmpeg.exe")

            SetStartInfo(playParm, startInfo);
            //if (playParm.IsAutoPlay)
            //    Play();
            return playProcess;
        }
        /// <summary>
        /// 播放
        /// </summary>
        /// <returns>播放进程</returns>
        public  Process Play()
        {
            if (playInfo == null)
                return null;
            if (playState == PlayState.Load || playState == PlayState.End)
            {
                playState = PlayState.Ready;
                PlayStateChange?.Invoke(PlayState.Ready, "Ready");
                playProcess.Start();
                playProcess.PriorityClass = ProcessPriorityClass.High;


                // p.StandardInput.WriteLine($"ffplay -x {width} -y {height}   -loop 1 {RtmpUrl}");


                new Thread(async () =>
                {
                    //await Task.Delay(1000);

                    //try
                    //{
                    //    while (playProcess == null || playProcess.MainWindowHandle == null || playProcess.MainWindowHandle == IntPtr.Zero)
                    //    {
                    //        await Task.Delay(5);
                    //    }
                    //}
                    //catch
                    //{
                    //    await Task.Delay(1000);
                    //}
                    int x = 0;
                //try
                //{

                //    while (playIntPtr, == null || playProcess.MainWindowHandle == IntPtr.Zero)
                //    {
                //        if (x >= 100)
                //        {
                //            Process[] p = Process.GetProcessesByName("");
                //            if (p.Length > 0)
                //            {
                //                playIntPtr = p[0].MainWindowHandle;
                //                uint xx = GetLastError();
                //                //intPtr = FindWindow(null, "ASTER 控制 v2.26 (Token x64 OFF T1)");
                //                break;
                //            }
                //        }

                //        x++;
                //        await Task.Delay(20);
                //    }

                //    if (x < 100)
                //        playIntPtr = playProcess.MainWindowHandle;
                //    //intPtr = FindWindow(null, "虚拟程序 控制 1.2.22.50.30.7.1.3.20 (Token x64 OFF T1)");
                //}
                //catch
                //{

                //    await Task.Delay(100);
                //    Process[] p = Process.GetProcessesByName(appProcess);
                //    if (p.Length > 0)
                //    {
                //        playIntPtr = p[0].MainWindowHandle;
                //    }
                //    //intPtr = FindWindow(null, "ASTER 控制 v2.26 (Token x64 OFF T1)");//ASTER 控制 v2.26 (Token x64 OFF T1)
                //}

               
                try
                {
                    while (playProcess == null || playProcess.MainWindowHandle == null || playProcess.MainWindowHandle == IntPtr.Zero)
                    {
                        if (x > 100)
                        {
                            playIntPtr = User32API.FindWindow(null, playInfo.Url);
                            break;
                        }
                        x++;
                        await Task.Delay(25);
                    }
                    if (x <= 100)
                        playIntPtr = playProcess.MainWindowHandle;
                }
                catch
                {
                    await Task.Delay(10);
                    playIntPtr = User32API.FindWindow(null, playInfo.Url);
                }
                x = 0;
                isPlaying = true;
                    playState = PlayState.Playing;
                    PlayStateChange?.Invoke(PlayState.Playing, "Playing");

                    if (playInfo.IsMute)
                    {
                        // await Task.Delay(1000);
                        ChangeMute();
                    }

                })
                { IsBackground = true }.Start();

                //if (!isReadData)
                //{
                if (playConfig.IsAsync)
                {
                    playProcess.BeginOutputReadLine();
                    playProcess.BeginErrorReadLine();
                }
                else
                {
                    //isReadData = true;
                    StreamReader readerError = null;
                    readerError = playProcess.StandardError;//截取输出流
                    StreamReader readerInfo = null;
                    readerInfo = playProcess.StandardOutput;//截取输出流
                    new Thread(() =>
                    {
                        GetInfoData(readerInfo);

                    })
                    { IsBackground = true }.Start();
                    new Thread(() =>
                    {
                        GetProgress(readerError);

                    })
                    { IsBackground = true, Priority = ThreadPriority.Highest }.Start();
                }
            }
            return playProcess;
        }
        private async void GetInfoData(StreamReader reader)
        {
            string line = reader.ReadLine();//每次读取一行
            while (!reader.EndOfStream)
            {
                //if (line.Length > 0)
                //{
                DataReceivedEvent(line);
                //}
                await Task.Delay(playConfig.ThreadTike);
                if (playState != PlayState.Playing && playState != PlayState.Pasue)
                { break; }
                line = reader.ReadLine();
            }
        }

        private async void GetProgress(StreamReader reader)
        {
            string line = reader.ReadLine();//每次读取一行
            while (!reader.EndOfStream)
            {
                if (line.Length > 0)
                {
                    //!RtmpUrl.ToUpper().StartsWith("RTMP") && RtmpUrl.ToUpper().StartsWith("RTSP") && RtmpUrl.ToUpper().StartsWith("UDP") && RtmpUrl.ToUpper().StartsWith("TCP")
                    if (playConfig.IsUseEvent)
                    {  //3.08 A-V:  0.024 fd=   7 aq=   56KB vq=   77KB sq=    0B f=0/0   
                        if (line.Contains("fd=") && line.Contains("aq") && line.Contains("vq") && line.Contains("sq"))
                        {
                            if (isPlaying)
                            {
                                GetProgress(line);
                                PlayProgressChange?.Invoke(playState, currentDuration);
                            }

                        }
                        else if (line.Contains("Seek to") && line.Contains("of total duration"))
                        {
                            //SetPace(GetPace(args));
                            GetPace(line);
                            PlayProgressSeekChange?.Invoke(playState, currentDuration);
                            //PlayProgressChange?.Invoke(playState, currentDuration);
                            //new Thread(() => { SetPace(GetPace(line)); }) { IsBackground = true }.Start();
                            //Console.WriteLine(line);
                        }
                    }
                    Console.WriteLine("Error:" + line);
                    DataErrorReceived?.Invoke(playProcess, playState, line);

                }
                //if(isPlaying)
                await Task.Delay(playConfig.ThreadTike);
                //else
                //    await Task.Delay(10);
                if (playState == PlayState.End || playState == PlayState.Null)
                { break; }
                line = reader.ReadLine();
            }
        }

        private void SetStartInfo(PlayInfo playParm, ProcessStartInfo startInfo)
        {
            startInfo.Arguments = $"-analyzeduration 10000  " +

                $"-threads auto " +//自动线程               
                $"-hide_banner " +//不打印文件参数
                $"-x {playParm.VideoWidth} -y {playParm.VideoHeight} " +//size
                $"-loop 1 " +//循环
                $"-framedrop "; //当CPU资 源占用过高时，自动丢帧
            if (!isShowBorder)
                startInfo.Arguments += $"-noborder ";//无边框
                //$"-fflags nobuffer ";
                //if (!isShowBorder)
                //    startInfo.Arguments += $"-noborder ";//无边框
                ;
            if (playInfo.IsLiveStream)
            {
                startInfo.Arguments += $"-infbuf "; //设置无极限的播放器buffer，这个选项常见于实时流媒体播放场景
            }
            if (!playInfo.IsLiveStream)
            {

                startInfo.Arguments += $"-autoexit -ss {playParm.Position:hh\\:mm\\:ss} ";//起始位置秒
            }
            startInfo.Arguments += $"{playParm.Url}"; //文件地址
            //-autoexit -autoexit

            if (string.IsNullOrWhiteSpace(playConfig.DllDirectory))
                startInfo.WorkingDirectory = playConfig.WorkDirectory;
            else
                startInfo.WorkingDirectory = playConfig.DllDirectory;
            startInfo.UseShellExecute = false;//当 UseShellExecute 为 false 时，使用 Process 组件仅能启动可执行文件。
            startInfo.RedirectStandardInput = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true; //UseShellExecute为false时才有效

            playProcess.StartInfo = startInfo;

        }

        void PlayEnd()
        {
            currentDuration = new TimeSpan(0);
            isPlaying = false;
            playState = PlayState.End;
            PlayStateChange?.Invoke(PlayState.End, "End of play");

            PlayClose();
            //playState = PlayState.Null;
        }
        /// <summary>
        /// 关闭播放
        /// </summary>
        public void PlayClose()
        {
            if (playProcess != null)
            {
                try
                {
                    //PlayStateChange = null;
                    if (playProcess.StartInfo != null && !playProcess.HasExited && playIntPtr != null && playIntPtr != IntPtr.Zero)
                        playProcess.CloseMainWindow();
                    playProcess.Close();
                    playProcess = null;
                    currentDuration = new TimeSpan(0);

                }
                catch { }
            }
        }
        #endregion

        #region 事件
        void DataReceivedEvent(object e, DataReceivedEventArgs args)
        {
            if (args.Data == null)
                return;
            //SuperFramework.SuperFile.FileRWHelper.AppendText(@"d:\txt.txt", DateTime.Now + "   " + args.Data + Environment.NewLine);
            //playState = PlayState.End;
            //Console.WriteLine("Data:"+args.Data);
            if (args.Data.Contains("Event:"))
            {
                AppEventHandel(args.Data);
            }
            DataReceived?.Invoke(playProcess, playState, args.Data);
            //dataReceivedEventHandler?.Invoke(e, args);
            //txtHold.Text = $"Playing···{Environment.NewLine + url}";
            //PlayStateChange?.Invoke(PlayState.Playing, txtHold.Text);
        }

        private void AppEventHandel(string data)
        {
            string[] strs = data.Split(':');
            if (strs.Length == 2)
            {
                strs = strs[1].Split(';');
                if (strs.Length == 3)
                {
                    if (Enum.TryParse(strs[0], out AppEvent type))
                    {
                        AppEventReceived?.Invoke(type, strs[1]);
                    }
                }
            }
        }

        void DataReceivedEvent(string e)
        {
            if (e == null)
                return;
            Console.WriteLine("Data:" + e);
            DataReceived?.Invoke(playProcess, playState, e);
        }
        void DataErrorReceivedEvent(object e, DataReceivedEventArgs args)
        {
            if (args.Data == null)
                return;
            //Console.WriteLine("Error:" + args.Data);

            if (args.Data.Contains("fd=") && args.Data.Contains("aq") && args.Data.Contains("vq") && args.Data.Contains("sq"))
            {
                GetProgress(args.Data);
                PlayProgressChange?.Invoke(playState, currentDuration);
                //await GetProgress(args);
            }

            //new Thread(() => { SetProgress(GetProgress(args)); }) { IsBackground = true }.Start();
            else if (args.Data.Contains("Seek to") && args.Data.Contains("of total duration"))
            {
                GetPace(args.Data);
                PlayProgressSeekChange?.Invoke(playState, currentDuration);
                //new Thread(() => { SetPace(GetPace(args)); }) { IsBackground = true }.Start();
            }

        }
        #endregion


        ///// <summary>
        ///// 设置进度
        ///// </summary>
        ///// <param name="timeSpan"></param>
        //private void SetProgress(TimeSpan timeSpan)
        //{

        //    //if ((int)currentDuration.TotalMilliseconds != (int)timeSpan.TotalMilliseconds)
        //        currentDuration = timeSpan;

        //}
        ///// <summary>
        ///// 设置跳跃点
        ///// </summary>
        ///// <param name="timeSpan"></param>
        //void SetPace(TimeSpan? timeSpan)
        //{
        //    if (timeSpan != null)
        //        SetProgress((TimeSpan)timeSpan);
        //}

        #region 获取进度
        ///// <summary>
        ///// 获取进度
        ///// </summary>
        ///// <param name="args"></param>
        //private TimeSpan GetProgress(DataReceivedEventArgs args)
        //{
        //    TimeSpan timeSpan = currentDuration;
        //    try
        //    {
        //        if (!string.IsNullOrWhiteSpace(args.Data))
        //        {

        //            if ((int)timeSpan.TotalSeconds != (int)videoInfo.Duration.TotalSeconds)
        //            {
        //                Regex re = new Regex(@"[0-9]\d*.\d*\d A-V:.*");
        //                Match m = re.Match(args.Data);
        //                re = new Regex(@"[0-9]\d*.\d*\d A-V:*");
        //                m = re.Match(m.Value);
        //                string[] vs = args.Data.Split(':');

        //                if (vs.Length == 2)
        //                {
        //                    playState = PlayState.Playing;
        //                    string[] val = vs[0].Trim(' ').Split(' ');

        //                    if (val.Length == 2)
        //                    {
        //                        val = val[0].Split('.');
        //                        if (val[0] != "nan")
        //                        {
        //                            timeSpan = new TimeSpan(0, 0, 0, Convert.ToInt32(val[0]), Convert.ToInt32(val[1])*10);

        //                            //if (tikes.Count(o => o == Convert.ToInt32(val[0])) == 0)
        //                            //{
        //                            //    tikes.Enqueue((int)(Convert.ToDouble(val[0]) * 1000));

        //                            //}
        //                        }
        //                    }
        //                }
        //                else
        //                { }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    if (isPlaying)
        //                    {
        //                        // await Task.Delay(25);

        //                        if (playProcess != null)
        //                        {
        //                            if (playIntPtr, != null && playProcess.MainWindowHandle != IntPtr.Zero)
        //                                playProcess.CloseMainWindow();
        //                        }
        //                    }
        //                }
        //                catch { }
        //            }

        //        }
        //    }
        //    catch { }
        //    return timeSpan;
        //}
        /// <summary>
        /// 获取进度
        /// </summary>
        /// <param name="args"></param>
        private async void GetProgress(string args)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(args))
                {
                    //3.08 A-V:  0.024 fd=   7 aq=   56KB vq=   77KB sq=    0B f=0/0   
                    if ((int)currentDuration.TotalSeconds != (int)videoInfo.Duration.TotalSeconds || (int)currentDuration.TotalSeconds == 0)
                    {
                        //Regex re = new Regex(@"[0-9]\d*.\d*\d A-V:.*");
                        //Match m = re.Match(args);
                        //re = new Regex(@"[0-9]\d*.\d*\d A-V:*");
                        //m = re.Match(m.Value);
                        args = args.Trim();

                        string[] vs = args.Split(':');
                        //3.08 A-V
                        if (vs.Length == 2)
                        {
                            // playState = PlayState.Playing;
                            string[] val = vs[0].Trim(' ').Split(' ');

                            if (val.Length == 2)
                            {
                                val = val[0].Split('.');
                                if (int.TryParse(val[0], out int result))
                                {
                                    currentDuration = new TimeSpan(0, 0, 0, result, Convert.ToInt32(val[1]) * 10);
                                    //timeSpan= new TimeSpan(0, 0, 0, Convert.ToInt32(val[0]), Convert.ToInt32(val[1]));
                                }
                            }
                        }

                    }
                    else
                    {
                        try
                        {
                            if (isPlaying)
                            {
                                await Task.Delay(100);

                                if (playProcess != null)
                                {
                                    if (playIntPtr != null && playIntPtr != IntPtr.Zero)
                                        playProcess.CloseMainWindow();
                                }
                            }
                        }
                        catch { }
                    }

                }
            }
            catch { }
            //  return timeSpan;
        }
        ///// <summary>
        ///// 获取跳跃
        ///// </summary>
        ///// <param name="args"></param>
        //private TimeSpan? GetPace(DataReceivedEventArgs args)
        //{
        //    TimeSpan timeSpan = currentDuration;
        //    if (!string.IsNullOrWhiteSpace(args.Data))
        //    {//seek to 37% ( 0:01:29) of total duration ( 0:03:59)
        //        if (args.Data.Contains("Seek to") && args.Data.Contains("of total duration"))
        //        {
        //            string str = args.Data.Replace("(", "T").Replace(")", "T");
        //            string[] strs = str.Split('T');
        //            if (strs.Length >= 4)
        //            {
        //                string[] time = strs[1].Split(':');
        //                if (time.Length == 3)
        //                    timeSpan = new TimeSpan(Convert.ToInt16(time[0]), Convert.ToInt16(time[1]), Convert.ToInt16(time[2]));
        //            }
        //        }
        //        else
        //            return null;

        //    }
        //    return timeSpan;
        //}
        /// <summary>
        /// 获取跳跃
        /// </summary>
        /// <param name="args"></param>
        private void GetPace(string args)
        {
            if (!string.IsNullOrWhiteSpace(args))
            {//seek to 37% ( 0:01:29) of total duration ( 0:03:59)
                if (args.Contains("Seek to") && args.Contains("of total duration"))
                {
                    string str = args.Replace("(", "T").Replace(")", "T");
                    string[] strs = str.Split('T');
                    if (strs.Length >= 4)
                    {
                        string[] time = strs[1].Trim().Split(':');
                        if (time.Length == 3)
                            currentDuration = new TimeSpan(Convert.ToInt16(time[0]), Convert.ToInt16(time[1]), Convert.ToInt16(time[2]));
                    }
                }

            }
        }
        #endregion

        #region  控制
        /// <summary>
        /// 暂停
        /// </summary>
        public void PlayPause()
        {
            if (playState == PlayState.Playing)
            {
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Space, (IntPtr)3735553);
                //ControlEx("{SPACE}");
                playState = PlayState.Pasue;
                isPlaying = false;
                PlayStateChange?.Invoke(PlayState.Pasue, "Pasue");
            }
        }


        /// <summary>
        /// 播放播放
        /// </summary>
        public void PlayContinue()
        {
            if (playState == PlayState.Pasue)
            {
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Space, (IntPtr)3735553);
                // ControlEx("{SPACE}");
                playState = PlayState.Playing;
                isPlaying = true;
                PlayStateChange?.Invoke(PlayState.Playing, "Playing");
            }
            else if (playState == PlayState.Load || playState == PlayState.End)
                Play();
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                playState = PlayState.End;
                isPlaying = false;
                //currentDuration = new TimeSpan(0);
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Escape, (IntPtr)65537);
                // ControlEx("{Escape}");              

                 //PlayStateChange?.Invoke(PlayState.End, "End");

            }
        }
        /// <summary>
        /// 改变静音
        /// </summary>
        public void ChangeMute()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.ProcessKey, (IntPtr)3276801);
                //ControlEx("{M}");
                isMute = !isMute;
                if (isMute)
                    PlayStateChange?.Invoke(PlayState.MuteTrue, "MuteTrue");
                else
                    PlayStateChange?.Invoke(PlayState.MuteFalse, "MuteFalse");
            }
            //else
            //{
            //    //isMute = !isMute;
            //    if (isMute)
            //        PlayStateChange?.Invoke(PlayState.MuteTrue, "MuteTrue");
            //    else
            //        PlayStateChange?.Invoke(PlayState.MuteFalse, "MuteFalse");
            //}
        }
        /// <summary>
        /// 改变循环节目
        /// </summary>
        public void ChangeProgram()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                // ControlEx("{C}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.ProcessKey, (IntPtr)3014657);
            }
        }
        /// <summary>
        /// 改变显示模式
        /// </summary>
        public void ChangeMode()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                // ControlEx("{W}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.ProcessKey, (IntPtr)1114113);
            }
        }
        /// <summary>
        /// 改变全屏显示模式
        /// </summary>
        public void ChangeFullScreenMode()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                // ControlEx("{W}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.ProcessKey, (IntPtr)2162689);
            }
        }
        /// <summary>
        /// 逐帧播放
        /// </summary>
        public void FrameOne()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                //ControlEx("{S}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.ProcessKey, (IntPtr)2031617);
            }
        }
        /// <summary>
        /// 音量加
        /// </summary>
        public void VolumUp()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                // ControlEx("{NUMPADMULT}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Multiply, (IntPtr)3604481);
                if (currentVolume < 41)
                    currentVolume++;
                VolumeChange?.Invoke(VolumeStae.Up, currentVolume);
            }
        }
        /// <summary>
        /// 音量减
        /// </summary>
        public void VolumDown()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                //ControlEx("{NUMPADDIV}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Divide, (IntPtr)20250625);
                if (currentVolume > 0)
                    currentVolume--;
                VolumeChange?.Invoke(VolumeStae.Down, currentVolume);
            }
        }

        /// <summary>
        /// 向后十秒钟
        /// </summary>
        public void ProgressLeft10()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                //Control("{LEFT}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Left, (IntPtr)21692417);
            }
        }
        /// <summary>
        /// 向前十秒钟
        /// </summary>
        public void ProgressRight10()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                //Control("{RIGHT}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Right, (IntPtr)21823489);
            }
        }
        /// <summary>
        /// 向前1分钟
        /// </summary>
        public void ProgressUp60()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                //ControlEx("{UP}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Up, (IntPtr)21495809);
            }
        }
        /// <summary>
        /// 向后1分钟
        /// </summary>
        public void ProgressDown60()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                //ControlEx("{DOWN}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Down, (IntPtr)22020097);
            }
        }
        /// <summary>
        /// 上一个节目否则向后十分钟
        /// </summary>
        public void UpFileOr600()
        {
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                //ControlEx("{PGDN}");
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Next, (IntPtr)22085633);
            }
        }
        /// <summary>
        /// 下一个节目否则向前十分钟
        /// </summary>
        public void NextFileOr600()
        {
            //Control("{PGUP}");
            if (playState == PlayState.Playing || playState == PlayState.Pasue)
            {
                User32API.PostMessage(playIntPtr, (int)APIEnum.KeyType.WM_KEYDOWN, (IntPtr)System.Windows.Forms.Keys.Prior, (IntPtr)21561345);
            }
        }
        //        三、播放控制选项说明
        //q, ESC    退出播放
        //f    全屏切换
        //p, SPC    暂停
        //m    静音切换
        //9, 0    9减少音量，0增加音量
        //, *    /减少音量，*增加音量
        //a    循环切换音频流
        //v    循环切换视频流
        //t    循环切换字幕流
        //c    循环切换节目
        //w    循环切换过滤器或显示模式
        //s    逐帧播放
        //left/right 向后/向前拖动10秒
        //down/up 向后/向前拖动1分钟
        //page down/page up    拖动上一个/下一个。或者如果没有章节向后/向前拖动10分钟。
        //鼠标右键单击 拖动与显示宽度对应百分比的文件进行播放
        //鼠标左键双击 全屏切换
        #endregion

      
    }
}
