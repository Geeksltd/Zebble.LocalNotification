namespace Zebble
{
    using Android.App;
    using Android.Content;

    static class LocalNotificationExtensions
    {
        public static PendingIntent ToPendingBroadcast(this Intent intent)
        {
            return PendingIntent.GetBroadcast(UIRuntime.CurrentActivity, 0, intent, PendingIntentFlags.UpdateCurrent);
        }
    }
}
