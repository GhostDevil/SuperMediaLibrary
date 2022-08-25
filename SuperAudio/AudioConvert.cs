using System;
using System.IO;
using System.Text;

namespace SuperMedia.SuperVideo
{
    public class AudioConvert
    {
        FileStream fileStream = null;
        BinaryWriter binaryWriter = null;
        /// <summary>
        /// PCM to WAV
        /// 添加Wav头文件
        /// 参考资料：http://blog.csdn.net/bluesoal/article/details/932395
        /// </summary>
        private void CreateSoundFile(string path)
        {

            try
            {
                fileStream = new FileStream(path, FileMode.Create);
            }
            catch (Exception ex)
            {

                //mWaveFile = new FileStream(System.DateTime.Now.ToString("yyyyMMddHHmmss") + "test2.wav", FileMode.Create);
            }

            binaryWriter = new BinaryWriter(fileStream);

            //Set up file with RIFF chunk info. 每个WAVE文件的头四个字节便是“RIFF”。
            char[] ChunkRiff = { 'R', 'I', 'F', 'F' };
            char[] ChunkType = { 'W', 'A', 'V', 'E' };
            char[] ChunkFmt = { 'f', 'm', 't', ' ' };
            char[] ChunkData = { 'd', 'a', 't', 'a' };

            short shPad = 1;                // File padding

            int nFormatChunkLength = 0x10; // Format chunk length.

            int nLength = 0;                // File length, minus first 8 bytes of RIFF description. This will be filled in later.

            short shBytesPerSample = 0;     // Bytes per sample.

            short BitsPerSample = 16; //每个采样需要的bit数  

            //这里需要注意的是有的值是short类型，有的是int，如果错了，会导致占的字节长度过长or过短
            short channels = 1;//声道数目，1-- 单声道；2-- 双声道

            // 一个样本点的字节数目
            shBytesPerSample = 2;

            // RIFF 块
            binaryWriter.Write(ChunkRiff);
            binaryWriter.Write(nLength);
            binaryWriter.Write(ChunkType);

            // WAVE块
            binaryWriter.Write(ChunkFmt);
            binaryWriter.Write(nFormatChunkLength);
            binaryWriter.Write(shPad);


            binaryWriter.Write(channels); // Mono,声道数目，1-- 单声道；2-- 双声道
            binaryWriter.Write(16000);// 16KHz 采样频率                   
            binaryWriter.Write(32000); //每秒所需字节数
            binaryWriter.Write(shBytesPerSample);//数据块对齐单位(每个采样需要的字节数)
            binaryWriter.Write(BitsPerSample);  // 16Bit,每个采样需要的bit数  

            // 数据块
            binaryWriter.Write(ChunkData);
            binaryWriter.Write((int)0);   // The sample length will be written in later.
        }
        ///// <summary>  
        ///// 获取完整的wav流  
        ///// </summary>  
        ///// <param name="soundBytes">PCM流</param>  
        ///// <returns>wav流</returns>  
        //private byte[] GetAudioByte(HttpPostedFileBase soundBytes)
        //{
        //    try
        //    {
        //        string tempPath = string.Format(@"{0}\{1}.wav", AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString("n"));
        //        //添加wav文件头  
        //        CreateSoundFile(tempPath);
        //        BinaryWriter binaryWriter = new BinaryWriter(fileStream);
        //        byte[] bytes = new byte[soundBytes.InputStream.Length];
        //        soundBytes.InputStream.Read(bytes, 0, bytes.Length);
        //        binaryWriter.Write(bytes, 0, bytes.Length);
        //        binaryWriter.Seek(4, SeekOrigin.Begin);
        //        binaryWriter.Write((int)(bytes.Length + 36));   // 写文件长度  
        //        binaryWriter.Seek(40, SeekOrigin.Begin);
        //        binaryWriter.Write(bytes.Length);
        //        fileStream.Close();

