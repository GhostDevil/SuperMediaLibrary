#if NET472_OR_GREATER
using NAudio.CoreAudioApi;
using NAudio.Dsp;
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
    /// Windows音频会话应用编程接口
    /// </summary>
    public class WasapiCaptureRecorder :NAudioStruct.RecorderViedw, IDisposable
    {
        private WasapiCapture capture;
        private WaveFileWriter writer;
        public WaveFileWriter waveFileReal = null;
        private readonly SynchronizationContext synchronizationContext;

        /// <summary>
        /// 采集数据
        /// </summary>
        /// <remarks>
        /// 参数 ：数据，傅里叶，是否有效
        /// </remarks>
        public event Action<byte[], double[],bool> DataAvailable;

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
        /// <summary>
        /// 是否分析声音
        /// </summary>
        public bool IsAnalyzeVoice { get; set; } = false;
        /// <summary>
        /// 记录有人说话时间
        /// </summary>
        private DateTime beginTime = DateTime.Now;
        /// <summary>
        /// 是否有人说话标志
        /// </summary>
        private bool isSpeeak = false;
        /// <summary>
        /// 缓存截取声音片段
        /// </summary>
        private List<byte> cacheBuffer = new List<byte>();

        MMDeviceEnumerator mMDeviceEnumerator = null;
        /// <summary>
        /// 构造录音对象
        /// </summary>
        /// <param name="dataFlow">设备类型</param>
        public WasapiCaptureRecorder(DataFlow dataFlow = DataFlow.Capture)
        {
            synchronizationContext = SynchronizationContext.Current;            
            CaptureDevices = GetWasapiDevices(dataFlow);            

            if (mMDeviceEnumerator.HasDefaultAudioEndpoint(DataFlow.Capture, Role.Console))
            {
                var defaultDevice = mMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                SelectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
            }
            else
            {
                SelectedDevice = CaptureDevices?[0];
            }

        }
        /// <summary>
        /// 获取捕获设备
        /// </summary>
        /// <param name="dataFlow">设备类型</param>
        /// <returns></returns>
        List<MMDevice> GetWasapiDevices(DataFlow dataFlow = DataFlow.All)
        {
            mMDeviceEnumerator = new MMDeviceEnumerator();
            var devices = mMDeviceEnumerator.EnumerateAudioEndPoints(dataFlow, DeviceState.Active).ToList();
            return devices;
        }
        /// <summary>
        /// 停止录音
        /// </summary>
        public void Stop()
        {
            capture?.StopRecording();
        }
        /// <summary>
        /// 开始录音
        /// </summary>
        /// <param name="isSaveFile">是否存储文件</param>
        /// <param name="fileName">文件名称</param>
        public bool Record(bool isSaveFile = false, string fileName = "")
        {
            try
            {
                IsSaveFile = isSaveFile;
                Directory.CreateDirectory(OutputFolder);
                if (((MMDevice)SelectedDevice).DataFlow == DataFlow.Capture)
                {
                    capture = new WasapiCapture((MMDevice)SelectedDevice);
                    capture.WaveFormat = Sample == SampleType.IEEE_Float ? WaveFormat.CreateIeeeFloatWaveFormat((int)SampleRate, ChannelCount) : new WaveFormat((int)SampleRate, BitDepth, ChannelCount);

                }
                else
                {
                    capture = new WasapiLoopbackCapture((MMDevice)SelectedDevice);
                }
                capture.ShareMode = UseMode == UseMode.Exclusive ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared;
                if (string.IsNullOrWhiteSpace(fileName))
                    CurrentFileName = string.Format("NAudioDemo {0:yyy-MM-dd HH-mm-ss}.wav", DateTime.Now);
                else
                    CurrentFileName = fileName;
                RecordLevel = ((MMDevice)SelectedDevice).AudioEndpointVolume.MasterVolumeLevelScalar;
                ((MMDevice)SelectedDevice).AudioEndpointVolume.Mute = false;
                capture.RecordingStopped += OnRecordingStopped;
                capture.DataAvailable += CaptureOnDataAvailable;
            
                capture.StartRecording();
                Message = "Recording...";
                return true;
            }
            catch (Exception e)
            {
                Message = e.Message;
            }
            return false;
        }

        private void CaptureOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            UpdatePeakMeter();
            if (writer == null)
            {
                writer = new WaveFileWriter(Path.Combine(OutputFolder, CurrentFileName), capture.WaveFormat);
            }
            if (IsSaveFile && Peak >= LoudnessStant)
                writer.Write(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
            if (waveFileReal == null)
            {
                waveFileReal = new WaveFileWriter(Path.Combine(OutputFolder, "real_" + CurrentFileName), capture.WaveFormat);
            }
            
            double[] result = null;
            //try
            //{
            //    // 既然我们已经知道了, 那些数据都是一个个的采样, 自然也可以通过它们来绘制频谱, 只需要进行快速傅里叶变换即可
            //    // 而且有意思的是, NAudio 也为我们准备好了快速傅里叶变换的方法, 位于 NAudio.Dsp 命名空间下
            //    // 提示: 进行傅里叶变换所需要的复数(Complex)类也位于 NAudio.Dsp 命名空间, 它有两个字段, X(实部) 与 Y(虚部)
            //    // 下面给出在 IeeeFloat 格式下的音乐可视化的简单示例:
            //    if (Sample == SampleType.IEEE_Float)
            //    {
            //        float[] samples = Enumerable
            //                      .Range(0, waveInEventArgs.BytesRecorded / 4)
            //                      .Select(i => BitConverter.ToSingle(waveInEventArgs.Buffer, i * 4))
            //                      .ToArray();   // 获取采样

            //        int log = (int)Math.Ceiling(Math.Log(samples.Length, 2));
            //        float[] filledSamples = new float[(int)Math.Pow(2, log)];
            //        Array.Copy(samples, filledSamples, samples.Length);   // 填充数据

            //        int sampleRate = (sender as WasapiCapture).WaveFormat.SampleRate;    // 获取采样率
            //        Complex[] complexSrc = filledSamples.Select(v => new Complex() { X = v }).ToArray();  // 转换为复数
            //        FastFourierTransform.FFT(false, log, complexSrc);     // 进行傅里叶变换
            //        result = complexSrc.Select(v => Math.Sqrt(v.X * v.X + v.Y * v.Y)).ToArray();    // 取得结果
            //    }
            //}
            //catch { }
            int bytesPerSample = capture.WaveFormat.BitsPerSample / 8;
            float[] Samples;
            lock (SamplesLock)
                Samples = Enumerable
                              .Range(0, waveInEventArgs.BytesRecorded / 4)
                              .Select(i => BitConverter.ToSingle(waveInEventArgs.Buffer, i * 4)).ToArray();   // 获取采样
            ProcessFrame(Samples);
            result = DftData;
            //if (Peak >= LoudnessStant)
                DataAvailable?.BeginInvoke(waveInEventArgs.Buffer.ToArray(), result, Peak >= LoudnessStant,null, null);
            if (IsAnalyzeVoice)
                AnalyzeVoice(waveInEventArgs.Buffer);
        }
    #region 傅里叶数据
        double frequencyPerIndex;
        double[] DftData;
        readonly object SamplesLock = new object();
        readonly object DftDataLock = new object();
        /// <summary>
        /// 根据 Samples, 将采样进行傅里叶变换以求得 DftData
        /// </summary>
        /// <param name="Samples">保存的样本</param>
        private void ProcessFrame(float[] Samples)
        {
            if (Samples == null)
                return;
            float[][] chanelSamples;
            lock (SamplesLock)
                chanelSamples = Enumerable
                    .Range(0, ChannelCount)
                    .Select(i => Enumerable
                        .Range(0, Samples.Length / ChannelCount)
                        .Select(j => Samples[i + j * ChannelCount])
                        .ToArray())
                    .ToArray();

            float[] chanelAverageSamples = Enumerable
                .Range(0, chanelSamples[0].Length)
                .Select(i => Enumerable
                    .Range(0, ChannelCount)
                    .Select(j => chanelSamples[j][i])
                    .Average())
                .ToArray();

            float[] sampleSrc = chanelAverageSamples;
            int log = (int)Math.Floor(Math.Log(sampleSrc.Length, 2));
            float[] filledSamples = new float[(int)Math.Pow(2, log)];
            Array.Copy(sampleSrc, filledSamples, Math.Min(sampleSrc.Length, filledSamples.Length));   // 填充数据
            Complex[] complexSrc = filledSamples.Select((v, i) => new Complex() { X = v }).ToArray();
            FastFourierTransform.FFT(false, log, complexSrc);     // 进行傅里叶变换
            double[] result = complexSrc.Select(v => Math.Sqrt(v.X * v.X + v.Y * v.Y)).ToArray();    // 取得结果
            double[] reresult = result.Take(result.Length / 2).ToArray();                            // 取一半

            frequencyPerIndex = (double)SampleRate / filledSamples.Length;
            UpdateDftData(reresult, 0.8, 0.5);
        }
        /// <summary>
        /// 平滑的更新 DftData
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="upParam"></param>
        /// <param name="downParam"></param>
        /// <returns></returns>
        private double[] UpdateDftData(double[] newData, double upParam = 1, double downParam = 1)
        {
            if (DftData == null || DftData.Length == 0)
                return DftData = newData.Select(v => v * upParam).ToArray();
            lock (DftDataLock)
            {
                try
                {
                    return DftData = newData.Select((v, i) =>
                    {
                        double lastData = GetCurvePoint(DftData, (double)i / newData.Length);
                        double incre = v - lastData;
                        return lastData + incre * (incre > 0 ? upParam : downParam);
                    }).ToArray();
                }
                catch (IndexOutOfRangeException)
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 从 double 中平滑的取得一个值
        /// 例如 curve[0] = 0, curve[1] = 100, 那么通过此方法访问 curve[0.5], 可得到 50
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        double IndexCurvePoint(double[] curve, double index)
        {
            int
                floor = (int)Math.Min(Math.Floor(index), curve.Length - 1),
                ceiling = (int)Math.Min(Math.Ceiling(index), curve.Length - 1);
            if (floor == ceiling)
                return curve[floor];
            double
                left = curve[floor],
                right = curve[ceiling];
            return left + (right - left) * (index - floor);
        }
        /// <summary>
        /// 从 double 中平滑的获取一个值
        /// 索引以百分比的形式指定, 基本原理时调用 GetCurvePoint
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        double GetCurvePoint(double[] sequence, double percent)
        {
            return IndexCurvePoint(sequence, percent * sequence.Length);
        }
    #endregion

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
                    isSpeeak = true;
                    beginTime = DateTime.Now;
                }
                else
                {
                    if (isSpeeak)
                    {
                        if ((DateTime.Now - beginTime).TotalSeconds < 2)
                        {
                            cacheBuffer.AddRange(buf);
                        }
                        else
                        {
                            cacheBuffer.AddRange(buf);
                            if (waveFileReal != null && IsSaveFile)
                            {
                                waveFileReal.Write(cacheBuffer.ToArray(), 0, cacheBuffer.ToArray().Count());
                                waveFileReal.Flush();
                            }
                            ReciveRealData?.Invoke(cacheBuffer.ToArray());
                            cacheBuffer.Clear();
                            isSpeeak = false;
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
            catch
            {
                cacheBuffer.Clear();
                isSpeeak = false;
            }
        }
        void UpdatePeakMeter()
        {
            try
            {
                // 无法在与创建的线程不同的线程上访问此，因此返回GUI线程
                synchronizationContext?.Post(s => Peak = ((MMDevice)SelectedDevice).AudioMeterInformation.MasterPeakValue, null);
            }
            catch(Exception ex) { }
        }

        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            writer?.Dispose();
            writer = null;
            waveFileReal?.Dispose();
            waveFileReal = null;
            if (e.Exception == null)
                Message = "Recording Stopped";
            else
                Message = "Recording Error: " + e.Exception.Message;

        }
        //private void GetDefaultRecordingFormat(MMDevice value)
        //{
        //    using (var c = new WasapiCapture(value))
        //    {
        //        Sample = c.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat ? SampleType.IEEE_Float : SampleType.PCM;
        //        SampleRate = (RateMode)c.WaveFormat.SampleRate;
        //        BitDepth = c.WaveFormat.BitsPerSample;
        //        ChannelCount = c.WaveFormat.Channels;
        //        Message = "";
        //    }
        //}        
        public void Dispose()
        {
            Stop();
            capture?.Dispose();
            capture = null;
        }
    }
#endif
}
