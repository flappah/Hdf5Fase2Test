using Hdf5Fase2Test.Base;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using static Hdf5Fase2Test.Base.Enumerations;

namespace Hdf5Fase2Test.Types
{
    [Serializable]
    public class TidalExtreme : TidalEntity
    {
        public int DaysAfterMoonPhase { get; set; }
        public double Height { get; set; }
        public MoonPhaseEnum MoonPhase { get; set; }
        public TidalExtremeNameEnum TideType { get; set; }

        /// <summary>
        ///     Recalculates moonphase. All MoonPhase.Empty values are recalculated based on the days 
        ///     since the latest valid moonphase (NM, FQ, FM, LQ). 
        /// </summary>
        /// <param name="dayNumber"></param>
        /// <param name="previousDate"></param>
        /// <param name="lastValidMoonPhase"></param>
        /// <returns></returns>
        public virtual TidalExtreme RecalcMoonPhase(ref short dayNumber, ref string previousDate, ref MoonPhaseEnum lastValidMoonPhase)
        {
            if (previousDate != String.Empty &&
                previousDate != this.TimeStamp.ToString("yyyyMMdd"))
            {
                dayNumber++;
                previousDate = this.TimeStamp.ToString("yyyyMMdd");
            }

            if (this.MoonPhase == MoonPhaseEnum.Empty)
            {
                if (lastValidMoonPhase != MoonPhaseEnum.Empty)
                {
                    this.MoonPhase = lastValidMoonPhase;
                    this.DaysAfterMoonPhase = dayNumber;
                }
            }
            else
            {
                lastValidMoonPhase = this.MoonPhase;
                dayNumber = 0;
                previousDate = this.TimeStamp.ToString("yyyyMMdd");
            }

            return this;
        }

        /// <summary>
        ///     Parses XML data into this TidalExtreme object
        /// </summary>
        /// <param name="node"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void Parse(XmlNode node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            XmlNode? tidalInfoDateNode = node.ParentNode?.ParentNode?.SelectSingleNode("nummer");
            string tidalInfoDateString = "";
            if (tidalInfoDateNode != null && tidalInfoDateNode.HasChildNodes)
            {
                tidalInfoDateString = tidalInfoDateNode.InnerText ?? string.Empty;
            }

            //string timeZone = "";
            //XmlNode? tidalTimeZoneNode = node.ParentNode?.ParentNode?.ParentNode?.ParentNode?.SelectSingleNode("timezone/local");
            //if (tidalTimeZoneNode != null && tidalTimeZoneNode.HasChildNodes)
            //{
            //    timeZone = tidalTimeZoneNode.InnerText ?? string.Empty;
            //}

            XmlNode? tidalInfoTimeNode = node.SelectSingleNode("time");
            if (tidalInfoTimeNode != null && tidalInfoTimeNode.HasChildNodes)
            {
                //string dst = "";
                //if (tidalInfoTimeNode.Attributes != null && tidalInfoTimeNode.Attributes.Count > 0)
                //{
                //    dst = tidalInfoTimeNode.Attributes["dst"]?.Value?.ToString() ?? String.Empty;
                //    if (dst.ToUpper().Equals("YES"))
                //    {
                //        timeZone = "MEST";
                //    }
                //}                

                var tidalInfoTimeString = tidalInfoTimeNode.InnerText ?? string.Empty;
                if (DateTime.TryParse(tidalInfoDateString + " " + tidalInfoTimeString, out DateTime tidalInfoDateTime))
                {
                    TimeZoneInfo localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    TimeStamp = TimeZoneInfo.ConvertTimeToUtc(tidalInfoDateTime, localTimeZone);
                }
            }

            XmlNode? tidalInfoHeightNode = node.SelectSingleNode("height");
            if (tidalInfoHeightNode != null && tidalInfoHeightNode.HasChildNodes)
            {
                var tidalInfoHeightString = tidalInfoHeightNode.InnerText ?? string.Empty;  
                if (double.TryParse(tidalInfoHeightString, out double tidalInfoHeightValue))
                {
                    Height = tidalInfoHeightValue / 10.0;
                }
            }

            XmlNode? tidalInfoTypeNode = node.SelectSingleNode("tide");
            if (tidalInfoTypeNode != null && tidalInfoTypeNode.HasChildNodes)
            { 
                var tidalInfoTypeString = tidalInfoTypeNode.InnerText ?? string.Empty;
                if (tidalInfoTypeString.ToUpper().Equals("HIGH"))
                {
                    TideType = TidalExtremeNameEnum.High;
                }
                else
                {
                    TideType = TidalExtremeNameEnum.Low;
                }
            }

            XmlNode? tidalInfoMoonPhaseNode = node.ParentNode?.ParentNode?.SelectSingleNode(@"moon");
            if (tidalInfoMoonPhaseNode != null && tidalInfoMoonPhaseNode.HasChildNodes)
            {
                var tidalInfoMoonPhaseString = tidalInfoMoonPhaseNode.InnerText ?? string.Empty;
                if (Enum.TryParse<MoonPhaseEnum>(tidalInfoMoonPhaseString, out MoonPhaseEnum moonPhase))
                {
                    MoonPhase = moonPhase;
                    DaysAfterMoonPhase = 0;
                }
            }
        }

        public override XmlSchema? GetSchema()
        {
            throw new NotImplementedException();
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
            writer.WriteStartElement("TidalExtreme");

            writer.WriteStartElement("DaysAfterMoonPhase");
            writer.WriteString(DaysAfterMoonPhase.ToString(new CultureInfo("en-US")));
            writer.WriteEndElement();

            writer.WriteStartElement("Height");
            writer.WriteString(Height.ToString(new CultureInfo("en-US")));
            writer.WriteEndElement();

            writer.WriteStartElement("Height");
            writer.WriteString(Height.ToString(new CultureInfo("en-US")));
            writer.WriteEndElement();

            writer.WriteStartElement("MoonPhase");
            writer.WriteString(MoonPhase.ToString().LastPart("."));
            writer.WriteEndElement();

            writer.WriteStartElement("TideType");
            writer.WriteString(TideType.ToString().LastPart("."));
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
}
