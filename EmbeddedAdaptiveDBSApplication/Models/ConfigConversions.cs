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
using System.Windows;
using EmbeddedAdaptiveDBSApplication.ViewModels;
using Medtronic.NeuroStim.Olympus.DataTypes.Sensing;
using Medtronic.TelemetryM.CtmProtocol.Commands;

namespace EmbeddedAdaptiveDBSApplication.Models
{
    /// <summary>
    /// Class that does the conversions from config file format to medtronic api format
    /// Also contains a method that displays an error to user through a messagebox, disposes of summit system/manager and shuts down program
    /// </summary>
    public static class ConfigConversions
    {
        /// <summary>
        /// Converts the tdEvokedResponseEnable to Medtronic enums
        /// </summary>
        /// <param name="TdEvokedResponseValue">Value of 0, 16 or 32 for standard, evoked 0 or evoked 1</param>
        /// <returns>TdEvokedResponseEnable as Medtronic enum</returns>
        public static TdEvokedResponseEnable TdEvokedResponseEnableConvert(uint TdEvokedResponseValue)
        {
            TdEvokedResponseEnable tdEvokedResponseEnable = TdEvokedResponseEnable.Standard;
            switch (TdEvokedResponseValue)
            {
                case 0:
                    tdEvokedResponseEnable = TdEvokedResponseEnable.Standard;
                    break;
                case 16:
                    tdEvokedResponseEnable = TdEvokedResponseEnable.Evoked0Input;
                    break;
                case 32:
                    tdEvokedResponseEnable = TdEvokedResponseEnable.Evoked1Input;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert TdEvokedResponseEnable");
                    break;
            }
            return tdEvokedResponseEnable;
        }
        /// <summary>
        /// Converts the FftWeightMultiplies to Medtronic enum
        /// </summary>
        /// <param name="FftWeightMultipliesValue">Value from 0-7 based on shift 0-7</param>
        /// <returns>FftWeightMultiplies as Medtronic enum shift 0-7</returns>
        public static FftWeightMultiplies FftWeightMultipliesConvert(uint FftWeightMultipliesValue)
        {
            FftWeightMultiplies fftWeightMultiplies = FftWeightMultiplies.Shift7;
            switch (FftWeightMultipliesValue)
            {
                case 0:
                    fftWeightMultiplies = FftWeightMultiplies.Shift0;
                    break;
                case 1:
                    fftWeightMultiplies = FftWeightMultiplies.Shift1;
                    break;
                case 2:
                    fftWeightMultiplies = FftWeightMultiplies.Shift2;
                    break;
                case 3:
                    fftWeightMultiplies = FftWeightMultiplies.Shift3;
                    break;
                case 4:
                    fftWeightMultiplies = FftWeightMultiplies.Shift4;
                    break;
                case 5:
                    fftWeightMultiplies = FftWeightMultiplies.Shift5;
                    break;
                case 6:
                    fftWeightMultiplies = FftWeightMultiplies.Shift6;
                    break;
                case 7:
                    fftWeightMultiplies = FftWeightMultiplies.Shift7;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert FftWeightMultiplies");
                    break;
            }
            return fftWeightMultiplies;
        }
        /// <summary>
        /// Converts the beeps enables from the config file to the medtronic api
        /// </summary>
        /// <param name="appModel">Application model converted from config file</param>
        /// <returns>The beep enables in medtronic api</returns>
        public static CtmBeepEnables BeepEnablesConvert(AppModel appModel)
        {
            CtmBeepEnables beepEnables = 0;
            if (appModel.CTMBeepEnables.None)
            {
                beepEnables = CtmBeepEnables.None;
            }
            if (appModel.CTMBeepEnables.GeneralAlert)
            {
                if (beepEnables.Equals(0))
                {
                    beepEnables = CtmBeepEnables.GeneralAlert;
                }
                else
                {
                    beepEnables = beepEnables | CtmBeepEnables.GeneralAlert;
                }
            }
            if (appModel.CTMBeepEnables.TelMCompleted)
            {
                if (beepEnables.Equals(0))
                {
                    beepEnables = CtmBeepEnables.TelMCompleted;
                }
                else
                {
                    beepEnables = beepEnables | CtmBeepEnables.TelMCompleted;
                }
            }
            if (appModel.CTMBeepEnables.DeviceDiscovered)
            {
                if (beepEnables.Equals(0))
                {
                    beepEnables = CtmBeepEnables.DeviceDiscovered;
                }
                else
                {
                    beepEnables = beepEnables | CtmBeepEnables.DeviceDiscovered;
                }
            }
            if (appModel.CTMBeepEnables.NoDeviceDiscovered)
            {
                if (beepEnables.Equals(0))
                {
                    beepEnables = CtmBeepEnables.NoDeviceDiscovered;
                }
                else
                {
                    beepEnables = beepEnables | CtmBeepEnables.NoDeviceDiscovered;
                }
            }
            if (appModel.CTMBeepEnables.TelMLost)
            {
                if (beepEnables.Equals(0))
                {
                    beepEnables = CtmBeepEnables.TelMLost;
                }
                else
                {
                    beepEnables = beepEnables | CtmBeepEnables.TelMLost;
                }
            }
            return beepEnables;
        }
        /// <summary>
        /// Converts the enabled power bands from the sense_config file to the correct api format
        /// This ensures that power bands that are enabled are only enabled if the corresponding time domain channels is also enabled.
        /// If there is the case where there is a powerband that is enabled and the corresponding time domain channel is not enabled, then it will proceed as though the power band channel was set to disabled.
        /// </summary>
        /// <param name="senseModel">This is the sense model that was converted from the sense_config.json file and contains which power band and time domain channels are enabled</param>
        /// <returns>Correct BandEnables format for all enabled power bands or else 0 if no power bands are enabled</returns>
        public static BandEnables PowerBandEnablesConvert(SenseModel senseModel)
        {
            BandEnables bandEnables = 0;
            //Check to make sure Power channel AND respective TD Channel is enabled
            if(senseModel.Sense.PowerBands[0].IsEnabled && senseModel.Sense.TimeDomains[0].IsEnabled)
            {
                bandEnables = BandEnables.Ch0Band0Enabled;
            }
            if(senseModel.Sense.PowerBands[1].IsEnabled && senseModel.Sense.TimeDomains[0].IsEnabled)
            {
                if (bandEnables.Equals(0))
                {
                    bandEnables = BandEnables.Ch0Band1Enabled;
                }
                else
                {
                    bandEnables = bandEnables | BandEnables.Ch0Band1Enabled;
                }
            }
            if (senseModel.Sense.PowerBands[2].IsEnabled && senseModel.Sense.TimeDomains[1].IsEnabled)
            {
                if (bandEnables.Equals(0))
                {
                    bandEnables = BandEnables.Ch1Band0Enabled;
                }
                else
                {
                    bandEnables = bandEnables | BandEnables.Ch1Band0Enabled;
                }
            }
            if (senseModel.Sense.PowerBands[3].IsEnabled && senseModel.Sense.TimeDomains[1].IsEnabled)
            {
                if (bandEnables.Equals(0))
                {
                    bandEnables = BandEnables.Ch1Band1Enabled;
                }
                else
                {
                    bandEnables = bandEnables | BandEnables.Ch1Band1Enabled;
                }
            }
            if (senseModel.Sense.PowerBands[4].IsEnabled && senseModel.Sense.TimeDomains[2].IsEnabled)
            {
                if (bandEnables.Equals(0))
                {
                    bandEnables = BandEnables.Ch2Band0Enabled;
                }
                else
                {
                    bandEnables = bandEnables | BandEnables.Ch2Band0Enabled;
                }
            }
            if (senseModel.Sense.PowerBands[5].IsEnabled && senseModel.Sense.TimeDomains[2].IsEnabled)
            {
                if (bandEnables.Equals(0))
                {
                    bandEnables = BandEnables.Ch2Band1Enabled;
                }
                else
                {
                    bandEnables = bandEnables | BandEnables.Ch2Band1Enabled;
                }
            }
            if (senseModel.Sense.PowerBands[6].IsEnabled && senseModel.Sense.TimeDomains[3].IsEnabled)
            {
                if (bandEnables.Equals(0))
                {
                    bandEnables = BandEnables.Ch3Band0Enabled;
                }
                else
                {
                    bandEnables = bandEnables | BandEnables.Ch3Band0Enabled;
                }
            }
            if (senseModel.Sense.PowerBands[7].IsEnabled && senseModel.Sense.TimeDomains[3].IsEnabled)
            {
                if (bandEnables.Equals(0))
                {
                    bandEnables = BandEnables.Ch3Band1Enabled;
                }
                else
                {
                    bandEnables = bandEnables | BandEnables.Ch3Band1Enabled;
                }
            }
            return bandEnables;
        }

