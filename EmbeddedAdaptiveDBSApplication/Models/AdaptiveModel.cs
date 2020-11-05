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
    /// Model for Adaptive Config JSON File
    /// This class is used to convert the json file into class data
    /// </summary>
    public class AdaptiveModel
    {
        /// <summary>
        /// Comment giving directions for the config file
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Detection object
        /// </summary>
        public Detection Detection { get; set; }
        /// <summary>
        /// Adaptive object
        /// </summary>
        public Adaptive Adaptive { get; set; }
    }
    /// <summary>
    /// Detection Object
    /// </summary>
    public class Detection
    {
        /// <summary>
        /// LD0 Object
        /// </summary>
        public LD0 LD0 { get; set; }
        /// <summary>
        /// LD1 Object
        /// </summary>
        public LD1 LD1 { get; set; }
    }
    /// <summary>
    /// LD0 Object
    /// </summary>
    public class LD0
    {
        /// <summary>
        /// Comment for Ld0
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// B0 threshold
        /// </summary>
        public uint B0 { get; set; }
        /// <summary>
        /// B1 threshold
        /// </summary>
        public uint B1 { get; set; }
        /// <summary>
        /// Update rate in Hz
        /// </summary>
        public ushort UpdateRate { get; set; }
        /// <summary>
        /// Onset duration
        /// </summary>
        public ushort OnsetDuration { get; set; }
        /// <summary>
        /// Termination Duration
        /// </summary>
        public ushort TerminationDuration { get; set; }
        /// <summary>
        /// Hold off time on startup
        /// </summary>
        public ushort HoldOffOnStartupTime { get; set; }
        /// <summary>
        /// Blanking upon state change
        /// </summary>
        public ushort StateChangeBlankingUponStateChange { get; set; }
        /// <summary>
        /// Fractional fixed point value
        /// </summary>
        public byte FractionalFixedPointValue { get; set; }
        /// <summary>
        /// If dual threshold is enabled or not
        /// </summary>
        public bool DualThreshold { get; set; }
        /// <summary>
        /// If to blank both LD's
        /// </summary>
        public bool BlankBothLD { get; set; }
        /// <summary>
        /// Inputs object
        /// </summary>
        public Inputs Inputs { get; set; }
        /// <summary>
        /// Weight Vector for each channel 0-3
        /// </summary>
        public List<uint> WeightVector { get; set; }
        /// <summary>
        /// Normalization Mulitply Vector for each channel 0-3
        /// </summary>
        public List<uint> NormalizationMultiplyVector { get; set; }
        /// <summary>
        /// Normalization Subtract Vector for each channel 0-3
        /// </summary>
        public List<uint> NormalizationSubtractVector { get; set; }
    }
    /// <summary>
    /// LD1 object
    /// </summary>
    public class LD1
    {
        /// <summary>
        /// Comment for Ld0
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// If LD1 is enabled or no
        /// </summary>
        public bool IsEnabled { get; set; }
        /// <summary>
        /// B0 threshold
        /// </summary>
        public uint B0 { get; set; }
        /// <summary>
        /// B1 threshold
        /// </summary>
        public uint B1 { get; set; }
        /// <summary>
        /// Update rate in Hz
        /// </summary>
        public ushort UpdateRate { get; set; }
        /// <summary>
        /// Onset duration
        /// </summary>
        public ushort OnsetDuration { get; set; }
        /// <summary>
        /// Termination Duration
        /// </summary>
        public ushort TerminationDuration { get; set; }
        /// <summary>
        /// Hold off time on startup
        /// </summary>
        public ushort HoldOffOnStartupTime { get; set; }
        /// <summary>
        /// Blanking upon state change
        /// </summary>
        public ushort StateChangeBlankingUponStateChange { get; set; }
        /// <summary>
        /// Fractional fixed point value
        /// </summary>
        public byte FractionalFixedPointValue { get; set; }
        /// <summary>
        /// If dual threshold is enabled or not
        /// </summary>
        public bool DualThreshold { get; set; }
        /// <summary>
        /// If to blank both LD's
        /// </summary>
        public bool BlankBothLD { get; set; }
        /// <summary>
        /// Inputs object
        /// </summary>
        public Inputs Inputs { get; set; }
        /// <summary>
        /// Weight Vector for each channel 0-3
        /// </summary>
        public List<uint> WeightVector { get; set; }
        /// <summary>
        /// Normalization Mulitply Vector for each channel 0-3
        /// </summary>
        public List<uint> NormalizationMultiplyVector { get; set; }
        /// <summary>
        /// Normalization Subtract Vector for each channel 0-3
        /// </summary>
        public List<uint> NormalizationSubtractVector { get; set; }
    }
    /// <summary>
    /// Inputs Object
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// Channel 0 Band 0
        /// </summary>
        public bool Ch0Band0 { get; set; }
        /// <summary>
        /// Channel 0 Band 1
        /// </summary>
        public bool Ch0Band1 { get; set; }
        /// <summary>
        /// Channel 1 Band 0
        /// </summary>
        public bool Ch1Band0 { get; set; }
        /// <summary>
        /// Channel 1 Band 1
        /// </summary>
        public bool Ch1Band1 { get; set; }
        /// <summary>
        /// Channel 2 Band 0
        /// </summary>
        public bool Ch2Band0 { get; set; }
        /// <summary>
        /// Channel 2 Band 1
        /// </summary>
        public bool Ch2Band1 { get; set; }
        /// <summary>
        /// Channel 3 Band 0
        /// </summary>
        public bool Ch3Band0 { get; set; }
        /// <summary>
        /// Channel 3 Band 1
        /// </summary>
        public bool Ch3Band1 { get; set; }
    }
    /// <summary>
    /// Adaptive Object
    /// </summary>
    public class Adaptive
    {
        /// <summary>
        /// Program Object
        /// </summary>
        public Program0 Program0 { get; set; }
    }
    /// <summary>
    /// Program Object
    /// </summary>
    public class Program0
    {
        /// <summary>
        /// Comment explaining object
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// Delta rise time
        /// </summary>
        public uint RiseTimes { get; set; }
        /// <summary>
        /// Delta fall time
        /// </summary>
        public uint FallTimes { get; set; }
        /// <summary>
        /// Target Rate in Hz
        /// </summary>
        public double RateTargetInHz { get; set; }
        /// <summary>
        /// State 0 in mA, set to 25.5 to hold
        /// </summary>
        public double State0AmpInMilliamps { get; set; }
        /// <summary>
        /// State 1 in mA, set to 25.5 to hold
        /// </summary>
        public double State1AmpInMilliamps { get; set; }
        /// <summary>
        /// State 2 in mA, set to 25.5 to hold
        /// </summary>
        public double State2AmpInMilliamps { get; set; }
        /// <summary>
        /// State 3 in mA, set to 25.5 to hold
        /// </summary>
        public double State3AmpInMilliamps { get; set; }
        /// <summary>
        /// State 4 in mA, set to 25.5 to hold
        /// </summary>
        public double State4AmpInMilliamps { get; set; }
        /// <summary>
        /// State 5 in mA, set to 25.5 to hold
        /// </summary>
        public double State5AmpInMilliamps { get; set; }
        /// <summary>
        /// State 6 in mA, set to 25.5 to hold
        /// </summary>
        public double State6AmpInMilliamps { get; set; }
        /// <summary>
        /// State 7 in mA, set to 25.5 to hold
        /// </summary>
        public double State7AmpInMilliamps { get; set; }
        /// <summary>
        /// State 8 in mA, set to 25.5 to hold
        /// </summary>
        public double State8AmpInMilliamps { get; set; }
    }
}
