using Hdf5Fase2Test.Base;
using Hdf5Fase2Test.Types;
using System.Xml;
using static Hdf5Fase2Test.Base.Enumerations;

namespace Hdf5Fase2Test.Analysis
{
    public class TidalExtremesAnalyser
    {     
        /// <summary>
        ///     Parses XML data into TidalExtremes objects
        /// </summary>
        /// <param name="station"></param>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual TidalEntity[] Parse(XmlDocument xmlDoc)
        {
            if (xmlDoc is null)
            {
                throw new ArgumentNullException(nameof(xmlDoc));
            }

            var tidalExtremes = new List<TidalExtreme>();

            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("nlho", "http://www.hydro.nl/gml");
            nsManager.AddNamespace("gml", "http://www.opengis.net/gml");

            var tidalExtremeNodes = xmlDoc.SelectNodes("//days/day/extrema/extreme", nsManager);
            if (tidalExtremeNodes != null && tidalExtremeNodes.Count > 0)
            {
                MoonPhaseEnum lastValidMoonPhase = MoonPhaseEnum.Empty;
                short dayNumber = 0;
                string previousDate = string.Empty;
                foreach (XmlNode extremeNode in tidalExtremeNodes)
                {
                    var newExtreme = new TidalExtreme();
                    newExtreme.Parse(extremeNode);
                    newExtreme = newExtreme.RecalcMoonPhase(ref dayNumber, ref previousDate, ref lastValidMoonPhase);

                    tidalExtremes.Add(newExtreme);
                }

                return RecalLeadingMoonphases(tidalExtremes).ToArray();
            }

            return tidalExtremes.ToArray();
        }

        /// <summary>
        ///     The first element rarely has a moonphase not equal to empty. To make sure all those
        ///     empty moonphases are valid a recalculation is needed. It finds out what the first valid
        ///     moonphase is and then based on the previous moonphase and the number of days that moonphase
        ///     should have happened (usually it is max 7 days before) the valid moonphase is rendered.
        /// </summary>
        /// <param name="tidalExtremes"></param>
        /// <returns></returns>
        private List<TidalExtreme> RecalLeadingMoonphases(List<TidalExtreme> tidalExtremes)
        {
            TidalExtreme? firstExtremeWithValidMoonPhase =
                tidalExtremes.Find(te => te.MoonPhase != MoonPhaseEnum.Empty);

            if (firstExtremeWithValidMoonPhase != null)
            {
                MoonPhaseEnum firstMoonPhase = firstExtremeWithValidMoonPhase.MoonPhase;
                DateTime datetimeOfFirstMoonPhase = firstExtremeWithValidMoonPhase.TimeStamp;

                var moonPhase = new MoonPhase(firstMoonPhase);
                MoonPhaseEnum previousMoonPhase = moonPhase.GetPreviousMoonPhase();

                int i = 0;
                do
                {
                    int dayCountBeforeFirstMoonPhase;
                    if ((datetimeOfFirstMoonPhase - tidalExtremes[i].TimeStamp).Days == 0 && (datetimeOfFirstMoonPhase - tidalExtremes[i].TimeStamp).Hours > 0)
                    {
                        dayCountBeforeFirstMoonPhase = 1;
                    }
                    else
                    {
                        dayCountBeforeFirstMoonPhase = (datetimeOfFirstMoonPhase - tidalExtremes[i].TimeStamp).Days;
                    }

                    if (tidalExtremes[i].MoonPhase == MoonPhaseEnum.Empty)
                    {
                        tidalExtremes[i].DaysAfterMoonPhase = (dayCountBeforeFirstMoonPhase < 7 ? 7 - dayCountBeforeFirstMoonPhase : 8 - dayCountBeforeFirstMoonPhase);
                        tidalExtremes[i].MoonPhase = previousMoonPhase;
                    }
                }
                while (++i < tidalExtremes.Count() && tidalExtremes[i].MoonPhase != firstMoonPhase);
            }

            return tidalExtremes;
        }
    }
}
