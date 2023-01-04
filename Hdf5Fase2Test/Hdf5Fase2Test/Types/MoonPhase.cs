using static Hdf5Fase2Test.Base.Enumerations;

namespace Hdf5Fase2Test.Types
{
    public class MoonPhase
    {
        private MoonPhaseEnum _moonPhase;

        public MoonPhase() { }

        public MoonPhase(MoonPhaseEnum moonPhase)
        {
            _moonPhase = moonPhase;
        }

        public MoonPhase(string moonPhase)
        {
            Enum.TryParse(moonPhase, out _moonPhase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MoonPhaseEnum GetFirstMoonPhase()
        {
            var moonphases = GetMoonPhases();
            return moonphases.First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MoonPhaseEnum GetLastMoonPhase()
        {
            var moonphases = GetMoonPhases();
            return moonphases.Last();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<MoonPhaseEnum> GetMoonPhases()
        {
            var moonphases = Enum.GetValues<MoonPhaseEnum>().ToList();
            return moonphases.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phase"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public MoonPhaseEnum GetPreviousMoonPhase()
        {
            var moonphases = GetMoonPhases();
            var currentIndex = moonphases.FindIndex(mf => mf.ToString() == _moonPhase.ToString());
            if (currentIndex > 1)
            {
                return moonphases[currentIndex - 1];
            }

            return moonphases.Last();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phase"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public MoonPhaseEnum GetNextMoonPhase()
        {
            var moonphases = GetMoonPhases();
            var currentIndex = moonphases.FindIndex(mf => mf.ToString() == _moonPhase.ToString());
            if (currentIndex < moonphases.Count() - 1)
            {
                return moonphases[currentIndex + 1];
            }

            return moonphases.First();
        }
    }
}
