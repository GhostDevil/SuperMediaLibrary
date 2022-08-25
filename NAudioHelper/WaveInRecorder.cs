#if NET472_OR_GREATER
using NAudio.CoreAudioApi;
using NAudio.Wave;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static SuperMedia.NAudioHelper.NAudioStruct;

namespace SuperMedia.NAudioHelper
{
#if NET472_OR_GREATER
    /// <summary>
    /// 波形输入 
    /// <para>2021-06-07</para>
    /// </summary>
    public class WaveInRecorder : NAudioStruct.RecorderViedw, IDisposable
    {
        public IWaveIn captureDevice = null;
        private readonly SynchronizationContext synchronizationContext;
        public WaveFileWriter waveFile = null;
        public WaveFileWriter waveFileReal = null;
        /// <summary>
        /// 波形值集合
        /// </summary>
        public List<float> volumeMeters = new List<float>();
        /// <summary>
        /// 是否分析声音
        /// </summary>
        public bool IsAnalyzeVoice { get; set; } = false;
        /// <summary>
        /// 是否需要波形值
        /// </summary>
        public bool IsNeedWaveform { get; set; } = false;
        /// <summary>
        /// 记录有人说话时间
        /// </summary>
        private DateTime BeginTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 是否有人说话标志
        /// </summary>
        private bool IsSpeeak { get; set; } = false;

        /// <summary>
        /// 缓存截取声音片段
        /// </summary>
        private List<byte> cacheBuffer = new List<byte>();

        /// <summary>
        /// 采集数据
        /// </summary>
        /// <remarks>
        /// 参数 ：数据，傅里叶，是否有效
        /// </remarks>
        public event Action<byte[], double[], bool> DataAvailable;

        /// <summary>
        /// 委托声音触发
        /// </summary>
        /// <remarks>
        /// 参数 ：数据
        /// </remarks>
        public event Action<byte[]> ReciveRealData;
        /// <summary>
        /// 波形值改变
        /// </summary>
        /// <remarks>
        /// 参数 ：波形值
        /// </remarks>
        public event Action<List<float>> VolumeMetersChange;
        /// <summary>
        /// 发生异常
        /// </summary>
        /// <remarks>
        /// 参数 ：异常
        /// </remarks>
        public event Action<Exception> ExceptionHappen;

