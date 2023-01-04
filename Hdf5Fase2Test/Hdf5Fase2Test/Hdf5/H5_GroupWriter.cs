using Hdf5Fase2Test.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hdf5Fase2Test.Hdf5
{
    public class H5_GroupWriter : H5_ItemBase
    {
        public long H5_GroupId { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public H5_GroupWriter()
        {
            H5_GroupId = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentGroupId"></param>
        /// <param name="templateNode"></param>
        /// <param name="attributesDictionary"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public H5_GroupWriter Write(long parentGroupId, XmlNode templateNode, Dictionary<string, string> attributesDictionary)
        {
            if (templateNode is null)
            {
                throw new ArgumentNullException(nameof(templateNode));
            }

            if (attributesDictionary is null)
            {
                throw new ArgumentNullException(nameof(attributesDictionary));
            }

            string level = GetLevel(templateNode) + "/";

            string baseGroupName = "";
            if (templateNode.Attributes != null && templateNode.Attributes.Count > 0)
            {
                baseGroupName = GetTemplateAttributeValue(templateNode, "name");

                H5_GroupId = HDF5CSharp.Hdf5.CreateOrOpenGroup(parentGroupId, baseGroupName);
                ParseTemplateAttributes(H5_GroupId, templateNode, attributesDictionary, level);

                var groupNodes = templateNode.SelectNodes(@"Group");
                if (groupNodes != null && groupNodes.Count > 0)
                {
                    foreach (XmlNode groupNode in groupNodes)
                    {
                        var h5GroupWriter = new H5_GroupWriter();
                        h5GroupWriter.Write(H5_GroupId, groupNode, attributesDictionary);
                    }
                }

                var dataSetNodes = templateNode.SelectNodes(@"DataSet");
                if (dataSetNodes != null && dataSetNodes.Count > 0)
                {
                    foreach (XmlNode dataSetNode in dataSetNodes)
                    {
                        var h5dataSetWriter = new H5_DataSetWriter();
                        h5dataSetWriter.Write(H5_GroupId, dataSetNode, attributesDictionary);
                    }
                }

                HDF5CSharp.Hdf5.CloseGroup(H5_GroupId);
            }

            return this;
        }
    }
}
