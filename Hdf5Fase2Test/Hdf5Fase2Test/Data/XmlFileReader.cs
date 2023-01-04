using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hdf5Fase2Test.Data
{
    public class XmlFileReader
    {
        public XmlDocument Read(string fileName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            return xmlDoc;
        }
    }
}
