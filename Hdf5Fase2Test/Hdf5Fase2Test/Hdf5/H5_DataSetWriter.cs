using HDF5CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Hdf5Fase2Test.Hdf5
{
    public class H5_DataSetWriter : H5_ItemBase
    {
        public long H5_DataSetId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public H5_DataSetWriter() 
        {
            H5_DataSetId = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateNode"></param>
        /// <param name="attributesDictionary"></param>
        /// <returns></returns>
        public H5_DataSetWriter Write(long parentGroupId, XmlNode templateNode, Dictionary<string, string> attributesDictionary)
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

            Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
            if (templateNode.Attributes != null && templateNode.Attributes.Count > 0)
            {
                string attributesValue = GetTemplateAttributeValue(templateNode, "attributes");
                if (attributesValue.ToUpper().Equals("YES"))
                {
                    XmlNodeList? attributeNodes = templateNode.SelectNodes("Attributes/Attribute");
                    if (attributeNodes != null && attributeNodes.Count > 0)
                    {
                        attributes = CreateH5Attributes(0, attributeNodes, attributesDictionary, level);
                    }
                }

                string name = GetTemplateAttributeValue(templateNode, "name");
                string value = GetTemplateAttributeValue(templateNode, "value");
                string commaSeparator = GetTemplateAttributeValue(templateNode, "separator");

                if (String.IsNullOrEmpty(value) == false)
                {
                    // value is embedded in the template. Use this for the dataset
                    if (String.IsNullOrEmpty(commaSeparator) == false)
                    {
                        string[] values = value.Split(new[] { commaSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        HDF5CSharp.Hdf5.WriteStrings(parentGroupId, name, values);
                    }
                    else
                    {
                        HDF5CSharp.Hdf5.WriteStrings(parentGroupId, name, new[] { value });
                    }
                }
                else
                {
                    // retrieve dataset type and create dataset
                    string objectType = templateNode.Attributes["type"]?.Value ?? string.Empty;
                    if (String.IsNullOrEmpty(objectType) == false)
                    {
                        var rowNodes = templateNode.SelectNodes(@"Rows/Row");
                        if (rowNodes != null && rowNodes.Count > 0)
                        {
                            var dataSet = CreateDataSetType(objectType) as Hdf5DataSetBase;
                            if (dataSet != null)
                            {
                                foreach (XmlNode rowNode in rowNodes)
                                {
                                    var itemNodes = rowNode.SelectNodes(@"Item");
                                    if (itemNodes != null && itemNodes.Count > 0)
                                    {
                                        dataSet.Analyse(itemNodes);
                                    }
                                }

                                dataSet.Write(parentGroupId, name, attributes);
                            }
                        }
                    }
                }
            }

            return this;
        }
    }
}
