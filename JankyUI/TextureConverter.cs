using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace JankyUI
{
    class TextureConverter : TypeConverter
    {
        public static TextureConverter Instance { get; } = new TextureConverter();

        private TextureConverter()
        {

        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!(value is string str))
                throw new NotSupportedException($"Can't convert from type '{value.GetType()}'");
            if (str.IsNullOrWhiteSpace())
                throw new FormatException("Input string was in an invalid format.");
            var spl = str.Split(';');
            if (!Uri.TryCreate(spl[0], UriKind.Absolute, out var uri))
                throw new FormatException("Input string was in an invalid format.");
            switch (uri.Scheme) {
                case "res":
                    {
                        if(uri.Host != "")
                            throw new FormatException("Input string was in an invalid format.");
                        if (spl.Length != 2)
                            throw new FormatException("Input string was in an invalid format.");
                        var targetAssembly = Assembly.Load(spl[1]);
                        var resName = uri.AbsolutePath.Replace('/', '.').Substring(1);
                        var resources = targetAssembly.GetManifestResourceNames().Select(x => new
                        {
                            Name = x.Substring(x.IndexOf('.') + 1),
                            TrueName = x
                        });
                        var resource = resources.FirstOrDefault(x => x.Name.Equals(resName, StringComparison.OrdinalIgnoreCase));
                        if (resource == null)
                            throw new Exception($"Resource not found: '{resName}'");

                        using (var stream = targetAssembly.GetManifestResourceStream(resource.TrueName))
                        {

                            var bytes = new byte[stream.Length];
                            stream.Position = 0;
                            stream.Read(bytes, 0, bytes.Length);
                            var tex = new Texture2D(2, 2, TextureFormat.BGRA32, false);
                            tex.LoadImage(bytes);
                            return tex;
                        }
                    }
                default:
                    throw new NotSupportedException($"Scheme '{uri.Scheme}' not Supported");
            }
        }
    }
}
