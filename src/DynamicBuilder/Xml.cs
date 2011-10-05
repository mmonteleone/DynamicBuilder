#region license
/* DynamicBuilder
 * Suspiciously pleasant XML construction API for C# 4, inspired by Ruby's Builder
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
#region documentation
/*
Inspired heavily by the Ruby Builder library, *DynamicBuilder* exploits 
new features of C# 4 like dynamic method invocation and optional parameters 
along with anonymous objects and delegates to yield an API which **accomplishes 
the unthinkable: *pleasant* XML construction with .NET**.  The API can be 
learned in five minutes and integrated into existing code in even less time as 
it's simply a single small class.

Examples!
---------

### Nodes via dynamic invocation

    dynamic xml = new DynamicBuilder.Xml();

    // non-existent "hello" method resolves to a "hello" node at runtime
    xml.hello("world");

    // yields  <hello>world</hello>

### Attributes via anonymous objects

    dynamic xml = new DynamicBuilder.Xml();    

    // passing an anonymous object resolves to xml attributes
    xml.user("John Doe", new { username="jdoe" = 2, usertype = "admin" });

    // yields  <user username="jdoe" usertype="admin">John Doe</user>
    
### Nesting via anonymous delegates

    dynamic xml = new DynamicBuilder.Xml();
    
    // passing an anonymous delegate creates a nested context
    xml.user(Xml.Fragment(u => {
        u.firstname("John");
        u.lastname("Doe");
        u.email("jdoe@example.org");
    }));
    
    // yields...    
    <user>
        <firstname>John</firstname>
        <lastname>Doe</lastname>
        <email>jdoe@example.org</email>
    </user>

### Putting it all together: building an Atom syndication feed:

    // First let's get some posts from a hypothetical `postRepository`
    IList<BlogPost> posts = postRepository.GetLatest(50);

    // now let's build an atom feed dynamically
    dynamic xml = new DynamicBuilder.Xml();

    // set an xml declaration tag
    xml.Declaration();
    
    // create the feed and metadata
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

Installation
------------

**Requirements**  
DynamicBuilder requires the .NET 4 framework to compile, and (for now) 
the .NET 2 framework for running its xUnit test suite.

**Installation**  
Since this is such a small piece of code (just a small single class), 
it is recommended to simply copy the source, `Xml.cs`, directly into 
your project.  It does not really warrant the overhead of being a 
referenced, compiled, assembly.  

1. Download the source.
2. `cd` into the project's directory `> build release`
3. Copy build\Release\Xml.cs into your own project.
   *  Alternatively, the assembly `DynamicBuilder.dll` can be added to your project as well.
4. Either modify `Xml.cs` to share your project's namespace, or state `using DynamicBuilder` within your code

To run DynamicBuilder's [xUnit][2]-based tests, use
    > build test

Usage
-----

### Declaring XML

**Instantiation**

Create a new dynamic xml instance

    dynamic xml = new DynamicBuilder.Xml();
    
**Adding nodes**

Unresolvable methods on the xml instance will dynamically resolve to new XML nodes

    xml.foo();       // yields <foo />
    xml.foo("bar");  // yields <foo>bar</foo>
    xml.count(2);    // yields <count>2</count>
    
Attributes can be specified via anonymous objects

    xml.foo("bar", new { spam = "eggs" });  // yields <foo spam="eggs">bar</foo>
    
Nested nodes can be specified with anonymous delegates (although due to a limitation 
of C# 4, they need to be either explicitly cast as `Action` or simply passed through 
the `Xml.Fragment()` helper method which does this for us in a more fluent manner.

    // yields <foo><bar>baz</bar></foo>
    
    xml.foo(Xml.Fragment(() => {
        xml.bar("baz");        
    }));
    
    // this yields the exact same, but allows aliasing of the `xml` 
    // instance within nested contexts for greater readability
    
    xml.foo(Xml.Fragment(foo => {
       foo.bar("baz");
    }));

**Adding Special Content**

An XML declaration node can be added via the `.Declaration()` method

    // yields <?xml version="1.0" encoding="utf-8"?>
    xml.Declaration();

    // yields <?xml version="1.0" encoding="utf-8"?>
    xml.Declaration(encoding: "utf-8");
    
    // yields <?xml version="1.0" encoding="utf-16" standalone="yes"?>
    xml.Declaration(encoding: "utf-16", standalone: "yes");
    
A Document Type can be added via the `.DocumentType()` method

    // yields <!DOCTYPE html>
    
    xml.DocumentType("html");  
  
    // yields
    // <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"
    //   http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
    
    xml.DocumentType("html", 
        publicId: "-//W3C//DTD XHTML 1.0 Strict//EN", 
        systemId: "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd");        

Comments can be added via the `Comment()` method

    xml.Comment("Foo");  // yields <!--foo-->
    
CData can be added via the `CData()` method

    xml.CData("data content");  // yields <![CDATA[data content]]>

Text can be added via the `Text()` method

    xml.Text("raw text");  // yields "raw text"

**Escaping**

In the event that you need to name a tag the same as any of the special content 
methods, you can escape the name with an underscore.

    xml._Comment("foo");  // yields <Comment>foo</Comment>

**Namespaces**

Control of namespacing is still an area open for improvement within DynamicBuilder, 
but it currently punts the challenge to the underlying native support provided by 
`XDocument`, including `XDocument`'s limitations especially regarding prefixing.  

Currently, simply including an attribute named `xmlns` will translate not only to a 
proper `xmlns` attribute, but also an actual namespace being set on the underlying 
`XDocument` model.

    // yields <feed xmlns="http://www.w3.org/2005/Atom" />
    xml.feed(new { xmlns = "http://www.w3.org/2005/Atom" });

### Using and outputting generated XML

`DynamicBuilder.Xml` instances allow for simple immediate conversion to strings 
without requiring manual `XmlWriter` or memory stream/encoding management.

    Console.WriteLine(xml.ToString(false));  // prints the XML without indentation
    Console.Writeline(xml.ToString(true));   // prints the XML with indentation
    Console.WriteLine(xml);                  // implicit conversion to non-indented string

`DynamicBuilder.Xml` allows for easy export of its content as either a string or 
any manner of native .NET XML types, including

    // for querying via LINQ to XML
    xml.ToXDocument();  
    xml.ToXElement();   
    
    // for other .NET API usage or further/advanced manipulation
    xml.ToXmlDocument();
    xml.ToXmlNode();
    xml.ToXmlElement();

To Do/Known issues
------------------

* Limitations of the `XDocument` API translate to limitations of `DynamicBuilder.Xml` as `XDocument` is the underlying model.
* Namespace support is still somewhat weak
* Explicitly declaring XML version is not always respected depending on output To* type
* Need a way to declare raw non-escaped markup
* Still requires .NET 2 for testing - perhaps due to issue with integrating .NET 4 builds of [xUnit][2]?

Credit
------

* Copyright 2010 [Michael Monteleone][0]
* Directly inspired by [Jim Weirich][3]'s [Builder library for Ruby][4]

Version History
---------------

* 0.6.0.0 - Upgraded to final release of .NET 4 Framework, testing bugfixes
* 0.5.0.0 - initial release

*/
#endregion
using System;
using System.Linq;
using System.Xml.Linq;
using System.Dynamic;
using System.IO;
using System.Xml;
using System.Text;

