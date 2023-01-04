using Hdf5Fase2Test.DataSets;
using Hdf5Fase2Test.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Hdf5Fase2Test.Groups
{
    public class GroupGroup : GroupBase
    {
        public long H5_GroupId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public GroupGroup()
        {
            Data = new Station[0];
        }

        /// <summary>
        ///     Creates attributes and assigns auto or static values to the attributes
        /// </summary>
        /// <param name="templateNode">node of the element in the S111 XML template</param>
        /// <param name="attributesDictionary">input attribute dictionary</param>
        /// <returns>altered attribute dictionary</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Dictionary<string, string> CreateAttributeValues(XmlNode templateNode, Dictionary<string, string> attributesDictionary)
        {
            if (Data == null || Data.Length == 0)
            {
                throw new ArgumentNullException("Data");
            }

            var localAttributesDictionary = new Dictionary<string, string>(attributesDictionary);

            XmlNodeList? attributeNodes = templateNode.SelectNodes("Attributes/Attribute");
            if (attributeNodes != null && attributeNodes.Count > 0)
            {
                string level = GetLevel(templateNode) + "/";

                foreach (XmlNode attributeNode in attributeNodes)
                {
                    if (attributeNode.Attributes?.Count > 0)
                    {
                        var mode = attributeNode.Attributes["mode"]?.InnerText.ToString() ?? string.Empty;
                        var name = attributeNode.Attributes["name"]?.InnerText.ToString() ?? string.Empty;
                        var type = attributeNode.Attributes["type"]?.InnerText.ToString() ?? string.Empty;
                        var mask = attributeNode.Attributes["mask"]?.InnerText.ToString() ?? string.Empty;
                        var value = attributeNode.Attributes["value"]?.InnerText.ToString() ?? string.Empty;

                        var key = $"{level}{name}";
                        if (mode.ToUpper().Equals("AUTO"))
                        {
                            switch (name.ToUpper())
                            {
                                case "ENDDATETIME":
                                    var lastTimeStamp = ((Station)Data[0]).TimeSeries.First().Value.Last().TimeStamp;
                                    if (localAttributesDictionary.ContainsKey(key) == false)
                                    {
                                        localAttributesDictionary.Add(key, lastTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
                                    }
                                    break;

                                case "NUMBEROFTIMES":
                                    var times = ((Station)Data[0]).TimeSeries.First().Value.Count();
                                    if (localAttributesDictionary.ContainsKey(key) == false)
                                    {
                                        localAttributesDictionary.Add(key, times.ToString());
                                    }
                                    break;

                                case "STARTDATETIME":
                                    var firstTimeStamp = ((Station)Data[0]).TimeSeries.First().Value.First().TimeStamp;
                                    if (localAttributesDictionary.ContainsKey(key) == false)
                                    {
                                        localAttributesDictionary.Add(key, firstTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
                                    }
                                    break;

                                case "STATIONIDENTIFICATION":
                                    var stationIdent = ((Station)Data[0]).Name;
                                    if (localAttributesDictionary.ContainsKey(key) == false)
                                    {
                                        localAttributesDictionary.Add(key, stationIdent);
                                    }
                                    break;

                                case "STATIONNAME":
                                    var stationName = ((Station)Data[0]).Name;
                                    if (localAttributesDictionary.ContainsKey(key) == false)
                                    {
                                        localAttributesDictionary.Add(key, stationName);
                                    }
                                    break;
                            }
                        }
                        else if (mode.ToUpper().Equals("STATIC"))
                        {
                            if (localAttributesDictionary.ContainsKey(key) == false)
                            {
                                localAttributesDictionary.Add(key, value);
                            }
                        }
                    }
                }
            }

            return localAttributesDictionary;
        }

        /// <summary>
        ///     Writes group and its attributes to an H5 Group 
        /// </summary>
        /// <param name="parentGroupId"></param>
        /// <param name="templateNode"></param>
        /// <param name="attributesDictionary"></param>
        /// <param name="parentLevel"></param>
        public override void Write(long parentGroupId, XmlNode templateNode, Dictionary<string, string> attributesDictionary, string parentLevel = "")
        {
            Station[] localData = Array.ConvertAll(Data ?? new object[0], s => new Station
            {
                Foid = ((Station)s).Foid,
                Name = ((Station)s).Name,
                Position = ((Station)s).Position,
                Longitude = ((Station)s).Longitude,
                Latitude = ((Station)s).Latitude,
                TidalExtremes = Array.ConvertAll(((Station)s).TidalExtremes, te => new TidalExtreme
                {
                    DaysAfterMoonPhase = te.DaysAfterMoonPhase,
                    Height = te.Height,
                    MoonPhase = te.MoonPhase,
                    TideType = te.TideType,
                    TimeStamp = te.TimeStamp
                }),
                TidalStreams = Array.ConvertAll(((Station)s).TidalStreams, ts => new TidalStream
                {
                    Direction = ts.Direction,
                    Speed = ts.Speed,
                    HourlyOffset = ts.HourlyOffset,
                    NeapValue = ts.NeapValue,
                    SpringValue = ts.SpringValue,
                    TimeStamp = ts.TimeStamp
                }),
                TimeSeries = ((Station)s).TimeSeries
            });

            string baseGroupName = "";
            if (templateNode.Attributes != null && templateNode.Attributes.Count > 0)
            {
                baseGroupName = GetTemplateAttributeValue(templateNode, "name");

                string multiple = GetTemplateAttributeValue(templateNode, "multiple");
                if (multiple.ToUpper().Equals("TRUE"))
                {
                    string upperLimitValue = GetTemplateAttributeValue(templateNode, "upper_limit");
                    if (short.TryParse(upperLimitValue, out short upperLimit) == false)
                    {
                        upperLimit = 1;
                    }

                    int i = 0;
                    do
                    {
                        //Data = new object[] { localData[i] };

                        // and now create auto generated variables
                        var localAttributesDictionary = CreateAttributeValues(templateNode, attributesDictionary);

                        string groupName = "";
                        if (upperLimitValue.Length == 2)
                        {
                            groupName = $"{baseGroupName}_{(++i).ToString("00")}";
                        }
                        else if (upperLimitValue.Length == 3)
                        {
                            groupName = $"{baseGroupName}_{(++i).ToString("000")}";
                        }
                        H5_GroupId = HDF5CSharp.Hdf5.CreateOrOpenGroup(parentGroupId, groupName);

                        // make a valid level indentifier
                        string level = $"{parentLevel}{groupName}/";
                        // parse template at level and insert data
                        ParseTemplateAttributes(H5_GroupId, templateNode, localAttributesDictionary, level);

                        // is not used in S111!
                        //var groupNodes = templateNode.SelectNodes("Group");
                        //if (groupNodes != null && groupNodes.Count > 0)
                        //{
                        //    foreach (XmlNode groupNode in groupNodes)
                        //    {
                        //        if (groupNode != null)
                        //        {
                        //            string objectTypeName = groupNode.Attributes?["name"]?.Value.ToString() ?? string.Empty;

                        //            if (String.IsNullOrEmpty(objectTypeName) == false)
                        //            {
                        //                var groupFactory = new GroupFactory();
                        //                var groupInstance = groupFactory.Create(objectTypeName);
                        //                groupInstance.Data = Data;
                        //                groupInstance.Write(H5_GroupId, groupNode, localAttributesDictionary, level);
                        //            }
                        //        }
                        //    }
                        //}

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
                                        dataSetInstance.Data = localData[i - 1].TimeSeries.First().Value.ToArray();
                                        dataSetInstance.Write(H5_GroupId, dataSetNode, localAttributesDictionary, level);
                                }
                                }
                            }
                        }

                        HDF5CSharp.Hdf5.CloseGroup(H5_GroupId);
                    }
                    while (i < localData.Length && i < upperLimit);
                }
            }
        }
    }
}
