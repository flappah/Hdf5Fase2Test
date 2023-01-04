using Hdf5Fase2Test.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using Hdf5Fase2Test.Hdf5;

namespace Hdf5Fase2Test.DataSets
{
    public class geometryValuesDataSet : DataSetBase
    {
        /// <summary>
        ///     Retrieves stations values
        /// </summary>
        /// <param name="index">index in Data</param>
        /// <param name="key">data element to retrieve</param>
        /// <returns>data value</returns>
        private object GetData(int index, string key)
        {
            if (Data != null)
            {
                var station = Data[index] as Station;

                switch (key.ToUpper())
                {
                    case "LONGITUDE":
                        return station.Longitude;

                    case "LATITUDE":
                        return station.Latitude;
                }
            }

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
                        // if type is statically given, try to resolve the type and fill with row nodes
                        var dataSetType = CreateDataSetType(objectType) as Hdf5DataSetBase;
                        if (dataSetType != null)
                        {
                            var rowNodes = templateNode.SelectNodes(@"Rows/Row");
                            if (rowNodes != null && rowNodes.Count > 0)
                            {
                                // retrieve statically assigned data from template
                                foreach (XmlNode rowNode in rowNodes)
                                {
                                    var itemNodes = rowNode.SelectNodes(@"Item");
                                    if (itemNodes != null && itemNodes.Count > 0)
                                    {
                                        dataSetType.Analyse(itemNodes);
                                        dataSetType.Write(parentGroupId, name, attributes);
                                    }
                                }
                            }
                            else
                            {
                                // retrieve data from data_field with method GetData()
                                var columnNodes = templateNode.SelectNodes(@"Columns/Column");
                                if (columnNodes != null && columnNodes.Count > 0)
                                {
                                    // create a lookup table for property names 
                                    var fieldPropertyMappings = new Dictionary<string, string>();
                                    foreach (XmlNode columnNode in columnNodes)
                                    {
                                        string property_name = GetTemplateAttributeValue(columnNode, "name");
                                        string property_data_field = GetTemplateAttributeValue(columnNode, "data_field");

                                        fieldPropertyMappings.Add(property_name, property_data_field);
                                    }

                                    var dataType = CreateDataType(objectType);
                                    FieldInfo[] fields = dataType.GetType().GetFields();
                                    PropertyInfo? itemsProperty = dataSetType.GetType().GetProperty("Items");

                                    if (itemsProperty != null && itemsProperty.PropertyType != null)
                                    {
                                        var items = itemsProperty.GetValue(dataSetType);

                                        var addMethod = itemsProperty.PropertyType.GetMethod("Add");
                                        if (addMethod != null)
                                        {
                                            // for every station
                                            for (int i = 0; i < Data.Length; i++)
                                            {
                                                // enumerate fields of the data type
                                                foreach (FieldInfo field in fields)
                                                {
                                                    if (fieldPropertyMappings.ContainsKey(field.Name))
                                                    {
                                                        // and assign the data value
                                                        var dataValue = GetData(i, fieldPropertyMappings[field.Name]);
                                                        field.SetValue(dataType, dataValue);
                                                    }
                                                }

                                                addMethod.Invoke(items, new[] { dataType });
                                            }
                                        }
                                    }
                                }
                            }

                            dataSetType.Write(parentGroupId, name, attributes);
                        }
                    }
                }
            }
        }
    }
}
