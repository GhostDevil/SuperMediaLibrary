using SuperMedia.FFmpegHelper;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static SuperMedia.FFmpegHelper.FFVideoInfo;

namespace SuperMedia.SuperVideo
{
    /// <summary>
    /// 日 期:2020-03-10
    /// 作 者:不良帥
    /// 描 述:ffmpeg 辅助
    /// </summary>
    public class SuperFFMpeg
    {
        #region 音频转换
        /// <summary>
        /// 将Wav音频转成Amr手机音频
        /// </summary>
        /// <param name="ffmegPath">ffmeg.exe文件路径</param>
        /// <param name="fileName">WAV文件的路径(带文件名)</param>
        /// <param name="targetFilName">生成目前amr文件路径（带文件名）</param>
        public static void ConvertToAmr(string ffmegPath, string fileName, string targetFilName)
        {
            string c = ffmegPath + @"\ffmpeg.exe -y -i " + fileName + " -ar 8000 -ab 12.2k -ac 1 " + targetFilName;
            Cmd(c);
        }
        /// <summary>
        /// 执行Cmd命令
        /// </summary>
        private static void Cmd(string c)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.Start();
                process.StandardInput.WriteLine(c);
                process.StandardInput.AutoFlush = true;
                process.StandardInput.WriteLine("exit");
                StreamReader reader = process.StandardOutput;//截取输出流           
                process.WaitForExit();
            }
            catch { }
        }
        /// <summary>
        /// 获取文件的byte[]
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] GetFileByte(string fileName)
        {
            FileStream pFileStream = null;
            byte[] pReadByte = new byte[0];
            try
            {
                pFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader r = new BinaryReader(pFileStream);
                r.BaseStream.Seek(0, SeekOrigin.Begin);    //将文件指针设置到文件开
                pReadByte = r.ReadBytes((int)r.BaseStream.Length);
                return pReadByte;
            }
            catch
            {
                return pReadByte;
            }
            finally
            {
                if (pFileStream != null)
                    pFileStream.Close();
            }
        }
        /// <summary>
        /// 将文件的byte[]生成文件
        /// </summary>
        /// <param name="pReadByte"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool WriteFile(byte[] pReadByte, string fileName)
        {
            FileStream pFileStream = null;
            try
            {
                pFileStream = new FileStream(fileName, FileMode.OpenOrCreate);
                pFileStream.Write(pReadByte, 0, pReadByte.Length);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (pFileStream != null)
                    pFileStream.Close();
            }
            return true;
        }
        #endregion

        #region 获取文件而不创建文件锁
        public static System.Drawing.Image LoadImageFromFile(string fileName)
        {
            System.Drawing.Image theImage = null;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open,
            FileAccess.Read))
            {
                byte[] img;
                img = new byte[fileStream.Length];
                fileStream.Read(img, 0, img.Length);
                fileStream.Close();
                theImage = System.Drawing.Image.FromStream(new MemoryStream(img));
                img = null;
            }
            GC.Collect();
            return theImage;
        }

        public static MemoryStream LoadMemoryStreamFromFile(string fileName)
        {
            MemoryStream ms = null;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open,
            FileAccess.Read))
            {
                byte[] fil;
                fil = new byte[fileStream.Length];
                fileStream.Read(fil, 0, fil.Length);
                fileStream.Close();
                ms = new MemoryStream(fil);
            }
            GC.Collect();
            return ms;
        }
        #endregion

        #region 获取视频信息
        /// <summary>
        /// 获取视频信息
        /// </summary>
        /// <param name="ffmegPath"></param>
        /// <param name="inputPath"></param>
        /// <returns></returns>

        public static FFVideoInfo GetVideoInfo(string ffmegPath, string inputPath)
        {
            FFVideoInfo vf = null;
            try
            {
                vf = new FFVideoInfo(inputPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            GetVideoInfo(ffmegPath, ref vf);
            return vf;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ffmegPath"></param>
        /// <param name="input"></param>
        public static void GetVideoInfo(string ffmegPath, ref FFVideoInfo input)
        {
            //set up the parameters for video info
            string Params = string.Format("-i {0}", input.Path);
            string output = SuperFFHelper.RunProcess(ffmegPath, Params);
            input.RawInfo = output;

            //get duration
            Regex re = new Regex("[D|d]uration:.((\\d|:|\\.)*)");
            Match m = re.Match(input.RawInfo);

            if (m.Success)
            {
                string duration = m.Groups[1].Value;
                string[] timepieces = duration.Split(new char[] { ':', '.' });
                if (timepieces.Length == 4)
                {
                    input.Duration = new TimeSpan(0, Convert.ToInt16(timepieces[0]), Convert.ToInt16(timepieces[1]), Convert.ToInt16(timepieces[2]), Convert.ToInt16(timepieces[3]));
                }
            }

            //get audio bit rate
            re = new Regex("[B|b]itrate:.((\\d|:)*)");
            m = re.Match(input.RawInfo);
            double kb = 0.0;
            if (m.Success)
            {
                Double.TryParse(m.Groups[1].Value, out kb);
            }
            input.BitRate = kb;

            //get the audio format
            re = new Regex("[A|a]udio:.*");
            m = re.Match(input.RawInfo);
            if (m.Success)
            {
                input.AudioFormat = m.Value;
            }

            //get the video format
            re = new Regex("[V|v]ideo:.*");
            m = re.Match(input.RawInfo);
            if (m.Success)
            {
                input.VideoFormat = m.Value;
            }

            //get the video format
            re = new Regex("(\\d{2,3})x(\\d{2,3})");
            m = re.Match(input.RawInfo);
            if (m.Success)
            {
                int width = 0; int height = 0;
                int.TryParse(m.Groups[1].Value, out width);
                int.TryParse(m.Groups[2].Value, out height);
                input.Width = width;
                input.Height = height;
            }
            input.InfoGathered = true;
        }
        #endregion

        #region 转换为 FLV
        /// <summary>  
        /// 视频格式转为Flv  
        /// </summary>  
        /// <param name="vFileName">原视频文件地址</param>  
        /// <param name="widthAndHeight">宽和高参数，如：480*360</param>  
        /// <param name="exportName">生成后的FLV文件地址</param>  
        /// <returns></returns>  
        public static bool Convert2Flv(string vFileName, string widthAndHeight, string exportName,string workingDirectory= "tools")
        {
            try
            {
                string Command = " -i \"" + vFileName + "\" -y -ab 32 -ar 22050 -b 800000 -s " + widthAndHeight + " \"" + exportName + "\""; //Flv格式     
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = @"ffmpeg.exe";
                p.StartInfo.Arguments = Command;
                p.StartInfo.WorkingDirectory = workingDirectory;
                #region 方法一  
                //p.StartInfo.UseShellExecute = false;//不使用操作系统外壳程序 启动 线程  
                //p.StartInfo.RedirectStandardError = true;//把外部程序错误输出写到StandardError流中(这个一定要注意,FFMPEG的所有输出信息,都为错误输出流,用 StandardOutput是捕获不到任何消息的...  
                //p.StartInfo.CreateNoWindow = false;//不创建进程窗口  
                //p.Start();//启动线程  
                //p.BeginErrorReadLine();//开始异步读取  
                //p.WaitForExit();//等待完成  
                //p.Close();//关闭进程  
                //p.Dispose();//释放资源  
                #endregion
                #region 方法二  
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.Start();//启动线程  
                p.WaitForExit();//等待完成  
                p.Close();//关闭进程  
                p.Dispose();//释放资源  
                #endregion  
            }
            catch (System.Exception e)
            {
                throw e;
            }
            return true;
        }
        /// <summary>  
        /// 生成FLV视频的缩略图  
        /// </summary>  
        /// <param name="vFileName">视频文件地址</param>  
        /// <param name="flvImgSize">宽和高参数，如：240*180</param>  
        /// <returns>返回文件路径</returns>  
        public static string CatchImg(string vFileName, string flvImgSize, int second=0, string workingDirectory = "tools")
        {
            if (!System.IO.File.Exists(vFileName))
                return "";
            try
            {
                string flv_img_p = vFileName.Substring(0, vFileName.Length -4) + "_thumb.jpg";
                string Command = " -i \"" + vFileName+ "\" -y -f image2 -ss " + second + " -t 0.1 -s " + flvImgSize + " \"" + flv_img_p + "\"";
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = @"ffmpeg.exe";
                p.StartInfo.Arguments = Command;
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.StartInfo.WorkingDirectory = workingDirectory;
                //不创建进程窗口  
                p.StartInfo.CreateNoWindow = true;
                p.Start();//启动线程  
                p.WaitForExit();//等待完成  
                p.Close();//关闭进程  
                p.Dispose();//释放资源  
                System.Threading.Thread.Sleep(4000);
                //注意:图片截取成功后,数据由内存缓存写到磁盘需要时间较长,大概在3,4秒甚至更长;  
                if (System.IO.File.Exists(flv_img_p))
                {
                    return flv_img_p;
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public OutputPackage ConvertToFLV(string ffmegPath, string savePath, FFVideoInfo input)
        {
            if (!input.InfoGathered)
            {
                GetVideoInfo(ffmegPath, input.Path);
            }
            OutputPackage ou = new OutputPackage();

            //set up the parameters for getting a previewimage
            string filename = System.Guid.NewGuid().ToString() + ".jpg";
            int secs;

            //divide the duration in 3 to get a preview image in the middle of the clip
            //instead of a black image from the beginning.
            secs = (int)Math.Round(TimeSpan.FromTicks(input.Duration.Ticks / 3).TotalSeconds, 0);
            string Params = string.Format("-i {0} {1} -vcodec mjpeg -ss {2} -vframes 1 -an -f rawvideo", input.Path, savePath, secs);
            string output = SuperFFHelper.RunProcess(ffmegPath, Params);

            ou.RawOutput = output;

            if (File.Exists(savePath))
            {
                ou.PreviewImage = LoadImageFromFile(savePath);
                try
                {
                    File.Delete(savePath);
                }
                catch (Exception) { }
            }
            else
            { //try running again at frame 1 to get something
                Params = string.Format("-i {0} {1} -vcodec mjpeg -ss {2} -vframes 1 -an -f rawvideo", input.Path, savePath, 1);
                output = SuperFFHelper.RunProcess(ffmegPath, Params);

                ou.RawOutput = output;

                if (File.Exists(savePath))
                {
                    ou.PreviewImage = LoadImageFromFile(savePath);
                    try
                    {
                        File.Delete(savePath);
                    }
                    catch (Exception) { }
                }
            }


            filename = System.Guid.NewGuid().ToString() + ".flv";
            Params = string.Format("-i {0} -y -ar 22050 -ab 64 -f flv {1}", input.Path, savePath);
            output = SuperFFHelper.RunProcess(ffmegPath, Params);

            if (File.Exists(savePath))
            {
                ou.VideoStream = LoadMemoryStreamFromFile(savePath);
                try
                {
                    File.Delete(savePath);
                }
                catch (Exception) { }
            }
            return ou;
        }
        #endregion


        #region 视频功能
        /// <summary>
        /// 视频录制
        /// </summary>
        /// <param name="url">视频地址</param>
        /// <param name="handler">退出事件</param>
        /// <param name="dllDirectory">Dll目录</param>
        /// <param name="minute">录制时长 分钟</param>
        /// <param name="savePath">保存地址</param>
        /// <param name="windowStyle">是否显示窗口</param>
        /// <returns>当前控制进程对象</returns>
        public static Process RecordVideo(string url, string dllDirectory = "", string savePath = "", EventHandler handler = null, int minute = 0, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
        {
            try
            {
                //转发的命令
                // proc.StartInfo.Arguments = $"/c ffmpeg -i {RtmpUrl} -rtsp_transport tcp -c:a copy -c:v libx264 -f flv rtmp://192.168.1.226:1935/live/abc";
                Process p = new Process();
                var RtmpUrl = url;
                string fileName;
                if (string.IsNullOrWhiteSpace(savePath))
                    fileName = Guid.NewGuid().ToString("N") + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4";
                else
                    fileName = savePath;
                string dir = Environment.CurrentDirectory;
                ProcessStartInfo startInfo = new ProcessStartInfo(@"cmd.exe");//(@"cmd.exe");(/dllDirectory+ "/ffmpeg.exe")
                if (minute == 0)
                    startInfo.Arguments = $"/c ffmpeg -i {RtmpUrl} -c:a copy -c:v copy {fileName}";//如果启动cmd 则参数格式 $"/c ffmpeg -i {RtmpUrl} -c:a copy -c:v copy {fileName}";启动ffmpeg.exe 则 -i开始
                else
                    startInfo.Arguments = $"/c ffmpeg -i {RtmpUrl} -c:a copy -c:v copy -t {minute * 60} {fileName}"; //-t  单位秒

                if (string.IsNullOrWhiteSpace(dllDirectory))
                    startInfo.WorkingDirectory = dir;
                else
                    startInfo.WorkingDirectory = dllDirectory;

                //if (windowStyle == ProcessWindowStyle.Hidden)
                //    startInfo.UseShellExecute = true;
                //else
                    startInfo.UseShellExecute = true;//当 UseShellExecute 为 false 时，使用 Process 组件仅能启动可执行文件。
                startInfo.RedirectStandardInput = false;
                startInfo.RedirectStandardOutput = false;
                startInfo.RedirectStandardError = false;
                startInfo.WindowStyle = windowStyle;
                
               // startInfo.CreateNoWindow = createNoWindow; //UseShellExecute为false时才有效
                p.EnableRaisingEvents = true;
                p.Exited += (s, o) =>
                {
                    Process p1 = s as Process;
                    Console.WriteLine("录制结束：" + savePath);
                    handler?.Invoke(savePath, o);
                    p1.Dispose();
                };
                p.StartInfo = startInfo;
                p.Start();
                return p;
                #region
                //p.WaitForExit();
                //return null;
                //Process proc = new Process();
                //proc.StartInfo.FileName = @"C:\Windows\system32\cmd.exe";
                //proc.StartInfo.WorkingDirectory = dir;
                //proc.StartInfo.UseShellExecute = true;
                //proc.StartInfo.RedirectStandardInput = false;
                //proc.StartInfo.RedirectStandardOutput = false;
                //proc.StartInfo.RedirectStandardError = false;
                //proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                //proc.StartInfo.CreateNoWindow = false;
                //if(time==0)
                //    proc.StartInfo.Arguments = $"/k ffmpeg -i {RtmpUrl} -c:a copy -c:v copy {fileName}"; 
                //else
                //    proc.StartInfo.Arguments = $"/k ffmpeg -i {RtmpUrl} -c:a copy -c:v copy -t {minute * 60} {fileName}"; //-t  单位秒
                //proc.EnableRaisingEvents = true;
                //proc.Exited += handler;
                //proc.Start();
                //return proc;
                //proc.WaitForExit();
                #endregion
            }
            catch { return null; }
        }
        /// <summary>
        /// 停止录像
        /// </summary>
        /// <param name="pid"></param>
        public static void StopRecord(int pid)
        {
            ExitCmd(pid);
        }
        /// <summary>
        /// 设置ffmpeg.exe的路径
        /// </summary>
        static string FFmpegPath = @"E:\Coders\SmartClass\bin\Debug\FFmpeg\bin\x64\";
 
        public static Process RunMyProcess(string Parameters)
        {
            var p = new Process();
            p.StartInfo.FileName = FFmpegPath;
            p.StartInfo.Arguments = Parameters;
            //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = true;
            //不显示程序窗口
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            return p;
            Console.WriteLine("\n开始转码...\n");
            //p.WaitForExit();
            //p.Close();
        }
        #endregion

        #region 注意
         static void ConvertVideo(string strArg)
        {
            Process p = new Process();//建立外部调用线程
            p.StartInfo.FileName = "ffmpegPath";//要调用外部程序的绝对路径
            p.StartInfo.Arguments = strArg;
            p.StartInfo.UseShellExecute = false;//不使用操作系统外壳程序启动线程(一定为FALSE,详细的请看MSDN)
            p.StartInfo.RedirectStandardError = true;//把外部程序错误输出写到StandardError流中(这个一定要注意,FFMPEG的所有输出信息,都为错误输出流,用StandardOutput是捕获不到任何消息的...这是我耗费了2个多月得出来的经验...mencoder就是用standardOutput来捕获的)
            p.StartInfo.CreateNoWindow = false;//不创建进程窗口
            p.ErrorDataReceived += new DataReceivedEventHandler(Output);//外部程序(这里是FFMPEG)输出流时候产生的事件,这里是把流的处理过程转移到下面的方法中,详细请查阅MSDN
            p.Start();//启动线程
            p.BeginErrorReadLine();//开始异步读取
            p.WaitForExit();//阻塞等待进程结束
            p.Close();//关闭进程
            p.Dispose();//释放资源
        }
       
        private static void Output(object sendProcess, DataReceivedEventArgs output)
        {
            if (!String.IsNullOrEmpty(output.Data))
            {
                //处理方法...
                Console.WriteLine("Error："+output.Data);
            }
        }
        //停止录制 停止录制的过程比较复杂，需要用到 kernel32.dll 中的接口。为了能向控制台发送指令，
        //首先要将当前的Unity进程附加到前面开启的ffmpeg进程的控制台中；然后要设置Unity进程响应控制台指令的句柄，这里设置为空，即收到控制台指令后不执行任何操作；
        //接下来向控制台发送 Ctrl C 结束指令，ffmpeg进程在收到该指令后将会结束录制，
        //如果上一步没有把Unity进程的响应方式设为空，那么Unity进程也会跟着结束；ffmpeg收到结束指令后，还要进行一些操作才能正确停止录制，大概要1秒，所以要利用协程等待几秒；
        //最后，将前面设置的句柄卸载，然后释放已附加的控制台。卸载句柄和释放控制台一定不能忘，否则就会出现ffmpeg无法正常停止录制的情况。

        #endregion

        #region 控制台操作
        #region 模拟控制台信号需要使用的DLL
        [DllImport("kernel32.dll")]
        static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();
        #endregion

         async static void ExitCmd(int pid)
        {
            // 将当前进程附加到pid进程的控制台
            AttachConsole(pid);
            // 将控制台事件的处理句柄设为Zero，即当前进程不响应控制台事件
            // 避免在向控制台发送【Ctrl C】指令时连带当前进程一起结束
            SetConsoleCtrlHandler(IntPtr.Zero, true);
            // 向控制台发送 【Ctrl C】结束指令
            // ffmpeg会收到该指令停止录制
            GenerateConsoleCtrlEvent(0, 0);
            await Task.Delay(10);

            // 卸载控制台事件的处理句柄，不然之后的ffmpeg调用无法正常停止
            SetConsoleCtrlHandler(IntPtr.Zero, false);
            // 剥离已附加的控制台
            FreeConsole();

        }
        #endregion

        public static void ConvertToTsFile(string file)
        {
            //将 mp4 文件转换成 ts 文件
            // ffmpeg -i test.mp4 -c copy -bsf h264_mp4toannexb output.ts
            //string cmd = $"ffmpeg -i {file} -c copy -bsf h264_mp4toannexb {file}.ts";
            //string mps = "ffmpeg -i D:\魏新雨_余情未了.flac -c:a libmp3lame -map 0:0 -f segment -segment_time 10 -segment_list outputlist.m3u8 -segment_format mpegts output%03d.ts";
            //string str = "ffmpeg -i D:\魏新雨_余情未了.flac -ab 320k -map_metadata 0 -id3v2_version 3 D:\魏新雨_余情未了.flac.mp3";
            ////ffmpeg -i d:/美女吃烤鸭.2160P.42秒.HD.Club-4K-Chimei-inn-60mbps.mp4 -c:v libx264 -c:a aac -strict -2 -f hls -hls_list_size 0 -hls_time 10  d:/美女吃烤鸭.2160P.42秒.HD.Club-4K-Chimei-inn-60mbps.mp4.m3u8
            //fmpeg -i d:/美女吃烤鸭.2160P.42秒.HD.Club-4K-Chimei-inn-60mbps.mp4 -f segment -segment_time 10 -segment_format mpegts -segment_list list_file.m3u8 -c copy -bsf:v h264_mp4toannexb -map 0 output_file-%d.ts
            // 1.ffmpeg切片命令，以H264和AAC的形式对视频进行输出

            //ffmpeg -i input.mp4 -c:v libx264 -c:a aac -strict -2 -f hls output.m3u8

            //2.ffmpeg转化成HLS时附带的指令

            //-hls_time n: 设置每片的长度，默认值为2。单位为秒

            //-hls_list_size n: 设置播放列表保存的最多条目，设置为0会保存有所片信息，默认值为5

            //-hls_wrap n: 设置多少片之后开始覆盖，如果设置为0则不会覆盖，默认值为0.这个选项能够避免在磁盘上存储过多的片，而且能够限制写入磁盘的最多的片的数量

            //-hls_start_number n: 设置播放列表中sequence number的值为number，默认值为0

            //3.对ffmpeg切片指令的使用

            //ffmpeg -i output.mp4 -c:v libx264 -c:a aac -strict -2 -f hls -hls_list_size 0 -hls_time 5 output1.m3u8


            //ffmpeg.exe 获取PCM数据


            //通过调用命令行

            //-ar: 指定采样率;
            //-ac: 指定声道数；
            //-f f32le: 表示每个采样点用32位浮点数来表示（le表示小端，be表示大端）
            //-f参数指定format PCM一般常见的是16bit(音乐)和8bit(语音)

            //ffmpeg -i D:\魏新雨_余情未了.flac.mp3 -hide_banner -codec:a pcm_f32le -ar 48000 -ac 2 -f u8 D:\魏新雨_余情未了.flac.mp3.pcm
            //ffmpeg -i D:\魏新雨_余情未了.flac.mp3 -codec:a pcm_f32le -ar 48000 -ac 2 -f u8 D:\魏新雨_余情未了.flac.mp3.pcm
            //ffmpeg -i D:\魏新雨_余情未了.flac.mp3 -codec:a pcm_f32le -ar 48000 -ac 2 -f f32le D:\魏新雨_余情未了.flac.mp3.pcm
            //ffmpeg -i D:\魏新雨_余情未了.flac.mp3 -f s16le -acodec pcm_s16le -b:a 16 -ar 8000 -ac 1 output.raw


            //ffmpeg -ss 4 -t 16 -i D:\魏新雨_余情未了.flac.mp3 -f s16le -acodec pcm_s16le -b:a 16 -ar 8000 -ac 1 output.raw //输出格式为RAW,也就是PCM签名的16位小端，没有WAV头

            //相应配置项的解释：

            //这一段获取的output 文件为    input.mp3 从4s 开始到20s 的数据 ，转存为 采样率8000khz，声道为单声道，位深为16bit 的pcm 原始数据

        }
        /// <summary>
        /// 转换音频为PCM原始数据
        /// </summary>
        /// <param name="ffmegPath"></param>
        /// <param name="convertInfo"></param>
        /// <param name="outputStr"></param>
        /// <param name="outputFilePath"></param>
        /// <returns></returns>
        public bool ConvertPCM(string ffmegPath,ConvertInfo convertInfo, ref string outputStr, string outputFilePath=null)
        {
            try
            {
                string cmd = $"ffmpeg -hide_banner -i {convertInfo.file} ";
                if (convertInfo.ar > 0)
                    cmd += $"-ar {convertInfo.ar} ";
                if (convertInfo.ac > 0)
                    cmd += $"-ac {convertInfo.ac} ";
                if (!string.IsNullOrWhiteSpace(convertInfo.format))
                    cmd += $"-f {convertInfo.format} ";
                if (!string.IsNullOrWhiteSpace(outputFilePath))
                    cmd += $" {outputFilePath} ";
                else
                    cmd += $" {convertInfo.file}.pcm ";
                outputStr = SuperFFHelper.RunProcess(ffmegPath, cmd);
            }
            catch (Exception ex) 
            {
                outputStr = ex.Message;
                return false;
            }
            return true;
        }
        /// <summary>
        /// 转换信息
        /// </summary>
        public class ConvertInfo
        {
            /// <summary>
            /// 文件
            /// </summary>
            public string file;
           /// <summary>
           /// 采样率
           /// </summary>
            public int ar;
            /// <summary>
            /// 声道数
            /// </summary>
            public int ac;
            /// <summary>
            /// 格式
            /// </summary>
            public string format;
            /// <summary>
            /// 开始时间 秒
            /// </summary>
            public int startTime = 0;
            /// <summary>
            /// 时间长度
            /// </summary>
            public int countTime;
        }
    }
}
