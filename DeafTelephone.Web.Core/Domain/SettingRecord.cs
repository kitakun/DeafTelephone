namespace DeafTelephone.Web.Core.Domain
{
    public record SettingRecord()
    {
        public long Id { get; init; }

        public SettingRecordEnum Type { get; init; }

        public string Value { get; init; }
    }
}
