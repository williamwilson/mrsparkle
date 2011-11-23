using System;

namespace Sparkle
{
    /// <summary>
    /// Represents a logged chat message.
    /// </summary>
    public sealed class Message
    {
        /// <summary>
        /// Gets or sets the body of the message.
        /// </summary>
        public string Body
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the user who sent the message.
        /// </summary>
        public string From
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unique identifier of the message.
        /// </summary>
        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the room to which the message was sent.
        /// </summary>
        public string Room
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time at which the message was sent.
        /// </summary>
        public DateTime Time
        {
            get;
            set;
        }
    }
}