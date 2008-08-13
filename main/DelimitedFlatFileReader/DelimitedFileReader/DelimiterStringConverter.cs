using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Microsoft.Samples.SqlServer.SSIS.DelimitedFileReader
{
    class DelimiterStringConverter : StringConverter
    {
        const string None = "<none>";
        static string[] readableDelimConsts = new string[] { "{CR}", "{LF}", "{t}" };
        static string[] delimValueConsts = new string[] { "\r", "\n", "\t" };

        private static string ToReadableDelim(string delimValue)
        {
            if (string.IsNullOrEmpty(delimValue))
            {
                return None;
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder(delimValue);
                for (int i = 0; i < delimValueConsts.Length; i++)
                {
                    sb = sb.Replace(delimValueConsts[i], readableDelimConsts[i]);
                }
                return sb.ToString();
            }
        }

        private static string ToValueDelim(string delimValue)
        {
            if (string.Compare(delimValue, None) == 0)
            {
                return string.Empty;
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder(delimValue);
                for (int i = 0; i < delimValueConsts.Length; i++)
                {
                    sb = sb.Replace(readableDelimConsts[i], delimValueConsts[i]);
                }
                return sb.ToString();
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                return ToValueDelim((string)value);
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value is string && destinationType == typeof(string))
            {
                return ToReadableDelim((string)value);
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
