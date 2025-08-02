using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace DataEngine.Data
{
    [System.Serializable]
    public class SerializedData : Dictionary<object, object> { }


    [System.Serializable]
    public class InstanceLoader : ILoader
    {
        [SerializeField] private string name = "loader";
        [SerializeField] private DataLoader.DataLoaderPath m_pathType;
        [SerializeField] private bool encrypted;

        private FileDataHandler m_file;
        private SerializedData m_data;

        #region Properties
        public string currentPath
        {
            get
            {
                string path = string.Empty;
                switch (m_pathType)
                {
                    case DataLoader.DataLoaderPath.Root: path = System.Environment.CurrentDirectory; break;
                    case DataLoader.DataLoaderPath.Assets: path = Application.dataPath; break;
                    case DataLoader.DataLoaderPath.Custom: path = DataLoader.path; break;
                    case DataLoader.DataLoaderPath.Default: path = Application.consoleLogPath; break;
                    case DataLoader.DataLoaderPath.Persistent: path = Application.persistentDataPath; break;
                    default: break;
                }
                return path;
            }
        }
        #endregion

        #region Load
        public void Load() 
        {
            if (m_file == null)
            {
                string filename = $"{name}.json";

                m_file = new FileDataHandler(currentPath, filename, DataLoader.key);
                m_file.SetThreadLock(true);

                if (!m_file.Exists())
                {
                    m_file.SaveRaw(string.Empty);
                }
            }
            else
            {
                m_data = m_file.Load<SerializedData>(encrypted);
            }
        }
        #endregion

        #region Save
        public void Save() 
        {
            if (m_file == null)
            {
                string filename = $"{name}.json";

                m_file = new FileDataHandler(currentPath, filename, DataLoader.key);
                m_file.SetThreadLock(true);
            }
            else
            {
                m_file.Save(m_data, encrypted);
            }
        }
        #endregion

        #region KeyHandling
        public object this[object key]
        {
            set { m_data[key] = value; }
            get { return m_data[key]; }
        }

        public void AddKey(object key, object value)
        {
            if(m_data == null)
            {
                m_data = new SerializedData();
            }

            m_data.Add(key, value);
        }

        public bool RemoveKey(object key)
        {
            return m_data.Remove(key);
        }

        public void ClearKeys()
        {
            m_data.Clear();
        }

        public bool ContainsKey(object key)
        {
            return m_data.ContainsKey(key);
        }

        public T CastKey<T>(object key)
        {
            if(m_data == null || m_data.Count <= 0)
            {
                Load();
            }

            JObject @object = (JObject)m_data[key];

            return @object.ToObject<T>();
        }

        public SerializedData GetData()
        {
            return m_data;
        }
        #endregion
    }
}