        /// <summary>
        /// Method that converts the Detection inputs based on which values in config file were set to true under Inputs
        /// </summary>
        /// <param name="Ch0Band0">True if enabled and false if disabled</param>
        /// <param name="Ch0Band1">True if enabled and false if disabled</param>
        /// <param name="Ch1Band0">True if enabled and false if disabled</param>
        /// <param name="Ch1Band1">True if enabled and false if disabled</param>
        /// <param name="Ch2Band0">True if enabled and false if disabled</param>
        /// <param name="Ch2Band1">True if enabled and false if disabled</param>
        /// <param name="Ch3Band0">True if enabled and false if disabled</param>
        /// <param name="Ch3Band1">True if enabled and false if disabled</param>
        /// <returns>DetectionInputs in the medtronic api format for all enabled values or else DetectionInputs.None if no true values passed in</returns>
        public static DetectionInputs DetectionInputsConvert(bool Ch0Band0, bool Ch0Band1, bool Ch1Band0, bool Ch1Band1, bool Ch2Band0, bool Ch2Band1, bool Ch3Band0, bool Ch3Band1)
        {
            DetectionInputs detectionInputs = DetectionInputs.None;
            try
            {
                if (Ch0Band0)
                {
                    detectionInputs = DetectionInputs.Ch0Band0;
                }
                if (Ch0Band1)
                {
                    if (detectionInputs.Equals(DetectionInputs.None))
                    {
                        detectionInputs = DetectionInputs.Ch0Band1;
                    }
                    else
                    {
                        detectionInputs = detectionInputs | DetectionInputs.Ch0Band1;
                    }
                }
                if (Ch1Band0)
                {
                    if (detectionInputs.Equals(DetectionInputs.None))
                    {
                        detectionInputs = DetectionInputs.Ch1Band0;
                    }
                    else
                    {
                        detectionInputs = detectionInputs | DetectionInputs.Ch1Band0;
                    }
                }
                if (Ch1Band1)
                {
                    if (detectionInputs.Equals(DetectionInputs.None))
                    {
                        detectionInputs = DetectionInputs.Ch1Band1;
                    }
                    else
                    {
                        detectionInputs = detectionInputs | DetectionInputs.Ch1Band1;
                    }
                }
                if (Ch2Band0)
                {
                    if (detectionInputs.Equals(DetectionInputs.None))
                    {
                        detectionInputs = DetectionInputs.Ch2Band0;
                    }
                    else
                    {
                        detectionInputs = detectionInputs | DetectionInputs.Ch2Band0;
                    }
                }
                if (Ch2Band1)
                {
                    if (detectionInputs.Equals(DetectionInputs.None))
                    {
                        detectionInputs = DetectionInputs.Ch2Band1;
                    }
                    else
                    {
                        detectionInputs = detectionInputs | DetectionInputs.Ch2Band1;
                    }
                }
                if (Ch3Band0)
                {
                    if (detectionInputs.Equals(DetectionInputs.None))
                    {
                        detectionInputs = DetectionInputs.Ch3Band0;
                    }
                    else
                    {
                        detectionInputs = detectionInputs | DetectionInputs.Ch3Band0;
                    }
                }
                if (Ch3Band1)
                {
                    if (detectionInputs.Equals(DetectionInputs.None))
                    {
                        detectionInputs = DetectionInputs.Ch3Band1;
                    }
                    else
                    {
                        detectionInputs = detectionInputs | DetectionInputs.Ch3Band1;
                    }
                }
            }
            catch
            {
                DisplayErrorMessageAndClose("Error Converting Detection Inputs");
            }
            return detectionInputs;
        }

