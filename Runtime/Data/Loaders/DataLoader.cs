using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataEngine.Data.Serializer;
using Newtonsoft.Json.Linq;

namespace DataEngine.Data
{
    public class DataLoader
    {
        public enum DataLoaderPath
        {
            Root,
            Assets,
            Custom,
            Default,
            Persistent,
        }

        public static bool autoSave = false;
        public static DataLoaderPath pathType = DataLoaderPath.Persistent;
        public static string path = Application.dataPath + "/Data";
        public static string key = AesOperation.KEY;
        public static bool encryption = false;

        public static event EventHandler OnFileFinishWrite; 
        
        //properties
        public static string currentPath { 
            get {
                string path = string.Empty;
                switch (pathType)
                {
                    case DataLoaderPath.Root: path = Environment.CurrentDirectory; break;
                    case DataLoaderPath.Assets: path = Application.dataPath; break;
                    case DataLoaderPath.Custom: path = DataLoader.path; break;
                    case DataLoaderPath.Default: path = Application.consoleLogPath; break;
                    case DataLoaderPath.Persistent: path = Application.persistentDataPath; break;
                    default: break;
                }
                return path;
            } 
        }

        private class SerializedData : Dictionary<string, object> { }
        private static SerializedData g_data = new SerializedData();
        private static FileDataHandler g_file;
        private static FileMonitor g_monitor;

        private static string g_path;


        public static void Initialize(string filename, bool threadLock = true)
        {
            string path = string.Empty;
            switch (pathType)
            {
                case DataLoaderPath.Root: path = Environment.CurrentDirectory; break;
                case DataLoaderPath.Assets: path = Application.dataPath; break;
                case DataLoaderPath.Custom: path = DataLoader.path; break;
                case DataLoaderPath.Default: path = Application.consoleLogPath; break;
                case DataLoaderPath.Persistent: path = Application.persistentDataPath; break;
                default: break;
            }

            string name = $"{filename}.json";

            g_path = path;
            g_file = new FileDataHandler(path, name, key);
            g_file.SetThreadLock(threadLock);
            g_data.Clear();

            if (g_monitor != null)
            {
                g_monitor.FileFinishedWriting -= OnFileFinishWrite;
                g_monitor.StopMonitoring();
            }

            g_monitor = new FileMonitor(path, name);
            g_monitor.FileFinishedWriting += OnFileFinishWrite;
        }

        public static void Initialize(string filename, string extension, bool threadLock = true)
        {
            string path = string.Empty;
            switch (pathType)
            {
                case DataLoaderPath.Root: path = Environment.CurrentDirectory; break;
                case DataLoaderPath.Assets: path = Application.dataPath; break;
                case DataLoaderPath.Custom: path = DataLoader.path; break;
                case DataLoaderPath.Default: path = Application.consoleLogPath; break;
                case DataLoaderPath.Persistent: path = Application.persistentDataPath; break;
                default: break;
            }

            string name = $"{filename}.{extension}";

            g_path = path;
            g_file = new FileDataHandler(path, name, key);
            g_file.SetThreadLock(threadLock);
            g_data.Clear();

            if (g_monitor != null)
            {
                g_monitor.FileFinishedWriting -= OnFileFinishWrite;
                g_monitor.StopMonitoring();
            }

            g_monitor = new FileMonitor(path, name);
            g_monitor.FileFinishedWriting += OnFileFinishWrite;
        }

        public static void InitializePath(string path, bool threadLock = true)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            string fileExtension = Path.GetExtension(path);
            string name = $"{fileNameWithoutExtension}.{fileExtension}";

            g_path = path;
            g_file = new FileDataHandler(path, name, key);
            g_file.SetThreadLock(threadLock);
            g_data.Clear();

            if (g_monitor != null)
            {
                g_monitor.FileFinishedWriting -= OnFileFinishWrite;
                g_monitor.StopMonitoring();
            }

            g_monitor = new FileMonitor(path, name);
            g_monitor.FileFinishedWriting += OnFileFinishWrite;
        }

