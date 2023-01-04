using Hdf5Fase2Test.Base;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using static Hdf5Fase2Test.Base.Enumerations;

namespace Hdf5Fase2Test.Types
{
    [Serializable]
    public class Station : IXmlSerializable
    {
        public string Foid { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }    
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public TidalExtreme[] TidalExtremes { get; set; }
        public TidalStream[] TidalStreams { get; set; }
        public Dictionary<int, List<TidalStream>> TimeSeries { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Station()
        {
            Longitude = 0.0;
            Latitude = 0.0;
            Position = string.Empty;
            Name = string.Empty;
            Foid = string.Empty;
            TidalExtremes = new TidalExtreme[0];
            TidalStreams = new TidalStream[0];
            TimeSeries = new Dictionary<int, List<TidalStream>>();
        }

        /// <summary>
        ///     Analyses both the extreme- and streams data and generate timeseries from it
        /// </summary>
        /// <returns>true if succesfull</returns>
        public virtual bool Analyse()
        {
            IEnumerable<TidalExtreme> highTides = TidalExtremes.Where(t => t.TideType == Enumerations.TidalExtremeNameEnum.High);

            // make collection of moonphases to facilitate in a fast determination of a moonphase at a specific date
            var datedMoonPhases = new Dictionary<string, MoonPhaseEnum>();
            foreach(TidalExtreme highTide in highTides)
            {
                if (highTide.DaysAfterMoonPhase == 0 &&
                    datedMoonPhases.ContainsKey(highTide.TimeStamp.ToString("yyyy-MM-dd")) == false)
                {
                    datedMoonPhases.Add(highTide.TimeStamp.ToString("yyyy-MM-dd"), highTide.MoonPhase);
                }
            }

            const int MAX_DAYS = 7;
            int datasetId = 0;

            // now loop over all high tides
            foreach(TidalExtreme highTide in highTides)
            {
                var timeSeries = new List<TidalStream>();

                // and use the -6 to +6 hourly offsets to calculate the time and height. 
                foreach(TidalStream tidalStream in TidalStreams)
                {
                    var newTidalStream = new TidalStream();
                    _= newTidalStream.Copy(tidalStream);

                    // calculate timestamp
                    DateTime highTideTimeStamp = highTide.TimeStamp;
                    DateTime calculatedStreamTimeStamp = highTideTimeStamp.AddHours(tidalStream.HourlyOffset);
                    newTidalStream.TimeStamp = calculatedStreamTimeStamp;

                    // calculate height by using the moonphases. 
                    if (datedMoonPhases.ContainsKey(newTidalStream.TimeStamp.ToString("yyyy-MM-dd")))
                    {
                        // now if it's New Moon or Full Moon
                        if (datedMoonPhases[newTidalStream.TimeStamp.ToString("yyyy-MM-dd")] == Enumerations.MoonPhaseEnum.NM ||
                            datedMoonPhases[newTidalStream.TimeStamp.ToString("yyyy-MM-dd")] == Enumerations.MoonPhaseEnum.FM)
                        {
                            newTidalStream.Speed = newTidalStream.SpringValue;
                        }
                        else
                        {
                            newTidalStream.Speed = newTidalStream.NeapValue;
                        }
                    }
                    else
                    {
                        // if it's somewhere in between, a partial value between neap and spring is calculated 
                        // based on the nr of days it's been since the last NM/FM

                        double totalDifference = newTidalStream.SpringValue - newTidalStream.NeapValue;
                        int days = highTide.DaysAfterMoonPhase > MAX_DAYS ? MAX_DAYS : highTide.DaysAfterMoonPhase;
                        if (highTide.DaysAfterMoonPhase == 0 &&
                            newTidalStream.TimeStamp.Day != highTide.TimeStamp.Day)
                        {
                            days++;
                        }

                        double partialDifference = totalDifference * ((double)days / (double)MAX_DAYS);

                        // we're now moving away from New Moon or Full Moon
                        if (highTide.MoonPhase == Enumerations.MoonPhaseEnum.NM ||
                            highTide.MoonPhase == Enumerations.MoonPhaseEnum.FM)
                        {
                            // we're moving away from NM/FM, so we're moving away from spring value
                            newTidalStream.Speed = Math.Round(newTidalStream.SpringValue - partialDifference, 2);
                        }
                        else
                        {
                            // we're moving away from FQ/LQ, so we're moving towards spring value
                            newTidalStream.Speed = Math.Round(newTidalStream.NeapValue + partialDifference, 2);
                        }
                    }

                    // add to the timeseries
                    timeSeries.Add(newTidalStream);
                }

                TimeSeries.Add(datasetId++, timeSeries);
            }

            return true;
        }

        public XmlSchema? GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Writes object to XmlWriter
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Station");

            writer.WriteStartElement("Foid");
            writer.WriteString(Foid.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Name");
            writer.WriteString(Name.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("Position");
            writer.WriteString(Position.ToString());
            writer.WriteEndElement();

            //if (TidalExtremes.Length > 0)
            //{
            //    writer.WriteStartElement("TidalExtremes");
            //    foreach (TidalExtreme tidalExtreme in TidalExtremes)
            //    {
            //        tidalExtreme.WriteXml(writer);
            //    }
            //    writer.WriteEndElement();
            //}

            //if (TidalStreams.Length > 0)
            //{
            //    writer.WriteStartElement("TidalStreams");
            //    foreach (TidalStream tidalStream in TidalStreams)
            //    {
            //        tidalStream.WriteXml(writer);
            //    }
            //    writer.WriteEndElement();
            //}

            if (TimeSeries.Count > 0)
            {
                foreach (KeyValuePair<int, List<TidalStream>> timeSeries in TimeSeries)
                {
                    writer.WriteStartElement("TimeSeries");
                    writer.WriteAttributeString("Id", timeSeries.Key.ToString());

                    writer.WriteStartElement("TidalStreams");
                    foreach (TidalStream tidalStream in timeSeries.Value)
                    {
                        tidalStream.WriteXml(writer);
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();
        }
    }
}
