using System;
using System.IO;
using System.Media;
using System.Text;

namespace SuperMedia.SuperVideo
{
    /// <summary>
    /// 日 期:2016-09-05
    /// 作 者:不良帥
    /// 描 述:wav音频辅助方法类
    /// </summary>
    public class MediaHelper
    {

        #region  同步播放wav文件 
        /// <summary>
        /// 以同步方式播放wav文件
        /// </summary>
        /// <param name="sp">SoundPlayer对象</param>
        /// <param name="wavFilePath">wav文件的路径</param>
        public static void SyncPlayWAV(SoundPlayer sp, string wavFilePath)
        {
            try
            {
                //设置wav文件的路径 
                sp.SoundLocation = wavFilePath;

                //使用异步方式加载wav文件
                sp.LoadAsync();

                //使用同步方式播放wav文件
                if (sp.IsLoadCompleted)
                {
                    sp.PlaySync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 以同步方式播放wav文件
        /// </summary>
        /// <param name="wavFilePath">wav文件的路径</param>
        public static void SyncPlayWAV(string wavFilePath)
        {
            try
            {
                //创建一个SoundPlaryer类，并设置wav文件的路径
                SoundPlayer sp = new SoundPlayer(wavFilePath);

                //使用异步方式加载wav文件
                sp.LoadAsync();

                //使用同步方式播放wav文件
                if (sp.IsLoadCompleted)
                {
                    sp.PlaySync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region  异步播放wav文件 
        /// <summary>
        /// 以异步方式播放wav文件
        /// </summary>
        /// <param name="sp">SoundPlayer对象</param>
        /// <param name="wavFilePath">wav文件的路径</param>
        public static void ASyncPlayWAV(SoundPlayer sp, string wavFilePath)
        {
            try
            {
                //设置wav文件的路径 
                sp.SoundLocation = wavFilePath;

                //使用异步方式加载wav文件
                sp.LoadAsync();

                //使用异步方式播放wav文件
                if (sp.IsLoadCompleted)
                {
                    sp.Play();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 以异步方式播放wav文件
        /// </summary>
        /// <param name="wavFilePath">wav文件的路径</param>
        public static void ASyncPlayWAV(string wavFilePath)
        {
            try
            {
                //创建一个SoundPlaryer类，并设置wav文件的路径
                SoundPlayer sp = new SoundPlayer(wavFilePath);

                //使用异步方式加载wav文件
                sp.LoadAsync();

                //使用异步方式播放wav文件
                if (sp.IsLoadCompleted)
                {
                    sp.Play();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region  停止播放wav文件 
        /// <summary>
        /// 停止播放wav文件
        /// </summary>
        /// <param name="sp">SoundPlayer对象</param>
        public static void StopWAV(SoundPlayer sp)
        {
            sp.Stop();
        }
        #endregion

        #region 获取mp3文件信息
        /// <summary>
        /// 获取MP3文件最后128个字节
        /// </summary>
        /// <param name="FileName">文件名</param>
        /// <returns>返回字节数组</returns>
        private static byte[] GetLast128(string FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            Stream stream = fs;
            stream.Seek(-128, SeekOrigin.End);
            const int seekPos = 128;
            int rl = 0;
            byte[] Info = new byte[seekPos];
            rl = stream.Read(Info, 0, seekPos);
            fs.Close();
            stream.Close();
            return Info;
        }
        /// <summary>
        /// 获取MP3歌曲的相关信息
        /// </summary>
        /// <param name = "filePath">文件路径</param>
        /// <returns>返回一个Mp3Info结构</returns>
        public static Mp3Info GetMp3Info(string filePath)
        {
            byte[] Info = GetLast128(filePath);
            Mp3Info mp3Info = new Mp3Info();
            string str = null;
            int i;
            int position = 0;//循环的起始值
            int currentIndex = 0;//Info的当前索引值
                                 //获取TAG标识
            for (i = currentIndex; i < currentIndex + 3; i++)
            {
                str = str + (char)Info[i];
                position++;
            }
            currentIndex = position;
            mp3Info.identify = str;

            //获取歌名
            str = null;
            byte[] bytTitle = new byte[30];//将歌名部分读到一个单独的数组中
            int j = 0;
            for (i = currentIndex; i < currentIndex + 30; i++)
            {
                bytTitle[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            mp3Info.Title = byteToString(bytTitle);

            //获取歌手名
            str = null;
            j = 0;
            byte[] bytArtist = new byte[30];//将歌手名部分读到一个单独的数组中
            for (i = currentIndex; i < currentIndex + 30; i++)
            {
                bytArtist[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            mp3Info.Artist = byteToString(bytArtist);

            //获取唱片名
            str = null;
            j = 0;
            byte[] bytAlbum = new byte[30];//将唱片名部分读到一个单独的数组中
            for (i = currentIndex; i < currentIndex + 30; i++)
            {
                bytAlbum[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            mp3Info.Album = byteToString(bytAlbum);

            //获取年
            str = null;
            j = 0;
            byte[] bytYear = new byte[4];//将年部分读到一个单独的数组中
            for (i = currentIndex; i < currentIndex + 4; i++)
            {
                bytYear[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            mp3Info.Year = byteToString(bytYear);
            //获取注释
            str = null;
            j = 0;
            byte[] bytComment = new byte[28];//将注释部分读到一个单独的数组中
            for (i = currentIndex; i < currentIndex + 25; i++)
            {
                bytComment[j] = Info[i];
                position++;
                j++;
            }
            currentIndex = position;
            mp3Info.Comment = byteToString(bytComment);

            //以下获取保留位
            mp3Info.reserved1 = (char)Info[++position];
            mp3Info.reserved2 = (char)Info[++position];
            mp3Info.reserved3 = (char)Info[++position];
            return mp3Info;
        }
        /// <summary>
        /// 将字节数组转换成字符串
        /// </summary>
        /// <param name = "b">字节数组</param>
        /// <returns>返回转换后的字符串</returns>
        private static string byteToString(byte[] b)
        {
            Encoding enc = Encoding.GetEncoding("GB2312");
            string str = enc.GetString(b);
            str = str.Substring(0, str.IndexOf("#CONTENT#") >= 0 ? str.IndexOf("#CONTENT#") : str.Length);//去掉无用字符
            return str;
        }

        /// <summary>
        /// MP3信息的结构体
        /// </summary>
        public struct Mp3Info
        {
            /// <summary>
            /// TAG，三个字节
            /// </summary>
            public string identify;    
            /// <summary>
            /// 歌曲名,30个字节
            /// </summary>
            public string Title;       
            /// <summary>
            /// 歌手名,30个字节
            /// </summary>
            public string Artist;     
            /// <summary>
            /// 所属唱片,30个字节
            /// </summary>
            public string Album;    
            /// <summary>
            /// 年,4个字符
            /// </summary>
            public string Year;      
            /// <summary>
            /// 注释,28个字节
            /// </summary>
            public string Comment;     
            /// <summary>
            /// 保留位,一个字节
            /// </summary>
            public char reserved1;
            /// <summary>
            /// 保留位,一个字节
            /// </summary>
            public char reserved2;
            /// <summary>
            /// 保留位,一个字节
            /// </summary>
            public char reserved3;     
        }
        #endregion

    }
}
