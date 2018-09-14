// Code in this extractor is adapted from samples provided by Microsoft

using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomExtractors
{
    // Read JSON structured data from a file
    // This extractor expects the file to contain a single JSON array
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class JsonExtractor : IExtractor
    {
        private string rowpath;

        public JsonExtractor(string rowpath = null)
        {
            this.rowpath = rowpath;
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            // Json.Net
            using (var reader = new JsonTextReader(new StreamReader(input.BaseStream)))
            {
                // Parse Json one token at a time
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        var token = JToken.Load(reader);

                        //  All objects are represented as rows
                        foreach (JObject o in SelectChildren(token, this.rowpath))
                        {
                            // All fields are represented as columns
                            this.JObjectToRow(o, output);

                            yield return output.AsReadOnly();
                        }
                    }
                }
            }
        }

        /// <summary/>
        private static IEnumerable<JObject> SelectChildren(JToken root, string path)
        {
            // JObject children (only)
            //   As JObject(fields) have a clear mapping to Row(columns) as opposed to JArray (positional) or JValue(scalar)
            //  Note: 
            //   We ignore other types (as opposed to fail fast) since JSON supports heterogeneous (schema)
            //   We support the values that can be mapped, without failing all of them if one of happens to not be an Object.

            // Path specified
            if (!string.IsNullOrEmpty(path))
            {
                return root.SelectTokens(path).OfType<JObject>();
            }

            // Single JObject
            var o = root as JObject;
            if (o != null)
            {
                return new[] { o };
            }

            // Multiple JObjects
            return root.Children().OfType<JObject>();
        }

        /// <summary/>
        protected virtual void JObjectToRow(JObject o, IUpdatableRow row)
        {
            foreach (var c in row.Schema)
            {
                JToken token = null;
                object value = c.DefaultValue;

                // All fields are represented as columns
                //  Note: Each JSON row/payload can contain more or less columns than those specified in the row schema
                //  We simply update the row for any column that matches (and in any order).
                if (o.TryGetValue(c.Name, out token) && token != null)
                {
                    // Note: We simply delegate to Json.Net for all data conversions
                    //  For data conversions beyond what Json.Net supports, do an explicit projection:
                    //      ie: SELECT DateTime.Parse(datetime) AS datetime, ...
                    //  Note: Json.Net incorrectly returns null even for some non-nullable types (sbyte)
                    //      We have to correct this by using the default(T) so it can fit into a row value
                    value = JsonFunctions.ConvertToken(token, c.Type) ?? c.DefaultValue;
                }

                // Update
                row.Set<object>(c.Name, value);
            }
        }
    }


    public static class JsonFunctions
    {
        // JsonTuple("json", [$e1], [$e2], ...)
        //     1. Parse Json (once for all paths)
        //     2. Apply the path expressions
        //     3. Tuples are returned as MAP[path, value]
        //             Path  = Path of resolved node (matching the expression)
        //             Value = Node contents (of the matching nodes)
        //   ie:
        //     JsonTuple(json, "id", "name")              -> field names          MAP{ {id, 1 }, {name, Ed } }
        //     JsonTuple(json, "$.address.zip")           -> nested fields        MAP{ {address.zip, 98052}  }
        //     JsonTuple(json, "$..address")              -> recursive children   MAP{ {address, 98052}, {order[0].address, 98065}, ...           }
        //     JsonTuple(json, "$[?(@.id > 1)].id")       -> path expression      MAP{ {id, 2 }, {order[7].id, 4}, ...                            }
        //     JsonTuple(json)                            -> children             MAP{ {id, 1 }, {name, Ed}, { email, donotreply@live,com }, ...  }
        public static SqlMap<string, string> JsonTuple(string json, params string[] paths)
        {
            // Delegate
            return JsonTuple<string>(json, paths);
        }

        public static SqlMap<string, T> JsonTuple<T>(string json, params string[] paths)
        {
            // Parse (once)
            //  Note: Json.Net NullRefs on <null> input Json
            //        Given <null> is a common column/string value, map to empty set for composability
            var root = string.IsNullOrEmpty(json) ? new JObject() : JToken.Parse(json);

            // Apply paths
            if (paths != null && paths.Length > 0)
            {
                return SqlMap.Create(paths.SelectMany(path => ApplyPath<T>(root, path)));
            }

            // Children
            return SqlMap.Create(ApplyPath<T>(root, null));
        }

        private static IEnumerable<KeyValuePair<string, T>> ApplyPath<T>(JToken root, string path)
        {
            // Children
            var children = SelectChildren<T>(root, path);
            foreach (var token in children)
            {
                // Token => T
                var value = (T)JsonFunctions.ConvertToken(token, typeof(T));

                // Tuple(path, value)
                yield return new KeyValuePair<string, T>(token.Path, value);
            }
        }
        private static IEnumerable<JToken> SelectChildren<T>(JToken root, string path)
        {
            // Path specified
            if (!string.IsNullOrEmpty(path))
            {
                return root.SelectTokens(path);
            }

            // Single JObject
            var o = root as JObject;
            if (o != null)
            {
                //  Note: We have to special case JObject.
                //      Since JObject.Children() => JProperty.ToString() => "{"id":1}" instead of value "1".
                return o.PropertyValues();
            }

            // Multiple JObjects
            return root.Children();
        }

        internal static string GetTokenString(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;

                case JTokenType.String:
                    return (string)token;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                    // For scalars we simply delegate to Json.Net (JsonConvert) for string conversions
                    //  This ensures the string conversion matches the JsonTextWriter
                    return JsonConvert.ToString(((JValue)token).Value);

                case JTokenType.Date:
                case JTokenType.TimeSpan:
                case JTokenType.Guid:
                    // For scalars we simply delegate to Json.Net (JsonConvert) for string conversions
                    //  Note: We want to leverage JsonConvert to ensure the string conversion matches the JsonTextWriter
                    //        However that places surrounding quotes for these data types.
                    var v = JsonConvert.ToString(((JValue)token).Value);
                    return v != null && v.Length > 2 && v[0] == '"' && v[v.Length - 1] == '"' ? v.Substring(1, v.Length - 2) : v;

                default:
                    // For containers we delegate to Json.Net (JToken.ToString/WriteTo) which is capable of serializing all data types, including nested containers
                    return token.ToString();
            }
        }

        internal static object ConvertToken(JToken token, Type type)
        {
            try
            {
                if (type == typeof(string))
                {
                    return JsonFunctions.GetTokenString(token);
                }

                // We simply delegate to Json.Net for data conversions
                return token.ToObject(type);
            }
            catch (Exception e)
            {
                // Make this easier to debug (with field and type context)
                //  Note: We don't expose the actual value to be converted in the error message (since it might be sensitive, information disclosure)
                throw new JsonSerializationException(
                    string.Format(typeof(JsonToken).Namespace + " failed to deserialize '{0}' from '{1}' to '{2}'", token.Path, token.Type.ToString(), type.FullName),
                    e);
            }
        }
    }
}