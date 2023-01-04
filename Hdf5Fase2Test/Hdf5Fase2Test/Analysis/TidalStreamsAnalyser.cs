using Hdf5Fase2Test.Types;
using System.Xml;

namespace Hdf5Fase2Test.Analysis
{
    public class TidalStreamsAnalyser 
    {
        /// <summary>
        ///     Parses XML data into TidalStreams objects
        /// </summary>
        /// <param name="station"></param>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual TidalEntity[] Parse(Station station, XmlDocument xmlDoc)
        {
            if (station is null)
            {
                throw new ArgumentNullException(nameof(station));
            }

            if (xmlDoc is null)
            {
                throw new ArgumentNullException(nameof(xmlDoc));
            }

            var tidalStreams = new List<TidalStream>(); 

            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("nlho", "http://www.hydro.nl/gml");
            nsManager.AddNamespace("gml", "http://www.opengis.net/gml");
            var allPositionNodes = xmlDoc.SelectNodes($@"//*/GEOM/gml:Point/gml:pos[.='{station.Position}']", nsManager);
            if (allPositionNodes != null && allPositionNodes.Count > 0)
            {
                foreach(XmlNode positionNode in allPositionNodes)
                {
                    XmlNode? targetNode = positionNode?.ParentNode?.ParentNode?.ParentNode;
                    if (targetNode != null)
                    {
                        var tidalStream = new TidalStream();
                        tidalStream.Parse(targetNode);
                        tidalStreams.Add(tidalStream);
                    }
                }

                return tidalStreams.ToArray();
            }

            return new TidalExtreme[0];
        }
    }
}
