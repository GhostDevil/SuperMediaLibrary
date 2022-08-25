#if NET472_OR_GREATER
using NAudio.CoreAudioApi;
using NAudio.Gui;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperMedia.NAudioHelper
{
    public class AudioPlayback
    {
#if NET472_OR_GREATER
        private IWavePlayer waveOut;
        private string fileName;
        private AudioFileReader audioFileReader;
        VolumeMeter volumeMeter1;
        VolumeMeter volumeMeter2;
        private WaveformPainter waveformPainter1;
        private WaveformPainter waveformPainter2;
        SampleChannel sampleChannel;
        public WaveCallbackStrategy WaveOutCallbackStrategy { get; set; } = WaveCallbackStrategy.NewWindow;
        /// <summary>
        /// waveout 回调机制
        /// </summary>
        public List<string> WaveOutCallbackStrategyName { get { return Enum.GetNames(typeof(WaveCallbackStrategy))?.ToList(); } }
        /// <summary>
        /// 播放插件
        /// </summary>
        public IEnumerable<IOutputDevicePlugin> plugins = null;
        /// <summary>
        /// 选择播放插件
        /// </summary>
        public IOutputDevicePlugin SelectedOutputDevicePlugin 
        { 
            get;
            set;
        }
        public List<string> Device 
        { 
            get { return SelectedOutputDevicePlugin?.Device; } 
        }
        public int deviceIndex=0;
        /// <summary>
        /// 输出设备
        /// </summary>
        public int SelectedOutputDeviceIndex
        {
            get { return deviceIndex; }
            set
            {
                deviceIndex = value;
                CreateWaveOut(deviceIndex);
            }
        }
        /// <summary>
        /// 声音 0-100
        /// </summary>
        public int Volumen
        {
            get { return (int)(sampleChannel?.Volume * 100); }
            set { setVolumeDelegate?.Invoke((float)value / 100); }
        }
        public PlaybackState? PlayState { get { return waveOut?.PlaybackState; } }
        
        private Action<float> setVolumeDelegate;
        public delegate void VolumeMeterChangeHandel(double left, double right);
        /// <summary>
        /// 峰值改变
        /// </summary>
        public event VolumeMeterChangeHandel VolumeMeterChange;
        public delegate void ExceptionHappenHandel(Exception ex);
        /// <summary>
        /// 发生异常
        /// </summary>
        public event ExceptionHappenHandel ExceptionHappen;
        public delegate void ProgressChangeHandel(double value);
        /// <summary>
        /// 播放进度改变时
        /// </summary>
        public event ProgressChangeHandel ProgressChange;
        public delegate void PlayChangeHandel(PlaybackState state);
        /// <summary>
        /// 播放改变时
        /// </summary>
        public event PlayChangeHandel PlayChange;
        /// <summary>
        /// 峰值延迟 ms
        /// </summary>
        public int LatencyMeter { get; set; } = 300;
        public AudioPlayback()
        {
            plugins = LoadOutputDevicePlugins(ReflectionHelper.CreateAllInstancesOf<IOutputDevicePlugin>());
            SetVolumeMeter();
        }
        private IEnumerable<IOutputDevicePlugin> LoadOutputDevicePlugins(IEnumerable<IOutputDevicePlugin> outputDevicePlugins)
        {
            return outputDevicePlugins.OrderBy(p => p.Priority);
        }
        /// <summary>
        /// 设置体积仪表
        /// </summary>
        void SetVolumeMeter()
        {
            volumeMeter1 = new VolumeMeter();
            volumeMeter2 = new VolumeMeter();
            waveformPainter1 = new WaveformPainter();
            waveformPainter2 = new WaveformPainter();
        }
        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="deviceIndex">设备索引号</param>
        /// <returns></returns>
        public bool Play(int deviceIndex,string file)
        {
            if (!SelectedOutputDevicePlugin.IsAvailable)
            {
                ExceptionHappen.BeginInvoke(new Exception("The selected output driver is not available on this system"),null,null);
                return false;
            }

            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    return true;
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                    PlayChange?.BeginInvoke(PlaybackState.Playing, null, null);
                    return true;
                }
                //else
                //{ 
                //    waveOut?.Dispose();
                //    waveOut = null;
                //}
            }
            else
            {
                try
                {
                    CreateWaveOut(deviceIndex);
                }
                catch (Exception driverCreateException)
                {
                    ExceptionHappen?.BeginInvoke(driverCreateException, null, null);
                    return false;
                }
            }
            fileName = file;
            ISampleProvider sampleProvider;
            try
            {
                sampleProvider = CreateInputStream(fileName);
            }
            catch (Exception createException)
            {
                ExceptionHappen?.BeginInvoke(createException, null, null);
                return false;
            }
            try
            {
                waveOut.Init(sampleProvider);
            }
            catch (Exception initException)
            {
                ExceptionHappen?.BeginInvoke(initException, null, null);
                return false;
            }
            setVolumeDelegate(Volumen / 100);
            waveOut.Play();
            PlayChange?.BeginInvoke(PlaybackState.Playing, null, null);
          
            _=Task.Factory.StartNew(new Action(async () =>
            {
                while (waveOut?.PlaybackState == PlaybackState.Playing)
                {
                    TimeSpan currentTime = (waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : audioFileReader.CurrentTime;
                    double value = 100 * (currentTime.TotalSeconds / audioFileReader.TotalTime.TotalSeconds);   
                    ProgressChange?.BeginInvoke(value,null,null);
                    await Task.Delay(25);
                }
            }));
            return true;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        public bool Pause()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Pause();
                    PlayChange?.BeginInvoke(PlaybackState.Paused, null, null);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Stop();
                    PlayChange?.BeginInvoke(PlaybackState.Stopped, null, null);
                    CloseWaveOut();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 创建播放对象
        /// </summary>
        /// <param name="deviceIndex">设备索引号</param>
        void CreateWaveOut(int deviceIndex)
        {
            
            if (SelectedOutputDevicePlugin != null)
            {                
                if (SelectedOutputDevicePlugin.IsAvailable)
                {
                    if (waveOut != null)
                    {
                        CloseWaveOut();
                    }
                    waveOut = SelectedOutputDevicePlugin.CreateDevice(deviceIndex, LatencyMeter);
                    waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                }
                else
                {
                    ExceptionHappen.BeginInvoke(new Exception("此输出设备在您的系统上不可用!!!"), null, null);
                }
            }
            
        }
        /// <summary>
        /// 关闭播放对象
        /// </summary>
        void CloseWaveOut()
        {
            try
            {
                if (waveOut?.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut?.Stop();
                    //PlayChange?.BeginInvoke(PlaybackState.Stopped, null, null);
                }
                if (audioFileReader != null)
                {
                    // this one really closes the file and ACM conversion
                    audioFileReader.Dispose();
                    setVolumeDelegate = null;
                    audioFileReader = null;
                }
                waveOut?.Dispose();
                waveOut = null;
            }
            catch(Exception ex) { ExceptionHappen?.BeginInvoke(ex, null, null); }
        }
        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                // MessageBox.Show(e.Exception.Message, "Playback Device Error");
            }
            if (audioFileReader != null)
            {
                audioFileReader.Position = 0;
            }
            PlayChange?.BeginInvoke(PlaybackState.Stopped, null, null);
            SetVolumeMeter();
        }
        private ISampleProvider CreateInputStream(string fileName)
        {
            try
            {
                audioFileReader = new AudioFileReader(fileName);
                sampleChannel = new SampleChannel(audioFileReader, true);
                sampleChannel.PreVolumeMeter += OnPreVolumeMeter;
                setVolumeDelegate = vol => sampleChannel.Volume = vol;
                var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
                postVolumeMeter.StreamVolume += OnPostVolumeMeter;
                return postVolumeMeter;
            }
            catch (Exception ex) { ExceptionHappen?.BeginInvoke(ex, null, null); }
            return null;
            
        }
        void OnPreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {

            try
            {
                // 我们知道它是立体声的
                //waveformPainter1?.AddMax(e.MaxSampleValues[0]);
                //waveformPainter2?.AddMax(e.MaxSampleValues[1]);
            }
            catch (Exception ex) { ExceptionHappen?.BeginInvoke(ex, null, null); }

        }

        void OnPostVolumeMeter(object sender, StreamVolumeEventArgs e)
        {

            try
            {
                // 我们知道它是立体声的
                //if (volumeMeter1 != null)
                //    volumeMeter1.Amplitude = e.MaxSampleValues[0];
                //if (volumeMeter2 != null)
                //    volumeMeter2.Amplitude = e.MaxSampleValues[1];
                VolumeMeterChange?.BeginInvoke(e.MaxSampleValues[0], e.MaxSampleValues[1], null, null);
            }
            catch(Exception ex) { ExceptionHappen?.BeginInvoke(ex, null, null); }
        }
        
        #region 插件
        public class AsioOutPlugin : IOutputDevicePlugin
        {
            public string Name { get; } = "AsioOut";
            public bool IsAvailable { get; } = AsioOut.isSupported(); 
            public int Priority { get; } = 4;

            public List<string> Device 
            {
                get 
                {
                    return AsioOut.GetDriverNames()?.ToList(); 
                }
            }

            public IWavePlayer CreateDevice(int selectedIndex, int latency)
            {
                var asioDriverNames = AsioOut.GetDriverNames();
                return new AsioOut(asioDriverNames[selectedIndex]);
            }
        }
        public class DirectSoundOutPlugin : IOutputDevicePlugin
        {
            private readonly bool isAvailable;

            public DirectSoundOutPlugin()
            {
                isAvailable = DirectSoundOut.Devices.Any();
            }
            public List<string> Device
            {
                get
                {
                    List<string> vsDirectSound = new List<string>();
                    foreach (var item in DirectSoundOut.Devices)
                    {
                        vsDirectSound.Add(item.Description);
                    }
                    return vsDirectSound;
                }
            }
            public IWavePlayer CreateDevice(int selectedIndex, int latency)
            {                
                return new DirectSoundOut(DirectSoundOut.Devices.ToList()[selectedIndex].Guid, latency);
            }
            public string Name
            {
                get { return "DirectSound"; }
            }

            public bool IsAvailable
            {
                get { return isAvailable; }
            }

            public int Priority
            {
                get { return 2; }
            }
        }
        public class WasapiOutPlugin : IOutputDevicePlugin
        {
            public IWavePlayer CreateDevice(int selectedIndex, int latency)
            {
                var wasapi = new WasapiOut(
                    InitialiseWasapiControls()[selectedIndex],
                    ShareMode,
                    UseEventCallback,
                    latency);
                return wasapi;
            }
            public List<string> Device
            {
                get
                {
                    List<string> vs = new List<string>();
                    foreach (var item in InitialiseWasapiControls())
                    {
                        vs.Add(item.FriendlyName);
                    }
                    return vs;
                }
            }
            public string Name
            {
                get { return "WasapiOut"; }
            }

            public bool IsAvailable
            {
                // supported on Vista and above
                get { return Environment.OSVersion.Version.Major >= 6; }
            }

            public int Priority
            {
                get { return 3; }
            }
            private List<MMDevice> InitialiseWasapiControls()
            {
                var enumerator = new MMDeviceEnumerator();
                var endPoints = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
                return endPoints;
 
            }
            public AudioClientShareMode ShareMode {get;set;}

            public bool UseEventCallback { get; set; }
        }
        public class WaveOutPlugin : IOutputDevicePlugin
        {
            public WaveCallbackStrategy CallbackStrategy { get; set; } = WaveCallbackStrategy.NewWindow;
           
            public IWavePlayer CreateDevice(int selectedIndex, int latency)
            {
                IWavePlayer device;
                if (CallbackStrategy == WaveCallbackStrategy.Event)
                {
                    var waveOut = new WaveOutEvent
                    {
                        DeviceNumber = selectedIndex - 1,
                        DesiredLatency = latency
                    };
                    device = waveOut;
                }
                else
                {
                    WaveCallbackInfo callbackInfo = CallbackStrategy == WaveCallbackStrategy.NewWindow ? WaveCallbackInfo.NewWindow() : WaveCallbackInfo.FunctionCallback();
                    WaveOut outputDevice = new WaveOut(callbackInfo)
                    {
                        DeviceNumber = selectedIndex - 1,
                        DesiredLatency = latency
                    };
                    device = outputDevice;
                }
                // TODO: configurable number of buffers

                return device;
            }
            public List<string> Device
            {
                get
                {
                    List<string> vs = new List<string>();
                    foreach (var item in InitialiseDevice())
                    {
                        vs.Add(item.ProductName);
                    }
                    return vs;
                }
            }
            public List<WaveOutCapabilities> InitialiseDevice()
            {
                List<WaveOutCapabilities> list = new List<WaveOutCapabilities>();
                if (WaveOut.DeviceCount <= 0) return list;

                for (var deviceId = -1; deviceId < WaveOut.DeviceCount; deviceId++)
                {
                    var capabilities = WaveOut.GetCapabilities(deviceId);
                    list.Add(capabilities);
                    //comboBoxWaveOutDevice.Items.Add($"Device {deviceId} ({capabilities.ProductName})");
                }
                return list;
                //comboBoxWaveOutDevice.SelectedIndex = 0;
            }

            public string Name
            {
                get { return "WaveOut"; }
            }

            public bool IsAvailable
            {
                get { return WaveOut.DeviceCount > 0; }
            }

            public int Priority
            {
                get { return 1; }
            }
            public List<string> Strategy { get; } = new List<string>() { "NewWindow", "FunctionCallback", "Event" };
        }
        #endregion

        #region 接口
        public interface IOutputDevicePlugin
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="latency">峰值获取延迟</param>
            /// <returns></returns>
            IWavePlayer CreateDevice(int selectedIndex,int latency);
            List<string> Device { get; }
            //UserControl CreateSettingsPanel();
            string Name { get; }
            bool IsAvailable { get; }
            int Priority { get; }
        }
        #endregion
#endif
    }

}
