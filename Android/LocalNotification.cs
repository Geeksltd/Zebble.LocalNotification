namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.Media;
    using Android.OS;
    using Java.Lang;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Android.Runtime;

    public static partial class LocalNotification
    {
        static NotificationChannel CurrentChannel;
        internal const string LocalNotificationKey = "LocalNotification";

        public static AndroidNotificationIcon Icon;
        public static AndroidNotificationIcon TransparentIcon;
        public static Color TransparentIconColor = Colors.White;

        internal static NotificationManager NotificationManager => NotificationManager.FromContext(Application.Context);

        static AlarmManager AlarmManager => UIRuntime.GetService<AlarmManager>(Context.AlarmService);

        static int GetUniqueId => (int)JavaSystem.CurrentTimeMillis() & 0xffffff;

        public static Task<bool> Show(string title, string body, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            var noti = CreateNotification(title, body, playSound, 0, parameters);
            CreateNotification(noti, UIRuntime.CurrentActivity);
            return Task.FromResult(true);
        }

        static AndroidLocalNotification CreateNotification(string title, string body, bool playSound, int id, Dictionary<string, string> parameters)
        {
            var result = new AndroidLocalNotification
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
                Parameters = parameters.DicToString()
            };

            return result;
        }

        public static Task<bool> Schedule(string title, string body, DateTime notifyTime, int id, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            var notification = CreateNotification(title, body, playSound, id, parameters);
            var intent = CreateAlarmHandlerIntent(id, notification);

            AlarmManager.SetExact(AlarmType.RtcWakeup, notifyTime.ToUnixEpoch(), intent);

            return Task.FromResult(result: true);
        }

        public static Task Cancel(int id)
        {
            AlarmManager.Cancel(CreateAlarmHandlerIntent(id));
            NotificationManager.Cancel(id);
            return Task.CompletedTask;
        }

        public static void Configure(string name, string description, int iconResourceId, int transparentIconResourceId, Color transparentIconColor, bool sound, NotificationImportance importance = NotificationImportance.High)
        {
            Icon = new AndroidNotificationIcon(iconResourceId);
            TransparentIcon = new AndroidNotificationIcon(transparentIconResourceId);
            TransparentIconColor = transparentIconColor;

            if (OS.IsAtLeast(BuildVersionCodes.O))
            {
                CurrentChannel = new NotificationChannel(name.ToCamelCaseId().ToLower(), name, importance)
                {
                    Description = description
                };

                if (sound) CurrentChannel.SetSound(GetSoundUri(), GetAudioAttributes());

                NotificationManager.CreateNotificationChannel(CurrentChannel);
            }
        }

        [Preserve]
        public static void Initialize(Intent intent, Action<Notification> OnTapped = null)
        {
            ScheduledAlarmService.Start();

            if (OnTapped == null) return;

            var extra = intent.GetStringExtra(LocalNotificationKey);
            intent.RemoveExtra(LocalNotificationKey);
            if (extra.LacksValue()) return;

            var notification = JsonConvert.DeserializeObject<AndroidLocalNotification>(extra);
            OnTapped(new Notification
            {
                Title = notification.Title,
                Body = notification.Body,
                Id = notification.Id,
                NotifyTime = notification.NotifyTime,
                Parameters = notification.Parameters.StringToDic()
            });
        }

        internal static AudioAttributes GetAudioAttributes() => new AudioAttributes.Builder().SetContentType(AudioContentType.Sonification).SetUsage(AudioUsageKind.Alarm).Build();

        internal static Android.Net.Uri GetSoundUri() => RingtoneManager.GetDefaultUri(RingtoneType.Notification);

        internal static Android.App.Notification CreateNotification(AndroidLocalNotification notification, Context context)
        {
            if (OS.IsAtLeast(BuildVersionCodes.O) && CurrentChannel == null)
                throw new System.Exception("In MainActivity.OnCreate() call LocalNotification.CreateChannel(...).");

            var native = notification.Render(context, CurrentChannel?.Id);

            NotificationManager.Notify(notification.Id, native);

            return native;
        }

        static PendingIntent CreateAlarmHandlerIntent(int id, AndroidLocalNotification notification = null)
        {
            var result = new Intent(Application.Context, typeof(ScheduledAlarmHandler))
                .SetAction("LocalNotifierIntent" + id);

            if (notification != null)
                result.PutExtra(LocalNotificationKey, JsonConvert.SerializeObject(notification));

            return result.ToPendingBroadcast();
        }
    }
}