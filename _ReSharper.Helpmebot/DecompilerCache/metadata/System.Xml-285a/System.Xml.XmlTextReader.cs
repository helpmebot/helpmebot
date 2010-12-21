// Type: System.Xml.XmlTextReader
// Assembly: System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.Xml.dll

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Xml
{
    public class XmlTextReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
    {
        protected XmlTextReader();
        protected XmlTextReader(XmlNameTable nt);
        public XmlTextReader(Stream input);
        public XmlTextReader(string url, Stream input);
        public XmlTextReader(Stream input, XmlNameTable nt);
        public XmlTextReader(string url, Stream input, XmlNameTable nt);
        public XmlTextReader(TextReader input);
        public XmlTextReader(string url, TextReader input);
        public XmlTextReader(TextReader input, XmlNameTable nt);
        public XmlTextReader(string url, TextReader input, XmlNameTable nt);
        public XmlTextReader(Stream xmlFragment, XmlNodeType fragType, XmlParserContext context);
        public XmlTextReader(string xmlFragment, XmlNodeType fragType, XmlParserContext context);
        public XmlTextReader(string url);
        public XmlTextReader(string url, XmlNameTable nt);
        public override XmlReaderSettings Settings { get; }
        public override XmlNodeType NodeType { get; }
        public override string Name { get; }
        public override string LocalName { get; }
        public override string NamespaceURI { get; }
        public override string Prefix { get; }
        public override bool HasValue { get; }
        public override string Value { get; }
        public override int Depth { get; }
        public override string BaseURI { get; }
        public override bool IsEmptyElement { get; }
        public override bool IsDefault { get; }
        public override char QuoteChar { get; }
        public override XmlSpace XmlSpace { get; }
        public override string XmlLang { get; }
        public override int AttributeCount { get; }
        public override bool EOF { get; }
        public override ReadState ReadState { get; }
        public override XmlNameTable NameTable { get; }
        public override bool CanResolveEntity { get; }
        public override bool CanReadBinaryContent { get; }
        public override bool CanReadValueChunk { get; }
        public bool Namespaces { get; set; }
        public bool Normalization { get; set; }
        public Encoding Encoding { get; }
        public WhitespaceHandling WhitespaceHandling { get; set; }
        public bool ProhibitDtd { get; set; }
        public EntityHandling EntityHandling { get; set; }
        public XmlResolver XmlResolver { set; }

        #region IXmlLineInfo Members

        public bool HasLineInfo();
        public int LineNumber { get; }
        public int LinePosition { get; }

        #endregion

        #region IXmlNamespaceResolver Members

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope);
        string IXmlNamespaceResolver.LookupNamespace(string prefix);
        string IXmlNamespaceResolver.LookupPrefix(string namespaceName);

        #endregion

        public override string GetAttribute(string name);
        public override string GetAttribute(string localName, string namespaceURI);
        public override string GetAttribute(int i);
        public override bool MoveToAttribute(string name);
        public override bool MoveToAttribute(string localName, string namespaceURI);
        public override void MoveToAttribute(int i);
        public override bool MoveToFirstAttribute();
        public override bool MoveToNextAttribute();
        public override bool MoveToElement();
        public override bool ReadAttributeValue();
        public override bool Read();
        public override void Close();
        public override void Skip();
        public override string LookupNamespace(string prefix);
        public override void ResolveEntity();
        public override int ReadContentAsBase64(byte[] buffer, int index, int count);
        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count);
        public override int ReadContentAsBinHex(byte[] buffer, int index, int count);
        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count);
        public override string ReadString();
        public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope);
        public void ResetState();
        public TextReader GetRemainder();
        public int ReadChars(char[] buffer, int index, int count);
        public int ReadBase64(byte[] array, int offset, int len);
        public int ReadBinHex(byte[] array, int offset, int len);
    }
}
