using System.Globalization;
using System.Xml;
using System.Xml.Schema;

namespace Hdf5Fase2Test.Types
{
    [Serializable]
    public class TidalStream : TidalEntity
    {
        public double Direction { get; set; }
        public double Speed { get; set; }
        public int HourlyOffset { get; set; }
        public double NeapValue { get; set; }
        public double SpringValue { get; set; }

        /// <summary>
        ///     Copies all data from the given tidalstreams object to this object
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public TidalStream Copy(TidalStream stream)
        {
            NeapValue = stream.NeapValue;
            SpringValue = stream.SpringValue;
            Direction = stream.Direction;
            Speed = stream.Speed;
            HourlyOffset = stream.HourlyOffset;

            return this;
        }

        public override XmlSchema? GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void Parse(XmlNode node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            Direction = -1.0; // is recalculated

            var neapNode = node.SelectSingleNode(@"NOTTXT");
            if (neapNode != null && neapNode.HasChildNodes)
            {
                string neapString = neapNode.FirstChild?.InnerText.ToString() ?? "";

                NeapValue = -1.0;
                if (double.TryParse(neapString, new CultureInfo("en-US"), out double neapValue))
                {
                    NeapValue = neapValue;
                }
            }

            var springNode = node.SelectSingleNode(@"CURVEL");
            if (springNode != null && springNode.HasChildNodes)
            {
                string springString = springNode.FirstChild?.InnerText.ToString() ?? "";

                SpringValue = -1.0;
                if (double.TryParse(springString, new CultureInfo("en-US"), out double springValue))
                {
                    SpringValue = springValue;
                }
            }

            var directionNode = node.SelectSingleNode(@"ORIENT");
            if (directionNode != null && directionNode.HasChildNodes)
            {
                string directionString = directionNode.FirstChild?.InnerText.ToString() ?? "";

                Direction = -1.0;
                if (double.TryParse(directionString, out double directionValue))
                {
                    Direction = directionValue;
                }
            }

            var hourlyOffsetNode = node.SelectSingleNode(@"INFORM");
            if (hourlyOffsetNode != null && hourlyOffsetNode.HasChildNodes)
            {
                string hourlyOffsetString = hourlyOffsetNode.FirstChild?.InnerText.ToString() ?? "";

                HourlyOffset = -1;
                if (int.TryParse(hourlyOffsetString, out int hourlyOffsetValue))
                {
                    HourlyOffset = hourlyOffsetValue;
                }
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Writes object to XmlWriter
        /// </summary>
        /// <param name="writer"></param>
        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("TidalStream");

            writer.WriteStartElement("Direction");
            writer.WriteString(Direction.ToString(new CultureInfo("en-US")));
            writer.WriteEndElement();

            writer.WriteStartElement("Speed");
            writer.WriteString(Speed.ToString(new CultureInfo("en-US")));
            writer.WriteEndElement();

            writer.WriteStartElement("HourlyOffset");
            writer.WriteString(HourlyOffset.ToString(new CultureInfo("en-US")));
            writer.WriteEndElement();

            writer.WriteStartElement("NeapValue");
            writer.WriteString(NeapValue.ToString(new CultureInfo("en-US")));
            writer.WriteEndElement();

            writer.WriteStartElement("SpringValue");
            writer.WriteString(SpringValue.ToString(new CultureInfo("en-US")));
            writer.WriteEndElement();

            writer.WriteStartElement("TimeStamp");
            writer.WriteString(TimeStamp.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
}
