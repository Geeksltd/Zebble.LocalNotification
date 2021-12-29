namespace Zebble
{
    using Android.App;
    using Android.Content;

    static class LocalNotificationExtensions
    {
        public static PendingIntent ToPendingBroadcast(this Intent intent, Android.Content.Context context)
        {
            return PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);
        }
    }
}
