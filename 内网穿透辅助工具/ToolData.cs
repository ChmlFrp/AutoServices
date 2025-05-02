using FRPAutoCheckService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace 内网穿透辅助工具
{
    public class ToolData
    {
        private static ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim();
        /// <summary>
        /// 配置文件
        /// </summary>
        public static MyConfig? config { get; set; }
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static string configFileName = Path.Combine(Application.StartupPath, "config.json");
        /// <summary>
        /// 日志文件路径
        /// </summary>
        private static string logFileName = Path.Combine(Application.StartupPath, "log.txt");

        /// <summary>
        /// 日志内容
        /// </summary>
        public static string logText = "";
        /// <summary>
        /// 加载日志内容
        /// </summary>
        public static void LoadLogText()
        {
            if (File.Exists(logFileName))
            {
                try
                {
                    using (FileStream fs = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            logText = sr.ReadToEnd();
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }


        /// <summary>
        /// 加载配置文件
        /// </summary>
        public static void LoadConfig()
        {
            if (File.Exists(configFileName))
            {
                try
                {
                    _configLock.EnterReadLock();
                    using (FileStream fs = new FileStream(configFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            string? jsonText = sr.ReadToEnd();
                            config = JsonConvert.DeserializeObject<MyConfig>(jsonText)!;
                        }
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    _configLock.ExitReadLock();
                }
            }
            else
            {
                config = new MyConfig();
            }
            
        }
        /// <summary>
        /// 保存配置文件
        /// </summary>
        public static void SaveConfig()
        {
            try
            {
                _configLock.EnterWriteLock();

                using (FileStream fs = new FileStream(configFileName,FileMode.Create,FileAccess.Write,FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        string? jsonText = JsonConvert.SerializeObject(config);
                        sw.Write(jsonText);
                        sw.Flush();
                    }
                }
                
            }
            catch (Exception)
            {

            }
            finally
            {
                _configLock.ExitWriteLock();
            }
        }
    }
}