        /// <summary>
        /// Converts the Time domain sample rate from config file to correct api call format
        /// Doesn't allow incorrect format to be input
        /// </summary>
        /// <param name="sampleRateToConvert">integer value of 250, 500 or 1000</param>
        /// <returns>Medtronic api format or displays error and closes program if incorrect</returns>
        public static TdSampleRates TDSampleRateConvert(int sampleRateToConvert)
        {
            TdSampleRates sampleRates = 0;
            switch (sampleRateToConvert)
            {
                case 250:
                    sampleRates = TdSampleRates.Sample0250Hz;
                    break;
                case 500:
                    sampleRates = TdSampleRates.Sample0500Hz;
                    break;
                case 1000:
                    sampleRates = TdSampleRates.Sample1000Hz;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert Sample Rate");
                    break;
            }
            return sampleRates;
        }

        /// <summary>
        /// Converts the Time domain High pass filter from config file to correct api call format
        /// </summary>
        /// <param name="TdHpfsToConvert">double value of 0.85, 1.2, 3.3, or 8.6</param>
        /// <returns>Medtronic api format or displays error and closes program if incorrect</returns>
        public static TdHpfs TdHpfsConvert(double TdHpfsToConvert)
        {
            TdHpfs hpfs = 0;
            switch (TdHpfsToConvert)
            {
                case 0.85:
                    hpfs = TdHpfs.Hpf0_85Hz;
                    break;
                case 1.2:
                    hpfs = TdHpfs.Hpf1_2Hz;
                    break;
                case 3.3:
                    hpfs = TdHpfs.Hpf3_3Hz;
                    break;
                case 8.6:
                    hpfs = TdHpfs.Hpf8_6Hz;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert Time Domain Hpf");
                    break;
            }
            return hpfs;
        }

