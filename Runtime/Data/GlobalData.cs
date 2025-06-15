using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DataEngine.Data
{
    public static partial class GlobalData
    {
        public static bool debug = false;

        private static IDictionary<Type, object> _map = new Dictionary<Type, object>();
        private static IDictionary<string, object> _mapKey = new Dictionary<string, object>();

        #region DictionaryHandler
        //Add
        public static void AddKey(Type key, object data)
        {
            if (!_map.ContainsKey(key))
            {
                _map.Add(key, data);
            }
        }

        public static void AddKey(string key, object data)
        {
            if (!_mapKey.ContainsKey(key)) 
            {
                _mapKey.Add(key, data);
            }
        }

        //AddOrUpdate
        public static void AddOrUpdate(Type type, object data)
        {
            if (!_map.ContainsKey(type))
            {
                _map.Add(type, data);
            }
            else
            {
                _map[type] = data;
            }
        }

        public static void AddOrUpdate(string key, object data)
        {
            if (!_mapKey.ContainsKey(key))
            {
                _mapKey.Add(key, data);
            }
            else
            {
                _mapKey[key] = data;
            }
        }

        //Remove
        public static bool RemoveKey(Type key)
        {
            if (_map.ContainsKey(key))
            {
                return _map.Remove(key);
            }
            return false;
        }

        public static bool RemoveKey(string key)
        {
            if (_mapKey.ContainsKey(key))
            {
                return _mapKey.Remove(key);
            }

            return false;
        }

        //Get
        public static T GetKey<T>()
        {
            Type type = typeof(T);
            if (_map.ContainsKey(type))
            {
                return (T)_map[typeof(T)];
            }
            return default;
        }

        public static T GetKey<T>(string key)
        {
            if (_mapKey.ContainsKey(key))
            {
                return (T)_mapKey[key];
            }
            return default;
        }

        //Clear
        public static void ClearKey()
        {
            _map.Clear();
            _mapKey.Clear();
        }

        //Contains
        public static bool ContainsKey(Type type)
        {
            if (debug)
            {
                Debug.LogError("key no contains in dictionary");
            }

            return _map.ContainsKey(type);
        }

        public static bool ContainsKey(string key)
        {
            if (debug)
            {
                Debug.LogError("key no contains in dictionary");
            }

            return _mapKey.ContainsKey(key);
        }

        public static IDictionary<Type, object> GetAllKey()
        {
            return _map;
        }
        #endregion
    }
}