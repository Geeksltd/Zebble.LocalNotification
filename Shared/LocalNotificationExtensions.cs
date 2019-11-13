namespace Zebble.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class LocalNotificationExtensions
    {
        public static string DicToString(this Dictionary<string, string> parameters)
            => parameters == null ? string.Empty : string.Join(";", parameters.Select(x => x.Key + "=" + x.Value).ToArray());

        public static Dictionary<string, string> StringToDic(this string parameter)
        {
            var parameters = new Dictionary<string, string>();
            var @params = parameter.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var param in @params)
            {
                var keyValue = param.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                parameters.Add(keyValue[0], keyValue[1]);
            }

            return parameters;
        }
    }
}