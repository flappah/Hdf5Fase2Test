using Hdf5Fase2Test.Data;
using Hdf5Fase2Test.Types;
using System.Globalization;
using System.Xml;
using static System.Collections.Specialized.BitVector32;

namespace Hdf5Fase2Test.Analysis
{
    public class StationBuilder
    {
        private XmlDocument? _extremesXmlData = null;
        private XmlDocument? _streamsXmlData = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extremesXmlName"></param>
        /// <param name="streamXmlName"></param>
        public virtual void Initialize(string extremesXmlName, string streamXmlName)
        {
            var reader = new XmlFileReader();

            _extremesXmlData = reader.Read(extremesXmlName);
            _streamsXmlData = reader.Read(streamXmlName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stations"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Station[] Analyse(Station[] stations)
        {
            foreach(Station station in stations)
            {
                station.Analyse();
            }

            return stations;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stations"></param>
        /// <returns></returns>
        public virtual (double minLon, double minLat, double maxLon, double maxLat) GetBoundingBox(Station[] stations)
        {
            double minLat = 90.0;
            double maxLat = -90.0;
            double minLon = 180.0;
            double maxLon = -180.0;

            foreach(Station station in stations)
            {
                string positionString = station.Position;
                if (String.IsNullOrEmpty(positionString) == false)
                {
                    string[] positionItems = positionString.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (positionItems.Length == 2)
                    {
                        if (double.TryParse(positionItems[0], new CultureInfo("en-US"), out double lon))
                        {
                            if (double.TryParse(positionItems[1], new CultureInfo("en-US"), out double lat))
                            {
                                if (lat < minLat)
                                {
                                    minLat = lat;
                                }

                                if (lat > maxLat)
                                {
                                    maxLat = lat;
                                }

                                if (lon < minLon)
                                {
                                    minLon = lon;
                                }

                                if (lon > maxLon)
                                {
                                    maxLon = lon;
                                }
                            }
                        }
                    }
                }
            }

            return (minLon, minLat, maxLon, maxLat);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        private List<Station> GetStations(XmlNodeList? nodeList)
        {
            if (nodeList == null)
            {
                throw new ArgumentNullException(nameof(nodeList));
            }

            if (_streamsXmlData == null)
            {
                throw new ArgumentNullException(nameof(_streamsXmlData));
            }

            var stations = new List<Station>();

            var nsManager = new XmlNamespaceManager(_streamsXmlData.NameTable);
            nsManager.AddNamespace("nlho", "http://www.hydro.nl/gml");
            nsManager.AddNamespace("gml", "http://www.opengis.net/gml");

            foreach (XmlNode node in nodeList)
            {
                if (node != null && node.Attributes != null)
                {
                    string foid = node.Attributes["foid"]?.Value.ToString() ?? string.Empty;
                    if (foid != null && stations.Exists(s => s.Foid == foid) == false)
                    {
                        var newStation = new Station
                        {
                            Foid = foid
                        };

                        var positionNode = node.SelectSingleNode("GEOM/gml:Point/gml:pos", nsManager);
                        if (positionNode != null && positionNode.HasChildNodes)
                        {
                            newStation.Position = positionNode.FirstChild?.InnerText.ToString() ?? "";
                        }

                        string[] positionItems = newStation.Position.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (double.TryParse(positionItems[0] ?? "", new CultureInfo("en-US"), out double longitude))
                        {
                            newStation.Longitude = longitude;
                        }

                        if (double.TryParse(positionItems[1] ?? "", new CultureInfo("en-US"), out double latitude))
                        {
                            newStation.Latitude = latitude;
                        }

                        newStation.Name = "VS-" + newStation.Position.Replace(" ", "-");

                        stations.Add(newStation);
                    }
                }
            }

            return stations;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Station[] ParseXmlData()
        {
            if (_extremesXmlData != null && _streamsXmlData != null)
            {
                /*
                 * Based on the MIN6 values extract all unique positions and use them as being the main identifier for the stations collection
                 */

                var nsManager = new XmlNamespaceManager(_streamsXmlData.NameTable);
                nsManager.AddNamespace("nlho", "http://www.hydro.nl/gml");
                nsManager.AddNamespace("gml", "http://www.opengis.net/gml");
                XmlNodeList? min6Values = _streamsXmlData.SelectNodes(@"//gml:featureMember/nlho:Min6", nsManager);

                var stations = new List<Station>();
                if (min6Values != null)
                {
                    stations = GetStations(min6Values);
                }

                var extremesAnalyser = new TidalExtremesAnalyser();
                TidalEntity[] tidalExtremes = extremesAnalyser.Parse(_extremesXmlData);

                foreach (Station station in stations)
                {
                    var streamsAnalyser = new TidalStreamsAnalyser();
                    TidalEntity[] tidalStreams = streamsAnalyser.Parse(station, _streamsXmlData);

                    station.TidalStreams = Array.ConvertAll<TidalEntity, TidalStream>(tidalStreams, t => t as TidalStream);
                    station.TidalExtremes = Array.ConvertAll<TidalEntity, TidalExtreme>(tidalExtremes, t => t as TidalExtreme);
                }

                return stations.ToArray();
            }

            return new Station[0];
        }
    }
}
