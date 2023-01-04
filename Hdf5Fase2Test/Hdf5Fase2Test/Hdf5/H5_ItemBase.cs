using HDF.PInvoke;
using Hdf5Fase2Test.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Hdf5Fase2Test.Hdf5
{
    public abstract class H5_ItemBase
    {
        /// <summary>
        ///     Create dataset of type specified by string to create and add items and write to an HDF5 dataset
        /// </summary>
        /// <param name="objectType">type of object as string</param>
        /// <returns></returns>
        /// <exception cref="Exception">When type can't be created</exception>
        protected object CreateDataSetType(string objectType)
        {
            var asm = Assembly.GetExecutingAssembly();
            ObjectHandle? handle = Activator.CreateInstance(asm.GetName().ToString(), $"Hdf5Fase2Test.Hdf5.Hdf5{objectType}DataSet");
            if (handle != null)
            {
                var datasetInstance = handle.Unwrap();

                if (datasetInstance != null)
                {
                    return datasetInstance;
                }
            }

            throw new Exception($"Couldn't create type '{objectType}'!");
        }

        /// <summary>
        ///     Create datatype that is associated with the dataset type
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object CreateDataType(string objectType)
        {
            var asm = Assembly.GetExecutingAssembly();
            ObjectHandle? handle = Activator.CreateInstance(asm.GetName().ToString(), $"Hdf5Fase2Test.Types.{objectType}");
            if (handle != null)
            {
                var datasetInstance = handle.Unwrap();

                if (datasetInstance != null)
                {
                    return datasetInstance;
                }
            }

            throw new Exception($"Couldn't create type '{objectType}'!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hdf5GroupId"></param>
        /// <param name="attributeNodes"></param>
        /// <param name="attributesDictionary"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected Dictionary<string, List<string>> CreateH5Attributes(long hdf5GroupId, XmlNodeList? attributeNodes, Dictionary<string, string> attributesDictionary, string level = "/")
        {
            if (attributeNodes == null)
            {
                throw new ArgumentNullException(nameof(attributeNodes));
            }

            if (attributesDictionary is null)
            {
                throw new ArgumentNullException(nameof(attributesDictionary));
            }

            var attributes = new Dictionary<string, List<string>>();    
            foreach (XmlNode attributeNode in attributeNodes)
            {
                if (attributeNode.Attributes?.Count > 0)
                {
                    var name = attributeNode.Attributes["name"]?.InnerText.ToString() ?? string.Empty;
                    var type = attributeNode.Attributes["type"]?.InnerText.ToString() ?? string.Empty;
                    var mask = attributeNode.Attributes["mask"]?.InnerText.ToString() ?? string.Empty;

                    if (String.IsNullOrEmpty(name) == false && String.IsNullOrEmpty(type) == false)
                    { 
                        IEnumerable<KeyValuePair<string, string>> dictionaryItem;

                        if (level.Contains("_") && level.Contains("Group_F") == false)
                        {
                            var regex = new Regex("([0-9]{2,3})");
                            string key = "/" + regex.Replace(level, "x") + name;
                            dictionaryItem = attributesDictionary.Where(ad => ad.Key.Equals(key));
                        }
                        else
                        {
                            dictionaryItem =
                                attributesDictionary.Where(ad => ad.Key.Equals($"{level}{name}"));
                        }

                        string value = "";
                        if (dictionaryItem != null)
                        {
                            value = dictionaryItem.FirstOrDefault().Value;
                        }

                        if (String.IsNullOrEmpty(name) == false &&
                            String.IsNullOrEmpty(type) == false &&
                            String.IsNullOrEmpty(value) == false)
                        {
                            if (hdf5GroupId > 0)
                            {
                                WriteH5Attribute(hdf5GroupId, name, type, value, mask);
                            }
                            else
                            {
                                attributes.Add(name, new List<string> { value });   
                            }
                        }
                    }
                }
            }

            return attributes;
        }

        /// <summary>
        ///     Retrieve attribute value
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected string GetTemplateAttributeValue(XmlNode node, string attributeName)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var attributeValue = node.Attributes?[attributeName]?.InnerText.ToString() ?? string.Empty;
            return attributeValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentNode"></param>
        /// <returns></returns>
        protected string GetLevel(XmlNode currentNode)
        {
            string level = string.Empty;
            if (currentNode.ParentNode != currentNode.OwnerDocument?.DocumentElement &&
                currentNode.ParentNode != null)
            {
                level = GetLevel(currentNode.ParentNode);
            }

            if (currentNode.Attributes != null && currentNode.Attributes.Count > 0)
            {
                level += "/" + currentNode.Attributes["name"]?.Value.ToString() ?? string.Empty;

                if (currentNode.Attributes["multiple"] != null &&
                    currentNode.Attributes["multiple"].Value.ToUpper().Equals("TRUE"))
                {
                    level += "_x";
                }
            }
            else
            {
                level += "/" + currentNode.Name ?? string.Empty;
            }

            return level;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="node"></param>
        /// <param name="attributesDictionary"></param>
        /// <param name="level"></param>
        protected void ParseTemplateAttributes (long groupId, XmlNode node, Dictionary<string, string> attributesDictionary, string level)
        {
            if (node.Attributes != null && node.Attributes.Count > 0)
            {
                var attributesValue = node.Attributes["attributes"]?.InnerText.ToString() ?? string.Empty;
                if (attributesValue.ToUpper().Equals("YES"))
                {
                    XmlNodeList? attributeNodes = node.SelectNodes("Attributes/Attribute");
                    if (attributeNodes != null && attributeNodes.Count > 0)
                    {
                        CreateH5Attributes(groupId, attributeNodes, attributesDictionary, level);
                    }
                }
            }
        }

        /// <summary>
        ///     Writes an attribute to the H5 group (file) based on its type and value
        /// </summary>
        /// <param name="hdf5GroupId"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="mask"></param>
        protected void WriteH5Attribute(long hdf5GroupId, string name, string type, string value, string mask = "")
        {
            switch (type.ToUpper())
            {
                case "BYTE":
                case "UINT8":
                    if (byte.TryParse(value, out byte byteValue) == false)
                    {
                        byteValue = 0;
                    }

                    HDF5CSharp.Hdf5.WriteAttribute(hdf5GroupId, name, byteValue);
                    break;

                case "SHORT":
                case "INT16":
                    if (short.TryParse(value, out short shortValue) == false)
                    {
                        shortValue = -1;
                    }

                    HDF5CSharp.Hdf5.WriteAttribute(hdf5GroupId, name, shortValue);
                    break;

                case "INT":
                case "INT32":
                    if (int.TryParse(value, out int intValue) == false)
                    {
                        intValue = -1;
                    }

                    HDF5CSharp.Hdf5.WriteAttribute(hdf5GroupId, name, intValue);
                    break;

                case "LONG":
                case "INT64":
                    if (long.TryParse(value, out long longValue) == false)
                    {
                        longValue = -1;
                    }

                    HDF5CSharp.Hdf5.WriteAttribute(hdf5GroupId, name, longValue);
                    break;

                case "DOUBLE":
                    if (double.TryParse(value, new CultureInfo("en-US"), out double doubleValue) == false)
                    {
                        doubleValue = -1.0;
                    }

                    HDF5CSharp.Hdf5.WriteAttribute(hdf5GroupId, name, doubleValue);
                    break;

                case "FLOAT":
                    if (float.TryParse(value, new CultureInfo("en-US"), out float floatValue) == false)
                    {
                        floatValue = -1f;
                    }

                    HDF5CSharp.Hdf5.WriteAttribute(hdf5GroupId, name, floatValue);
                    break;

                case "STRING":
                    HDF5CSharp.Hdf5.WriteAttribute(hdf5GroupId, name, value);
                    break;

                case "DATETIME":
                    if (DateTime.TryParse(value, out DateTime datetimeValue) == false)
                    {
                        datetimeValue = DateTime.MinValue;
                    }

                    HDF5CSharp.Hdf5.WriteAttribute(hdf5GroupId, name, datetimeValue.ToString(mask));
                    break;

                case "COMMONPOINTRULEENUM":
                    if (byte.TryParse(value, out byte cprValue))
                    {
                        var cprEnum = new CommonPointRuleEnum(cprValue);
                        cprEnum.WriteToHdf5(hdf5GroupId);
                    }
                    break;

                case "DATACODINGFORMATENUM":
                    if (byte.TryParse(value, out byte dcfValue))
                    {
                        var dcfEnum = new DataCodingFormatEnum(dcfValue);
                        dcfEnum.WriteToHdf5(hdf5GroupId);
                    }
                    break;

                case "DEPTHTYPEINDEXENUM":
                    if (byte.TryParse(value, out byte dtiValue))
                    {
                        var dtiEnum = new DepthTypeIndexEnum(dtiValue);
                        dtiEnum.WriteToHdf5(hdf5GroupId);
                    }
                    break;

                case "TYPEOFCURRENTDATAENUM":
                    if (byte.TryParse(value, out byte tcdValue))
                    {
                        var tcdEnum = new TypeOfCurrentDataEnum(tcdValue);
                        tcdEnum.WriteToHdf5(hdf5GroupId);
                    }
                    break;

                case "VERTICALCOORDINATEBASEENUM":
                    if (byte.TryParse(value, out byte vcbValue))
                    {
                        var vcbEnum = new VerticalCoordinateBaseEnum(vcbValue);
                        vcbEnum.WriteToHdf5(hdf5GroupId);
                    }
                    break;
            }
        }
    }
}
