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
    /// Model for Stim Parameters Object
    /// Easier to pass around object than to pass multiple values
    /// </summary>
    class StimParameterModel
    {
        /// <summary>
        /// Constructor setting all values to 0 and stim electrodes to empty
        /// </summary>
        public StimParameterModel()
        {
            PulseWidth = -1;
            StimRate = -1;
            StimAmp = -1;
        }
        /// <summary>
        /// Constructor to set values for stimulation data for pulse width, stim rate and stim amp
        /// </summary>
        /// <param name="pulsewidth">Sets the pulseWidth value</param>
        /// <param name="stimrate">Sets the stimRate value</param>
        /// <param name="stimamp">Sets the stimAmp value</param>
        public StimParameterModel(int pulsewidth, double stimrate, double stimamp)
        {
            PulseWidth = pulsewidth;
            StimRate = stimrate;
            StimAmp = stimamp;
        }
        
        public int PulseWidth { get; set; }
        public double StimRate { get; set; }
        public double StimAmp { get; set; }
    }
}
