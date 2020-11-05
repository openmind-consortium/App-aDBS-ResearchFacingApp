using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.Models;
using Medtronic.NeuroStim.Olympus.DataTypes.Sensing;
using Medtronic.SummitAPI.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EmbeddedAdaptiveDBSApplication.Services
{
    class SummitSensing
    {
        private ILog _log;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_log">Caliburn Micro Logger</param>
        public SummitSensing(ILog _log)
        {
            this._log = _log;
        }
        /// <summary>
        /// Write Sense Configuration Settings
        /// </summary>
        /// <param name="localModel">The config file to use for sensing parameters</param>
        /// <param name="theSummit">Summit system</param>
        /// <param name="showErrorMessage">True if you want a popup message on errors or false if show no error popup</param>
        /// <returns>true if successfully configuring sensing or false if unsuccessful</returns>
        public bool SummitConfigureSensing(SummitSystem theSummit, SenseModel localModel, bool showErrorMessage)
        {
            APIReturnInfo bufferReturnInfo;
            SensingState state;
            //This checks to see if sensing is already enabled. This can happen if adaptive is already running and we don't need to configure it. 
            //If it is, then skip setting up sensing
            try
            {
                theSummit.ReadSensingState(out state);
                if (!state.State.ToString().Equals("None"))
                {
                    return true;
                }
            }
            catch (Exception error)
            {
                _log.Error(error);
                return false;
            }
            if (!StopSensing(theSummit, showErrorMessage))
            {
                return false;
            }
            // Create a sensing configuration
            List<TimeDomainChannel> TimeDomainChannels = new List<TimeDomainChannel>(4);

            // Channel Specific configuration - 0
            TimeDomainChannels.Add(new TimeDomainChannel(
                GetTDSampleRate(localModel.Sense.TimeDomains[0].IsEnabled, localModel),
                ConfigConversions.TdMuxInputsConvert(localModel.Sense.TimeDomains[0].Inputs[0]),
                ConfigConversions.TdMuxInputsConvert(localModel.Sense.TimeDomains[0].Inputs[1]),
                TdEvokedResponseEnable.Standard,
                ConfigConversions.TdLpfStage1Convert(localModel.Sense.TimeDomains[0].Lpf1),
                ConfigConversions.TdLpfStage2Convert(localModel.Sense.TimeDomains[0].Lpf2),
                ConfigConversions.TdHpfsConvert(localModel.Sense.TimeDomains[0].Hpf)));

            // Channel Specific configuration - 1
            TimeDomainChannels.Add(new TimeDomainChannel(
                GetTDSampleRate(localModel.Sense.TimeDomains[1].IsEnabled, localModel),
                ConfigConversions.TdMuxInputsConvert(localModel.Sense.TimeDomains[1].Inputs[0]),
                ConfigConversions.TdMuxInputsConvert(localModel.Sense.TimeDomains[1].Inputs[1]),
                TdEvokedResponseEnable.Standard,
                ConfigConversions.TdLpfStage1Convert(localModel.Sense.TimeDomains[1].Lpf1),
                ConfigConversions.TdLpfStage2Convert(localModel.Sense.TimeDomains[1].Lpf2),
                ConfigConversions.TdHpfsConvert(localModel.Sense.TimeDomains[1].Hpf)));

            // Channel Specific configuration - 2
            TimeDomainChannels.Add(new TimeDomainChannel(
                GetTDSampleRate(localModel.Sense.TimeDomains[2].IsEnabled, localModel),
                ConfigConversions.TdMuxInputsConvert(localModel.Sense.TimeDomains[2].Inputs[0]),
                ConfigConversions.TdMuxInputsConvert(localModel.Sense.TimeDomains[2].Inputs[1]),
                TdEvokedResponseEnable.Standard,
                ConfigConversions.TdLpfStage1Convert(localModel.Sense.TimeDomains[2].Lpf1),
                ConfigConversions.TdLpfStage2Convert(localModel.Sense.TimeDomains[2].Lpf2),
                ConfigConversions.TdHpfsConvert(localModel.Sense.TimeDomains[2].Hpf)));

            // Channel Specific configuration - 3
            TimeDomainChannels.Add(new TimeDomainChannel(
                GetTDSampleRate(localModel.Sense.TimeDomains[3].IsEnabled, localModel),
                ConfigConversions.TdMuxInputsConvert(localModel.Sense.TimeDomains[3].Inputs[0]),
                ConfigConversions.TdMuxInputsConvert(localModel.Sense.TimeDomains[3].Inputs[1]),
                TdEvokedResponseEnable.Standard,
                ConfigConversions.TdLpfStage1Convert(localModel.Sense.TimeDomains[3].Lpf1),
                ConfigConversions.TdLpfStage2Convert(localModel.Sense.TimeDomains[3].Lpf2),
                ConfigConversions.TdHpfsConvert(localModel.Sense.TimeDomains[3].Hpf)));

            // Set up the FFT 
            FftConfiguration fftChannel = new FftConfiguration(
                ConfigConversions.FftSizesConvert(localModel.Sense.FFT.FftSize),
                localModel.Sense.FFT.FftInterval,
                ConfigConversions.FftWindowAutoLoadsConvert(localModel.Sense.FFT.WindowLoad),
                localModel.Sense.FFT.WindowEnabled,
                FftWeightMultiplies.Shift7,
                localModel.Sense.FFT.StreamSizeBins,
                localModel.Sense.FFT.StreamOffsetBins);
            _log.Info("FFT Size: " + localModel.Sense.FFT.FftSize + ". FFT Interval: " + localModel.Sense.FFT.FftInterval);

            // Set up the Power channels
            List<PowerChannel> powerChannels = new List<PowerChannel>();
            //This goes through each power channel and gets the lower power band and upper power band.
            //Medtronic api uses bin index values for setting power channels instead of actual values in Hz
            //This calls the CalculatePowerBins class to convert to proper medtronic api values from the user config file
            //User config file has estimated values in Hz for each channel.  
            //CalculatePowerBins converts them to actual power values and we are able to get the bin index value from that.
            CalculatePowerBins calculatePowerBins = new CalculatePowerBins(_log);
            List<PowerBandModel> powerBandsList = calculatePowerBins.GetPowerBands(localModel);
            if (powerBandsList == null || powerBandsList.Count < 3)
            {
                MessageBox.Show("Error calculating power bins. Please check that power bins are correct in the config file and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return false;
            }
            for (int i = 0; i < 4; i++)
            {
                //Add the lower and upper power bands to the power channel
                powerChannels.Add(new PowerChannel(powerBandsList[i].lowerIndexBand0, powerBandsList[i].upperIndexBand0, powerBandsList[i].lowerIndexBand1, powerBandsList[i].upperIndexBand1));
                _log.Info("Powerband 0 lower band: " + powerBandsList[i].lowerIndexBand0 + ". Powerband 0 upper band: " + powerBandsList[i].upperIndexBand0);
                _log.Info("Powerband 1 lower band: " + powerBandsList[i].lowerIndexBand1 + ". Powerband 1 upper band: " + powerBandsList[i].upperIndexBand1);
            }
            //Gets the enabled power bands from the sense config file and returns the correct api call for all enabled
            BandEnables theBandEnables = ConfigConversions.PowerBandEnablesConvert(localModel);

            // Set up the miscellaneous settings
            MiscellaneousSensing miscsettings = new MiscellaneousSensing();
            miscsettings.StreamingRate = ConfigConversions.MiscStreamRateConvert(localModel.Sense.Misc.StreamingRate);
            miscsettings.LrTriggers = ConfigConversions.MiscloopRecordingTriggersConvert(localModel.Sense.Misc.LoopRecordingTriggersState, localModel.Sense.Misc.LoopRecordingTriggersIsEnabled);
            miscsettings.LrPostBufferTime = localModel.Sense.Misc.LoopRecordingPostBufferTime;
            miscsettings.Bridging = ConfigConversions.MiscBridgingConfigConvert(localModel.Sense.Misc.Bridging);

            //Writes all sensing information here
            try
            {
                bufferReturnInfo = theSummit.WriteSensingTimeDomainChannels(TimeDomainChannels);
                if (!CheckForReturnError(bufferReturnInfo, "Writing Sensing Time Domain", showErrorMessage))
                {
                    return false;
                }
                bufferReturnInfo = theSummit.WriteSensingFftSettings(fftChannel);
                if (!CheckForReturnError(bufferReturnInfo, "Writing Sensing FFT", showErrorMessage))
                {
                    return false;
                }
                bufferReturnInfo = theSummit.WriteSensingPowerChannels(theBandEnables, powerChannels);
                if (!CheckForReturnError(bufferReturnInfo, "Writing Sensing Power", showErrorMessage))
                {
                    return false;
                }
                bufferReturnInfo = theSummit.WriteSensingMiscSettings(miscsettings);
                if (!CheckForReturnError(bufferReturnInfo, "Writing Sensing Misc", showErrorMessage))
                {
                    return false;
                }
                bufferReturnInfo = theSummit.WriteSensingAccelSettings(ConfigConversions.AccelSampleRateConvert(localModel.Sense.Accelerometer.SampleRate, localModel.Sense.Accelerometer.SampleRateDisabled));
                if (!CheckForReturnError(bufferReturnInfo, "Writing Sensing Accel", showErrorMessage))
                {
                    return false;
                }
            }
            catch (Exception error)
            {
                _log.Error(error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Starts sensing and streaming.  Skips starting sensing if Adaptive running.
        /// </summary>
        /// <param name="theSummit">Summit System</param>
        /// <param name="senseConfig">Sense Model</param>
        /// <param name="showErrorMessage">True if you want a popup message on errors or false if show no error popup</param>
        /// <returns>True if success and false if unsuccessful</returns>
        public bool StartSensing(SummitSystem theSummit, SenseModel senseConfig, bool showErrorMessage)
        {
            APIReturnInfo bufferReturnInfo;
            try
            {
                // Start sensing
                bufferReturnInfo = theSummit.WriteSensingState(ConfigConversions.TDSenseStatesConvert(
                    senseConfig.SenseOptions.TimeDomain,
                    senseConfig.SenseOptions.FFT,
                    senseConfig.SenseOptions.Power,
                    senseConfig.SenseOptions.LD0,
                    senseConfig.SenseOptions.LD1,
                    senseConfig.SenseOptions.AdaptiveState,
                    senseConfig.SenseOptions.LoopRecording,
                    senseConfig.SenseOptions.Unused), ConfigConversions.FFTChannelConvert(senseConfig));
                if (!CheckForReturnError(bufferReturnInfo, "Write Sensing State. Please check LD0/LD1 is false if not running adaptive therapy.", showErrorMessage))
                {
                    return false;
                }
            }
            catch (Exception error)
            {
                _log.Error(error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Starts streaming
        /// </summary>
        /// <param name="theSummit">the summit system</param>
        /// <param name="senseConfig">The sense Model from the config file</param>
        /// <param name="showErrorMessage">True if you want a popup message on errors or false if show no error popup</param>
        /// <returns>True if success and false if unsuccessful</returns>
        public bool StartStreaming(SummitSystem theSummit, SenseModel senseConfig, bool showErrorMessage)
        {
            APIReturnInfo bufferReturnInfo;
            try
            {
                // Start streaming
                bufferReturnInfo = theSummit.WriteSensingEnableStreams(
                    senseConfig.StreamEnables.TimeDomain,
                    senseConfig.StreamEnables.FFT,
                    senseConfig.StreamEnables.Power,
                    senseConfig.StreamEnables.AdaptiveTherapy,
                    senseConfig.StreamEnables.AdaptiveState,
                    senseConfig.StreamEnables.Accelerometry,
                    senseConfig.StreamEnables.TimeStamp,
                    senseConfig.StreamEnables.EventMarker);
                if (!CheckForReturnError(bufferReturnInfo, "Stream Enables", showErrorMessage))
                {
                    return false;
                }
            }
            catch (Exception error)
            {
                _log.Error(error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Stops sensing
        /// </summary>
        /// <param name="theSummit">Summit System</param>
        /// <param name="showErrorMessage">True if you want a popup message on errors or false if show no error popup</param>
        /// <returns>true if success or false if unsuccessful</returns>
        public bool StopSensing(SummitSystem theSummit, bool showErrorMessage)
        {
            if (theSummit == null)
            {
                return false;
            }
            bool success = true;
            APIReturnInfo bufferReturnInfo;
            try
            {
                bufferReturnInfo = theSummit.WriteSensingDisableStreams(true, true, true, true, true, true, true, true);
                if (!CheckForReturnError(bufferReturnInfo, "Turn off Streaming while turning off sensing", showErrorMessage))
                {
                    success = false;
                }
                bufferReturnInfo = theSummit.WriteSensingState(SenseStates.None, 0x00);
                if (!CheckForReturnError(bufferReturnInfo, "Turn off Sensing", showErrorMessage))
                {
                    success = false;
                }
            }
            catch (Exception error)
            {
                _log.Error(error);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Stops streaming
        /// </summary>
        /// <param name="localSummit">Summit System</param>
        /// <param name="showErrorMessage">True if you want a popup message on errors or false if show no error popup</param>
        /// <returns>true if success or false if unsuccessful</returns>
        public bool StopStreaming(SummitSystem localSummit, bool showErrorMessage)
        {
            if (localSummit == null)
            {
                return false;
            }
            bool success = true;
            APIReturnInfo bufferReturnInfo;
            try
            {
                bufferReturnInfo = localSummit.WriteSensingDisableStreams(true, true, true, true, true, true, true, true);
                if (!CheckForReturnError(bufferReturnInfo, "Turn off Streaming", showErrorMessage))
                {
                    success = false;
                }
            }
            catch (Exception error)
            {
                _log.Error(error);
                success = false;
            }
            return success;
        }
        #region Helper Methods
        /// <summary>
        /// Checks to make sure upper power band is greater than lower power band index
        /// The upper power band index must be greater or equal to lower power band index
        /// </summary>
        /// <param name="lower">Lower Power band index</param>
        /// <param name="upper">Upper Power band index</param>
        /// <returns>The new upper band index which is now equal to the lower index if the upper index happens to be less than the lower. Else it returns the original upper index that was passed in</returns>
        private ushort CheckThatUpperPowerBandGreaterThanLowerPowerBand(ushort lower, ushort upper)
        {
            if (upper < lower)
            {
                upper = lower;
            }
            return upper;
        }
        /// <summary>
        /// Gets the TDSampleRate for the TimeDomain Channel
        /// Calls TD SampleRateConvert from ConfigConversions class
        /// Checks for disabled time domain channels and returns proper sample rate or disabled for disabled channels
        /// </summary>
        /// <param name="sampleRateIsEnabled">Either true or false depending on value for Time Domain Channel IsEnabled value from config file</param>
        /// <param name="localModel">Sense model</param>
        /// <returns>If the sampleRateIsEnabled variable is set to false, then it returns the TdSampleRates.Disabled. Otherwise it returns the correct TdSampleRates variable for the corresponding TD sample rate from the config file</returns>
        private TdSampleRates GetTDSampleRate(bool sampleRateIsEnabled, SenseModel localModel)
        {
            TdSampleRates the_sample_rate = ConfigConversions.TDSampleRateConvert(localModel.Sense.TDSampleRate);
            if (!sampleRateIsEnabled)
            {
                the_sample_rate = TdSampleRates.Disabled;
            }
            return the_sample_rate;
        }
        #endregion

        /// <summary>
        /// Checks for return error code from APIReturnInfo from Medtronic
        /// If there is an error, the method calls error handling method SetEmbeddedOffGroupAStimOnWhenErrorOccurs() to turn embedded off, change to group A and turn Stim ON
        /// The Error location and error descriptor from the returned API call are displayed to user in a message box.
        /// </summary>
        /// <param name="info">The APIReturnInfo value returned from the Medtronic API call</param>
        /// <param name="errorLocation">The location where the error is being check. Can be turning stim on, changing group, etc</param>
        /// <param name="showErrorMessage">True if you want a popup message on errors or false if show no error popup</param>
        /// <returns>True if there was an error or false if no error</returns>
        private bool CheckForReturnError(APIReturnInfo info, string errorLocation, bool showErrorMessage)
        {
            _log.Info("Sense Settings :: Location: " + errorLocation + ". Reject Code: " + info.RejectCode + ". Reject description: " + info.Descriptor);
            if (info.RejectCode != 0)
            {
                _log.Warn("Medtronic API return error during " + errorLocation + ". Reject code: " + info.RejectCode + ". Reject description: " + info.Descriptor);
                if (showErrorMessage)
                {
                    MessageBox.Show("Medtronic API return error during " + errorLocation + ". If sensing doesn't start then please check your sense settings and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
                return false;
            }
            return true;
        }
    
    }
}
