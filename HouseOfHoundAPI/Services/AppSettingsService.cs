using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Web.Security;

public static class AppSettingsService
{
    private const string Purpose = "HouseOfHoundAPI.ApplicationSettings";
    private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>();

    public static string GetValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        return Cache.GetOrAdd(key, LoadValue);
    }

    public static string GetRequiredValue(string key)
    {
        var value = GetValue(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new ConfigurationErrorsException(key + " is not configured.");

        return value;
    }

    public static void SaveSecret(string key, string value)
    {
        SaveValue(key, Protect(key, value), true);
    }

    public static void SavePlainText(string key, string value)
    {
        SaveValue(key, value, false);
    }

    public static void ClearCache(string key = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Cache.Clear();
            return;
        }

        string ignored;
        Cache.TryRemove(key, out ignored);
    }

    private static string LoadValue(string key)
    {
        try
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 SettingValue, IsEncrypted
FROM dbo.ApplicationSettings
WHERE SettingKey = @SettingKey
  AND IsActive = 1;", conn))
            {
                cmd.Parameters.AddWithValue("@SettingKey", key);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var value = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        var isEncrypted = !reader.IsDBNull(1) && reader.GetBoolean(1);
                        return isEncrypted ? Unprotect(key, value) : value;
                    }
                }
            }
        }
        catch (SqlException)
        {
            // Allows existing environments to boot before the ApplicationSettings table is deployed.
        }

        return ConfigurationManager.AppSettings[key];
    }

    private static void SaveValue(string key, string value, bool isEncrypted)
    {
        using (var conn = Db.OpenConnection())
        using (var cmd = new SqlCommand(@"
MERGE dbo.ApplicationSettings AS target
USING (SELECT @SettingKey AS SettingKey) AS source
ON target.SettingKey = source.SettingKey
WHEN MATCHED THEN
    UPDATE SET SettingValue = @SettingValue,
               IsEncrypted = @IsEncrypted,
               IsActive = 1,
               UpdatedUTC = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (SettingKey, SettingValue, IsEncrypted, IsActive)
    VALUES (@SettingKey, @SettingValue, @IsEncrypted, 1);", conn))
        {
            cmd.Parameters.AddWithValue("@SettingKey", key);
            cmd.Parameters.AddWithValue("@SettingValue", value ?? "");
            cmd.Parameters.AddWithValue("@IsEncrypted", isEncrypted);
            cmd.ExecuteNonQuery();
        }

        ClearCache(key);
    }

    private static string Protect(string key, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value ?? "");
        var protectedBytes = MachineKey.Protect(bytes, Purpose, key);
        return Convert.ToBase64String(protectedBytes);
    }

    private static string Unprotect(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";

        var protectedBytes = Convert.FromBase64String(value);
        var bytes = MachineKey.Unprotect(protectedBytes, Purpose, key);
        return Encoding.UTF8.GetString(bytes);
    }
}
