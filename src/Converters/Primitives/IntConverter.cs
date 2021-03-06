﻿using System;
using System.Text;

namespace Json.Converters.Primitives
{
    public class IntConverter : IJsonConverter
    {
        public object Read(Type type, string json)
        {
            if (int.TryParse(json, out int number)) return number;
            else throw new Exception();
        }

        public void Write(object value, StringBuilder writer)
        {
            writer.Append(value.ToString());
        }
    }
}
