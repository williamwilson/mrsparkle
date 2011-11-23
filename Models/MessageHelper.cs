using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Collections.Generic;
using System;
using System.Drawing;

namespace Sparkle.Models
{
    /// <summary>
    /// Contains helper methods for messages.
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// Generates a colored element containing the specified name.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IHtmlString ColorName(this HtmlHelper html, string name)
        {
            /* note: it is a whole lot easier to generate decent colors using HSV(or L) */

            /* generate a value from 0 to 360 based on the name for the hue */
            int hue = name.GetHashCode() % 360;

            /* have a standard saturation and value (or lightness) */
            double saturation = 0.7d;
            double value = 0.9d;
            double chroma = value * saturation;

            double huePrime = hue / 60.0d;
            double x = chroma * (1 - Math.Abs((huePrime % 2.0d) - 1));

            double r1 = 0;
            double g1 = 0;
            double b1 = 0;
            if (huePrime >= 0 && huePrime < 1)
            {
                r1 = chroma;
                g1 = x;
                b1 = 0;
            }
            else if (huePrime >= 1 && huePrime < 2)
            {
                r1 = x;
                g1 = chroma;
                b1 = 0;
            }
            else if (huePrime >= 2 && huePrime < 3)
            {
                r1 = 0;
                g1 = chroma;
                b1 = x;
            }
            else if (huePrime >= 3 && huePrime < 4)
            {
                r1 = 0;
                g1 = x;
                b1 = chroma;
            }
            else if (huePrime >= 4 && huePrime < 5)
            {
                r1 = x;
                g1 = 0;
                b1 = chroma;
            }
            else if (huePrime >= 5 && huePrime < 6)
            {
                r1 = chroma;
                g1 = 0;
                b1 = x;
            }

            double r = r1 + (value - chroma);
            double g = g1 + (value - chroma);
            double b = b1 + (value - chroma);
            string color = ((int)Math.Truncate(r * 255)).ToString("x2") + ((int)Math.Truncate(g * 255)).ToString("x2") + ((int)Math.Truncate(b * 255)).ToString("x2");

            return html.Raw(string.Format("<span style=\"color: #{0}\">{1}</span>", color, name));
        }

        /// <summary>
        /// Processes the body of a message and converts hash tags into links.
        /// </summary>
        /// <param name="html">The HtmlHelper for rendering views.</param>
        /// <param name="urlHelper">The UrlHelper for the current request.</param>
        /// <param name="body">The body of the message to process.</param>
        /// <returns>A string containing encoded HTML representing the body of the message with links created for each hash tag.</returns>
        public static IHtmlString LinkifyTags(this HtmlHelper html, UrlHelper urlHelper, string body)
        {
            StringBuilder linkified = new StringBuilder();
            StringBuilder tag = new StringBuilder();
            Dictionary<string, object> linkHtmlAttributes = new Dictionary<string, object>();
            linkHtmlAttributes.Add("style", "taglink");

            int index = body.IndexOf('#', 0);
            int endOfLastTag = 0;
            while (index >= 0)
            {
                linkified.Append(body.Substring(0, index));

                tag.Clear();
                int c;
                for (c = index + 1; c < body.Length; c++)
                {
                    if (char.IsWhiteSpace(body[c]))
                    {
                        break;
                    }

                    tag.Append(body[c]);
                }
                endOfLastTag = c;

                RouteValueDictionary routingValues = new RouteValueDictionary();
                routingValues.Add("tag", tag.ToString());
                linkified.Append(html.ActionLink("#" + tag.ToString(), "MessagesByTag", "Message", routingValues, linkHtmlAttributes).ToHtmlString());

                index = body.IndexOf('#', index + 1);
            }

            linkified.Append(body.Substring(endOfLastTag));

            return html.Raw(linkified.ToString());
        }
    }
}