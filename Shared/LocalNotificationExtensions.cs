namespace Zebble.Device
{
    using Olive;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class LocalNotificationExtensions
    {
        public static string DicToString(this Dictionary<string, string> value)
        {
            if (value is null) return string.Empty;

            return string.Join(";", value.Select(x => x.Key + "=" + x.Value).ToArray());
        }

        public static Dictionary<string, string> StringToDic(this string value)
        {
            var result = new Dictionary<string, string>();

            if (value.IsEmpty()) return result;

            var @params = value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var param in @params)
            {
                var keyValue = param.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                result.Add(keyValue[0], keyValue[1]);
            }

            return result;
        }
    }
}