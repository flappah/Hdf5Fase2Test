using Hdf5Fase2Test.Hdf5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hdf5Fase2Test.DataSets
{
    public abstract class DataSetBase : H5_ItemBase
    {
        public object[]? Data { get; set; }

        public abstract void Write(long parentGroupId, XmlNode templateNode, Dictionary<string, string> attributesDictionary, string parentLevel = "");
    }
}
