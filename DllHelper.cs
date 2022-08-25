using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SuperMedia
{
    /// <summary>
    /// Dll配置
    /// </summary>
    public class DllHelper
    {

        private const string LD_LIBRARY_PATH = "LD_LIBRARY_PATH";
        /// <summary>
        /// 获取设置Dll路径
        /// </summary>
        /// <returns></returns>
        public static string RegisterBinaries(string dirPath)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    var current = Environment.CurrentDirectory;
                    var probe = $"{dirPath}/{(Environment.Is64BitProcess ? @"x64" : @"x86")}";
                    while (current != null)
                    {
                        var ffmpegDirectory = Path.Combine(current, probe);
                        if (Directory.Exists(ffmpegDirectory))
                        {
                            Console.WriteLine($"binaries found in: {ffmpegDirectory}");
                            RegisterLibrariesSearchPath(ffmpegDirectory);
                            return ffmpegDirectory;
                        }
                        current = Directory.GetParent(current)?.FullName;
                    }
                    return current;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    var libraryPath = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);
                    return RegisterLibrariesSearchPath(libraryPath);
                default:
                    return "";
            }
        }
        private static string RegisterLibrariesSearchPath(string path)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    SetDllDirectory(path);
                    return path;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    string currentValue = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH);
                    if (string.IsNullOrWhiteSpace(currentValue) == false && currentValue.Contains(path) == false)
                    {
                        currentValue = currentValue + Path.PathSeparator + path;
                        Environment.SetEnvironmentVariable(LD_LIBRARY_PATH, currentValue);
                    }
                    return currentValue;
                default:
                    return "";
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}
