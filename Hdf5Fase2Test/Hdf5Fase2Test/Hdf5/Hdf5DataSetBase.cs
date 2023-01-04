using System.Globalization;
using System.Reflection;
using System.Xml;

namespace Hdf5Fase2Test.Hdf5
{
    public abstract class Hdf5DataSetBase
    {
        //public virtual List<object>? Items { get; set; }

        public abstract void Analyse(XmlNodeList? itemNodes);
        public abstract (int success, long createdGroupId) Write(long parentGroupId, string name, Dictionary<string, List<string>> attributes);

    }
}
