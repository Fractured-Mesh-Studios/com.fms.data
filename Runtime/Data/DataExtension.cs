using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataEngine
{
    public static class DataExtension
    {
        /// <summary>
        /// Attempts to convert an object to the specified type using JSON deserialization.
        /// Supports JObject, JArray, and JSON strings.
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="src">Source object</param>
        /// <returns>Deserialized object or default(T) if conversion fails</returns>
        public static T ToObject<T>(this object src)
        {
            if (src == null)
            {
                Debug.LogWarning($"[ToObject] Source is <null>. Cannot convert to {typeof(T).Name}.");
                return default;
            }

            try
            {
                // If already the desired type, return it
                if (src is T value)
                    return value;

                // If it's a JObject or JArray, try deserializing directly
                if (src is JObject jObject)
                    return jObject.ToObject<T>();

                if (src is JArray jArray)
                    return jArray.ToObject<T>();

                // If it's a string, try parsing as JObject or JArray
                if (src is string jsonString)
                {
                    jsonString = jsonString.Trim();

                    try
                    {
                        if (jsonString.StartsWith("{"))
                        {
                            JObject parsed = JObject.Parse(jsonString);
                            return parsed.ToObject<T>();
                        }
                        else if (jsonString.StartsWith("["))
                        {
                            JArray parsed = JArray.Parse(jsonString);
                            return parsed.ToObject<T>();
                        }
                        else
                        {
                            Debug.LogWarning($"[ToObject] Invalid JSON format. Cannot parse string to {typeof(T).Name}.");
                        }
                    }
                    catch (Exception parseEx)
                    {
                        Debug.LogWarning($"[ToObject] Failed to parse string as JSON.\nString: {jsonString}");
                        Debug.LogException(parseEx);
                        return default;
                    }
                }

                Debug.LogWarning($"[ToObject] Unsupported conversion from type {src.GetType().Name} to {typeof(T).Name}. Expected JObject, JArray, or JSON string.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ToObject] Exception occurred while converting to {typeof(T).Name} from {src.GetType().Name}.");
                Debug.LogException(ex);
            }

            return default;
        }
    }
}