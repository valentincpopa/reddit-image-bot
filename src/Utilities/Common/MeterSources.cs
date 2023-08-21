using System.Diagnostics.Metrics;

namespace RedditImageBot.Utilities.Common
{
    public static class MeterSources
    {
        private const string _meterSourceName = "RedditImageBot";

        static MeterSources()
        {
            Meter = new(_meterSourceName);
            MessageCounter = Meter.CreateCounter<long>("message_counter");
        }

        public static Meter Meter { get; }
        public static Counter<long> MessageCounter { get; }
    }
}
