namespace Zebble.Device
{
    using Android.App;
    using Android.Media;
    using Olive;

    public static partial class LocalNotification
    {
        internal static NotificationChannel CurrentChannel;

        static void CreateChannel(string name, string description, bool sound, NotificationImportance importance = NotificationImportance.High)
        {
            CurrentChannel = new NotificationChannel(name.ToCamelCaseId().ToLower(), name, importance)
            {
                Description = description
            };

            if (sound) CurrentChannel.SetSound(GetSoundUri(), GetAudioAttributes());

            GetNotificationManager(UIRuntime.CurrentActivity).CreateNotificationChannel(CurrentChannel);
        }

        internal static Android.Net.Uri GetSoundUri() => RingtoneManager.GetDefaultUri(RingtoneType.Notification);

        internal static AudioAttributes GetAudioAttributes()
        {
            return new AudioAttributes.Builder()
                .SetContentType(AudioContentType.Sonification)
                .SetUsage(AudioUsageKind.Notification)
                .Build();
        }
    }
}