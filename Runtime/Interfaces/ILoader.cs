using Codice.CM.Common;
using DataEngine.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataEngine
{
    public interface ILoader
    {
        object this[object key] { get; set; }

        void AddKey(object key, object value);
        bool RemoveKey(object key);
        void ClearKeys();
        bool ContainsKey(object key);
        T CastKey<T>(object key);

        public SerializedData GetData();
    }
}
