using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SuperMedia.FFmpegHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class SuperFFProbe
    {

        public static List<StreamInfo> GetStreamInfos(string ffPorbePath,string filePath)
        {
            string par = $"ffprobe -prefix -hide_banner -print_format json -show_streams {filePath} >{ffPorbePath}\\info.txt";
            string str = RunProcess(ffPorbePath, par);
            List<StreamInfo> list=new  List<StreamInfo>();
            if (File.Exists(ffPorbePath + "\\info.txt"))
            {
                string txt=File.ReadAllText(ffPorbePath + "\\info.txt");
                list = System.Text.Json.JsonSerializer.Deserialize<List<StreamInfo>>(txt);
            }
           
            return list;
        }
        /// <summary>
        /// 调用 exe 执行命令
        /// </summary>
        /// <param name="ffmegPath">视频处理器ffmpeg.exe的位置</param>
        /// <param name="Parameters">命令参数</param>
        /// <returns>返回执行结果</returns>
        protected static string RunProcess(string ffmegPath, string Parameters)
        {
            //创建一个ProcessStartInfo对象 并设置相关属性
            ProcessStartInfo oInfo = new ProcessStartInfo();
            oInfo.WorkingDirectory = ffmegPath;
            oInfo.FileName = "cmd.exe";
            oInfo.Arguments = Parameters;
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            //oInfo.RedirectStandardOutput = true;
            //oInfo.RedirectStandardError = true;
            //oInfo.RedirectStandardInput = true;

            //创建一个字符串和StreamReader 用来获取处理结果
            string output = null;
            StreamReader srOutput = null;

            try
            {
                //调用ffmpeg开始处理命令
                Process proc = new Process();

                proc.StartInfo = oInfo;
                proc.Start();
                proc.WaitForExit();

                ////获取输出流
                //srOutput = proc.StandardOutput;

                ////转换成string
                //output = srOutput.ReadToEnd();

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
        /// <summary>
        /// 多媒体流中每一个包的信息  
        /// </summary>
        public class Packets
        {
            //"codec_type": "video",
            //   "stream_index": 0,
            //   "pts": 0,
            //   "pts_time": "0.000000",
            //   "dts": -1001,
            //   "dts_time": "-0.041708",
            //   "duration": 1001,
            //   "duration_time": "0.041708",
            //   "size": "2.509000 K",
            //   "pos": "56979",
            //   "flags": "K_"
        }
        /// <summary>
        /// 多媒体流中的每一帧以及字幕的信息 
        /// </summary>
        public class Frames
        {
            //"media_type": "video",
            //   "stream_index": 0,
            //   "key_frame": 1,
            //   "pkt_pts": 0,
            //   "pkt_pts_time": "0.000000",
            //   "pkt_dts": 0,
            //   "pkt_dts_time": "0.000000",
            //   "best_effort_timestamp": 0,
            //   "best_effort_timestamp_time": "0.000000",
            //   "pkt_duration": 1001,
            //   "pkt_duration_time": "0.041708",
            //   "pkt_pos": "56979",
            //   "pkt_size": "2.509000 K",
            //   "width": 1920,
            //   "height": 1080,
            //   "pix_fmt": "yuv420p",
            //   "sample_aspect_ratio": "1:1",
            //   "pict_type": "I",
            //   "coded_picture_number": 0,
            //   "display_picture_number": 0,
            //   "interlaced_frame": 0,
            //   "top_field_first": 0,
            //   "repeat_pict": 0,
            //   "chroma_location": "left"
        }
        /// <summary>
        /// 多媒体流中每一个流的信息
        /// </summary>
        public class FileStreams
        {
            List<StreamInfo> Streams;
        }
        /// <summary>
        /// 多媒体流中每一个流的信息
        /// </summary>
        public class StreamInfo
        {

            //"index": 0,
            //    "codec_name": "h264",
            //    "codec_long_name": "H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10",
            //    "profile": "Main",
            //    "codec_type": "video",
            //    "codec_time_base": "1001/48000",
            //    "codec_tag_string": "avc1",
            //    "codec_tag": "0x31637661",
            //    "width": 1920,
            //    "height": 1080,
            //    "coded_width": 1920,
            //    "coded_height": 1088,
            //    "has_b_frames": 1,
            //    "sample_aspect_ratio": "1:1",
            //    "display_aspect_ratio": "16:9",
            //    "pix_fmt": "yuv420p",
            //    "level": 40,
            //    "chroma_location": "left",
            //    "refs": 1,
            //    "is_avc": "true",
            //    "nal_length_size": "4",
            //    "r_frame_rate": "24000/1001",
            //    "avg_frame_rate": "24000/1001",
            //    "time_base": "1/24000",
            //    "start_pts": 0,
            //    "start_time": "0.000000",
            //    "duration_ts": 3547544,
            //    "duration": "147.814333",
            //    "bit_rate": "4.918868 M",
            //    "bits_per_raw_sample": "8",
            //    "nb_frames": "3544",

            public int Index;
            public string Codec_name;
            public string Codec_long_name;
            public string Profile;
            public string Codec_type;
            public string Codec_time_base;
            public string Codec_tag_string;
            public string Codec_tag;
            public int Width;
            public int Height;
            public int Coded_width;
            public int Coded_height;
            public int Has_b_frames;
            public string Sample_aspect_ratio;
            public string Display_aspect_ratio;
            public string Pix_fmt;
            public int Level;
            public string Chroma_location;
            public int Refs;
            public bool Is_avc;
            public string Nal_length_size;
            public string R_frame_rate;
            public string Avg_frame_rate;
            public string Time_base;
            public int Start_pts;
            public string Start_time;
            public long Duration_ts;
            public string Duration;
            public string Bit_rate;
            public string Bits_per_raw_sample;
            public string Nb_frames;

            public DispositionInfo Disposition;
            public Tag Tags;
            public List<Side_data_list> Side_data_list;
        }
        public class DispositionInfo
        {
            public int Default;
            public int Dub;
            public int Original;
            public int Comment;
            public int Lyrics;
            public int Karaoke;
            public int Forced;
            public int Hearing_impaired;
            public int Visual_impaired;
            public int Clean_effects;
            public int Attached_pic;
            public int Timed_thumbnails;
        }
        public class Tag
        {
            public string Creation_time;
            public string Language;
            public string Handler_name;
            public string Encoder;
        }
        public class Side_data_list
        {
            public string Side_data_type;
        }
    }
}
