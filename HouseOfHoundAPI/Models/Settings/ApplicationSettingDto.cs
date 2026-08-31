namespace HouseOfHoundAPI.Models.Settings
{
    public class ApplicationSettingDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsSecret { get; set; }
    }
}
