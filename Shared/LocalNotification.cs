﻿namespace Zebble
{
    using System.Collections.Generic;

    public static partial class LocalNotification
    {
        public static readonly AsyncEvent<KeyValuePair<string, string>[]> Tapped = new AsyncEvent<KeyValuePair<string, string>[]>();
    }
}