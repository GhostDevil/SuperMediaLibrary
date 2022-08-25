using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SuperMedia.FFmpegHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class SuperFFHelper
    {
        #region 获取视频信息
        /// <summary>
        /// 获取视频信息
        /// </summary>
        /// <param name="ffmegPath"></param>
        /// <param name="inputPath"></param>
        /// <returns></returns>

        public static FFVideoInfo GetVideoInfo(string ffmegPath, string inputPath)
        {
            FFVideoInfo vf;
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
        /// 获取视频信息
        /// </summary>
        /// <param name="ffmegPath"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        static void GetVideoInfo(string ffmegPath, ref FFVideoInfo input)
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
                int.TryParse(m.Groups[1].Value, out int width);
                int.TryParse(m.Groups[2].Value, out int height);
                input.Width = width;
                input.Height = height;
            }
            input.InfoGathered = true;
        }
        #endregion

        #region 调用ffmpeg.exe 执行命令
        /// <summary>
        /// 调用 exe 执行命令
        /// </summary>
        /// <param name="ffmegPath">视频处理器ffmpeg.exe的位置</param>
        /// <param name="Parameters">命令参数</param>
        /// <returns>返回执行结果</returns>
        internal static string RunProcess(string ffmegPath, string Parameters)
        {
            //创建一个ProcessStartInfo对象 并设置相关属性
            var oInfo = new ProcessStartInfo(ffmegPath, Parameters);
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            oInfo.RedirectStandardOutput = true;
            oInfo.RedirectStandardError = true;
            oInfo.RedirectStandardInput = true;

            //创建一个字符串和StreamReader 用来获取处理结果
            string output = null;
            StreamReader srOutput = null;

            try
            {
                //调用ffmpeg开始处理命令
                var proc = Process.Start(oInfo);
                proc.WaitForExit();

                //获取输出流
                srOutput = proc.StandardError;

                //转换成string
                output = srOutput.ReadToEnd();

                //关闭处理程序
                proc.Close();
            }
            catch (Exception)
            {
                output = string.Empty;
            }
            finally
            {
                //释放资源
                if (srOutput != null)
                {
                    srOutput.Close();
                    srOutput.Dispose();
                }
            }
            return output;
        }
        #endregion


    }
}