        /// <summary>
        /// Converts the Time domain Low pass filter stage 1 from config file to correct api call format
        /// </summary>
        /// <param name="tdlpf1ToConvert">int value of 50, 100 or 450</param>
        /// <returns>Medtronic api format or displays error and closes program if incorrect</returns>
        public static TdLpfStage1 TdLpfStage1Convert(int tdlpf1ToConvert)
        {
            TdLpfStage1 tdlpf1 = 0;
            switch (tdlpf1ToConvert)
            {
                case 50:
                    tdlpf1 = TdLpfStage1.Lpf50Hz;
                    break;
                case 100:
                    tdlpf1 = TdLpfStage1.Lpf100Hz;
                    break;
                case 450:
                    tdlpf1 = TdLpfStage1.Lpf450Hz;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert Time Domain Lpf1");
                    break;
            }
            return tdlpf1;
        }

        /// <summary>
        /// Converts the Time domain Low pass filter stage 2 from config file to correct api call format
        /// </summary>
        /// <param name="tdlpf2ToConvert">int value of 100, 160 350 or 1700</param>
        /// <returns>Medtronic api format or displays error and closes program if incorrect</returns>
        public static TdLpfStage2 TdLpfStage2Convert(int tdlpf2ToConvert)
        {
            TdLpfStage2 tdlpf2 = 0;
            switch (tdlpf2ToConvert)
            {
                case 100:
                    tdlpf2 = TdLpfStage2.Lpf100Hz;
                    break;
                case 160:
                    tdlpf2 = TdLpfStage2.Lpf160Hz;
                    break;
                case 350:
                    tdlpf2 = TdLpfStage2.Lpf350Hz;
                    break;
                case 1700:
                    tdlpf2 = TdLpfStage2.Lpf1700Hz;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert Time Domain Lpf2");
                    break;
            }
            return tdlpf2;
        }

