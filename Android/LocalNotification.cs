namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Java.Lang;
    using Newtonsoft.Json;
    using Olive;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class LocalNotification
    {
        static PowerManager GetPowerManager(Android.Content.Context context) => PowerManager.FromContext(context);
        static AlarmManager GetAlarmManager(Android.Content.Context context) => AlarmManager.FromContext(context);

        public static async Task<bool> Show(string title, string body, bool playSound = false, Dictionary<string, string> parameters = null, bool isAutoCancel = true)
        {
            var notification = CreateNotification(title, body, playSound, "", parameters, 0, isAutoCancel);

            return await Show(UIRuntime.CurrentActivity, notification);
        }

        internal static async Task<bool> Show(Android.Content.Context context, AndroidLocalNotification notification)
        {
            if (await Permission.LocalNotification.IsRequestGranted() == false)
            {
                await Alert.Show("Permission was not granted to show local notifications.");
                return false;
            }

            var native = notification.Render(context);

            GetNotificationManager(context).Notify(notification.Id.GetHashCode(), native);

            EnsureScreenLightIsOn(context);

            return true;
        }

        public static Task<bool> Schedule(
            string title, string body, DateTime notifyTime, string id,
            bool playSound = false, Dictionary<string, string> parameters = null, int priority = 0, bool isAutoCancel = true
        ) => Schedule(UIRuntime.CurrentActivity, title, body, notifyTime, id, playSound, parameters, priority, isAutoCancel);

        internal static async Task<bool> Schedule(
            Android.Content.Context context, string title, string body, DateTime notifyTime,
            string id, bool playSound = false, Dictionary<string, string> parameters = null, int priority = 0, bool isAutoCancel = true)
        {
            if (await Permission.LocalNotification.IsRequestGranted() == false)
            {
                await Alert.Show("Permission was not granted to show local notifications.");
                return false;
            }

            var notification = CreateNotification(title, body, playSound, id, parameters, priority, isAutoCancel);

            var intent = CreateAlarmHandlerIntent(context, id, notification);
            var milliseconds = ((DateTimeOffset)notifyTime).ToUnixTimeMilliseconds();

            var alarmManager = GetAlarmManager(context);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, milliseconds, intent);
            else
                alarmManager.SetExact(AlarmType.RtcWakeup, milliseconds, intent);

            return true;
        }

        public static Task Cancel(string id) => Cancel(UIRuntime.CurrentActivity, id);

        internal static Task Cancel(Android.Content.Context context, string id)
        {
            GetAlarmManager(context).Cancel(CreateAlarmHandlerIntent(context, id));
            GetNotificationManager(context).Cancel(id.GetHashCode());

            return Task.CompletedTask;
        }

        static int GetUniqueId => (int)JavaSystem.CurrentTimeMillis() & 0xffffff;

        static AndroidLocalNotification CreateNotification(string title, string body, bool playSound, string id, Dictionary<string, string> parameters, int priority = 0, bool isAutoCancel = true)
        {
            return new AndroidLocalNotification
            {
                Title = title,
                Body = body,
                PlaySound = playSound,
                Id = id,
                IntentId = GetUniqueId,
                ChannelId = CurrentChannel?.Id,
                Icon = Icon,
                TransparentIcon = TransparentIcon,
                TransparentIconColor = TransparentIconColor.ToStringOrEmpty().Or("transparent"),
                NotifyTime = DateTime.Now,
                Priority = priority,
                Parameters = parameters,
                IsAutoCancel = isAutoCancel
            };
        }

        static PendingIntent CreateAlarmHandlerIntent(Android.Content.Context context, string id, AndroidLocalNotification notification = null)
        {
            var result = new Intent(context, typeof(ScheduledNotificationsBroadcastReceiver))
                .SetAction($"ScheduledNotification-{id}");

            if (notification is not null)
                result.PutExtra(LocalNotificationKey, JsonConvert.SerializeObject(notification));

            return result.ToPendingBroadcast(context);
        }

        static void EnsureScreenLightIsOn(Android.Content.Context context)
        {
            try
            {
                var powerManager = GetPowerManager(context);
                var isScreenOn = OS.IsAtLeast(BuildVersionCodes.KitkatWatch) ?
                    powerManager.IsInteractive : powerManager.IsScreenOn;

                if (isScreenOn) return;

                var wl = powerManager.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, null);
                wl.Acquire(3000); //set your time in milliseconds
            }
            catch { }
        }
    }
}