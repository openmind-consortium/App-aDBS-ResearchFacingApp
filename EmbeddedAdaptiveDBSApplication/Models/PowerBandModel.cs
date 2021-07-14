/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/

namespace EmbeddedAdaptiveDBSApplication.Models
{
    class PowerBandModel
    {
        /// <summary>
        /// Lower index for band 0
        /// </summary>
        public ushort lowerIndexBand0 { get; set; }
        /// <summary>
        /// Upper index for band 0
        /// </summary>
        public ushort upperIndexBand0 { get; set; }
        /// <summary>
        /// Lower index for band 1
        /// </summary>
        public ushort lowerIndexBand1 { get; set; }
        /// <summary>
        /// Upper index for band 1
        /// </summary>
        public ushort upperIndexBand1 { get; set; }
        /// <summary>
        /// Actual value in Hz for lower power band 0
        /// </summary>
        public double lowerActualValueHzBand0 { get; set; }
        /// <summary>
        /// Actual value in Hz for upper power band 0
        /// </summary>
        public double UpperActualValueHzBand0 { get; set; }
        /// <summary>
        /// Actual value in Hz for lower power band 1
        /// </summary>
        public double lowerActualValueHzBand1 { get; set; }
        /// <summary>
        /// Actual value in Hz for upper power band 1
        /// </summary>
        public double upperActualValueHzBand1 { get; set; }
    }
}
