using Hdf5Fase2Test.Hdf5;
using System.Xml;

namespace Hdf5Fase2Test.Groups
{
    public abstract class GroupBase : H5_ItemBase
    {
        public object[]? Data { get; set; }

        public abstract Dictionary<string, string> CreateAttributeValues(XmlNode templateNode, Dictionary<string, string> attributesDictionary);
        public abstract void Write(long parentGroupId, XmlNode templateNode, Dictionary<string, string> attributesDictionary, string parentLevel = "");
    }
}