        /// <summary>
        /// Converts the time domain channels from config file format to medtronic api format
        /// </summary>
        /// <param name="TdMuxInputsToConvert">integer value of 0-15 based on the time domain input</param>
        /// <returns>TdMuxInputs in medtronic api format based on input parameter value or displays error and closes program if incorrect</returns>
        public static TdMuxInputs TdMuxInputsConvert(int TdMuxInputsToConvert)
        {
            TdMuxInputs tdInputs = 0;
            switch (TdMuxInputsToConvert)
            {
                case 0:
                case 8:
                    tdInputs = TdMuxInputs.Mux0;
                    break;
                case 1:
                case 9:
                    tdInputs = TdMuxInputs.Mux1;
                    break;
                case 2:
                case 10:
                    tdInputs = TdMuxInputs.Mux2;
                    break;
                case 3:
                case 11:
                    tdInputs = TdMuxInputs.Mux3;
                    break;
                case 4:
                case 12:
                    tdInputs = TdMuxInputs.Mux4;
                    break;
                case 5:
                case 13:
                    tdInputs = TdMuxInputs.Mux5;
                    break;
                case 6:
                case 14:
                    tdInputs = TdMuxInputs.Mux6;
                    break;
                case 7:
                case 15:
                    tdInputs = TdMuxInputs.Mux7;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert Time Domain Inputs");
                    break;
            }
            return tdInputs;
        }

        /// <summary>
        /// Converts the FFT size from config file to correct api call format
        /// </summary>
        /// <param name="FftSizesToConvert">integer value of 64, 256, or 1024</param>
        /// <returns>Medtronic api format for FftSizes or displays error and closes program if incorrect</returns>
        public static FftSizes FftSizesConvert(int FftSizesToConvert)
        {
            FftSizes fftSizes = 0;
            switch (FftSizesToConvert)
            {
                case 64:
                    fftSizes = FftSizes.Size0064;
                    break;
                case 256:
                    fftSizes = FftSizes.Size0256;
                    break;
                case 1024:
                    fftSizes = FftSizes.Size1024;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert FFT Size");
                    break;
            }
            return fftSizes;
        }

        /// <summary>
        /// Converts the Window Load from config file to correct api call format
        /// </summary>
        /// <param name="FftWindowAutoLoadsToConvert">integer value of 25, 50 or 100</param>
        /// <returns>Medtronic api format for FftWindowAutoLoads or displays error and closes program if incorrect</returns>
        public static FftWindowAutoLoads FftWindowAutoLoadsConvert(int FftWindowAutoLoadsToConvert)
        {
            FftWindowAutoLoads fftWindowAutoLoads = 0;
            switch (FftWindowAutoLoadsToConvert)
            {
                case 25:
                    fftWindowAutoLoads = FftWindowAutoLoads.Hann25;
                    break;
                case 50:
                    fftWindowAutoLoads = FftWindowAutoLoads.Hann50;
                    break;
                case 100:
                    fftWindowAutoLoads = FftWindowAutoLoads.Hann100;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert FFT Window Load");
                    break;
            }
            return fftWindowAutoLoads;
        }

        /// <summary>
        /// Gets the medtronic api format for acceleration rate from the config file format
        /// Checks to make sure that the sample rate is disabled first.
        /// </summary>
        /// <param name="AccelSampleRateToConvert">integer value of 4,8,16,32 or 64</param>
        /// <param name="SampleRateDisabled">true if disabled or false if enabled</param>
        /// <returns>AccelSampleRate if SampleRateDisabled is false and AccelSampleRateToConvert is correct. Returns AccelSampleRate.Disabled if SampleRateDisabled is true. If there was an error, a message is displayed and program terminated</returns>
        public static AccelSampleRate AccelSampleRateConvert(int AccelSampleRateToConvert, bool SampleRateDisabled)
        {
            if (SampleRateDisabled)
            {
                return AccelSampleRate.Disabled;
            }
            AccelSampleRate accelSampleRate = 0;
            switch (AccelSampleRateToConvert)
            {
                case 4:
                    accelSampleRate = AccelSampleRate.Sample04;
                    break;
                case 8:
                    accelSampleRate = AccelSampleRate.Sample08;
                    break;
                case 16:
                    accelSampleRate = AccelSampleRate.Sample16;
                    break;
                case 32:
                    accelSampleRate = AccelSampleRate.Sample32;
                    break;
                case 64:
                    accelSampleRate = AccelSampleRate.Sample64;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert Accelerometer Sample Rate");
                    break;
            }
            return accelSampleRate;
        }