        #region LOAD
        public static T Load<T>(string key)
        {
            if (g_file != null)
            {
                g_data = g_file.Load<SerializedData>(encryption);

                var kvp = g_data.FirstOrDefault(x => x.Key == key);

                object value = IsJToken<T>(kvp.Value);

                try
                {
                    return (T)value;
                }
                catch(InvalidCastException e)
                {
                    Debug.LogError($"Failed to cast value with key '{key}' to type <b>{typeof(T)}</b>: {e.Message}");
                    return default;
                }
            }
            else
            {
                Debug.LogError("The static loading and saving system needs to be initialized before it can be used in the project.");
                return default;
            }
        }

        public static string LoadRaw()
        {
            if (g_file != null)
            {
                return g_file.LoadRaw();
            }
            else
            {
                Debug.LogError("The static loading and saving system needs to be initialized before it can be used in the project.");
                return default;
            }
        }

        public static void LoadRawInto(ref string data)
        {
            data = LoadRaw();
        }

        public static void LoadInto<T>(string key, ref T data)
        {
            data = Load<T>(key);
        }
        #endregion

        #region SAVE
        public static void Save()
        {
            g_file.Save(g_data, encryption);
        }

        public static void Save<T>(string key, T data)
        {
            if (g_file != null)
            {
                if (!g_data.ContainsKey(key))
                {
                    g_data.Add(key, data);
                }
                else
                {
                    g_data[key] = data;
                }

                g_file.Save(g_data, encryption);
            }
            else
            {
                Debug.LogError("The static loading and saving system needs to be initialized before it can be used in the project.");
            }

        }

        public static void SaveRaw(string data)
        {
            if (g_file != null)
            {
                g_file.SaveRaw(data);
            }
            else
            {
                Debug.LogError("The static loading and saving system needs to be initialized before it can be used in the project.");
            }
        }

        public static void SaveInto<T>(string key, T data, string path)
        {
            Initialize(path);
            Save(key, data);
        } 
        #endregion

        #region KEYHANDLING
        public static void AddKey<T>(string key, T value)
        {
            if(!g_data.ContainsKey(key))
                g_data.Add(key, value);
            else
                g_data[key] = value;

            AutoSave();
        }

        public static void RemoveKey(string key)
        {
            g_data.Remove(key);

            AutoSave();
        }

        public static bool ContainsKey(string key)
        {
            return g_data.ContainsKey(key);
        }

        public static string[] GetKeys()
        {
            List<string> keys = new List<string>();
            g_data = g_file.Load<SerializedData>();
            foreach(var item in g_data)
            {
                keys.Add(item.Key);
            }
            return keys.ToArray();
        }

        public static object[] GetValues()
        {
            List<object> values = new List<object>();
            g_data = g_file.Load<SerializedData>();
            foreach (var item in g_data)
            {
                values.Add(item.Value);
            }
            return values.ToArray();
        }

        public static void Clear()
        {
            g_data.Clear();

            AutoSave();
        }
        #endregion

        #region FILES
        public static void RemoveFile(string path, bool absolute = false)
        {
            string fullpath = Application.persistentDataPath + '/';

            fullpath = absolute ? path : fullpath + path;

            if (File.Exists(fullpath))
                File.Delete(fullpath);
            else
                Debug.LogError($"File <{path}> not exist in directory");
        }

        public static string[] GetFiles(string pattern)
        {
            List<string> fileNames = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(g_path);

            FileInfo[] files = directory.GetFiles(pattern);
            foreach (FileInfo file in files)
            {
                fileNames.Add(file.Name);
            }
            return fileNames.ToArray();
        }

        public static string[] GetFiles(string path, string pattern)
        {
            List<string> fileNames = new List<string>();
            DirectoryInfo directory = new DirectoryInfo(path);

            FileInfo[] files = directory.GetFiles(pattern);
            foreach (FileInfo file in files)
            {
                fileNames.Add(file.Name);
            }
            return fileNames.ToArray();
        }
        #endregion

        #region PRIVATE
        private static void AutoSave()
        {
            if (autoSave)
            {
                g_file.Save(g_data, encryption);
            }
        }

        private static T IsJToken<T>(object data)
        {
            var serializer = new JsonSerializer();

            if (data is JToken token)
            {
                if (token.Type == JTokenType.String)
                {
                    return serializer.Deserialize<T>(data.ToString());
                }
                else
                {
                    return token.ToObject<T>();
                }
            }

            return (T)data;
        }
        #endregion
    }
}
