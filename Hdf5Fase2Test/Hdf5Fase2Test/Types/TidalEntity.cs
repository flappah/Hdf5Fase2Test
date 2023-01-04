using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Hdf5Fase2Test.Types
{
    public abstract class TidalEntity : IXmlSerializable
    {
        public DateTime TimeStamp { get; set; }

        public abstract XmlSchema? GetSchema();
        public abstract void Parse(XmlNode node);
        public abstract void ReadXml(XmlReader reader);
        public abstract void WriteXml(XmlWriter writer);
    }
}
