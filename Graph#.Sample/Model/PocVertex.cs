using Core.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Xml.Serialization;

namespace GraphSharp.Sample.Model
{
    [DebuggerDisplay("{ID}")]
    public class PocVertex :ITVertex
    {
        public PocVertex()
        {
        }

        public PocVertex(string id, int fontSize)
        {
            ID = id;
            FontSize = fontSize;
        }

        [XmlAttribute]
        public string ID { get; set; }

        [XmlAttribute]
        public int FontSize { get; set; }

        public Point Point { get; set; }
       

        public override string ToString()
        {
            return ID;
        }
    }
}