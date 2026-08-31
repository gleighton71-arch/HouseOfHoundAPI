using HouseOfHoundAPI.Models.Insurers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class InsurerRepository
{
    private readonly string _connectionString;

    public InsurerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Insurer> GetAll(bool includePolicies = true)
    {
        var insurers = new List<Insurer>();

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(@"
                SELECT Id, InsurerId, Name, ContactName, ContactEmail, ContactPhone, IsActive, CreatedUtc
                FROM dbo.Insurers
                ORDER BY Name;", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    insurers.Add(MapInsurer(reader));
                }
            }

            if (includePolicies)
            {
                foreach (var insurer in insurers)
                {
                    insurer.Policies = GetPolicies(conn, insurer.Id);
                }
            }
        }

        return insurers;
    }

    public Insurer Get(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            Insurer insurer = null;
            using (var cmd = new SqlCommand(@"
                SELECT Id, InsurerId, Name, ContactName, ContactEmail, ContactPhone, IsActive, CreatedUtc
                FROM dbo.Insurers
                WHERE Id = @Id;", conn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) insurer = MapInsurer(reader);
                }
            }

            if (insurer != null)
            {
                insurer.Policies = GetPolicies(conn, insurer.Id);
            }

            return insurer;
        }
    }

    public int Create(Insurer insurer)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            INSERT INTO dbo.Insurers (InsurerId, Name, ContactName, ContactEmail, ContactPhone, IsActive, CreatedUtc)
            OUTPUT INSERTED.Id
            VALUES (@InsurerId, @Name, @ContactName, @ContactEmail, @ContactPhone, @IsActive, SYSUTCDATETIME());", conn))
        {
            AddInsurerParameters(cmd, insurer);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public bool Update(int id, Insurer insurer)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            UPDATE dbo.Insurers
            SET InsurerId = @InsurerId,
                Name = @Name,
                ContactName = @ContactName,
                ContactEmail = @ContactEmail,
                ContactPhone = @ContactPhone,
                IsActive = @IsActive
            WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            AddInsurerParameters(cmd, insurer);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public IEnumerable<InsurerPolicy> GetPolicies(int insurerRecordId)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            return GetPolicies(conn, insurerRecordId);
        }
    }

    public InsurerPolicy GetPolicy(int policyRecordId)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            SELECT Id, InsurerRecordId, PolicyId, Name, BriefDetails, IsActive, CreatedUtc
            FROM dbo.InsurerPolicies
            WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = policyRecordId;
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                return reader.Read() ? MapPolicy(reader) : null;
            }
        }
    }

    public int CreatePolicy(int insurerRecordId, InsurerPolicy policy)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            INSERT INTO dbo.InsurerPolicies (InsurerRecordId, PolicyId, Name, BriefDetails, IsActive, CreatedUtc)
            OUTPUT INSERTED.Id
            VALUES (@InsurerRecordId, @PolicyId, @Name, @BriefDetails, @IsActive, SYSUTCDATETIME());", conn))
        {
            policy.InsurerRecordId = insurerRecordId;
            AddPolicyParameters(cmd, policy);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public bool UpdatePolicy(int policyRecordId, InsurerPolicy policy)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            UPDATE dbo.InsurerPolicies
            SET PolicyId = @PolicyId,
                Name = @Name,
                BriefDetails = @BriefDetails,
                IsActive = @IsActive
            WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = policyRecordId;
            AddPolicyParameters(cmd, policy);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    private static List<InsurerPolicy> GetPolicies(SqlConnection conn, int insurerRecordId)
    {
        var policies = new List<InsurerPolicy>();
        using (var cmd = new SqlCommand(@"
            SELECT Id, InsurerRecordId, PolicyId, Name, BriefDetails, IsActive, CreatedUtc
            FROM dbo.InsurerPolicies
            WHERE InsurerRecordId = @InsurerRecordId
            ORDER BY Name;", conn))
        {
            cmd.Parameters.Add("@InsurerRecordId", SqlDbType.Int).Value = insurerRecordId;
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    policies.Add(MapPolicy(reader));
                }
            }
        }

        return policies;
    }

    private static void AddInsurerParameters(SqlCommand cmd, Insurer insurer)
    {
        cmd.Parameters.Add("@InsurerId", SqlDbType.NVarChar, 50).Value = insurer.InsurerId;
        cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = insurer.Name;
        cmd.Parameters.Add("@ContactName", SqlDbType.NVarChar, 200).Value = (object)insurer.ContactName ?? DBNull.Value;
        cmd.Parameters.Add("@ContactEmail", SqlDbType.NVarChar, 200).Value = (object)insurer.ContactEmail ?? DBNull.Value;
        cmd.Parameters.Add("@ContactPhone", SqlDbType.NVarChar, 50).Value = (object)insurer.ContactPhone ?? DBNull.Value;
        cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = insurer.IsActive;
    }

    private static void AddPolicyParameters(SqlCommand cmd, InsurerPolicy policy)
    {
        if (!cmd.Parameters.Contains("@InsurerRecordId"))
        {
            cmd.Parameters.Add("@InsurerRecordId", SqlDbType.Int).Value = policy.InsurerRecordId;
        }
        cmd.Parameters.Add("@PolicyId", SqlDbType.NVarChar, 50).Value = policy.PolicyId;
        cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = policy.Name;
        cmd.Parameters.Add("@BriefDetails", SqlDbType.NVarChar, 1000).Value = (object)policy.BriefDetails ?? DBNull.Value;
        cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = policy.IsActive;
    }

    private static Insurer MapInsurer(SqlDataReader reader)
    {
        return new Insurer
        {
            Id = (int)reader["Id"],
            InsurerId = reader["InsurerId"].ToString(),
            Name = reader["Name"].ToString(),
            ContactName = reader["ContactName"] == DBNull.Value ? null : reader["ContactName"].ToString(),
            ContactEmail = reader["ContactEmail"] == DBNull.Value ? null : reader["ContactEmail"].ToString(),
            ContactPhone = reader["ContactPhone"] == DBNull.Value ? null : reader["ContactPhone"].ToString(),
            IsActive = (bool)reader["IsActive"],
            CreatedUtc = (DateTime)reader["CreatedUtc"]
        };
    }

    private static InsurerPolicy MapPolicy(SqlDataReader reader)
    {
        return new InsurerPolicy
        {
            Id = (int)reader["Id"],
            InsurerRecordId = (int)reader["InsurerRecordId"],
            PolicyId = reader["PolicyId"].ToString(),
            Name = reader["Name"].ToString(),
            BriefDetails = reader["BriefDetails"] == DBNull.Value ? null : reader["BriefDetails"].ToString(),
            IsActive = (bool)reader["IsActive"],
            CreatedUtc = (DateTime)reader["CreatedUtc"]
        };
    }
}
