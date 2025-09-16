using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataEngine.Data
{
    [System.Serializable]
    public class SerializedData : Dictionary<object, object> { }

    [System.Serializable]
    public class InstanceLoader : ILoader
    {
        [Tooltip("if leaved empty a name is generated")]
        [SerializeField] private string name = string.Empty;
        [SerializeField] private DataLoader.DataLoaderPath m_pathType;
        [SerializeField] private bool encrypted;

        private FileDataHandler m_file;
        private SerializedData m_data = new SerializedData();

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

        public InstanceLoader(string filename)
        {
            this.name = filename;
        }


        #region Load
        public void Load() 
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogError($"{GetType().Name} <name> field is empty or white space");
                return;
            }

            if (m_file == null)
            {
                m_file = new FileDataHandler(currentPath, $"{name}.json", DataLoader.key);
                m_file.SetLock(true);

                if (!m_file.Exists())
                {
                    m_file.SaveRaw(string.Empty);
                }
                else
                {
                    m_data = m_file.Load<SerializedData>(encrypted);
                }
            }
            else
            {
                if (m_file.name != name)
                {
                    
                    m_file = new FileDataHandler(currentPath, $"{name}.json", DataLoader.key);
                    m_file.SetLock(true);
                }

                m_data = m_file.Load<SerializedData>(encrypted);
            }
        }
        #endregion

        #region Save
        public void Save() 
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogError($"{GetType().Name} <name> field is empty or white space");
                return;
            }

            if (m_file == null)
            {
                m_file = new FileDataHandler(currentPath, $"{name}.json", DataLoader.key);
                m_file.SetLock(true);
                m_file.Save(m_data, encrypted);
            }
            else
            {
                if (m_file.name != name)
                {
                    m_file = new FileDataHandler(currentPath, $"{name}.json", DataLoader.key);
                    m_file.SetLock(true);
                }

                m_file.Save(m_data, encrypted);
            }
        }
        #endregion

        public void Clear()
        {
            m_data.Clear();
            m_file = null;
            name = string.Empty;
        }

        #region KeyHandling
        public object this[object key]
        {
            set 
            {
                m_data[key] = value; 
            }
            get 
            {
                return m_data[key];
            }
        }

        public void AddKey(object key, object value)
        {
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
            if(m_data.Count <= 0)
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