namespace DynamicBuilder
{
    /// <summary>
    /// A tiny C# 4 internal-DSL for declaratively generating XML.
    /// The generated XML can be used as-is, exported as string content, or as virtually 
    /// every native .NET XML type for further manipulation/usage/querying.
    /// 
    /// Inpired quite heavily by the Builder library for Ruby http://builder.rubyforge.org/, 
    /// and made possible thanks to C# 4's dynamic invocation support.
    /// </summary>
    public class Xml : DynamicObject
    {
        // Uses the System.Xml.Linq types for internally modeling the XML
        XDocument root = new XDocument();
        // holds the current container being worked on in cases of nesting
        XContainer current;

        /// <summary>
        /// Returns a lambda as a strongly-typed Action for use by lambda-accepting
        /// dynamic dispatch on Xml.  Not unequivalent to simply casting the same 
        /// lambda when passing to Xml, except slightly cleaner syntax.  This is only 
        /// necessary since dynamic calls cannot accept weakly-typed lambdas /sigh
        /// </summary>
        /// <param name="fragmentBuilder"></param>
        /// <returns>passed block, typed as an action</returns>
        public static Action Fragment(Action fragmentBuilder)
        {
            if (fragmentBuilder == null) { throw new ArgumentNullException("fragmentBuilder"); }

            return fragmentBuilder;
        }

        /// <summary>
        /// Returns a lambda as a strongly-typed Generic Actions of type dynamic for
        /// use by the lambda-accepting dynamic dispatch on Xml.  Not unequivalent to 
        /// simply casting the same lambda when passing to Xml, except slightly cleaner syntax
        /// This is only necessary since dynamic calls cannot accept weakly-typed lambdas /sigh
        /// </summary>
        /// <param name="fragmentBuilder"></param>
        /// <returns>passed lambda, typed as an Action&lt;dynamic&gt;</returns>
        public static Action<dynamic> Fragment(Action<dynamic> fragmentBuilder)
        {
            if (fragmentBuilder == null) { throw new ArgumentNullException("fragmentBuilder"); }

            return fragmentBuilder;
        }

