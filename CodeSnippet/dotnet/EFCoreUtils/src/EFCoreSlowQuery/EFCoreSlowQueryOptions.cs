using System;

namespace EFCoreSlowQuery
{
    public class EFCoreSlowQueryOptions
    {
        public static readonly EFCoreSlowQueryOptions DefaultOptions = new EFCoreSlowQueryOptions();
        /// <summary>
        /// The section name in configures file, like appsettings.json.
        /// </summary>
        public const string OptionsName = "EFCoreSlowQuery";

        public string ServiceName { set; get; } = "";

        private int _slowQueryThresholdMilliseconds = 100;
        /// <summary>
        /// If the SQL execution time is greater than this value, the SQL will record in slow query log.
        /// </summary>
        public int SlowQueryThresholdMilliseconds
        {
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException($"{nameof(SlowQueryThresholdMilliseconds)}", "Value must be greater than 0.");
                }

                _slowQueryThresholdMilliseconds = value;
            }
            get => _slowQueryThresholdMilliseconds;
        }

        public bool RecordSlowQueryLog { set; get; } = true;
    }
}
