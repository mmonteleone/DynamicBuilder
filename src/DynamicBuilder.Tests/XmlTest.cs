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
using Xunit;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace DynamicBuilder.Tests
{
    public class XmlTest
    {
        [Fact]
        public void Xml_Fragment_NullBuilder_ThrowsNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Xml.Fragment((Action)null));
        }

        [Fact]
        public void Xml_Fragment_NonNullBuilder_ReturnsBuilder()
        {
            Action fragmentBuilder = () => { };
            Assert.Same(fragmentBuilder, Xml.Fragment((Action)fragmentBuilder));
        }

        [Fact]
        public void Xml_FragmentT_NullBuilder_ThrowsNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Xml.Fragment((Action<dynamic>)null));
        }

        [Fact]
        public void Xml_FragmentT_NonNullBuilder_ReturnsBuilder()
        {
            Action<dynamic> fragmentBuilder = x => { };
            Assert.Same(fragmentBuilder, Xml.Fragment(fragmentBuilder));
        }

        [Fact]
        public void Xml_Build_NullBuilder_ThrowsNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Xml.Build(null));
        }

        [Fact]
        public void Xml_Build_ExecutesBuilderOnNewXml_ReturnsXml()
        {
            bool executed=false;
            Xml result = Xml.Build(x =>
            {
                executed = true;
                x.hello("world");
            });

            Assert.True(executed);
            Assert.Equal("<hello>world</hello>", result);
        }

        [Fact]
        public void Xml_TryInvokeMember_CallsTagWithBinderNameAndArgs()
        {
            dynamic xml = new Xml();
            xml.hello("world", new { a = "b" });

            Assert.Equal("<hello a=\"b\">world</hello>", xml);
        }

        [Fact]
        public void Xml_Tag_NullName_ThrowsNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Xml().Tag(null));
        }

        [Fact]
        public void Xml_Tag_Name_BecomesTagName()
        {
            var xml = new Xml();
            xml.Tag("hello");
            Assert.Equal("<hello />", xml);
        }

        [Fact]
        public void Xml_Tag_EscapedTagNameWithSameNameAsMethod_Escapes()
        {
            var xml = new Xml();
            // .Comment would have been a reserved call for declaring an xml comment
            // so _ escapes it
            xml.Tag("_Comment");
            Assert.Equal("<Comment />", xml);
        }

        [Fact]
        public void Xml_Tag_ActionArguments_CreateNestedNodes()
        {
            var xml = new Xml();
            xml.Tag("outer", Xml.Fragment(() =>
            {
                xml.Tag("inner1", Xml.Fragment(() =>
                {
                    xml.Tag("inner2");
                }));
            }));

            Assert.Equal("<outer><inner1><inner2 /></inner1></outer>", xml);
        }

        [Fact]
        public void Xml_Tag_DynamicActionArguments_CreateNestedNodes()
        {
            var xml = new Xml();
            xml.Tag("outer", Xml.Fragment(outer =>
            {
                outer.Tag("inner1", Xml.Fragment(inner1 =>
                {
                    inner1.Tag("inner2");
                }));
            }));

            Assert.Equal("<outer><inner1><inner2 /></inner1></outer>", xml);
        }

        [Fact]
        public void Xml_Tag_StringArguments_CreateTextNode()
        {
            var xml = new Xml();
            xml.Tag("tag", "value");
            Assert.Equal("<tag>value</tag>", xml);
        }

        [Fact]
        public void Xml_Tag_ValueTypeArguments_CreateToStringedTextNode()
        {
            var xml = new Xml();
            xml.Tag("tag", 123);
            Assert.Equal("<tag>123</tag>", xml);
        }

        [Fact]
        public void Xml_Tag_ObjectAttributeArguments_ConvertsToAttributes()
        {
            var xml = new Xml();
            xml.Tag("tag", new { a = "b", x = 9, z = true });
            Assert.Equal("<tag a=\"b\" x=\"9\" z=\"true\" />", xml);
        }

        [Fact]
        public void Xml_Tag_ObjectAttributeXmlnsArgumetns_ConvertsToActualXmlNamespaces()
        {
            var xml = new Xml();
            xml.Tag("tag", new { a = "b", xmlns="http://www.w3.org/2005/Atom" });
            Assert.Equal("http://www.w3.org/2005/Atom", xml.ToXElement().Name.NamespaceName);
            Assert.Equal("<tag a=\"b\" xmlns=\"http://www.w3.org/2005/Atom\" />", xml);
        }

        [Fact]
        public void Xml_Tag_Combo_String_And_Attributes_ReturnsProperNode()
        {
            var xml = new Xml();
            xml.Tag("tag", "value", new { a = "b" });
            Assert.Equal("<tag a=\"b\">value</tag>", xml);
        }

        [Fact]
        public void Xml_Tag_Combo_Attributes_And_Fragment_ReturnsProperNode()
        {
            var xml = new Xml();
            xml.Tag("tag", new { a = "b" }, Xml.Fragment(inner => inner.Tag("inner", "innerval")));
            Assert.Equal("<tag a=\"b\"><inner>innerval</inner></tag>", xml);
        }

        [Fact]
        public void Xml_Comment_NullComment_ThrowsNullException()
        {
            var xml = new Xml();
            Assert.Throws<ArgumentNullException>(() => xml.Comment(null));
        }

        [Fact]
        public void Xml_Comment_AddsXmlCommentCode()
        {
            var xml = new Xml();
            xml.Comment("comment");
            xml.Tag("hello", "world");
            Assert.Equal("<!--comment--><hello>world</hello>", xml);
        }

        [Fact]
        public void Xml_CData_NullData_ThrowsNullException()
        {
            var xml = new Xml();
            Assert.Throws<ArgumentNullException>(() => xml.CData(null));
        }

        [Fact]
        public void Xml_CData_AddsCDataNode()
        {
            var xml = new Xml();
            xml.Tag("tag", Xml.Fragment(() =>
            {
                xml.CData("content");
                xml.Tag("hello", "world");
            }));
            Assert.Equal("<tag><![CDATA[content]]><hello>world</hello></tag>", xml);
        }

        [Fact]
        public void Xml_Text_NullText_ThrowsNullException()
        {
            var xml = new Xml();
            Assert.Throws<ArgumentNullException>(() => xml.Text(null));
        }

        [Fact]
        public void Xml_Text_AddsTextNode()
        {
            var xml = new Xml();
            xml.Tag("tag", Xml.Fragment(() =>
            {
                xml.Text("some text");
            }));
            Assert.Equal("<tag>some text</tag>", xml);
        }

        [Fact]
        public void Xml_DocumentType_NullName_ThrowsNullException()
        {
            var xml = new Xml();
            Assert.Throws<ArgumentNullException>(() => xml.DocumentType(null));
        }

        [Fact]
        public void Xml_DocumentType_SetsAllParamsToDocumentType()
        {
            var xml = new Xml();
            xml.DocumentType("html", "publicid", "systemid", "internalsubset");
            Assert.Equal("html", xml.ToXDocument().DocumentType.Name);
            Assert.Equal("publicid", xml.ToXDocument().DocumentType.PublicId);
            Assert.Equal("systemid", xml.ToXDocument().DocumentType.SystemId);
            Assert.Equal("internalsubset", xml.ToXDocument().DocumentType.InternalSubset);
        }

        [Fact]
        public void Xml_Declaration_NotCalled_NoDeclarationSet()
        {
            var xml = new Xml();
            Assert.Null(xml.ToXDocument().Declaration);
        }

        [Fact]
        public void Xml_Declaration_NoParams_Uses_1_0_Utf_8()
        {
            var xml = new Xml();
            xml.Declaration();
            xml.Tag("hello");
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><hello />", xml);
        }

        [Fact]
        public void Xml_Declaration_SetsAllParamsToDeclaration()
        {
            var xml = new Xml();
            xml.Declaration(encoding: "utf-16", standalone: "yes");
            xml.Tag("hello");
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?><hello />", xml);
        }

        [Fact]
        public void Xml_CastAsString_CallsToStringWithNoIndent()
        {
            var xml = new Xml();
            xml.Tag("outer", Xml.Fragment(() => xml.Tag("inner")));
            Assert.Equal("<outer><inner /></outer>", xml);
        }

        [Fact]
        public void Xml_ToString_IndentFalse_UsingUtf8_NoDeclaration_OutputsProperString()
        {
            var xml = new Xml();
            xml.Tag("outer", Xml.Fragment(() => xml.Tag("inner")));
            Assert.Equal("<outer><inner /></outer>", xml.ToString(false));
        }

        [Fact]
        public void Xml_ToString_IndentFalse_UsingUtf8_WithDeclaration_OutputsProperString()
        {
            var xml = new Xml();
            xml.Declaration(encoding:"utf-8");
            xml.Tag("outer", Xml.Fragment(() => xml.Tag("inner")));
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><outer><inner /></outer>", xml.ToString(false));
        }

        [Fact]
        public void Xml_ToString_IndentFalse_UsingUtf16_WithDeclaration_OutputsProperString()
        {
            var xml = new Xml();
            xml.Declaration(encoding: "utf-16");
            xml.Tag("outer", Xml.Fragment(() => xml.Tag("inner")));
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-16\"?><outer><inner /></outer>", xml.ToString(false));
        }

        [Fact]
        public void Xml_ToString_IndentTrue_UsingUtf8_NoDeclaration_OutputsProperString()
        {
            var xml = new Xml();
            xml.Tag("outer", Xml.Fragment(() => xml.Tag("inner")));
            Assert.Equal("<outer>\r\n  <inner />\r\n</outer>", xml.ToString(true));
        }

        [Fact]
        public void Xml_ToString_IndentTrue_UsingUtf8_WithDeclaration_OutputsProperString()
        {
            var xml = new Xml();
            xml.Declaration(encoding: "utf-8");
            xml.Tag("outer", Xml.Fragment(() => xml.Tag("inner")));
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<outer>\r\n  <inner />\r\n</outer>", xml.ToString(true));
        }

        [Fact]
        public void Xml_ToString_IndentTrue_UsingUtf16_WithDeclaration_OutputsProperString()
        {
            var xml = new Xml();
            xml.Declaration(encoding: "utf-16");
            xml.Tag("outer", Xml.Fragment(() => xml.Tag("inner")));
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<outer>\r\n  <inner />\r\n</outer>", xml.ToString(true));
        }

        [Fact]
        public void Xml_ToXDocument_ReturnsProperXDocument()
        {
            var xml = new Xml();
            xml.Tag("tag", "value");
            XDocument output = xml.ToXDocument();
            Assert.Equal("<tag>value</tag>", output.FirstNode.ToString());
        }

        [Fact]
        public void Xml_ToXElement_ReturnsProperXElement()
        {
            var xml = new Xml();
            xml.Tag("tag", "value");
            XElement output = xml.ToXElement();
            Assert.Equal("tag", output.Name);
        }

        [Fact]
        public void Xml_ToXmlDocumenet_ReturnsProperXmlDocument()
        {
            var xml = new Xml();
            xml.Tag("tag", "value");
            XmlDocument output = xml.ToXmlDocument();
            Assert.Equal("tag", output.FirstChild.Name);
        }

        [Fact]
        public void Xml_ToXmlNode_NoDocType_ReturnsProperNode()
        {
            var xml = new Xml();
            xml.Tag("tag", "value");
            XmlNode output = xml.ToXmlNode();
            Assert.Equal("tag", output.Name);
        }

        [Fact]
        public void Xml_ToXmlNode_WithDocType_ReturnsProperNode()
        {
            var xml = new Xml();
            // shouldn't return this doc type as the node even though 
            // it will now technicall be first node
            xml.DocumentType("html");
            xml.Tag("tag", "value");
            XmlNode output = xml.ToXmlNode();
            Assert.Equal("tag", output.Name);
        }

        [Fact]
        public void Xml_ToXmlElement_ReturnsNodeAsXmlElement()
        {
            var xml = new Xml();
            xml.Tag("tag", "value");
            XmlElement output = xml.ToXmlElement();
            Assert.Equal("tag", output.Name);
        }

        [Fact]
        public void Xml_Tag_Not_Writes_Blank_XmlNamespaces()
        {
          const string xmlns = "http://foo.com";

          dynamic builder = new Xml();
          builder.Foo(new { xmlns = xmlns }, Xml.Fragment(x =>
            x.Bar("foobar")
          ));

          var namespaces = FindNamespaces(builder);

          Assert.Equal(1, namespaces.Length);
          Assert.Equal(xmlns, namespaces[0]);
        }

        [Fact]
        public void Xml_Tag_Adds_Nested_XmlNamespace_Properly()
        {
          const string xmlns = "http://foo.com";
          const string xmlns2 = "http://foobar.com";

          dynamic builder = new Xml();
          builder.Foo(new { xmlns = xmlns }, Xml.Fragment(x =>
            x.Bar("foobar", new { xmlns = xmlns2 })
          ));

          var namespaces = FindNamespaces(builder);

          Assert.Equal(2, namespaces.Length);
          Assert.Equal(xmlns, namespaces[0]);
          Assert.Equal(xmlns2, namespaces[1]);
        }

        private static XNamespace[] FindNamespaces(dynamic builder)
        {
          XDocument xDocument = builder.ToXDocument();
          var namespaces = xDocument.Root
                                    .DescendantsAndSelf()
                                    .Select(x => x.Name.Namespace)
                                    .Distinct()
                                    .ToArray();
          return namespaces;
        }
    }
}