        //        byte[] audioBytes = ConvertToBinary(tempPath);
        //        //删除文件  
        //        if (System.IO.File.Exists(tempPath))
        //        {
        //            FileInfo fi = new FileInfo(tempPath);
        //            if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
        //                fi.Attributes = FileAttributes.Normal;
        //            System.IO.File.Delete(tempPath);
        //        }
        //        return audioBytes;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //    finally
        //    {
        //        if (fileStream != null)
        //        {
        //            fileStream.Close();
        //        }
        //    }
        //}
        /// <summary>  
        /// Wave Hander 信息
        /// </summary>  
        public struct HeaderType
        {
            /// <summary>
            /// RIFF类资源文件头部 4byte
            /// </summary>
            public byte[] riff;
            /// <summary>
            /// 文件长度4byte
            /// </summary>
            public uint file_len;
            /// <summary>
            /// "WAVE"标志4byte
            /// </summary>
            public byte[] wave;
            /// <summary>
            /// "fmt"标志4byte
            /// </summary>
            public byte[] fmt;
            /// <summary>
            /// 过渡字节4byte
            /// </summary>
            public uint NI1;
            /// <summary>
            /// 格式类别(10H为PCM形式的声音数据)2byte
            /// </summary>
            public ushort format_type;
            /// <summary>
            /// Channels 1 = 单声道; 2 = 立体声2byte
            /// </summary>
            public ushort Channels;
            /// <summary>
            /// 采样频率4byte
            /// </summary>
            public uint frequency;
            /// <summary>
            /// 音频数据传送速率4byte
            /// </summary>
            public uint trans_speed;
            /// <summary>
            /// 数据块的调整数（按字节算的）2byte
            /// </summary>
            public ushort dataBlock;
            /// <summary>
            /// 样本的数据位数(8/16) 2byte
            /// </summary>
            public ushort sample_bits;
            /// <summary>
            /// 数据标记符"data" 4byte
            /// </summary>
            public byte[] data;
            /// <summary>
            /// 语音数据的长度 4byte
            /// </summary>
            public uint wav_len;
        }
        /// <summary>  
        ///  WAV to PCM  读取WAVE文件，包括文件头和数据部分 
        /// </summary>  
        /// <param name="filepath">wav文件路径</param> 
        /// <param name="savePath">pcm保存路径</param>
        /// <returns>true：成功 false：失败</returns>  
        public static bool WavToPcm(string filepath, string savePath)
        {
            String fileName = filepath;//保存文件名  
            if (File.Exists(fileName) == false)//文件不存在  
            {
                throw new Exception("File is Not Exits.");
            }
            //只读方式打开文件  
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
            if (file == null || file.Length < 44) //长度少于44，或者打开失败  
            {
                file.Close();//  
                throw new Exception("File is not Wava.");
            }
            byte[] buff = new byte[44]; //header byte  
            file.Read(buff, 0, 44);//读取文件头 
            byte[] databuff;            //data byte  
            HeaderType wavHander = new HeaderType();       //定义一个头结构体  
            wavHander.riff = new byte[4];//RIFF  
            wavHander.wave = new byte[4];//WAVE  
            wavHander.fmt = new byte[4];//fmt   
            wavHander.data = new byte[4];//data  
            if (fixedData(buff, wavHander) == false)//按位置保存文件头信息到结构体中  
                throw new Exception("File is not Wava.");
            databuff = new byte[4];//分配内存  
            try
            {
                file.Read(databuff, 0, databuff.Length);//读取文件数据去数据  
                WriteFile(savePath, databuff);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                file.Close();//关闭文件  
            }

        }
        /// <summary>  
        /// PCM to WAV
        /// </summary>  
        /// <param name="filepath">pcm文件路径</param>  
        /// <param name="savePath">wav保存路径</param>
        /// <returns>读取成功返回真</returns>  
        public bool PcmToWav(string filepath,string savePath)
        {
            HeaderType wavHander = new HeaderType();       //定义一个头结构体  
            byte[] buff = new byte[44]; //header byte  
            byte[] databuff;            //data byte  
            wavHander.riff = new byte[4];//RIFF  
            wavHander.wave = new byte[4];//WAVE  
            wavHander.fmt = new byte[4];//fmt   
            wavHander.data = new byte[4];//data  

            String fileName = filepath;//临时保存文件名  
            if (File.Exists(fileName) == false)//文件不存在  
            {
                throw new Exception("File is Not Exits.");
            }
            //自读方式打开  
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
            if (file == null)//打开成功  
            {
                file.Close();//关闭文件  
                throw new Exception("File is not Wava.");
            }
            int filelen = (int)file.Length;//获取文件长度  
            databuff = new byte[filelen + 44];//分配 内存 
            file.Read(databuff, 44, filelen);//读取文件，保存在内存中  
            file.Close();//关闭文件  
            InitHeader(wavHander, ref databuff);
            WriteFile(savePath, databuff);
            return true;
        }
        /// <summary>  
        /// 为PCM文件构建文件头，准备转换为WAV文件  
        /// </summary>  
        /// <returns>构建成功返回真</returns>  
        private static bool InitHeader(HeaderType wavHander, ref byte[] databuff)
        {
            wavHander.riff = Encoding.ASCII.GetBytes("RIFF");   /*RIFF类资源文件头部 4byte*/
            wavHander.file_len = (uint)(databuff.Length);              /*文件长度4byte*/
            wavHander.wave = Encoding.ASCII.GetBytes("WAVE");     /*"WAVE"标志4byte*/
            wavHander.fmt = Encoding.ASCII.GetBytes("fmt ");      /*"fmt"标志4byte*/
            wavHander.NI1 = 0x10;                               /*过渡字节4byte*/
            wavHander.format_type = 0x01;                       /*格式类别(10H为PCM形式的声音数据)2byte*/
            wavHander.Channels = 0x01;                          /*Channels 1 = 单声道; 2 = 立体声2byte*/
            wavHander.frequency = 0x1F40;                       /*采样频率4byte*/
            wavHander.trans_speed = 0x3E80;                     /*音频数据传送速率4byte*/
            wavHander.dataBlock = 0x02;                         /*数据块的调整数（按字节算的）2byte*/
            wavHander.sample_bits = 0x10;                       /*样本的数据位数(8/16) 2byte*/
            wavHander.data = Encoding.ASCII.GetBytes("data");   /*数据标记符"data" 4byte*/
            wavHander.wav_len = (uint)(databuff.Length - 44);                /*语音数据的长度 4byte*/
            byte[] byt2;//临时变量 ，保存2位的整数  
            byte[] byt4;//临时变量， 保存4位的整数  
            Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(wavHander.riff), 0, 4, databuff, 0);/*RIFF类资源文件头部 4byte*/
            byt4 = BitConverter.GetBytes(wavHander.file_len); /*文件长度4byte*/
            Array.Copy(byt4, 0, databuff, 4, 4);
            Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(wavHander.wave), 0, 4, databuff, 8);/*"WAVE"标志4byte*/
            Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(wavHander.fmt), 0, 4, databuff, 12);/*"fmt"标志4byte*/
            byt4 = BitConverter.GetBytes(wavHander.NI1);/*过渡字节4byte*/
            Array.Copy(byt4, 0, databuff, 16, 4);
            byt2 = BitConverter.GetBytes(wavHander.format_type);/*格式类别(10H为PCM形式的声音数据)2byte*/
            Array.Copy(byt2, 0, databuff, 20, 2);
            byt2 = BitConverter.GetBytes(wavHander.Channels);/*Channels 1 = 单声道; 2 = 立体声2byte*/
            Array.Copy(byt2, 0, databuff, 22, 2);
            byt4 = BitConverter.GetBytes(wavHander.frequency);/*采样频率4byte*/
            Array.Copy(byt4, 0, databuff, 24, 4);
            byt4 = BitConverter.GetBytes(wavHander.trans_speed);/*音频数据传送速率4byte*/
            Array.Copy(byt4, 0, databuff, 28, 4);
            byt2 = BitConverter.GetBytes(wavHander.dataBlock);/*数据块的调整数（按字节算的）2byte*/
            Array.Copy(byt2, 0, databuff, 32, 2);
            byt2 = BitConverter.GetBytes(wavHander.sample_bits);/*样本的数据位数(8/16) 2byte*/
            Array.Copy(byt2, 0, databuff, 34, 2);
            Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(wavHander.data), 0, 4, databuff, 36);/*数据标记符"data" 4byte*/
            byt4 = BitConverter.GetBytes(wavHander.wav_len); /*语音数据的长度 4byte*/
            Array.Copy(byt4, 0, databuff, 40, 4);
            return true;
        }
        
        /// <summary>  
        /// 把文件头数组信息保存到结构体中  
        /// </summary>  
        /// <param name="pbuff">文件头数组</param>  
        /// <returns>true：成功 false：失败</returns>  
        static bool fixedData(byte[] pbuff, HeaderType wavHander)
        {

            Array.Copy(pbuff, 0, wavHander.riff, 0, 4);//  
            if (Encoding.ASCII.GetString(wavHander.riff) != "RIFF")//确定文件是WAVA类型  
                return false;
            wavHander.file_len = BitConverter.ToUInt32(pbuff, 4);
            Array.Copy(pbuff, 8, wavHander.wave, 0, 4);
            Array.Copy(pbuff, 12, wavHander.fmt, 0, 4);
            wavHander.NI1 = BitConverter.ToUInt32(pbuff, 16);
            wavHander.format_type = BitConverter.ToUInt16(pbuff, 20);
            wavHander.Channels = BitConverter.ToUInt16(pbuff, 22);
            wavHander.frequency = BitConverter.ToUInt32(pbuff, 24);
            wavHander.trans_speed = BitConverter.ToUInt32(pbuff, 28);
            wavHander.dataBlock = BitConverter.ToUInt16(pbuff, 32);
            wavHander.sample_bits = BitConverter.ToUInt16(pbuff, 34);
            Array.Copy(pbuff, 36, wavHander.data, 0, 4);
            wavHander.wav_len = BitConverter.ToUInt32(pbuff, 40);
            return true;
        }

        #region 写文件操作
        /// <summary>  
        /// 写文件操作  
        /// </summary>  
        /// <param name="filepath">文件路径</param>  
        /// <param name="pbuff">文件数据</param>  
        private static void WriteFile(string filepath, byte[] pbuff)
        {
            if (File.Exists(filepath) == true)
                File.Delete(filepath);
            FileStream sw = File.OpenWrite(filepath);
            if (pbuff != null && sw != null)
            {
                sw.Write(pbuff, 0, pbuff.Length);
                sw.Close();
            }
        }
        #endregion
    }
}
