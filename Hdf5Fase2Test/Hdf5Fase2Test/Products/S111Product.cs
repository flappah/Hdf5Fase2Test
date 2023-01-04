using HDF.PInvoke;
using Hdf5Fase2Test.Base;
using Hdf5Fase2Test.Groups;
using Hdf5Fase2Test.Hdf5;
using Hdf5Fase2Test.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hdf5Fase2Test.Products
{
    public class S111Product : ProductBase
    {
        public string TemplateName => @"H5_Templates\H5_S111_Template.xml";
        public string FileName { get; set; }
        public string FolderName { get; set; }
        public List<H5_ItemBase> Root { get; set; }
        public new Station[] Data { get; set; }
        public long H5_FileId { get; set; }

        public delegate void ProgressFunction();
        public event ProgressFunction? Progress;

        /// <summary>
        /// 
        /// </summary>
        public S111Product()
        {
            FileName = string.Empty;
            FolderName = string.Empty;
            Data = new Station[0];
            Root = new List<H5_ItemBase>();
            ValidFeatureTypes = new[] { "SurfaceCurrent" };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributesDictionary"></param>
        /// <returns></returns>
        public virtual S111Product CreateProductFromTemplate(Dictionary<string, string> attributesDictionary)
        {
            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));
            }

            if (ValidFeatureTypes == null)
            {
                throw new ArgumentNullException(nameof(ValidFeatureTypes));
            }

            if (attributesDictionary is null)
            {
                throw new ArgumentNullException(nameof(attributesDictionary));
            }

            if (String.IsNullOrEmpty(FileName))
            {
                return new S111Product();
            }

            int timeSeriesYear = DateTime.Now.Year;
            if (((Station[]) Data)?.Count() > 0 &&
                ((Station[])Data).First().TimeSeries?.Count() > 0 &&
                ((Station[])Data).First().TimeSeries.First().Value.Count() > 0)
            {
                timeSeriesYear = ((Station[])Data).First().TimeSeries.First().Value.Last().TimeStamp.Year;
            }
            else if (((Station[])Data)?.Count() > 0 &&
                ((Station[])Data).First().TidalExtremes?.Count() > 0)
            {
                timeSeriesYear = ((Station[])Data).First().TidalExtremes.Last().TimeStamp.Year;
            }

            for (int i = 1; i < 13; i++)
            {
                if (Progress != null)
                {
                    Progress();
                }

                var localStations = new List<Station>();

                foreach (Station station in Data)
                {
                    var monthlyTimeSeries = new Dictionary<int, List<TidalStream>>();

                    foreach (KeyValuePair<int, List<TidalStream>> timeSerie in station.TimeSeries)
                    {
                        List<TidalStream> monthlyValues = timeSerie.Value.Where(v => v.TimeStamp.Month == i && v.TimeStamp.Year == timeSeriesYear).ToList();

                        if (monthlyValues.Count() > 0)
                        {
                            monthlyTimeSeries.Add(timeSerie.Key, monthlyValues);
                        }
                    }

                    localStations.Add(new Station()
                    {
                        Foid = station.Foid,
                        Name = station.Name,
                        Longitude = station.Longitude,
                        Latitude = station.Latitude,
                        Position = station.Position,
                        TidalExtremes = station.TidalExtremes,
                        TidalStreams = station.TidalStreams,
                        TimeSeries = monthlyTimeSeries
                    });
                }

                string localFileName = FileName.Replace("[Sequence]", $"{i.ShortMonthName()}-{timeSeriesYear.ToString()}");
                H5_FileId = HDF5CSharp.Hdf5.CreateFile($"{(String.IsNullOrEmpty(FolderName) ? "" : FolderName + @"\")}{localFileName}");

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(TemplateName);

                var s111ProductNode = xmlDoc.SelectSingleNode("S111Product");
                if (s111ProductNode != null && s111ProductNode.HasChildNodes)
                {
                    ParseTemplateAttributes(H5_FileId, s111ProductNode, attributesDictionary, "/");

                    var groupNodes = s111ProductNode.SelectNodes(@"Group");
                    if (groupNodes != null && groupNodes.Count > 0)
                    {
                        foreach (XmlNode groupNode in groupNodes)
                        {
                            var isFeature = GetTemplateAttributeValue(groupNode, "feature");
                            if (isFeature.ToUpper().Equals("TRUE"))
                            {
                                // Group contains a feature. Determine featuretype. Group name contains featuretype
                                string featureType = GetTemplateAttributeValue(groupNode, "name");

                                if (String.IsNullOrEmpty(featureType) == false &&
                                    ValidFeatureTypes.Contains(featureType))
                                {
                                    var factory = new GroupFactory();
                                    var surfaceCurrentFeature = factory.Create(featureType) as SurfaceCurrentGroup;

                                    if (surfaceCurrentFeature != null)
                                    {
                                        // now set data container and start HDF5 write
                                        surfaceCurrentFeature.Data = localStations.ToArray();
                                        var localAttributesDictionary = surfaceCurrentFeature.CreateAttributeValues(groupNode, attributesDictionary);
                                        surfaceCurrentFeature.Write(H5_FileId, groupNode, localAttributesDictionary);
                                    }
                                }
                            }
                            else
                            {
                                var h5GroupWriter = new H5_GroupWriter();
                                h5GroupWriter.Write(H5_FileId, groupNode, attributesDictionary);
                            }
                        }
                    }

                    var dataSetNodes = s111ProductNode.SelectNodes(@"DataSet");
                    if (dataSetNodes != null && dataSetNodes.Count > 0)
                    {
                        foreach (XmlNode dataSetNode in dataSetNodes)
                        {
                            var h5DataSetWriter = new H5_DataSetWriter();
                            h5DataSetWriter.Write(H5_FileId, dataSetNode, attributesDictionary);
                        }
                    }
                }

                HDF5CSharp.Hdf5.CloseFile(H5_FileId);
            }

            return this;
        }       
    }
}
