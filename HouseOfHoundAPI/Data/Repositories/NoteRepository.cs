using HouseOfHoundAPI.Models;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace HouseOfHound.Api
{
    public class NoteRepository
    {
        public List<Note> GetDogNotes(int dogId)
        {
            var notes = new List<Note>();

            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        Id,
                        DogId,
                        Content,
                        CreatedDateUTC,
                        RequiresAction
                    FROM dbo.Note
                    WHERE DogId = @DogId
                    ORDER BY CreatedDateUTC DESC;", conn))
                {
                    cmd.Parameters.AddWithValue("@DogId", dogId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notes.Add(MapNote(reader));
                        }
                    }
                }
            }

            return notes;
        }

        public Note GetNoteById(int id)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        Id,
                        DogId,
                        Content,
                        CreatedDateUTC,
                        RequiresAction
                    FROM dbo.Note
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapNote(reader);
                        }
                    }
                }
            }

            return null;
        }

        public Note CreateNote(Note note)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.Note
                    (
                        DogId,
                        Content,
                        CreatedDateUTC,
                        RequiresAction
                    )
                    OUTPUT 
                        INSERTED.Id,
                        INSERTED.DogId,
                        INSERTED.Content,
                        INSERTED.CreatedDateUTC,
                        INSERTED.RequiresAction
                    VALUES
                    (
                        @DogId,
                        @Content,
                        SYSUTCDATETIME(),
                        @RequiresAction
                    );", conn))
                {
                    cmd.Parameters.AddWithValue("@DogId", note.DogId);
                    cmd.Parameters.AddWithValue("@Content", note.Content);
                    cmd.Parameters.AddWithValue("@RequiresAction", note.RequiresAction);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapNote(reader);
                        }
                    }
                }
            }

            throw new Exception("Failed to create note.");
        }

        public bool UpdateNote(Note note)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.Note
                    SET
                        Content = @Content,
                        RequiresAction = @RequiresAction
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", note.Id);
                    cmd.Parameters.AddWithValue("@Content", note.Content);
                    cmd.Parameters.AddWithValue("@RequiresAction", note.RequiresAction);

                    var rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }

        public bool DeleteNote(int id)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    DELETE FROM dbo.Note
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    var rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }

        private Note MapNote(SqlDataReader reader)
        {
            return new Note
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                CreatedDateUTC = reader.GetDateTime(reader.GetOrdinal("CreatedDateUTC")),
                RequiresAction = reader.GetBoolean(reader.GetOrdinal("RequiresAction"))
            };
        }
    }
}