        /// <summary>
        /// Alternate syntax for generating an XML object via this static factory
        /// method instead of expliclty creating a "dynamic" in client code.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static Xml Build(Action<dynamic> builder)
        {
            if (builder == null) { throw new ArgumentNullException("builder"); }

            var xbuilder = new Xml();
            builder(xbuilder);
            return xbuilder;
        }

        /// <summary>
        /// Constructs a new Dynamic XML Builder
        /// </summary>
        public Xml()
        {
            current = root;
        }

        /// <summary>
        /// Converts dynamically invoked method calls into nodes.  
        /// example 1:  xml.hello("world") becomes <hello>world</hello>
        /// example 2:  xml.hello("world2", new { foo = "bar" }) becomes <hello foo="bar">world</hello>
        /// </summary>
        /// <param name="binder">invoke member binder</param>
        /// <param name="args">args</param>
        /// <param name="result">result (always true)</param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            string tagName = binder.Name;
            Tag(tagName, args);
            return true;
        }

        /// <summary>
        /// Builds an XML node along with setting its inner content, attributes, and possibly nested nodes
        /// Usually No need to call this directly as it's mainly used as the implementation for dynamicaly invoked
        /// members on an XML instance.
        /// </summary>
        /// <param name="tagName">name for node tag</param>
        /// <param name="args">text content and/or attributes represented as an anonymous object 
        /// and/or lambda for generating child nodes</param>
        public void Tag(string tagName, params object[] args)
        {
            if (String.IsNullOrEmpty(tagName)) { throw new ArgumentNullException("tagName"); }

            // allow for naming tags same as reserved Xml methods 
            // like 'Comment' and 'CData' by prefixing "_" 
            // escape character on tag name/method call
            if (tagName.IndexOf('_') == 0)
                tagName = tagName.Substring(1);

            string content = null;
            object attributes = null;
            Action fragment = null;

            // Analyze all the arguments passed
            args.ToList().ForEach(arg =>
            {
                // argument was a delegate for building child nodes
                if (arg is Action)
                    fragment = arg as Action;
                else if (arg is Action<dynamic>)
                    fragment = () => (arg as Action<dynamic>)(this);

                // argument was a string literal
                else if (arg is string)
                    content = arg as string;

                // argument was a value type literal
                else if (arg.GetType().IsValueType)
                    content = arg.ToString();

                // otherwise, argument is considered to be an anonymous
                // object literal which will be reflected into node attributes
                else
                    attributes = arg;
            });

            // make a new element for this Tag() call
            var element = new XElement(tagName);
            current.Add(element);

            // if a fragment delegate was passed for building inner nodes
            // capture this element as the new current outer parent
            if (fragment != null)
            {
                current = element;
            }

            // add literal string content if there was any
            if (!String.IsNullOrEmpty(content))
            {
                element.Add(content);
            }
            // add attributes to the element if they were passed
            if (attributes != null)
            {
                attributes.GetType().GetProperties().ToList().ForEach(prop =>
                {
                    // if the attribute was named "xmlns", let's treat it 
                    // like an actual xml namespace and do the right thing by
                    // applying it as a namespace to the element. 
                    if (prop.Name == "xmlns")
                    {
                        XNamespace ns = prop.GetValue(attributes, null) as string;
                        element.Name = ns + element.Name.ToString();
                    }
                    // otherwise, just convert the property name/value to an attribute pair
                    // on the element
                    else
                    {
                        element.Add(new XAttribute(prop.Name, prop.GetValue(attributes, null)));
                    }
                });
            }

            // if a fragment delegate was passed for building inner nodes
            // now go ahead and execute the delegate, and then set the current outer parent
            // node back to its original value
            if (fragment != null)
            {
                fragment();
                current = element.Parent;
            }
        }

        /// <summary>
        /// Add a literal comment to the XML
        /// </summary>
        /// <param name="comment">comment content</param>
        public void Comment(string comment)
        {
            if (String.IsNullOrEmpty(comment)) { throw new ArgumentNullException("comment"); }

            current.Add(new XComment(comment));
        }

        /// <summary>
        /// Add literal CData content to the XML
        /// </summary>
        /// <param name="data">data</param>
        public void CData(string data)
        {
            if (String.IsNullOrEmpty(data)) { throw new ArgumentNullException("data"); }

            current.Add(new XCData(data));
        }

