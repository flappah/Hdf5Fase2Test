using HDF5.NET;
using Hdf5Fase2Test.DataSets;
using Hdf5Fase2Test.Hdf5;
using Hdf5Fase2Test.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hdf5Fase2Test.Groups
{
    /// <summary>
    ///     SurfaceCurrent is a feature. Every feature is a specified H5_GroupWriter that is able to write
    ///     encapsulated data to the Hdf5 output format an H5_GroupWriter is able to generate.
    /// </summary>
    public class SurfaceCurrentGroup : GroupBase
    {
        public long H5_GroupId { get; set; }
        public bool _instance;

        /// <summary>
        /// 
        /// </summary>
        public SurfaceCurrentGroup()
        {
            Data = new Station[0];
            _instance = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        public SurfaceCurrentGroup(bool instance)
        {
            Data = new Station[0];
            _instance = instance;
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
                            if (_instance)
                            {
                                switch (name.ToUpper())
                                {
                                    case "DATETIMEOFFIRSTRECORD":
                                        var minTimeStamp = DateTime.MaxValue;
                                        foreach (Station station in Data)
                                        {
                                            var min = station.TimeSeries.Select(v => v.Value.MinBy(h => h.TimeStamp)).MinBy(r => r.TimeStamp)?.TimeStamp;
                                            if (min != null && min < minTimeStamp)
                                            {
                                                minTimeStamp = (DateTime)min;
                                            }
                                        }

                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, minTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
                                        }
                                        break;

                                    case "DATETIMEOFLASTRECORD":
                                        var maxTimeStamp = DateTime.MinValue;
                                        foreach (Station station in Data)
                                        {
                                            var max = station.TimeSeries.Select(v => v.Value.MaxBy(h => h.TimeStamp)).MaxBy(r => r.TimeStamp)?.TimeStamp;
                                            if (max != null && max > maxTimeStamp)
                                            {
                                                maxTimeStamp = (DateTime)max;
                                            }
                                        }

                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, maxTimeStamp.ToString("yyyy-MM-dd HH:mm:ss"));
                                        }
                                        break;

                                    case "EASTBOUNDLONGITUDE":
                                        var eastBoundLongitude = localAttributesDictionary["/eastBoundLongitude"];
                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, eastBoundLongitude);
                                        }
                                        break;

                                    case "NORTHBOUNDLATITUDE":
                                        var northBoundLatitude = localAttributesDictionary["/northBoundLatitude"];
                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, northBoundLatitude);
                                        }
                                        break;

                                    case "NUMBEROFSTATIONS":
                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, Data.Length.ToString());
                                        }
                                        break;

                                    case "NUMBEROFTIMES":
                                        int totalNumberOfTimes = 0;
                                        foreach (Station station in Data)
                                        {
                                            totalNumberOfTimes += station.TimeSeries.First().Value.Count();
                                        }

                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, totalNumberOfTimes.ToString());
                                        }
                                        break;

                                    case "NUMGRP":
                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, Data.Length.ToString());
                                        }
                                        break;

                                    case "SOUTHBOUNDLATITUDE":
                                        var southBoundLatitude = localAttributesDictionary["/southBoundLatitude"];
                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, southBoundLatitude);
                                        }
                                        break;

                                    case "WESTBOUNDLONGITUDE":
                                        var westBoundLongitude = localAttributesDictionary["/westBoundLongitude"];
                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, westBoundLongitude);
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                switch (name.ToUpper())
                                {
                                    case "MAXDATASETCURRENTSPEED":
                                        double maxHeight = 0.0;

                                        foreach (Station station in Data)
                                        {
                                            var max = station.TimeSeries.Select(v => v.Value.MaxBy(h => h.Speed)).MaxBy(r => r.Speed);
                                            if (max?.Speed > maxHeight)
                                            {
                                                maxHeight = max.Speed;
                                            }
                                        }

                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, maxHeight.ToString(new CultureInfo("en-US")));
                                        }
                                        break;

                                    case "MINDATASETCURRENTSPEED":
                                        double minHeight = 999.0;

                                        foreach (Station station in Data)
                                        {
                                            var min = station.TimeSeries.Select(v => v.Value.MinBy(h => h.Speed)).MinBy(r => r.Speed);
                                            if (min?.Speed < minHeight)
                                            {
                                                minHeight = min.Speed;
                                            }
                                        }

                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, minHeight.ToString(new CultureInfo("en-US")));
                                        }
                                        break;

                                    case "NUMINSTANCES":
                                        if (localAttributesDictionary.ContainsKey(key) == false)
                                        {
                                            localAttributesDictionary.Add(key, ((Station)Data.First()).TimeSeries.Count.ToString(new CultureInfo("en-US")));
                                        }
                                        break;

                                }
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

                    var duplicatedStationData = new Station[localData.Length];
                    Array.Copy(localData, duplicatedStationData, localData.Length);

                    int numberOfTimeSeries = localData.First().TimeSeries.Count;
                    int i = 0;
                    do
                    {
                        var localStationData = new List<Station>();
                        // Create a local segmented Data storage container
                        foreach(Station station in duplicatedStationData)
                        {
                            int firstKeyValue = station.TimeSeries.First().Key;

                            localStationData.Add(new Station
                            {
                                Foid = station.Foid,
                                Name = station.Name,
                                Position = station.Position,
                                Longitude = station.Longitude,
                                Latitude = station.Latitude,
                                TidalExtremes = station.TidalExtremes,
                                TidalStreams = station.TidalStreams,
                                TimeSeries = new Dictionary<int, List<TidalStream>> { { 1, station.TimeSeries[i + firstKeyValue] } }
                            });
                        }

                        Data = localStationData.ToArray();

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
                        H5_GroupId = HDF5CSharp.Hdf5.CreateOrOpenGroup(parentGroupId, groupName.Replace("_", "."));

                        // make a valid level indentifier
                        string level = GetLevel(templateNode) + "/";
                        string[] splittedLevels = level.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        if (splittedLevels.Length > 0)
                        {
                            splittedLevels[splittedLevels.Length - 1] = splittedLevels[splittedLevels.Length - 1].Replace($"{baseGroupName}_x", groupName);
                        }
                        level = String.Join("/", splittedLevels) + "/";
                        splittedLevels = new string[0];   
                        // parse template at level and insert data
                        ParseTemplateAttributes(H5_GroupId, templateNode, localAttributesDictionary, level);

                        var groupNodes = templateNode.SelectNodes("Group");
                        if (groupNodes != null && groupNodes.Count > 0)  
                        {
                            foreach (XmlNode groupNode in groupNodes)
                            {
                                if (groupNode != null)
                                {
                                    string objectTypeName = groupNode.Attributes?["name"]?.Value.ToString() ?? string.Empty;

                                    if (String.IsNullOrEmpty(objectTypeName) == false)
                                    {
                                        var groupFactory = new GroupFactory();
                                        var groupInstance = groupFactory.Create(objectTypeName);
                                        groupInstance.Data = localStationData.ToArray();
                                        groupInstance.Write(H5_GroupId, groupNode, localAttributesDictionary, level);
                                    }
                                }
                            }
                        }

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
                                        var dataSetFactory = new DataSetFactory();
                                        var dataSetInstance = dataSetFactory.Create(objectTypeName);
                                        dataSetInstance.Data = localStationData.ToArray();
                                        dataSetInstance.Write(H5_GroupId, dataSetNode, localAttributesDictionary, level);
                                    }
                                }
                            }
                        }

                        HDF5CSharp.Hdf5.CloseGroup(H5_GroupId);
                    }
                    while (i < numberOfTimeSeries && i < upperLimit);
                }
                else
                {
                    string level = GetLevel(templateNode) + "/";

                    H5_GroupId = HDF5CSharp.Hdf5.CreateOrOpenGroup(parentGroupId, baseGroupName);
                    ParseTemplateAttributes(H5_GroupId, templateNode, attributesDictionary, level);

                    var groupNodes = templateNode.SelectNodes(@"Group");
                    if (groupNodes != null && groupNodes.Count > 0)
                    {
                        foreach (XmlNode groupNode in groupNodes)
                        {
                            var surfaceCurrentGroup = new SurfaceCurrentGroup(true)
                            {
                                Data = localData
                            };
                            surfaceCurrentGroup.Write(H5_GroupId, groupNode, attributesDictionary);
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
            }
        }
    }
}
