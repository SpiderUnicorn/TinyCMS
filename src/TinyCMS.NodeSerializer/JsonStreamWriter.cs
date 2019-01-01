using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TinyCMS.Data;
using TinyCMS.Data.Extensions;
using TinyCMS.Interfaces;
using static TinyCMS.Serializer.JsonTypeNotation;

namespace TinyCMS.Serializer
{
    public class JsonStreamWriter : StreamWriter
    {
        public JsonStreamWriter(Stream stream) : base(stream) { }

        protected void WriteKeyAndValue(string key, object value)
        {
            WriteKey(key);
            WriteValue(value);
        }

        protected void WriteKey(string key)
        {
            WriteString(key);
            Write(Colon);
        }

        /// <summary>
        /// Does appropriate type checks on the value and calls
        /// a matching write function.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        public virtual void WriteValue(object value)
        {
            switch (value)
            {
                case string valueString:
                    WriteString(EscapeString(valueString));
                    break;
                case bool b:
                    WriteBool(b);
                    break;
                case DateTime dateTime:
                    WriteDateTime(dateTime);
                    break;
                case Enum @enum:
                    WriteEnum(@enum);
                    break;
                case int intNumber:
                    WriteNumber(intNumber);
                    break;
                case float floatNumber:
                    WriteNumber(floatNumber);
                    break;
                case double doubleNumber:
                    WriteNumber(doubleNumber);
                    break;
                case Dictionary<string, object> dictionary:
                    WriteObject(dictionary);
                    break;
                case IEnumerable<object> array:
                    WriteArray<object>(array, WriteValue);
                    break;
                default:
                    WriteObject(value.GetPropertyDictionary());
                    break;
            }
        }

        protected string EscapeString(string value)
        {
            return value
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\"", "\\\"");
        }

        protected void WriteString(string value)
        {
            Write(Quote);
            Write(value);
            Write(Quote);
        }

        protected void WriteBool(bool value) => Write(value ? True : False);
        protected void WriteDateTime(DateTime dateTime) => Write(dateTime.Ticks.ToString());
        protected void WriteEnum(Enum @enum) => WriteString(@enum.ToString());
        protected void WriteNumber(int value) => Write(value.ToString());
        protected void WriteNumber(float value) => Write(value.ToString());
        protected void WriteNumber(double value) => Write(value.ToString());

        protected void WriteArray<T>(IEnumerable<T> values, Action<T> fn)
        {
            Write(ArrayStart);
            WriteCommaSeparated(values, fn);
            Write(ArrayEnd);
        }

        protected void WriteCommaSeparated<T>(IEnumerable<T> values, Action<T> fn)
        {
            IterateCommaSeparated(values.GetEnumerator(), fn);
        }

        protected void IterateCommaSeparated<T>(IEnumerator<T> enumerator, Action<T> fn)
        {
            var isFirst = true;
            while (enumerator.MoveNext())
            {
            if (!isFirst)
            {
            Write(Comma);
                }
                fn(enumerator.Current);
                isFirst = false;
            }
        }

        protected void WriteCommaSeparated<TKey, TValue>(Dictionary<TKey, TValue> values, Action<TKey, TValue> fn)
        {
            IterateCommaSeparated(values.GetEnumerator(), entry => fn(entry.Key, entry.Value));
        }

        protected void WriteObject(Dictionary<string, object> propertyDictionary)
        {
            Write(ObjectStart);
            WriteCommaSeparated(propertyDictionary, WriteKeyAndValue);
            Write(ObjectEnd);
        }
    }
}
