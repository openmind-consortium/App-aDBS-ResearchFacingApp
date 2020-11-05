using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedAdaptiveDBSApplication.Models
{
    /// <summary>
    /// Therapy limits and electode stim contacts for a Group
    /// </summary>
    public class TherapyLimitsForGroupModel
    {
        /// <summary>
        ///Lower limit for pulse width
        /// </summary>
        public int PulseWidthLowerLimit { get; set; }
        /// <summary>
        /// Upper limit for pulse width
        /// </summary>
        public int PulseWidthUpperLimit { get; set; }
        /// <summary>
        /// Lower limit for stim rate
        /// </summary>
        public double StimRateLowerLimit { get; set; }
        /// <summary>
        /// Upper limit for stim rate
        /// </summary>
        public double StimRateUpperLimit { get; set; }
        /// <summary>
        /// Lower limit for Stim amp program 0
        /// </summary>
        public double StimAmpLowerLimitProg0 { get; set; }
        /// <summary>
        /// Upper limit for Stim amp program 0
        /// </summary>
        public double StimAmpUpperLimitProg0 { get; set; }
        /// <summary>
        /// Stim Contact for program 0
        /// </summary>
        public string StimElectrodesProg0 { get; set; }
        /// <summary>
        /// Lower limit for Stim amp program 1
        /// </summary>
        public double StimAmpLowerLimitProg1 { get; set; }
        /// <summary>
        /// Upper limit for Stim amp program 1
        /// </summary>
        public double StimAmpUpperLimitProg1 { get; set; }
        /// <summary>
        /// Stim Contact for program 1
        /// </summary>
        public string StimElectrodesProg1 { get; set; }
        /// <summary>
        /// Lower limit for Stim amp program 2
        /// </summary>
        public double StimAmpLowerLimitProg2 { get; set; }
        /// <summary>
        /// Upper limit for Stim amp program 1
        /// </summary>
        public double StimAmpUpperLimitProg2 { get; set; }
        /// <summary>
        /// Stim Contact for program 2
        /// </summary>
        public string StimElectrodesProg2 { get; set; }
        /// <summary>
        /// Lower limit for Stim amp program 3
        /// </summary>
        public double StimAmpLowerLimitProg3 { get; set; }
        /// <summary>
        /// Upper limit for Stim amp program 3
        /// </summary>
        public double StimAmpUpperLimitProg3 { get; set; }
        /// <summary>
        /// Stim Contact for program 3
        /// </summary>
        public string StimElectrodesProg3 { get; set; }
        /// <summary>
        /// ActiveRechargeOn if on or ActiveRechargeOff for off
        /// </summary>
        public string ActiveRechargeStatus { get; set; }
    }
}
