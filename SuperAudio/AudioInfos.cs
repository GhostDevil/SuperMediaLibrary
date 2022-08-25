using System;
using System.IO;
using System.Text;

namespace SuperUtilities.SuperAudio
{
    /// <summary>
    /// 获取音频文件信息
    /// </summary>
    public static class AudioInfos
    {
        /// <summary>
        /// 获取WAV文件信息
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        public static WavInfo GetWavInfo(string fileName)
        {
            WavInfo wavInfo = new WavInfo();
            FileInfo fi = new FileInfo(fileName);
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fs.Length >= 44)
                {
                    byte[] bInfo = new byte[44];
                    fs.Read(bInfo, 0, 44);
                    Encoding.Default.GetString(bInfo, 0, 4);
                    if (Encoding.Default.GetString(bInfo, 0, 4) == "RIFF" && Encoding.Default.GetString(bInfo, 8, 4) == "WAVE" && Encoding.Default.GetString(bInfo, 12, 4) == "fmt ")
                    {
                        wavInfo.groupid = Encoding.Default.GetString(bInfo, 0, 4);
                        System.BitConverter.ToInt32(bInfo, 4);
                        wavInfo.filesize = System.BitConverter.ToInt32(bInfo, 4);
                        wavInfo.rifftype = Encoding.Default.GetString(bInfo, 8, 4);
                        wavInfo.chunkid = Encoding.Default.GetString(bInfo, 12, 4);
                        wavInfo.chunksize = System.BitConverter.ToInt32(bInfo, 16);
                        wavInfo.wformattag = System.BitConverter.ToInt16(bInfo, 20);
                        wavInfo.wchannels = System.BitConverter.ToUInt16(bInfo, 22);
                        wavInfo.dwsamplespersec = System.BitConverter.ToUInt32(bInfo, 24);
                        wavInfo.dwavgbytespersec = System.BitConverter.ToUInt32(bInfo, 28);
                        wavInfo.wblockalign = System.BitConverter.ToUInt16(bInfo, 32);
                        wavInfo.wbitspersample = System.BitConverter.ToUInt16(bInfo, 34);
                        wavInfo.datachunkid = Encoding.Default.GetString(bInfo, 36, 4);
                        wavInfo.datasize = System.BitConverter.ToInt32(bInfo, 40);
                    }
                }
            }

            return wavInfo;
        }
        /// <summary>
        /// 获取音频文件长度
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns>返回单位(秒) -1为失败</returns>
        public static int GetWavDuration(string fileName)
        {
            int duration = 0;
            try
            {
                WavInfo wavInfo = GetWavInfo(fileName);
                duration = Convert.ToInt32(wavInfo.datasize / Convert.ToInt64(wavInfo.dwavgbytespersec));
            }
            catch (Exception ex)
            {
                duration = -1;
               //new StringBuilder("获取音频文件长度出错：").Append(fileName).Append(", ").Append(ex.Message);
            }

            return duration;
        }
        /// <summary>
        /// WAV文件信息
        /// </summary>
        public struct WavInfo
        {
            public string groupid;
            public string rifftype;
            public long filesize;
            public string chunkid;
            public long chunksize;
            public short wformattag; //记录着此声音的格式代号
            public ushort wchannels; //记录声音的频道数。
            public ulong dwsamplespersec;//记录每秒取样数。
            public ulong dwavgbytespersec;//记录每秒的数据量。
            public ushort wblockalign;//记录区块的对齐单位。
            public ushort wbitspersample;//记录每个取样所需的位元数。
            public string datachunkid;
            public long datasize;
        }
    }
}
