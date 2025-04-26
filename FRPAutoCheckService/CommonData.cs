using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FRPAutoCheckService
{
    public class CommonData
    {

        private static ReaderWriterLockSlim _configLock = new ReaderWriterLockSlim();
        /// <summary>
        /// 配置文件
        /// </summary>
        public static MyConfig config { get; set; }
        /// <summary>
        /// 配置文件路径
        /// </summary>
        private static string configFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        /// <summary>
        /// 日志文件路径
        /// </summary>
        private static string logFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
                

        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="content"></param>
        public static void PrintLog(string content)
        {
            if (!File.Exists(logFileName))
            {
                File.Create(logFileName).Close();
            }
            try
            {
                using (FileStream fs = new FileStream(logFileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        sw.WriteLine($"{dateStr}  {content}");
                        sw.Flush();
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 清空日志文件
        /// </summary>
        public static void ClearLog()
        {
            try 
            {
                if (File.Exists(logFileName))
                {
                    File.WriteAllText(logFileName,"");
                }
            }catch (Exception) { }
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
                            string jsonText = sr.ReadToEnd();
                            config = JsonConvert.DeserializeObject<MyConfig>(jsonText);
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

                using (FileStream fs = new FileStream(configFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        string jsonText = JsonConvert.SerializeObject(config);
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
