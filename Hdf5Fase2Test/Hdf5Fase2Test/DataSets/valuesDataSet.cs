using Hdf5Fase2Test.Hdf5;
using Hdf5Fase2Test.Types;
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
    public class valuesDataSet : DataSetBase
    {
        /// <summary>
        ///     Retrieves time-series values
        /// </summary>
        /// <param name="index">index in Data</param>
        /// <param name="key">data element to retrieve</param>
        /// <returns>data value</returns>
        private object GetData(int index, string key)
        {
            if (Data != null)
            {
                var localData = (TidalStream)Data[index];
                if (localData != null)
                {
                    switch (key.ToUpper())
                    {
                        case "SPEED":
                            return localData.Speed;

                        case "DIRECTION":
                            return localData.Direction;
                    }
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
                        var dataSetType = CreateDataSetType(objectType) as Hdf5DataSetBase;
                        if (dataSetType != null)
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
                                        // for every time-series
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

                            dataSetType.Write(parentGroupId, name, attributes);
                        }
                    }
                }
            }
        }
    }
}