        /// <summary>
        /// Converts which sense states are enabled based on which values are true. 
        /// Converts into medtronic api format based on the true values input as parameters
        /// </summary>
        /// <param name="TimeDomain">True if enabled and false if disabled</param>
        /// <param name="FFT">True if enabled and false if disabled</param>
        /// <param name="Power">True if enabled and false if disabled</param>
        /// <param name="LD0">True if enabled and false if disabled</param>
        /// <param name="LD1">True if enabled and false if disabled</param>
        /// <param name="AdaptiveState">True if enabled and false if disabled</param>
        /// <param name="LoopRecording">True if enabled and false if disabled</param>
        /// <param name="Unused">True if enabled and false if disabled</param>
        /// <returns>SenseStates for all the true values that were input as parameter. SenseStates.None if there were not any true values. If there was an error, a message is displayed and program terminated</returns>
        public static SenseStates TDSenseStatesConvert(bool TimeDomain, bool FFT, bool Power, bool LD0, bool LD1, bool AdaptiveState, bool LoopRecording, bool Unused)
        {
            SenseStates state = SenseStates.None;
            try
            {
                if (TimeDomain)
                {
                    state = SenseStates.LfpSense;
                }
                if (FFT)
                {
                    if (state.Equals(SenseStates.None))
                    {
                        state = SenseStates.Fft;
                    }
                    else
                    {
                        state = state | SenseStates.Fft;
                    }
                }
                if (Power)
                {
                    if (state.Equals(SenseStates.None))
                    {
                        state = SenseStates.Power;
                    }
                    else
                    {
                        state = state | SenseStates.Power;
                    }
                }
                if (LD0)
                {
                    if (state.Equals(SenseStates.None))
                    {
                        state = SenseStates.DetectionLd0;
                    }
                    else
                    {
                        state = state | SenseStates.DetectionLd0;
                    }
                }
                if (LD1)
                {
                    if (state.Equals(SenseStates.None))
                    {
                        state = SenseStates.DetectionLd1;
                    }
                    else
                    {
                        state = state | SenseStates.DetectionLd1;
                    }
                }
                if (AdaptiveState)
                {
                    if (state.Equals(SenseStates.None))
                    {
                        state = SenseStates.AdaptiveStim;
                    }
                    else
                    {
                        state = state | SenseStates.AdaptiveStim;
                    }
                }
                if (LoopRecording)
                {
                    if (state.Equals(SenseStates.None))
                    {
                        state = SenseStates.LoopRecording;
                    }
                    else
                    {
                        state = state | SenseStates.LoopRecording;
                    }
                }
                if (Unused)
                {
                    if (state.Equals(SenseStates.None))
                    {
                        state = SenseStates.Unused08;
                    }
                    else
                    {
                        state = state | SenseStates.Unused08;
                    }
                }
            }
            catch
            {
                DisplayErrorMessageAndClose("Error converting Sense States.");
            }
            return state;
        }

        /// <summary>
        /// Converts the fft channel
        /// </summary>
        /// <param name="localModel">SenseModel for the fft channel and if timedomain for specific channel is enabled</param>
        /// <returns>SenseTimeDomainChannel of channel or defaults to channel 0</returns>
        public static SenseTimeDomainChannel FFTChannelConvert(SenseModel localModel)
        {
            SenseTimeDomainChannel fftChannel = SenseTimeDomainChannel.Ch0;

            switch (localModel.Sense.FFT.Channel)
            {
                case 0:
                    if (localModel.Sense.TimeDomains[0].IsEnabled) { fftChannel = SenseTimeDomainChannel.Ch0; }
                    break;
                case 1:
                    if (localModel.Sense.TimeDomains[1].IsEnabled) { fftChannel = SenseTimeDomainChannel.Ch1; }
                    break;
                case 2:
                    if (localModel.Sense.TimeDomains[2].IsEnabled) { fftChannel = SenseTimeDomainChannel.Ch2; }
                    break;
                case 3:
                    if (localModel.Sense.TimeDomains[3].IsEnabled) { fftChannel = SenseTimeDomainChannel.Ch3; }
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert FFT Channel");
                    break;
            }
            return fftChannel;
        }

