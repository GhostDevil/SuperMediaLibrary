#if NET472_OR_GREATER
using NAudio.CoreAudioApi;
using NAudio.Wave;
#endif
using System.Collections.Generic;
using System.Linq;

namespace SuperMedia.NAudioHelper
{
    public class NAudioHelper
    {
#if NET472_OR_GREATER
        /// <summary>
        /// 获取当前系统麦克风音量
        /// </summary>
        /// <returns>volume 取值 0-100</returns>
        public static int GetCurrentMicVolume()
        {
            int volume = 0;
            var enumerator = new MMDeviceEnumerator();
            //获取音频输入设备
            IEnumerable<MMDevice> captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
            if (captureDevices.Count() > 0)
            {
                MMDevice mMDevice = captureDevices.ToList()[0];
                volume = (int)(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }
            return volume;
        }
        /// <summary>
        /// 设置当前系统麦克风音量 
        /// </summary>
        /// <param name="volume">volume 取值 0-100</param>
        public void SetCurrentMicVolume(int volume)
        {
            var enumerator = new MMDeviceEnumerator();
            IEnumerable<MMDevice> captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
            if (captureDevices.Count() > 0)
            {
                MMDevice mMDevice = captureDevices.ToList()[0];
                mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = volume / 100.0f;
            }
        }
        public void ConvertToWav(string sourceFile,string outFile)
        {
            // 对于 Wave, CueWave, Aiff, 这些格式都有其对应的 FileWriter, 我们可以直接调用其 Writer 的 Create***File 来
            // 从 IWaveProvider 创建对应格式的文件. 对于 MP3 这类没有 FileWriter 的格式, 可以调用 MediaFoundationEncoder

            // 例如一个文件, "./Disconnected.mp3", 我们要将它转换为 wav 格式, 只需要使用下面的代码, CurWave 与 Aiff 同理
            using (Mp3FileReader reader = new Mp3FileReader(sourceFile))
                WaveFileWriter.CreateWaveFile(outFile, reader);
        }
        public void ConvertToMp3(WaveFileReader sourceFile, string outFile)
        {
            // 从 IWaveProvider 创建 MP3 文件, 假如一个 WaveFileReader 为 src
            MediaFoundationEncoder.EncodeToMp3(sourceFile, outFile);
        }
#endif
    }
}
