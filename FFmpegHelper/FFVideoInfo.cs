using System;
using System.IO;

namespace SuperMedia.FFmpegHelper
{
    /// <summary>
    /// 支持mpeg，mpg，avi，dat，mkv，rmvb，rm，mov.
    /// </summary>
    public class FFVideoInfo
    {
        #region Properties
        /// <summary>
        /// 文件路径 支持mpeg，mpg，avi，dat，mkv，rmvb，rm，mov.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 时间长度
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// 比特率
        /// </summary>
        public double BitRate { get; set; }
        /// <summary>
        /// 数据速率
        /// </summary>
        public string AudioFormat { get; set; }
        /// <summary>
        /// 数据格式
        /// </summary>
        public string VideoFormat { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 原始流信息
        /// </summary>
        public string RawInfo { get; set; }
        /// <summary>
        /// 是否收集信息
        /// </summary>
        public bool InfoGathered { get; set; }
        #endregion

        #region Constructors
        public FFVideoInfo(string path)
        {
            this.Path = path;
            Initialize();
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            this.InfoGathered = false;
            //first make sure we have a value for the video file setting
            //if (string.IsNullOrEmpty(path))
            //{
            //    throw new Exception("Could not find the location of the video file");
            //}

            ////Now see if the video file exists
            //if (!File.Exists(path))
            //{
            //    throw new Exception("The video file " + path + " does not exist.");
            //}
        }
        #endregion

        public class OutputPackage
        {
            public MemoryStream VideoStream { get; set; }
            public System.Drawing.Image PreviewImage { get; set; }
            public string RawOutput { get; set; }
            public bool Success { get; set; }
        }
    }
}
