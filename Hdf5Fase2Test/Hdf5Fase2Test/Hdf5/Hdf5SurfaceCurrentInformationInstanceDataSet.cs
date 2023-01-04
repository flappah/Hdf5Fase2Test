using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Hdf5Fase2Test.Types;

namespace Hdf5Fase2Test.Hdf5
{
    public class Hdf5SurfaceCurrentInformationInstanceDataSet : Hdf5DataSetBase
    {
        public new List<SurfaceCurrentInformationInstance> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Hdf5SurfaceCurrentInformationInstanceDataSet()
        {
            Items = new List<SurfaceCurrentInformationInstance>();
        }

        /// <summary>
        ///     Assigns the data in the template to the created dataset
        /// </summary>
        /// <param name="datasetInstance">an instance of the dataset</param>
        /// <param name="itemNodes">nodes of template items (Rows/Row/Item)</param>
        /// <returns>datasetinstance</returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected SurfaceCurrentInformationInstance AssignTemplateDataToDataSet(XmlNodeList itemNodes)
        {
            if (itemNodes is null)
            {
                throw new ArgumentNullException(nameof(itemNodes));
            }

            object datasetInstance = new SurfaceCurrentInformationInstance();
            foreach (XmlNode itemNode in itemNodes)
            {
                var name = itemNode.Attributes?["name"]?.Value ?? string.Empty;
                var value = itemNode.Attributes?["value"]?.Value ?? string.Empty;

                if (string.IsNullOrEmpty(name) == false)
                {
                    FieldInfo[] fields = datasetInstance.GetType().GetFields();
                    if (fields != null && fields.Count() > 0)
                    {
                        FieldInfo? fieldToUpdate = fields.ToList().Find(f => f.Name == name);
                        if (fieldToUpdate != null)
                        {
                            switch (fieldToUpdate.FieldType.Name.ToUpper())
                            {
                                case "BYTE":
                                case "UINT8":
                                    if (byte.TryParse(value, out byte byteValue) == false)
                                    {
                                        byteValue = 0;
                                    }
                                    fieldToUpdate.SetValue(datasetInstance, byteValue);
                                    break;

                                case "SHORT":
                                case "INT16":
                                    if (short.TryParse(value, out short shortValue) == false)
                                    {
                                        shortValue = 0;
                                    }
                                    fieldToUpdate.SetValue(datasetInstance, shortValue);
                                    break;

                                case "INT":
                                case "INT32":
                                    if (int.TryParse(value, out int intValue) == false)
                                    {
                                        intValue = 0;
                                    }
                                    fieldToUpdate.SetValue(datasetInstance, intValue);
                                    break;

                                case "LONG":
                                case "INT64":
                                    if (long.TryParse(value, out long longValue) == false)
                                    {
                                        longValue = 0;
                                    }
                                    fieldToUpdate.SetValue(datasetInstance, longValue);
                                    break;

                                case "DOUBLE":
                                    if (double.TryParse(value, value.Contains(",") ? new CultureInfo("NL-nl") : new CultureInfo("EN-us"), out double doubleValue) == false)
                                    {
                                        doubleValue = 0.0;
                                    }
                                    fieldToUpdate.SetValue(datasetInstance, doubleValue);
                                    break;

                                case "FLOAT":
                                    if (float.TryParse(value, value.Contains(",") ? new CultureInfo("NL-nl") : new CultureInfo("EN-us"), out float floatValue) == false)
                                    {
                                        floatValue = 0.0f;
                                    }
                                    fieldToUpdate.SetValue(datasetInstance, floatValue);
                                    break;

                                default:
                                    fieldToUpdate.SetValue(datasetInstance, value);
                                    break;

                            }
                        }
                    }
                }
            }

            return (SurfaceCurrentInformationInstance)datasetInstance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemNodes"></param>
        public override void Analyse(XmlNodeList? itemNodes)
        {
            if (itemNodes == null)
            {
                throw new ArgumentNullException(nameof(itemNodes));
            }

            var surfaceCurrentInfoDataset = AssignTemplateDataToDataSet(itemNodes) as SurfaceCurrentInformationInstance?;
            if (surfaceCurrentInfoDataset != null)
            {
                Items.Add((SurfaceCurrentInformationInstance)surfaceCurrentInfoDataset);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentGroupId"></param>
        /// <param name="name"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override (int success, long createdGroupId) Write(long parentGroupId, string name, Dictionary<string, List<string>> attributes)
        {
            return HDF5CSharp.Hdf5.WriteCompounds(parentGroupId, name, Items, attributes);
        }

    }
}