        /// <summary>
        /// Add a text node to the XML (not commonly needed)
        /// </summary>
        /// <param name="text">text content</param>
        public void Text(string text)
        {
            if (String.IsNullOrEmpty(text)) { throw new ArgumentNullException("text"); }

            current.Add(new XText(text));
        }

        /// <summary>
        /// Apply a declaration to the XML
        /// </summary>
        /// <param name="version">XML version</param>
        /// <param name="encoding">XML encoding (currently only supports utf-8 or utf-16)</param>
        /// <param name="standalone">"yes" or "no"</param>
        public void Declaration(string version = null, string encoding = null, string standalone = null)
        {
            root.Declaration = new XDeclaration(version, encoding, standalone);
        }

        /// <summary>
        /// Apply a document type to the XML
        /// </summary>
        /// <param name="name">name of the DTD</param>
        /// <param name="publicId">public identifier for the DTD</param>
        /// <param name="systemId">system identifier for the DTD</param>
        /// <param name="internalSubset">internal subset for the DTD</param>
        public void DocumentType(string name, string publicId = null, string systemId = null, string internalSubset = null)
        {
            if (String.IsNullOrEmpty(name)) { throw new ArgumentNullException("name"); }

            root.Add(new XDocumentType(name, publicId, systemId, internalSubset));
        }

        // Convertors for exporting as several useful .NET representations of the XML contnet
        #region converters

        /// <summary>
        /// Implicit conversion to non-indented xml content string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static implicit operator string(Xml xml)
        {
            return xml.ToString(false);
        }

        /// <summary>
        /// Converts the Xml content to a string
        /// </summary>
        /// <param name="indent">whether or not to indent the output</param>
        /// <returns></returns>
        public string ToString(bool indent)
        {
            // HACK justificiation:

            // The native XDocument.ToString() method never includes the XDeclaration (bug?)
            // Thus this below hack for manually writing the document to a stream.

            // Moreover, there's no straightforward/elegant way of getting the XmlWriter to respect 
            // an XDeclaration's encoding property, so manually inspecting the prop's string content.

            // This current implementation limits an XDeclaration to either utf-8 or utf-16
            // but at least it gets us *that* far.

            // default to utf-8 encoding
            Encoding encoding = new UTF8Encoding(false);
            // if there was an explicit declaration that asked for utf-16, use UnicodeEncoding instead
            if (root.Declaration != null &&
                !String.IsNullOrEmpty(root.Declaration.Encoding) &&
                root.Declaration.Encoding.ToLowerInvariant() == "utf-16")
                encoding = new UnicodeEncoding(false, false);

            MemoryStream memoryStream = new MemoryStream();

            XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
            {
                Encoding = encoding,
                Indent = indent,
                CloseOutput = true,
                // if "Declaration" not eplicitly set, don't include xml declaration
                OmitXmlDeclaration = root.Declaration == null
            });
            root.Save(xmlWriter);
            xmlWriter.Flush();
            xmlWriter.Close();

            // convert the xml stream to a string with the proper encoding
            if (encoding is UnicodeEncoding)
                return Encoding.Unicode.GetString(memoryStream.ToArray());
            else
                return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        /// <summary>
        /// Exports the Xml content as a Linq-queryable XDocument
        /// </summary>
        /// <returns>Linq-queryable XDocument</returns>
        public XDocument ToXDocument()
        {
            return root;
        }

        /// <summary>
        /// Exports the Xml content as a Linq-queryable XElement
        /// </summary>
        /// <returns>Linq-queryable XElement</returns>
        public XElement ToXElement()
        {
            return root.Elements().FirstOrDefault();
        }

        /// <summary>
        /// Exports the Xml content as a standard XmlDocument
        /// </summary>
        /// <returns>XmlDocument</returns>
        public XmlDocument ToXmlDocument()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(root.CreateReader());
            return xmlDoc;
        }

        /// <summary>
        /// Exports the Xml content as a standard XmlNode by returning the 
        /// first node in the XDocument, excluding the DocumentType if it's set
        /// </summary>
        /// <returns>XmlNode</returns>
        public XmlNode ToXmlNode()
        {
            if (root.DocumentType != null && root.Nodes().Count() > 1)
                return ToXmlDocument().ChildNodes[1] as XmlNode;
            else if (root.DocumentType == null && root.Nodes().Count() >= 1)
                return ToXmlDocument().FirstChild as XmlNode;
            else
                return null as XmlNode;
        }

        /// <summary>
        /// Exports the Xml content as a standard XmlElement
        /// </summary>
        /// <returns>XmlElement</returns>
        public XmlElement ToXmlElement()
        {
            return ToXmlNode() as XmlElement;
        }

        #endregion
    }

}
