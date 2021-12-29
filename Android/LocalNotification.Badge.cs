namespace Zebble.Device
{
    public static partial class LocalNotification
    {
        public static void UpdateBadgeCount(int value) => UpdateBadgeCount(UIRuntime.CurrentActivity, value);

        public static void UpdateBadgeCount(Android.Content.Context context, int value)
        {
            XamarinShortcutBadger.ShortcutBadger.ApplyCount(context, value);
        }

        public static void RemoveBadgeCount() => RemoveBadgeCount(UIRuntime.CurrentActivity);

        public static void RemoveBadgeCount(Android.Content.Context context)
        {
            XamarinShortcutBadger.ShortcutBadger.RemoveCount(context);
        }
    }
}