/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedAdaptiveDBSApplication.Models
{
    /// <summary>
    /// Model for getting config file from Json
    /// </summary>
    public class StimSweepModel
    {
        /// <summary>
        /// Amp
        /// </summary>
        public List<double> AmpInmA { get; set; }
        /// <summary>
        /// Rate
        /// </summary>
        public List<double> RateInHz { get; set; }
        /// <summary>
        /// Pulse width
        /// </summary>
        public List<int> PulseWidthInMicroSeconds { get; set; }
        /// <summary>
        /// Time to run each stim sweep in seconds
        /// </summary>
        public List<double> TimeToRunInSeconds { get; set; }
        /// <summary>
        /// Group in A, B, C or D
        /// </summary>
        public List<string> GroupABCD { get; set; }
        /// <summary>
        /// Amount of time in seconds to put the event start and event stop event marker
        /// </summary>
        public double EventMarkerDelayTimeInSeconds { get; set; }
    }
    
}
