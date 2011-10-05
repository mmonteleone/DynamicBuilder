#region license
/* DynamicBuilder
 * http://github.com/mmonteleone/DynamicBuilder
 * 
 * Copyright (C) 2010-2011 Michael Monteleone (http://michaelmonteleone.net)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicBuilder.Example
{
    /// <summary>
    /// Sample showing dynamic XML generation while iterating
    /// against model data.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Let's generate an atom feed the DynamicBuilder way.
            // Yes there are atom libraries out there but the point here
            // is that you don't need it, and it's possibly less code without
            // not to mention, really any schema imaginable is no extra work to output

            // First let's make some 
            var posts = GetBlogPosts();


            // now let's build an xml feed dynamically
            dynamic xml = new Xml();

            // set an xml declaration tag
            xml.Declaration();
            // create feed and meta data
            xml.feed(new { xmlns = "http://www.w3.org/2005/Atom" }, Xml.Fragment(feed =>
            {
                feed.title("My Blog!");
                feed.subtitle("Others have blogs too, but this one is mine");
                feed.link(new { href = "http://example.org" });
                feed.link(new { href = "http://example.org/feed.xml", rel = "self" });
                feed.author(Xml.Fragment(author =>
                {
                    author.name("John Doe");
                    author.email("johndoe@example.org");
                }));

                // iterate through the posts, adding them to the feed
                // note this part could not have been done simply via nested
                // object initializers with System.Xml.Linq types
                foreach (var post in posts)
                {
                    feed.entry(Xml.Fragment(entry =>
                    {
                        entry.title(post.Title);
                        entry.link(new { href = post.PermaLink });
                        entry.updated(post.PublishDate);
                        entry.summary(post.Content);
                    }));
                }
            }));


            // Let's print out the contents of this string to the console with indentation set to "True"
            Console.WriteLine(xml.ToString(true));
            Console.ReadKey();
        }


        /// <summary>
        /// Stub method which returns some blog post model objects from somewhere like a DB 
        /// </summary>
        /// <returns></returns>
        static IList<BlogPost> GetBlogPosts()
        {
            return new List<BlogPost>
            {
                new BlogPost{
                     Title = "First post!",
                     PublishDate = DateTime.Parse("2/19/2010"),
                     Content = "This is the very first post",
                     PermaLink = "http://example.org/blog/1"
                },
                new BlogPost{
                     Title = "Second post!",
                     PublishDate = DateTime.Parse("2/25/2010"),
                     Content = "This is the second post, not much better than the first",
                     PermaLink = "http://example.org/blog/2"
                },
                new BlogPost{
                     Title = "Third post!",
                     PublishDate = DateTime.Parse("3/1/2010"),
                     Content = "Three strikes and this is now out of my feedreader.  Yes, even though it's now trying to improve by embedding <strong>html <em>content</em></strong>",
                     PermaLink = "http://example.org/blog/3"
                }
            };
        }
    }
}
