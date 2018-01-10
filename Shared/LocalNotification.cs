namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class LocalNotification
    {
        public static readonly AsyncEvent<KeyValuePair<string, string>[]> Tapped = new AsyncEvent<KeyValuePair<string, string>[]>();
    }
}
