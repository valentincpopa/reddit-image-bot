namespace RedditImageBot.Utilities.Common
{
    using System.Diagnostics;
    using System.Reflection;

    public static class ActivitySources
    {
        private const string _activitySourceName = "RedditImageBot";

        static ActivitySources()
        {
            var applicationInformationalVersion = Assembly.GetExecutingAssembly().GetName().Version;
            RedditImageBot = new(name: _activitySourceName, version: applicationInformationalVersion.ToString());
        }

        public static ActivitySource RedditImageBot { get; }
    }
}