        /// <summary>
        /// Converts the frame rate from config file digit to medtronic api format
        /// </summary>
        /// <param name="FrameRate">integer between 30-100 by 10's and is in ms</param>
        /// <returns>StreamingFrameRate in medtronic form.  Otherwise it error out with a messagebox</returns>
        public static StreamingFrameRate MiscStreamRateConvert(int FrameRate)
        {
            StreamingFrameRate localFrameRate = StreamingFrameRate.Frame50ms;
            switch (FrameRate)
            {
                case 30:
                    localFrameRate = StreamingFrameRate.Frame30ms;
                    break;
                case 40:
                    localFrameRate = StreamingFrameRate.Frame40ms;
                    break;
                case 50:
                    localFrameRate = StreamingFrameRate.Frame50ms;
                    break;
                case 60:
                    localFrameRate = StreamingFrameRate.Frame60ms;
                    break;
                case 70:
                    localFrameRate = StreamingFrameRate.Frame70ms;
                    break;
                case 80:
                    localFrameRate = StreamingFrameRate.Frame80ms;
                    break;
                case 90:
                    localFrameRate = StreamingFrameRate.Frame90ms;
                    break;
                case 100:
                    localFrameRate = StreamingFrameRate.Frame100ms;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert Misc Frame Rate");
                    break;
            }
            return localFrameRate;
        }

        /// <summary>
        /// Converts the loop recording trigger from config file to medtronic api
        /// </summary>
        /// <param name="triggers">Value from 0-8 that gives the state for the trigger</param>
        /// <param name="isEnabled">If this is false, then it returns None, otherwise it returns the corresponding state</param>
        /// <returns>If this isEnabled is false, then it returns None, otherwise it returns the corresponding state in Medtronic form</returns>
        public static LoopRecordingTriggers MiscloopRecordingTriggersConvert(int triggers, bool isEnabled)
        {
            if (!isEnabled)
            {
                return LoopRecordingTriggers.None;
            }
            LoopRecordingTriggers localTrigger = LoopRecordingTriggers.None;
            switch (triggers)
            {
                case 0:
                    localTrigger = LoopRecordingTriggers.State0;
                    break;
                case 1:
                    localTrigger = LoopRecordingTriggers.State1;
                    break;
                case 2:
                    localTrigger = LoopRecordingTriggers.State2;
                    break;
                case 3:
                    localTrigger = LoopRecordingTriggers.State3;
                    break;
                case 4:
                    localTrigger = LoopRecordingTriggers.State4;
                    break;
                case 5:
                    localTrigger = LoopRecordingTriggers.State5;
                    break;
                case 6:
                    localTrigger = LoopRecordingTriggers.State6;
                    break;
                case 7:
                    localTrigger = LoopRecordingTriggers.State7;
                    break;
                case 8:
                    localTrigger = LoopRecordingTriggers.State8;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert Misc Loop Recording Trigger");
                    break;
            }
            return localTrigger;

        }

        /// <summary>
        /// Converts the Bridging from config to medtronic api
        /// </summary>
        /// <param name="valueFromConfig">can be 0 = None, 1 = Bridge 0-2 enabled, 2 = Bridge 1-3 enabled</param>
        /// <returns>Medtronic version of bridge or displays error in message box</returns>
        public static BridgingConfig MiscBridgingConfigConvert(int valueFromConfig)
        {
            BridgingConfig temp = BridgingConfig.None;
            switch (valueFromConfig)
            {
                case 0:
                    temp = BridgingConfig.None;
                    break;
                case 1:
                    temp = BridgingConfig.Bridge0to2Enabled;
                    break;
                case 2:
                    temp = BridgingConfig.Bridge1to3Enabled;
                    break;
                default:
                    DisplayErrorMessageAndClose("Couldn't convert Misc Bridging");
                    break;
            }
            return temp;
        }

        /// <summary>
        /// Displays an error to the user in a message box, disposes of the summit manager/system and shuts program down
        /// </summary>
        /// <param name="error">A message detailing what the error is and possible location of error</param>
        public static void DisplayErrorMessageAndClose(string error)
        {
            string messageBoxText = error;
            string caption = "ERROR";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBox.Show(messageBoxText, caption, button, icon);
            MainViewModel.DisposeSummitManagerAndSystem();
            Environment.Exit(0);
        }
    }
}
