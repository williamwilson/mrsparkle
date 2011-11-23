using System;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;
using log4net;
using System.Diagnostics;

namespace Sparkle.Data
{
    /// <summary>
    /// Represents a repository (store) of messages.
    /// </summary>
    internal sealed class MessageRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MessageRepository)); 

        private const int UniqueConstraintViolation = 2601;

        /// <summary>
        /// Gets a connection to the sparkle database.
        /// </summary>
        /// <returns></returns>
        private static SqlConnection GetConnection()
        {
            return new SqlConnection("Data Source=hydrogen\\sqlexpress;Initial Catalog=sparkle;User Id=mrsparkle;Password=mrsp4rkle;");
        }

        /// <summary>
        /// Retrieves the messages for the specified room and date.
        /// </summary>
        /// <param name="room">The name of the room for which messages are to be retrieved.</param>
        /// <param name="date">The date for which messages are to be retrieved.</param>
        /// <returns>The collection of messages for the specified room on the specified date in ascending order.</returns>
        public IEnumerable<Message> GetMessagesByRoomAndDate(string room, DateTime date)
        {
            Log.DebugFormat("Retrieving messages by room and date.  room: '{0}' date: '{1}'", room, date);
            List<Message> messages = new List<Message>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                SqlCommand command = new SqlCommand("select m.id, m.sequence, m.room, m.[time], m.[from], m.body " +
                    "from [message] m where m.room = @room and m.[time] >= @date and m.[time] < dateadd(d, 1, @date) " +
                    "order by m.[time] asc, m.id, m.sequence;", connection);
                command.Parameters.AddWithValue("@room", room);
                command.Parameters.AddWithValue("@date", date);
                PopulateMessages(messages, command);
            }
            return messages;
        }

        /// <summary>
        /// Retrieves a list of messages containing the specified tag.
        /// </summary>
        /// <param name="tag">The tag for which to find messages.</param>
        /// <returns>A collection of the messages which have been stored with the specified tag in descending order (most recent first).</returns>
        public IEnumerable<Message> GetMessagesByTag(string tag)
        {
            Log.DebugFormat("Retrieving messages by tag.  tag: '{0}'", tag);
            List<Message> messages = new List<Message>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                SqlCommand command = new SqlCommand("select m.id, m.sequence, m.room, m.[time], m.[from], m.body " + 
                    "from hashtag t inner join [message] m on m.id = t.message_id where t.tag = @tag " + 
                    "order by m.[time] desc, m.id, m.sequence;", connection);
                command.Parameters.AddWithValue("@tag", tag);
                PopulateMessages(messages, command);
            }

            return messages;
        }

        /// <summary>
        /// Executes the specified command and populates a list of Messages.
        /// </summary>
        /// <param name="messages">The list to which messages are appended.</param>
        /// <param name="command">The SqlCommand to execute to retrieve message database records.</param>
        private static void PopulateMessages(List<Message> messages, SqlCommand command)
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                Message current = null;

                while (reader.Read())
                {
                    if (reader.GetInt32(1) == 0)
                    {
                        current = MapToMessage(reader);
                        messages.Add(current);
                    }
                    else
                    {
                        current.Body += reader.GetString(5);
                    }
                }
            }
        }

        /// <summary>
        /// Maps a database record to a Message.
        /// </summary>
        /// <param name="reader">A SqlDataReader whose current record is to be mapped.</param>
        /// <returns>A message object representing the current record in the specified SqlDataReader.</returns>
        private static Message MapToMessage(SqlDataReader reader)
        {
            Message message = new Message();
            message.ID = reader.GetString(0);
            message.Room = reader.GetString(2);
            message.Time = reader.GetDateTime(3);
            message.From = reader.GetString(4);
            message.Body = reader.GetString(5);
            return message;
        }

        /// <summary>
        /// Inserts the specified tag for the specified message.
        /// </summary>
        /// <param name="tag">The tag to create.</param>
        /// <param name="message">The message to which the tag is to be associated.</param>
        public void InsertHashTag(string tag, Message message)
        {
            Debug.Assert(message != null, "null message to InsertHashTag");
            Log.DebugFormat("Inserting hash tag.  tag: '{0}' message: '{1}'", tag, message.ID);

            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                SqlCommand command = new SqlCommand("insert into hashtag (tag, message_id) values (@tag, @messageid);", connection);
                command.Parameters.AddWithValue("@tag", tag);
                command.Parameters.AddWithValue("@messageid", message.ID);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserts the specified message into the database and eats any failure
        /// due to uniqueness of the ID.
        /// </summary>
        /// <param name="message">The message to insert.</param>
        public bool InsertMessage(Message message)
        {
            Debug.Assert(message != null, "null message to InsertMessage");
            Log.DebugFormat("Inserting message.  message: '{0}' length: {1}", message.ID, message.Body.Length);

            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                /* if the body exceeds the maximum size (1000), divide the message into multiple records */
                int sequence = 0;
                for (int i = 0; i < message.Body.Length; i = i + 1000, sequence++)
                {
                    Log.DebugFormat("Inserting message part {0}.  message: '{1}'", i, message.ID, message.Body.Length);
                    SqlCommand command = new SqlCommand("insert into message (id, sequence, room, [time], [from], body)" + 
                        "values (@id, @sequence, @room, @time, @from, @body);", connection);
                    command.Parameters.AddWithValue("@id", message.ID);
                    command.Parameters.AddWithValue("@sequence", sequence);
                    command.Parameters.AddWithValue("@room", message.Room);
                    command.Parameters.AddWithValue("@time", message.Time);
                    command.Parameters.AddWithValue("@from", message.From);
                    command.Parameters.AddWithValue("@body", message.Body.Substring(i, Math.Min(message.Body.Length - i, 1000)));

                    /* note: we will often run into unique constraint violations, with many users logging
                     * the same room.  this is okay, just eat it and indicate the insert failed */
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException sqlex)
                    {
                        if (sqlex.Number == UniqueConstraintViolation)
                        {
                            Log.DebugFormat("Unique constraint violated when inserting message.  message: '{0}' part: {1}", message.ID, i);
                            return false;
                        }
                        Log.Error(string.Format("Failed to insert message.  message: '{0}'", message.ID), sqlex);
                        throw;
                    }
                }
            }

            return true;
        }
    }
}