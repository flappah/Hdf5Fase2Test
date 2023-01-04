using Hdf5Fase2Test.Analysis;
using Hdf5Fase2Test.Products;
using Hdf5Fase2Test.Types;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Hdf5Fase2Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //    if (args.Length != 4)
            //    {
            //        Console.WriteLine("Insufficient arguments!");
            //    }
            //    else
            //    {

            Console.WriteLine("Generating stations data ..");

            var stationGenerator = new StationBuilder();
            stationGenerator.Initialize(@"F:\HP33\IJmuiden-buitenhaven_2022_0_INT.xml", @"F:\TidalStreamsData\tidalstreams_532_ijmuiden.xml");
            Station[] stations = stationGenerator.ParseXmlData();
            stationGenerator.Analyse(stations);

            Console.WriteLine($"Analysed and generated {stations.Length} station{(stations.Length == 1 ? "": "s")} ..");

            (double minLon, double minLat, double maxLon, double maxLat) boundingbox = stationGenerator.GetBoundingBox(stations);

            Console.WriteLine($"Determined boundingbox ({boundingbox.minLon}, {boundingbox.minLat}, {boundingbox.maxLon}, {boundingbox.maxLat}) ..");

            //var sb = new StringBuilder();
            //var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings { Encoding = Encoding.UTF8 });

            //xmlWriter.WriteStartElement("Stations");
            //foreach (var station in stations)
            //{
            //    station.WriteXml(xmlWriter);
            //}
            //xmlWriter.WriteEndElement();
            //xmlWriter.Flush();

            //Console.WriteLine(sb.ToString());

            Console.WriteLine("Generating HDF5 files for station data ..");

            string userDecidedFileName = "ijmuiden_test";

            Dictionary<string, string> attributeDictionary = new Dictionary<string, string>
            {
                { "/productSpecification", "INT.IHO.S-111.1.0" },
                { "/issueTime", DateTime.Now.ToString() },
                { "/issueDate", DateTime.Now.ToString() },
                { "/horizontalCRS", "4326" },
                { "/westBoundLongitude", boundingbox.minLon.ToString(new CultureInfo("en-US")) },
                { "/eastBoundLongitude", boundingbox.maxLon.ToString(new CultureInfo("en-US")) },
                { "/southBoundLatitude", boundingbox.minLat.ToString(new CultureInfo("en-US")) },
                { "/northBoundLatitude", boundingbox.maxLat.ToString(new CultureInfo("en-US")) },
                { "/geographicIdentifier", "IJmuiden" },
                { "/metaData", $"MD_111NL_{userDecidedFileName}_{(DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"))}.XML" },
                { "/depthTypeIndex", "2" },
                { "/surfaceCurrentDepth", "" },
                { "/verticalCoordinateBase", "3" },
                { "/verticalCS", "6499" },
                { "/Group_F/SurfaceCurrent/chunking", "0,0" },
                { "/SurfaceCurrent/commonPointRule", "3" },
                { "/SurfaceCurrent/dataCodingFormat", "8" },
                { "/SurfaceCurrent/dimension", "2" },
                { "/SurfaceCurrent/horizontalPositionUncertainty", "-1.0" },
                { "/SurfaceCurrent/methodCurrentsProduct", "Brief description of method" },
                { "/SurfaceCurrent/timeUncertainty", "-1.0" },
                { "/SurfaceCurrent/typeOfCurrentData", "3" },
                { "/SurfaceCurrent/verticalUncertainty", "-1.0" }
                //{ "/SurfaceCurrent/SurfaceCurrent_x/instanceChunking", "0,0" },
                //{ "/SurfaceCurrent/SurfaceCurrent_x/timeRecordInterval", "43200" },
                //{ "/SurfaceCurrent/SurfaceCurrent_x/Group_x/timeIntervalIndex", "1" },
                //{ "/SurfaceCurrent/SurfaceCurrent_x/Group_x/timeRecordInterval", "3600" }
            };

            var h5S111Product = new S111Product();

            h5S111Product.Data = stations;
            h5S111Product.FolderName = Path.GetTempPath();
            h5S111Product.FileName = $@"111NL_{userDecidedFileName}_[Sequence].h5";
            h5S111Product.Progress += delegate { Console.Write("#"); };
            h5S111Product.CreateProductFromTemplate(attributeDictionary);

            Console.WriteLine("\nReady.");
        }    
    }
}