#if NET472_OR_GREATER
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
#endif
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace SuperMedia.NAudioHelper
{
    public class NAudioStruct
    {
#if NET472_OR_GREATER
        public enum SampleType
        {

            IEEE_Float = 0,
            PCM = 1,
        }
        /// <summary>
        /// 使用模式
        /// </summary>
        public enum UseMode
        {
            /// <summary>
            /// 共享
            /// </summary>
            Shared=0,
            /// <summary>
            /// 独占
            /// </summary>
            Exclusive=1,
        }
        /// <summary>
        /// 采样率
        /// </summary>
        public enum RateMode
        {
            Rate_8000 = 8000,
            Rate_16000 = 16000,
            Rate_22050 = 22050,
            Rate_32000 = 32000,
            Rate_44100 = 44100,
            Rate_48000 = 48000,
            Rate_96000 = 96000,
            Rate_192000 = 192000,
        }
        /// <summary>
        /// 声道模式
        /// </summary>
        public enum ChannelMode
        {
            /// <summary>
            /// 单声道
            /// </summary>
            Mono = 1,
            /// <summary>
            /// 立体声
            /// </summary>
            Stereo = 2
        }

        public class RecorderViedw : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private void GetDefaultRecordingFormat(MMDevice value)
            {
                using (var c = new WasapiCapture(value))
                {
                    Sample = c.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat ? SampleType.IEEE_Float : SampleType.PCM;
                    SampleRate = (RateMode)c.WaveFormat.SampleRate;
                    BitDepth = c.WaveFormat.BitsPerSample;
                    ChannelCount = c.WaveFormat.Channels;
                    Message = "";
                }
            }
            //private void GetDefaultRecordingFormat(object value)
            //{
            //    WaveInCapabilities waveInCapabilities = (WaveInCapabilities)value;
            //         Sample =  SampleType.PCM;
            //        SampleRate = (RateMode)c.WaveFormat.SampleRate;
            //        BitDepth = c.WaveFormat.BitsPerSample;
            //        ChannelCount = c.WaveFormat.Channels;
            //        Message = "";
                
            //}
            /// <summary>
            /// 是否存储文件
            /// </summary>
            public bool IsSaveFile { get; set; } = true;
            /// <summary>
            /// 声音响度标准
            /// </summary>
            public float LoudnessStant { get; set; } = 0.08F;
            public string CurrentFileName { get; set; }
            private SampleType sample;
            private MMDevice selectedDevice;
            private RateMode sampleRate;
            private int bitDepth;
            private int channelCount;
            private string message;
            private float peak;
            private float recordLevel;
            private UseMode useMode;
            /// <summary>
            /// 文件输出目录
            /// </summary>
            public string OutputFolder { get; set; } = Path.Combine(Path.GetTempPath(), "Recorder");
            /// <summary>
            /// 捕获设备
            /// </summary>
            public List<MMDevice> CaptureDevices { get; internal set; }
            
            /// <summary>
            /// 模式
            /// </summary>
            public UseMode UseMode
            {
                get => useMode;
                set
                {
                    if (useMode != value)
                    {
                        useMode = value;
                        OnPropertyChanged(nameof(UseMode));
                    }
                }
            }
            /// <summary>
            /// 声音峰值
            /// </summary>
            public float Peak
            {
                get => peak;
                set
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (peak != value)
                    {
                        peak = value;
                        OnPropertyChanged(nameof(Peak));
                    }
                }
            }
            /// <summary>
            /// 选择的设备 WaveInCapabilities or MMDevice
            /// </summary>
            public MMDevice SelectedDevice
            {
                get => selectedDevice;
                set
                {
                    if (selectedDevice != value)
                    {
                        selectedDevice = value;
                        OnPropertyChanged(nameof(SelectedDevice));
                        GetDefaultRecordingFormat(value);
                    }
                }
            }


            /// <summary>
            /// 采样率
            /// </summary>
            public RateMode SampleRate
            {
                get => sampleRate;
                set
                {
                    if (sampleRate != value)
                    {
                        sampleRate = value;
                        OnPropertyChanged(nameof(SampleRate));
                    }
                }
            }
            /// <summary>
            /// 位深 Bit
            /// </summary>
            public int BitDepth
            {
                get => bitDepth;
                set
                {
                    if (bitDepth != value)
                    {
                        bitDepth = value;
                        OnPropertyChanged(nameof(BitDepth));
                    }
                }
            }
            ///// <summary>
            ///// 当前设备索引
            ///// </summary>
            //public int SelectedIndex
            //{
            //    get => selectedIndex;
            //    set
            //    {
            //        if (selectedIndex != value)
            //        {
            //            selectedIndex = value;
            //            OnPropertyChanged(nameof(SelectedIndex));
            //        }
            //    }
            //}
            /// <summary>
            /// 声道数
            /// </summary>
            public int ChannelCount
            {
                get => channelCount;
                set
                {
                    if (channelCount != value)
                    {
                        channelCount = value;
                        OnPropertyChanged(nameof(ChannelCount));
                    }
                }
            }

            public bool IsBitDepthConfigurable => Sample == SampleType.PCM;
            /// <summary>
            /// 模型
            /// </summary>
            public SampleType Sample
            {
                get => sample;
                set
                {
                    if (sample != value)
                    {
                        sample = value;
                        OnPropertyChanged("Sample");
                        BitDepth = sample == SampleType.PCM ? 16 : 32;
                        OnPropertyChanged(nameof(IsBitDepthConfigurable));
                    }
                }
            }
            /// <summary>
            /// 消息
            /// </summary>
            public string Message
            {
                get => message;
                set
                {
                    if (message != value)
                    {
                        message = value;
                        OnPropertyChanged(nameof(Message));
                    }
                }
            }
            /// <summary>
            /// 声音级别
            /// </summary>
            public float RecordLevel
            {
                get => recordLevel;
                set
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (recordLevel != value)
                    {
                        recordLevel = value;
                        if (SelectedDevice != null)
                        {
                            SelectedDevice.AudioEndpointVolume.MasterVolumeLevelScalar = value;
                        }
                        OnPropertyChanged(nameof(RecordLevel));
                    }
                }
            }
        }
#endif
        ///// <summary>
        ///// 设备模式
        ///// </summary>
        //public enum DeviceMode
        //{
        //    /// <summary>
        //    /// 录制麦克风
        //    /// </summary>
        //    WaveIn = 1,
        //    /// <summary>
        //    /// 录制麦克风
        //    /// </summary>
        //    WaveInEvent,
        //    /// <summary>
        //    /// 录制麦克风
        //    /// </summary>
        //    WasApi,
        //    /// <summary>
        //    /// 录制扬声器的声音 循环录制
        //    /// </summary>
        //    WasApiLoopback,
        //}
    }
}
