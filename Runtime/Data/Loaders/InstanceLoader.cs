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
        [SerializeField] private string m_name = string.Empty;
        [SerializeField] private DataLoader.DataLoaderPath m_pathType;
        [SerializeField] private bool m_encrypted;

        private FileDataHandler m_file;
        private SerializedData m_data = new SerializedData();

        #region Properties
        public DataLoader.DataLoaderPath pathType 
        {
            set { m_pathType = value; } 
            get { return m_pathType; } 
        }

        public bool encrypted
        {
            get { return m_encrypted; }
            set { m_encrypted = value; }
        }

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
            this.m_name = filename;
        }


        #region Load
        public void Load(bool locked = true) 
        {
            if (string.IsNullOrWhiteSpace(m_name))
            {
                Debug.LogError($"{GetType().Name} <name> field is empty or white space");
                return;
            }

            if (m_file == null)
            {
                m_file = new FileDataHandler(currentPath, $"{m_name}.json", DataLoader.key);
                m_file.SetLock(locked);

                if (!m_file.Exists())
                {
                    m_file.SaveRaw(string.Empty);
                }
                else
                {
                    m_data = m_file.Load<SerializedData>(m_encrypted);
                }
            }
            else
            {
                if (m_file.name != m_name)
                {
                    
                    m_file = new FileDataHandler(currentPath, $"{m_name}.json", DataLoader.key);
                    m_file.SetLock(locked);
                }

                m_data = m_file.Load<SerializedData>(m_encrypted);
            }
        }
        #endregion

        #region Save
        public void Save(bool locked = true) 
        {
            if (string.IsNullOrWhiteSpace(m_name))
            {
                Debug.LogError($"{GetType().Name} <name> field is empty or white space");
                return;
            }

            if (m_file == null)
            {
                m_file = new FileDataHandler(currentPath, $"{m_name}.json", DataLoader.key);
                m_file.SetLock(locked);
                m_file.Save(m_data, m_encrypted);
            }
            else
            {
                if (m_file.name != m_name)
                {
                    m_file = new FileDataHandler(currentPath, $"{m_name}.json", DataLoader.key);
                    m_file.SetLock(locked);
                }

                m_file.Save(m_data, m_encrypted);
            }
        }
        #endregion

        #region Exits
        public bool Exists()
        {
            m_file = new FileDataHandler(currentPath, $"{m_name}.json", DataLoader.key);
            m_file.SetLock(true);

            return m_file.Exists();
        }
        #endregion

        public void Clear()
        {
            m_data.Clear();
            m_file = null;
            m_name = string.Empty;
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
            if (m_data.Count <= 0)
                Load();

            var value = m_data[key];

            // Si el tipo esperado es string, devolvemos directo
            if (typeof(T) == typeof(string))
            {
                return (T)value;
            }

            // Si el tipo esperado es byte[] y el valor es Base64
            if (typeof(T) == typeof(byte[]) && value is string s)
            {
                return (T)(object)Convert.FromBase64String(s);
            }

            // Si es JObject
            if (value is JObject jObj)
            {
                return jObj.ToObject<T>();
            }

            throw new InvalidCastException($"No se puede castear el valor de tipo {value.GetType()} a {typeof(T)}");
        }

        public SerializedData GetData()
        {
            return m_data;
        }
        #endregion
    }
}