        public List<WaveInCapabilities> WaveInCapabilities { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public WaveInRecorder()
        {
            try
            {
                synchronizationContext = SynchronizationContext.Current;
                DataFlow dataFlow = DataFlow.Capture;
                var enumerator = new MMDeviceEnumerator();
                CaptureDevices = enumerator.EnumerateAudioEndPoints(dataFlow, DeviceState.Active)?.ToList();

                if (enumerator.HasDefaultAudioEndpoint(DataFlow.Capture, Role.Console))
                {
                    var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                    SelectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
                }
                else
                {
                    SelectedDevice = CaptureDevices?[0];
                }
                WaveInCapabilities = GetWaveInDevices();
            }
            catch { }
        }
        /// <summary>
        /// 开始音频采集
        /// </summary>
        /// <param name="isCallbacks">回传方式</param>
        public bool Record(bool isCallbacks = false, bool isSaveFile = true, string fileName = "")
        {
            bool b = false;
            IsSaveFile = isSaveFile;
            try
            {
                //List<WaveInCapabilities> waveInCapabilities = GetWaveInDevices();
                CreateWaveInDevice(WaveInCapabilities.FindIndex(o => SelectedDevice.FriendlyName.Contains(o.ProductName)), isCallbacks);
                Directory.CreateDirectory(OutputFolder);
                if (string.IsNullOrWhiteSpace(fileName))
                    CurrentFileName = string.Format("Recorder {0:yyy-MM-dd HH-mm-ss}.wav", DateTime.Now);
                else
                    CurrentFileName = fileName;
                try
                {
                    RecordLevel = SelectedDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
                    SelectedDevice.AudioEndpointVolume.Mute = false;
                }
                catch { }
                if (captureDevice != null)
                {
                    captureDevice.WaveFormat = Sample == SampleType.IEEE_Float ? WaveFormat.CreateIeeeFloatWaveFormat((int)SampleRate, ChannelCount) : new WaveFormat((int)SampleRate, BitDepth, ChannelCount);

                    captureDevice.DataAvailable += new EventHandler<WaveInEventArgs>(WaveSource_DataAvailable);
                    captureDevice.RecordingStopped += new EventHandler<StoppedEventArgs>(WaveSource_RecordingStopped);
                    captureDevice.StartRecording();
                    Message = "Recording...";
                    b = true;
                }

            }
            catch (Exception ex)
            {
                ExceptionHappen?.BeginInvoke(ex, null, null);
                if (captureDevice != null)
                {
                    captureDevice.DataAvailable -= new EventHandler<WaveInEventArgs>(WaveSource_DataAvailable);
                    captureDevice.RecordingStopped -= new EventHandler<StoppedEventArgs>(WaveSource_RecordingStopped);
                    captureDevice.Dispose();
                    captureDevice = null;
                }
            }
            finally
            {

            }
            return b;
        }
        /// <summary>
        /// 创建采集设备对象
        /// </summary>
        /// <param name="deviceWaveInNumber">设备编号</param>
        void CreateWaveInDevice(int deviceWaveInNumber, bool isCallbacks = false)
        {
            IWaveIn newWaveIn = null;
            if (deviceWaveInNumber < 0)
                deviceWaveInNumber = 0;
            if (isCallbacks)
                newWaveIn = new WaveInEvent() { DeviceNumber = deviceWaveInNumber - 1 };
            else
                newWaveIn = new WaveIn() { DeviceNumber = deviceWaveInNumber - 1 };
            newWaveIn.WaveFormat = new WaveFormat((int)SampleRate, ChannelCount);

            captureDevice = newWaveIn;
        }
        /// <summary>
        /// 获取捕获设备
        /// </summary>
        /// <returns></returns>
        List<WaveInCapabilities> GetWaveInDevices()
        {
            var devices = Enumerable.Range(-1, WaveIn.DeviceCount + 1).Select(n => WaveIn.GetCapabilities(n)).ToList();
            return devices;
        }
        /// <summary>
        /// 停止录音
        /// </summary>
        public void Stop()
        {
            captureDevice?.StopRecording();
        }
        /// <summary>
        /// 开始录音回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            UpdatePeakMeter();
            //if (Peak >= LoudnessStant)
            DataAvailable?.BeginInvoke(e.Buffer.ToArray(), null, Peak >= LoudnessStant, null, null);
            try
            {

                if (waveFile == null)
                {
                    waveFile = new WaveFileWriter(Path.Combine(OutputFolder, CurrentFileName), captureDevice.WaveFormat);
                }
                if (IsSaveFile && Peak >= LoudnessStant)
                {
                    waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                    //waveFile.Flush();
                }

                if (waveFileReal == null)
                {
                    waveFileReal = new WaveFileWriter(Path.Combine(OutputFolder, "real_" + CurrentFileName), captureDevice.WaveFormat);
                }

                if (IsAnalyzeVoice)
                    AnalyzeVoice(e.Buffer);
                if (IsNeedWaveform)
                {
                    //根据WaveIn.WaveFormat的channels去获取音频波形值
                    //录音时绘制波形图需要在DataAviliable回调函数中获取音频数据并将其从byte[]转换为float[]，然后用float[]数据做为波形图的输入即可，
                    //这个过程源码上写一个数据包的波形图数据为waveSource.WaveFormat.SampleRate / 100，原理上我还没搞懂，但是的确是这么操作显示是对的，
                    float[] sts = new float[e.Buffer.Length / ChannelCount];
                    int outIndex = 0;
                    for (int n = 0; n < e.Buffer.Length; n += ChannelCount)
                    {
                        sts[outIndex++] = BitConverter.ToInt16(e.Buffer, n) / 32768f;
                    }
                    if (VolumeMetersChange != null)
                    {
                        volumeMeters.Clear();
                        for (int n = 0; n < sts.Length; n += ChannelCount)
                        {
                            volumeMeters.Add(sts[n]);
                        }
                        VolumeMetersChange?.BeginInvoke(volumeMeters.ToList(), null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHappen?.BeginInvoke(ex, null, null);
            }
        }
        void UpdatePeakMeter()
        {
            try
            {
                // 无法在与创建的线程不同的线程上访问此，因此返回GUI线程
                synchronizationContext.Post(s => Peak = SelectedDevice.AudioMeterInformation.MasterPeakValue, null);
            }
            catch (Exception ex) { ExceptionHappen?.BeginInvoke(ex, null, null); }
        }
        /// <summary>
        /// 语音分析
        /// </summary>
        /// <param name="buf"></param>
        private void AnalyzeVoice(byte[] buf)
        {
            try
            {
                float max = LoudnessStant;
                int maxNumber = 0;
                // interpret as 16 bit audio
                for (int index = 0; index < buf.Length; index += 2)
                {
                    short sample = (short)((buf[index + 1] << 8) |
                    buf[index + 0]);
                    // to floating point
                    var sample32 = sample / 32768f;
                    // absolute value
                    if (sample32 < 0) sample32 = -sample32;
                    // is this the max value?
                    if (sample32 > max)
                    {
                        max = sample32;
                        maxNumber++;
                    }
                }

                if (max != LoudnessStant)
                {
                    cacheBuffer.AddRange(buf);
                    IsSpeeak = true;
                    BeginTime = DateTime.Now;
                }
                else
                {
                    if (IsSpeeak)
                    {
                        if ((DateTime.Now - BeginTime).TotalSeconds < 2)
                        {
                            cacheBuffer.AddRange(buf);
                        }
                        else
                        {
                            cacheBuffer.AddRange(buf);
                            if (waveFileReal != null)
                            {
                                waveFileReal.Write(cacheBuffer.ToArray(), 0, cacheBuffer.ToArray().Count());
                                waveFileReal.Flush();
                            }
                            ReciveRealData?.Invoke(cacheBuffer.ToArray());
                            cacheBuffer.Clear();
                            IsSpeeak = false;
                        }
                    }
                    else
                    {
                        if (cacheBuffer.Count > 3200 * 6)
                        {
                            cacheBuffer.RemoveRange(0, 3200);
                        }
                        cacheBuffer.AddRange(buf);
                    }

                }
            }
            catch (Exception ex)
            {
                ExceptionHappen?.BeginInvoke(ex, null, null);
                cacheBuffer.Clear();
                IsSpeeak = false;
            }
        }
        /// <summary>
        /// 先写入wav头文件
        /// </summary>
        void CreateWavFile()
        {
            byte[] bxt = new byte[44] { 82, 73, 70, 70, 36, 124, 7, 0, 87, 65, 86, 69, 102, 109, 116, 32, 16, 0, 0, 0, 1, 0, 1, 0, 64, 31, 0, 0, 128, 62, 0, 0, 2, 0, 16, 0, 100, 97, 116, 97, 0, 124, 7, 0 };
            waveFile.Write(bxt, 0, bxt.Length);
        }
        /// <summary>
        /// 录音结束回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (captureDevice != null)
            {
                captureDevice.Dispose();
                captureDevice = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
            if (waveFileReal != null)
            {
                waveFileReal.Dispose();
                waveFileReal = null;
            }

            if (e.Exception == null)
                Message = "Recording Stopped";
            else
                Message = "Recording Error: " + e.Exception.Message;
        }

        public void Dispose()
        {
            captureDevice?.Dispose();
            captureDevice = null;
        }

    }
#endif
}
