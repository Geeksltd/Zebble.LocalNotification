namespace Zebble
{
    using Android.App;
    using Android.Content;

    static class LocalNotificationExtensions
    {
        public static PendingIntent ToPendingBroadcast(this Intent intent)
        {
            return PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
        }
    }
}
