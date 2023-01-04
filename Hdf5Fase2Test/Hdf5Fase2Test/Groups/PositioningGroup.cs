using Hdf5Fase2Test.DataSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hdf5Fase2Test.Groups
{
    public class PositioningGroup : GroupBase
    {
        public long H5_GroupId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateNode"></param>
        /// <param name="attributesDictionary"></param>
        /// <returns></returns>
        public override Dictionary<string, string> CreateAttributeValues(XmlNode templateNode, Dictionary<string, string> attributesDictionary)
        {
            return attributesDictionary;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentGroupId"></param>
        /// <param name="templateNode"></param>
        /// <param name="attributesDictionary"></param>
        /// <param name="parentLevel"></param>
        public override void Write(long parentGroupId, XmlNode templateNode, Dictionary<string, string> attributesDictionary, string parentLevel = "")
        {
            string level = GetLevel(templateNode) + "/";
            var groupName = GetTemplateAttributeValue(templateNode, "name"); ;

            H5_GroupId = HDF5CSharp.Hdf5.CreateOrOpenGroup(parentGroupId, groupName);

            var dataSetNodes = templateNode.SelectNodes("DataSet");
            if (dataSetNodes != null && dataSetNodes.Count > 0)
            {
                // analyse template, retrieve structure and parse Data into template format
                foreach (XmlNode dataSetNode in dataSetNodes)
                {
                    if (dataSetNode != null)
                    {
                        string objectTypeName = dataSetNode.Attributes?["name"]?.Value.ToString() ?? string.Empty;

                        if (String.IsNullOrEmpty(objectTypeName) == false)
                        {
                            var factory = new DataSetFactory();
                            var dataSetInstance = factory.Create(objectTypeName);
                            dataSetInstance.Data = Data;
                            dataSetInstance.Write(H5_GroupId, dataSetNode, attributesDictionary, level);
                        }
                    }
                }
            }

            HDF5CSharp.Hdf5.CloseGroup(H5_GroupId);
        }
    }
}
