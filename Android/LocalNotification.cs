namespace Zebble.Device
{
    using Android.App;
    using Java.Lang;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Olive;
    using Android.Content;
    using Newtonsoft.Json;
    using Android.OS;

    public static partial class LocalNotification
    {
        static PowerManager PowerManager => PowerManager.FromContext(UIRuntime.CurrentActivity);
        static AlarmManager AlarmManager => AlarmManager.FromContext(UIRuntime.CurrentActivity);

        public static Task<bool> Show(string title, string body, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            var notification = CreateNotification(title, body, playSound, 0, parameters);

            return Show(notification);
        }

        internal static Task<bool> Show(AndroidLocalNotification notification)
        {
            var native = notification.Render(UIRuntime.CurrentActivity, CurrentChannel?.Id);

            NotificationManager.Notify(notification.Id, native);

            EnsureScreenLightIsOn();

            return Task.FromResult(result: true);
        }

        public static Task<bool> Schedule(string title, string body, DateTime notifyTime, int id, bool playSound = false,
            Dictionary<string, string> parameters = null)
        {
            var notification = CreateNotification(title, body, playSound, id, parameters);

            var intent = CreateAlarmHandlerIntent(id, notification);
            var milliseconds = ((DateTimeOffset)notifyTime).ToUnixTimeMilliseconds();

            AlarmManager.SetExact(AlarmType.RtcWakeup, milliseconds, intent);

            return Task.FromResult(result: true);
        }

        public static Task Cancel(int id)
        {
            AlarmManager.Cancel(CreateAlarmHandlerIntent(id));
            NotificationManager.Cancel(id);
            return Task.CompletedTask;
        }

        static int GetUniqueId => (int)JavaSystem.CurrentTimeMillis() & 0xffffff;

        static AndroidLocalNotification CreateNotification(string title, string body, bool playSound, int id, Dictionary<string, string> parameters)
        {
            return new AndroidLocalNotification
            {
                Title = title,
                Body = body,
                PlaySound = playSound,
                Id = id,
                IntentId = GetUniqueId,
                Icon = Icon,
                TransparentIcon = TransparentIcon,
                TransparentIconColor = TransparentIconColor.ToStringOrEmpty().Or("transparent"),
                NotifyTime = DateTime.Now,
                Parameters = parameters
            };
        }

        static PendingIntent CreateAlarmHandlerIntent(int id, AndroidLocalNotification notification = null)
        {
            var result = new Intent(UIRuntime.CurrentActivity, typeof(ScheduledNotificationsBroadcastReceiver))
                .SetAction($"ScheduledNotification-{id}");

            if (notification is not null)
                result.PutExtra(LocalNotificationKey, JsonConvert.SerializeObject(notification));

            return result.ToPendingBroadcast();
        }

        static void EnsureScreenLightIsOn()
        {
            try
            {
                var isScreenOn = OS.IsAtLeast(BuildVersionCodes.KitkatWatch) ?
                    PowerManager.IsInteractive : PowerManager.IsScreenOn;

                if (isScreenOn) return;

                var wl = PowerManager.NewWakeLock(WakeLockFlags.ScreenDim | WakeLockFlags.AcquireCausesWakeup, null);
                wl.Acquire(3000); //set your time in milliseconds
            }
            catch { }
        }
    }
}