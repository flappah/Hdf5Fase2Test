using Hdf5Fase2Test.Hdf5;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hdf5Fase2Test.DataSets
{
    public class uncertaintyDataSet : DataSetBase
    {
        /// <summary>
        ///     Retrieves time-series values
        /// </summary>
        /// <param name="index">index in Data</param>
        /// <param name="key">data element to retrieve</param>
        /// <returns>data value</returns>
        private object GetData(int index, string key)
        {
            // no implementation yet
            return "";
        }

        /// <summary>
        ///     Creates an HDF5 dataset and writes data to it
        /// </summary>
        /// <param name="parentGroupId"></param>
        /// <param name="templateNode"></param>
        /// <param name="attributesDictionary"></param>
        /// <param name="parentLevel"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void Write(long parentGroupId, XmlNode templateNode, Dictionary<string, string> attributesDictionary, string parentLevel = "")
        {
            if (templateNode is null)
            {
                throw new ArgumentNullException(nameof(templateNode));
            }

            if (attributesDictionary is null)
            {
                throw new ArgumentNullException(nameof(attributesDictionary));
            }

            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));
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

                if (String.IsNullOrEmpty(value) == false)
                {
                    // value is embedded in the template. Use this for the dataset
                    HDF5CSharp.Hdf5.WriteStrings(parentGroupId, name, new[] { value });
                }
                else
                {
                    // retrieve dataset type and create dataset
                    string objectType = templateNode.Attributes["type"]?.Value ?? string.Empty;
                    if (String.IsNullOrEmpty(objectType) == false)
                    {
                        var dataSetType = CreateDataSetType(objectType) as Hdf5UncertaintyDataSet;
                        if (dataSetType != null)
                        {
                            XmlNodeList? uncertaintyRowNodes = templateNode.SelectNodes("Rows/Row");
                            if (uncertaintyRowNodes != null && uncertaintyRowNodes.Count > 0)
                            {
                                // retrieve statically assigned data from template
                                foreach (XmlNode uncertaintyRowNode in uncertaintyRowNodes)
                                {
                                    XmlNodeList? itemNodes = uncertaintyRowNode.SelectNodes("Item");
                                    if (itemNodes != null && itemNodes.Count > 0)
                                    {
                                        // and assign the data value
                                        dataSetType.Analyse(itemNodes);
                                        dataSetType.Write(parentGroupId, name, attributes);
                                    }
                                }
                            }
                            else
                            {
                                // retrieve data from data_field with method GetData()
                            }
                        }
                    }
                }
            }
        }
    }
}
