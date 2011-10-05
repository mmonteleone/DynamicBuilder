DynamicBuilder
==============
[http://github.com/mmonteleone/DynamicBuilder][1]  
Suspiciously pleasant XML construction API for [C# 4][5], inspired by Ruby's [Builder][4]

What?
-----

Inspired heavily by the Ruby [Builder][4] library, *DynamicBuilder* exploits new features of [C# 4][5] like dynamic method invocation and optional parameters along with anonymous objects and delegates to yield an API which **accomplishes the unthinkable: *pleasant* XML construction with .NET**.  The API can be learned in five minutes and integrated into existing code in even less time as it's simply a single small class.

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

Comparison to other XML-generation methods:
-------------------------------------------

### The System.Xml Ghetto

The mean streets.  Power and control meets, well, nothing.  The original `System.Xml` types, with us since .NET 1.0, can be quite tedious to manipulate directly and have grown anachronistically low-level.

    // Direct node creation with System.Xml types
    
    XmlDocument doc = new XmlDocument();
    XmlElement userElement = doc.CreateElement("user");
    doc.AppendChild(userElement);
    XmlElement firstNameElement = doc.CreateElement("firstname");
    firstNameElement.InnerText = "John";
    userElement.AppendChild(firstNameElement);
    XmlElement lastNameElement = doc.CreateElement("lastname");
    lastNameElement.InnerText = "Doe";
    userElement.AppendChild(lastNameElement);
    XmlElement emailElement = doc.CreateElement("email");
    emailElement.InnerText = "jdoe@example.org";
    userElement.AppendChild(emailElement);        
    doc.Save(Console.Out);

    // Xml creation with an XmlTextWriter - maybe better?
    
    XmlTextWriter writer = new XmlTextWriter(Console.Out);
    writer.WriteStartElement("user");
    writer.WriteElementString("firstname", "John");
    writer.WriteElementString("firstname", "Doe");
    writer.WriteElementString("email", "jdoe@example.org");
    writer.WriteEndElement();
    writer.Close();
    
While these types still are behind the scenes of all subsequent .NET XML APIs (including *DynamicBuilder*), their verbose syntaxes mean they are no longer the best option for direct XML creation.
    
### The System.Xml.Serialization Suburbs

Medicated and mostly harmless.  This is an attractive choice when your serializable types map exactly to the XML you wish to generate.  Otherwise, hope you like creating boilerplate serializable classes and adapters just for serialization, or that you actually enjoy XSLT.

    [Serializable]
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }    
    
    User user = new User();
    XmlSerializer x = new XmlSerializer(typeof(User));
    x.Serialize(Console.Out, user);    

*DynamicBuilder* allows code that is just as terse as a Serializable class while still retaining the flexibility of manually generating specific XML content.

### The System.Xml.Linq New Urbanism

Attractive but superficial.  C# 3.0 brought LINQ to XML, and with it, `System.Xml.Linq`.  This revolutionized both the programmatic querying of XML as well as the declarative construction of it via object initialization.

    XElement user = new XElement("user", 
        new XElement("firstname", "John"),
        new XElement("lastname", "Doe"),
        new XElement("email", "jdoe@exampe.org")
    );
    
While a terrific improvement, it's still troublesome to use when the document must be generated programmatically.  The simple case of iterating over a list of blog posts in the above Atom feed example is not possible with the XElement object initialization syntax without building the document in multiple separate looped chunks and then adding the parts together.  *DynamicBuilder*'s use of anonymous delegates instead of just object initialization allows for all manner of logic within a single, unified, XML declaration.  *DynamicBuilder*'s anonymous object-to-attributes are also terser than instantiating multiple `XAttribute`s.

*LINQ to XML* remains the simplest XML querying/consumption mechanism, even against `DynamicBuilder.Xml`.  `DynamicBuilder.Xml` actually uses `System.Xml.Linq` types internally to model its XML, and can easily expose it via `xmlInstance.ToXElement()`.

Installation
------------

**Requirements**  
DynamicBuilder requires the .NET 4 framework to compile and/or run its xUnit test suite.

**Installation**  
Since this is such a small piece of code (just a small single class), it is recommended to simply copy the source, `Xml.cs`, directly into your project.  It does not really warrant the overhead of being a referenced, compiled, assembly.  

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
    
Nested nodes can be specified with anonymous delegates (although due to a limitation of C# 4, they need to be either explicitly cast as `Action` or simply passed through the `Xml.Fragment()` helper method which does this for us in a more fluent manner.

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

In the event that you need to name a tag the same as any of the special content methods, you can escape the name with an underscore.

    xml._Comment("foo");  // yields <Comment>foo</Comment>

**Namespaces**

Control of namespacing is still an area open for improvement within DynamicBuilder, but it currently punts the challenge to the underlying native support provided by `XDocument`, including `XDocument`'s limitations especially regarding prefixing.  

Currently, simply including an attribute named `xmlns` will translate not only to a proper `xmlns` attribute, but also an actual namespace being set on the underlying `XDocument` model.

    // yields <feed xmlns="http://www.w3.org/2005/Atom" />
    xml.feed(new { xmlns = "http://www.w3.org/2005/Atom" });

### Using and outputting generated XML

`DynamicBuilder.Xml` instances allow for simple immediate conversion to strings without requiring manual `XmlWriter` or memory stream/encoding management.

    Console.WriteLine(xml.ToString(false));  // prints the XML without indentation
    Console.Writeline(xml.ToString(true));   // prints the XML with indentation
    Console.WriteLine(xml);                  // implicit conversion to non-indented string

`DynamicBuilder.Xml` allows for easy export of its content as either a string or any manner of native .NET XML types, including

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

* Copyright 2010-2011 [Michael Monteleone][0]
* Directly inspired by [Jim Weirich][3]'s [Builder library for Ruby][4]

Version History
---------------

* 0.6.0.1 - Updated to latest xunit, project cleanup
* 0.6.0.0 - Upgraded to final release of .NET 4 Framework, testing bugfixes
* 0.5.0.0 - initial release

License
-------

The MIT License

Copyright (c) 2010-2011 Michael Monteleone

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

[0]: http://michaelmonteleone.net "Michael Monteleone"
[1]: http://github.com/mmonteleone/DynamicBuilder "Dynamic Builder on Github"
[2]: http://xunit.codeplex.com/Wikipage "xUnit"
[3]: http://onestepback.org "Jim Weirich"
[4]: http://builder.rubyforge.org/ "Builder for Ruby"
[5]: http://en.wikipedia.org/wiki/C_Sharp_4.0 "Wikipedia's article on C# 4"
