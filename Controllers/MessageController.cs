using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using Sparkle.Data;
using log4net;

namespace Sparkle.Controllers
{
    /// <summary>
    /// Handles HTTP requests involving messages.
    /// </summary>
    public class MessageController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MessageController));

        /// <summary>
        /// Creates (inserts) the specified message in the store.
        /// </summary>
        /// <param name="message">The message to store.</param>
        /// <returns>The result of the create action.</returns>
        public ActionResult Create(Message message)
        {
            Log.InfoFormat("Create message request received.");
            Log.DebugFormat("message: '{0}'\r\nroom: '{1}'\r\nfrom: '{2}'\r\ntime: '{3}'\r\nbody: '{4}'", message.ID, message.Room, message.From, message.Time, message.Body);

            try
            {
                MessageRepository repository = new MessageRepository();
                if (repository.InsertMessage(message))
                {
                    Log.DebugFormat("Message '{0}' created successfully.  Processing tags.", message.ID);
                    /* if the message is successfully created, parse it for any hashtags
                     * and insert them as needed */
                    IEnumerable<string> hashTags = GetHashTags(message.Body);
                    foreach (string tag in hashTags)
                    {
                        Log.DebugFormat("Creating tag for message. message: '{0}' tag: '{1}'", message.ID, tag);
                        repository.InsertHashTag(tag, message);
                    }
                }

                return Content(string.Empty);
            }
            catch (Exception ex)
            {
                Log.Error("Error creating message.", ex);
                return View("Error");
            }
        }

        /// <summary>
        /// Processes the specified message body and returns a collection of the unique hash tags present.
        /// </summary>
        /// <param name="body">The body of the message which is to be searched for hash tags.</param>
        /// <returns>A collection of the tags found in the message body.</returns>
        private static IEnumerable<string> GetHashTags(string body)
        {
            HashSet<string> tags = new HashSet<string>();
            StringBuilder tag = new StringBuilder();

            int index = body.IndexOf('#', 0);
            while (index >= 0)
            {
                tag.Clear();
                for (int c = index + 1; c < body.Length; c++)
                {
                    if (char.IsWhiteSpace(body[c]))
                    {
                        break;
                    }

                    tag.Append(body[c]);
                }

                if (tag.Length > 0)
                {
                    tags.Add(tag.ToString());
                }
                index = body.IndexOf('#', index + 1);
            }

            return tags;
        }

        /// <summary>
        /// Retrieves messages for the specified room on the specified date.  Optionally highlighting
        /// the message with the specified id (mid) if present.
        /// </summary>
        /// <param name="room">The name of the room for which messages are to be retrieved.</param>
        /// <param name="year">The year of the date.</param>
        /// <param name="month">The month of the date.</param>
        /// <param name="day">The day of the date.</param>
        /// <param name="mid">The ID of the message to highlight in the resulting list (if present).</param>
        /// <returns>The results of the retrieval action.</returns>
        public ActionResult MessagesByRoomAndDate(string room, int year, int month, int day, string mid)
        {
            Log.InfoFormat("Get messages by room and date request received.");
            Log.DebugFormat("room: '{0}' year: {1} month: {2} day: {3} messageID: '{4}'", room, year, month, day, mid);
            try
            {
                MessageRepository repository = new MessageRepository();
                DateTime date = new DateTime(year, month, day);
                IEnumerable<Message> messages = repository.GetMessagesByRoomAndDate(room, date);
                ViewBag.Title = string.Format("{0}: {1} < mrsparkle", room, date.ToShortDateString());
                ViewBag.Room = room;
                ViewBag.Date = date;
                ViewBag.MessageID = mid;
                return View(messages);
            }
            catch (Exception ex)
            {
                Log.Error("Error retrieving messages by room and date.", ex);
                return View("Error");
            }
        }

        /// <summary>
        /// Retrieves messages containing the specified tag.
        /// </summary>
        /// <param name="tag">The tag for which to search.</param>
        /// <returns>The results of the retrieval action.</returns>
        public ActionResult MessagesByTag(string tag)
        {
            Log.InfoFormat("Get messages by tag request received.");
            Log.DebugFormat("tag: '{0}'", tag);
            try
            {
                MessageRepository repository = new MessageRepository();
                IEnumerable<Message> messages = repository.GetMessagesByTag(tag);
                ViewBag.Title = string.Format("{0} < mrsparkle", tag);
                ViewBag.Tag = tag;
                return View(messages);
            }
            catch (Exception ex)
            {
                Log.Error("Error retrieving messages by tag.", ex);
                return View("Error");
            }
        }
    }
}