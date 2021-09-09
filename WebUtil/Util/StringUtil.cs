using Serilog;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WebUtil.Util
{
    public static class StringUtil
    {
        public static string CutAndComposite(this string input, string value, int startIndex, int n, string composite)
        {
            var position = input.IndexOfNth(value, startIndex, n);
            return input.Substring(0, position) + composite;
        }

        public static int IndexOfNth(this string input,
                             string value, int startIndex, int nth)
        {
            if (nth < 1)
                throw new NotSupportedException("Param 'nth' must be greater than 0!");
            if (nth == 1)
                return input.IndexOf(value, startIndex);
            var idx = input.IndexOf(value, startIndex);
            if (idx == -1)
                return -1;
            return input.IndexOfNth(value, idx + 1, --nth);
        }

        public static string Substring(this string input, string value)
        {
            var index = input.IndexOf(value);
            if (index == -1)
            {
                return input;
            }
            return input.Substring(0, index);
        }

        public static int ToInt(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            if( false == int.TryParse(str, NumberStyles.AllowThousands, null, out var value) )
            {
                Log.Error($"ToInt() failed. <str:{str}>");
                return 0;
            }

            return value;
        }

        public static int ToInt(this double value)
        {
            return Convert.ToInt32(value);
        }

        public static double ToDouble(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            return double.Parse(Regex.Match(str, @"[0-9\-.]+").Value);
        }

        public static float ToFloat(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0f;

            return float.Parse(Regex.Match(str, @"[0-9\-.]+").Value);
        }

        public static string ExtractKorean(this string str)
        {
            return Regex.Replace(str, "[^가-힣]", "");
        }

    }
}
