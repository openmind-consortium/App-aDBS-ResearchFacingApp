/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using EmbeddedAdaptiveDBSApplication.Models;
using Medtronic.NeuroStim.Olympus.DataTypes.DeviceManagement;
using Medtronic.NeuroStim.Olympus.DataTypes.Sensing;
using Medtronic.NeuroStim.Olympus.DataTypes.Therapy.Adaptive;
using Medtronic.SummitAPI.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Medtronic.SummitAPI.Events;
using Medtronic.NeuroStim.Olympus.Commands;
using Medtronic.NeuroStim.Olympus.DataTypes.PowerManagement;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using Medtronic.SummitAPI.Flash;
using System.Threading;
using System.Windows.Media;
using System.Windows.Input;
using EmbeddedAdaptiveDBSApplication.Services;
using Medtronic.NeuroStim.Olympus.DataTypes.Core;

namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    /// <summary>
    /// This class has most of the implementation from the Main tab. Most of the heavy lifting is done in this class
    /// It is also tied to the MainViewModel.cs class since both classes Bind to Main tab buttons/views and is partial to MainViewModel
    /// Includes switching stim on/off, change groups A,B,C,D, updateDBS, turnOffEmbedded, medtronic event handlers, status display for current/state/group/etc, loading config files for sense/adaptive, configure sense/adaptive
    /// </summary>
    public partial class MainViewModel : Screen
    {
        #region Variable Initializations
        //Event ID's are used to show events when writing event logs to medtonic files.
        //The id numbers are made up by programmer so researcher can easily look them up later based on value
        private static readonly string TURN_STIM_ON_EVENT_ID = "001";
        private static readonly string TURN_STIM_OFF_EVENT_ID = "002";
        private static readonly string TURN_TO_GROUP_A_EVENT_ID = "003";
        private static readonly string TURN_TO_GROUP_B_EVENT_ID = "004";
        private static readonly string TURN_TO_GROUP_C_EVENT_ID = "005";
        private static readonly string TURN_TO_GROUP_D_EVENT_ID = "006";
        private static readonly string TURN_EMBEDDED_OFF_EVENT_ID = "007";
        private static readonly string TURN_EMBEDDED_ON_EVENT_ID = "008";
        private static readonly string TURN_SENSE_ON_EVENT_ID = "009";
        private static readonly string TURN_SENSE_OFF_EVENT_ID = "010";
        private static readonly string TURN_ALIGN_EVENT_ID = "012";
        private static readonly string TURN_STIM_UP_EVENT_ID = "013";
        private static readonly string TURN_STIM_DOWN_EVENT_ID = "014";
        private static readonly string CHANGE_STIM_EVENT_ID = "015";
        private static readonly string TURN_RATE_UP_EVENT_ID = "016";
        private static readonly string TURN_RATE_DOWN_EVENT_ID = "017";
        private static readonly string CHANGE_RATE_EVENT_ID = "018";
        private static readonly string TURN_PW_UP_EVENT_ID = "019";
        private static readonly string TURN_PW_DOWN_EVENT_ID = "020";
        private static readonly string CHANGE_PW_EVENT_ID = "021";
        private static readonly string ERROR_IN_LOG_EVENT_ID = "099";
        //this is the location for the sense/adaptive config files
        private static readonly string senseFileLocation = @"C:\AdaptiveDBS\sense_config.json";
        private static readonly string adaptiveFileLocation = @"C:\AdaptiveDBS\adaptive_config.json";
        private static readonly string DISABLED = "Disabled";
        //Variables for stimulation data, adaptive config file, thread for updating dbs
        private StimulationData stimData;
        private SummitSensing summitSensing;
        private StimParameterModel stimModel = new StimParameterModel(0, 0, 0);
        private TherapyLimitsForGroupModel GroupATherapyLimits = new TherapyLimitsForGroupModel();
        private TherapyLimitsForGroupModel GroupBTherapyLimits = new TherapyLimitsForGroupModel();
        private TherapyLimitsForGroupModel GroupCTherapyLimits = new TherapyLimitsForGroupModel();
        private TherapyLimitsForGroupModel GroupDTherapyLimits = new TherapyLimitsForGroupModel();
        private string _stimRate, _stimAmp, _stimPW, _stimActive, _activeGroup, _stimState, _stimElectrode, _activeRechargeStatus;
        private int _pWLowerLimit, _pWUpperLimit;
        private double _rateLowerLimit, _rateUpperLimit, _ampLowerLimit, _ampUpperLimit;
        private static AdaptiveModel adaptiveConfig = null;
        private static Thread UpdateDBSThread;
        //This is used to check return values from medtronic api calls
        //Each return value is checked if reject code is an error. 
        //If it is an error, then error handling is done
        private APIReturnInfo bufferReturnInfo;
        //This message collection is used to show messages to user. Message boxes appear in Main tab and Report tab
        private BindableCollection<string> _message = new BindableCollection<string>();
        //Version number to show to user how many times the aDBS has been updated. 
        //This is also used to prepend to front of adaptive/sense config files that are saved in the Medtronic json file directory
        private static int versionNumber = 0;
        //y is used to plot the x axis for current/state
        private long powerAndDetectorXValue = 0;
        private double y_coordinateForCurrState = 0;
        private double accelerometerXvalue = 0;
        private int countOfX = 0;
        private int countOfY = 0;
        private int countOfZ = 0;
        private List<double> fftBins = new List<double>();
        private List<List<double>> rollingMean = new List<List<double>>();
        private int initialCountForRollingMean = 0;
        private string _stepValueInputBox, _stimChangeValueInput, _stimChangeRateInput, _stepRateValueInputBox, _stepPWInputBox, _stimChangePWInput;
        private bool _senseStreamOnEnabled = true;
        private bool _senseStreamOffEnabled = false;
        //Time Domain Variables New (unfinished) way
        private volatile ushort prevSTNTDPacketTicks;
        private volatile uint prevSTNTDPacketTimestampSeconds;
        private static double totalSTNTimeInSecondsOfLastPacket = 0.0;
        private static double tempTotalSTNTimeInSecondsOfLastPacket = 0.0;
        private volatile int dataSequenceOfLastPacket = 0;
        private volatile int numberOfDataSamplesInLastPacket = 0;
        private volatile bool isFirstSTNTDPacket = true;
        private volatile ushort prevM1TDPacketTicks;
        private volatile uint prevM1TDPacketTimestampSeconds;
        private static double totalM1TimeInSecondsOfLastPacket = 0.0;
        private volatile bool isFirstM1TDPacket = true;
        //Time domain old way variables
        private double timeDomainSTNXvalue = 0;
        private double timeDomainM1Xvalue = 0;
        #endregion

        #region Stim Therapy On-Off Button Clicks
        /// <summary>
        /// Binding button to turn Stim Therapy ON
        /// </summary>
        public async Task StimOnButtonClick()
        {
            IsSpinnerVisible = true;
            await StimOnChange();
            await Task.Run(() => UpdateStimStatusGroup(true, false, false));
            IsSpinnerVisible = false;
        }
        private async Task StimOnChange()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        //Turn stim on and update all stim displays to show new change
                        bufferReturnInfo = await Task.Run(() => theSummit.StimChangeTherapyOn());
                        if (bufferReturnInfo.RejectCodeType == typeof(MasterRejectCode)
                            && (MasterRejectCode)bufferReturnInfo.RejectCode == MasterRejectCode.ChangeTherapyPor)
                        {
                            resetPOR(theSummit);
                            bufferReturnInfo = await Task.Run(() => theSummit.StimChangeTherapyOn());
                            _log.Info("Turn stim therapy on after resetPOR success in update DBS button click");
                        }
                        Console.WriteLine("Turning Stim Therapy on: " + bufferReturnInfo.Descriptor);
                        Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy on: " + bufferReturnInfo.Descriptor);
                        //Check if api return value is not an error
                        if (CheckForReturnError(bufferReturnInfo, "Turn stim therapy on", true))
                        {
                            return;
                        }

                        //Log event that stim was turned on and check to make sure that event logging was successful
                        bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_STIM_ON_EVENT_ID + " Stim Therapy ON, " + stimModel.StimAmp + ", " + ActiveGroupDisplay, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Stim On");
                        _log.Info("Stim Turned on success");
                    }
                    catch (Exception e)
                    {
                        //If unsuccessful, display to user and run error handling
                        Messages.Insert(0, DateTime.Now + ":: ERROR: Turning stim therapy on");
                        SetEmbeddedOffGroupAStimOnWhenErrorOccurs();
                        _log.Error(e);
                    }
                }
                else
                {
                    _log.Warn("summit disposed in stim on button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in stim on button push");
            }
        }

        /// <summary>
        /// Binding button to turn Stim Therapy OFF
        /// </summary>
        public async Task StimOffButtonClick()
        {
            IsSpinnerVisible = true;
            await StimOffChange();
            await Task.Run(() => UpdateStimStatusGroup(true, false, false));
            IsSpinnerVisible = false;
        }
        private async Task StimOffChange()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        //Turn stim OFF and update all stim displays to show new change
                        //if we want RAMP then set StimChangeTherapyOff(true)
                        bufferReturnInfo = await Task.Run(() => theSummit.StimChangeTherapyOff(false));
                        Console.WriteLine("Turning Stim Therapy off: " + bufferReturnInfo.Descriptor);
                        Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy off: " + bufferReturnInfo.Descriptor);
                        //Check if api return value is not an error
                        if (CheckForReturnError(bufferReturnInfo, "Turn stim therapy off", true))
                        {
                            return;
                        }
                        //Log event that stim was turned OFF and check to make sure that event logging was successful
                        bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_STIM_OFF_EVENT_ID + " Stim Therapy OFF", DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Stim Off");
                        _log.Info("Stim off success");
                    }
                    catch (Exception e)
                    {
                        //If unsuccessful, display to user and run error handling
                        Messages.Insert(0, DateTime.Now + ":: ERROR: Turning stim therapy off");
                        SetEmbeddedOffGroupAStimOnWhenErrorOccurs();
                        _log.Error(e);
                    }
                }
                else
                {
                    _log.Warn("summit disposed in stim off button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in stim off button push");
            }
        }
        #endregion

        #region Stim Amp Change Button Clicks
        /// <summary>
        /// Decrements Stim value by the value set in Step
        /// </summary>
        public async Task DecrementStimButton()
        {
            StimSettingButtonsEnabled = false;
            IsSpinnerVisible = true;
            await DecrementStimChange();
            await Task.Run(() => UpdateStimStatusGroup(false, false, true));
            StimSettingButtonsEnabled = true;
            IsSpinnerVisible = false;
        }
        private async Task DecrementStimChange()
        {
            if (theSummit == null)
                return;
            if (theSummit.IsDisposed)
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(StepValueInputBox) || !Double.TryParse(StepValueInputBox, out double nothing))
            {
                ShowMessageBox("Step value missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(StepValueInputBox) && Double.TryParse(StepValueInputBox, out double result))
            {
                result = result * -1.0;
                try
                {
                    bufferReturnInfo = await Task.Run(() => theSummit.StimChangeStepAmp((byte)ProgramOptions.IndexOf(SelectedProgram), result, out currentValForAmp));
                    Messages.Insert(0, DateTime.Now + ":: Decrement Stim Amp: " + bufferReturnInfo.Descriptor);
                }
                catch (Exception e)
                {
                    ShowMessageBox("Error calling summit system. Please fix and try again", "Error");
                    _log.Error(e);
                    return;
                }

                if (CheckForReturnError(bufferReturnInfo, "Decrement Stim Amp", false))
                {
                    return;
                }

                try
                {
                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_STIM_DOWN_EVENT_ID + " Decrement Stim Amp: " + currentValForAmp, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                    CheckForReturnErrorInLog(bufferReturnInfo, "Decrement Stim Amp");
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
        /// <summary>
        /// Increments Stim value by the value set in Step
        /// </summary>
        public async Task IncrementStimButton()
        {
            StimSettingButtonsEnabled = false;
            IsSpinnerVisible = true;
            await IncrementStimChange();
            await Task.Run(() => UpdateStimStatusGroup(false, false, true));
            StimSettingButtonsEnabled = true;
            IsSpinnerVisible = false;
        }
        private async Task IncrementStimChange()
        {
            if (theSummit == null)
                return;
            if (theSummit.IsDisposed)
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(StepValueInputBox) || !Double.TryParse(StepValueInputBox, out double nothing))
            {
                ShowMessageBox("Step value missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(StepValueInputBox) && Double.TryParse(StepValueInputBox, out double result))
            {
                try
                {
                    bufferReturnInfo = await Task.Run(() => theSummit.StimChangeStepAmp((byte)ProgramOptions.IndexOf(SelectedProgram), result, out currentValForAmp));
                    Messages.Insert(0, DateTime.Now + ":: Increment Stim Amp: " + bufferReturnInfo.Descriptor);
                }
                catch (Exception e)
                {
                    ShowMessageBox("Error calling summit system. Please fix and try again", "Error");
                    _log.Error(e);
                    return;
                }

                if (CheckForReturnError(bufferReturnInfo, "Increment Stim Amp", false))
                {
                    return;
                }

                try
                {
                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_STIM_UP_EVENT_ID + " Increment Stim Amp: " + currentValForAmp, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                    CheckForReturnErrorInLog(bufferReturnInfo, "Increment Stim");
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
        /// <summary>
        /// Sets the stim value based on input box StimChangeValueInput
        /// </summary>
        public async Task SetStimValueInputButton()
        {
            StimSettingButtonsEnabled = false;
            IsSpinnerVisible = true;
            await SetStimChange();
            await Task.Run(() => UpdateStimStatusGroup(false, false, true));
            StimSettingButtonsEnabled = true;
            IsSpinnerVisible = false;
        }
        private async Task SetStimChange()
        {
            if (theSummit == null)
                return;
            if (theSummit.IsDisposed)
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(StimChangeValueInput) || !Double.TryParse(StimChangeValueInput, out double nothing))
            {
                ShowMessageBox("Stim value missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(StimChangeValueInput) && Double.TryParse(StimChangeValueInput, out double result))
            {
                double ampToChangeTo = 0;
                try
                {
                    ampToChangeTo = Math.Round(result + (-1 * stimModel.StimAmp), 1);
                    //set stim rate
                    if (ampToChangeTo != 0)
                    {
                        bufferReturnInfo = await Task.Run(() => theSummit.StimChangeStepAmp((byte)ProgramOptions.IndexOf(SelectedProgram), ampToChangeTo, out currentValForAmp));
                        Messages.Insert(0, DateTime.Now + ":: Change Stim Amp: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Change Stim Amp", false))
                        {
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    ShowMessageBox("Error calling summit system. Please fix and try again", "Error");
                    _log.Error(e);
                    return;
                }
                try
                {
                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, CHANGE_STIM_EVENT_ID + " Change Stim Amp: " + currentValForAmp, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                    CheckForReturnErrorInLog(bufferReturnInfo, "Change Stim");
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
        #endregion

        #region Stim rate change Button Clicks
        /// <summary>
        /// Decrements Stim value by the value set in Step
        /// </summary>
        public async Task DecrementStimRateButton()
        {
            StimSettingButtonsEnabled = false;
            IsSpinnerVisible = true;
            await DecrementStimRateChange();
            await Task.Run(() => UpdateStimStatusGroup(false, false, true));
            StimSettingButtonsEnabled = true;
            IsSpinnerVisible = false;
        }
        private async Task DecrementStimRateChange()
        {
            if (theSummit == null)
                return;
            if (theSummit.IsDisposed)
            {
                return;
            }
            Messages.Insert(0, DateTime.Now + ":: ");

            if (String.IsNullOrWhiteSpace(StepRateValueInputBox) || !Double.TryParse(StepRateValueInputBox, out double nothing))
            {
                ShowMessageBox("Step value missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(StepRateValueInputBox) && Double.TryParse(StepRateValueInputBox, out double result))
            {
                result = result * -1.0;
                try
                {
                    bufferReturnInfo = await Task.Run(() => theSummit.StimChangeStepFrequency(result, true, out currentValForRate));
                    Messages.Insert(0, DateTime.Now + ":: Decrement Rate: " + bufferReturnInfo.Descriptor);
                }
                catch (Exception e)
                {
                    ShowMessageBox("Error calling summit system. Please fix and try again", "Error");
                    _log.Error(e);
                    return;
                }

                if (CheckForReturnError(bufferReturnInfo, "Decrement Stim Rate", false))
                {
                    return;
                }
                try
                {
                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_RATE_DOWN_EVENT_ID + " Decrement Rate: " + currentValForRate, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                    CheckForReturnErrorInLog(bufferReturnInfo, "Decrement Rate");
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
        /// <summary>
        /// Increments Stim value by the value set in Step
        /// </summary>
        public async Task IncrementStimRateButton()
        {
            StimSettingButtonsEnabled = false;
            IsSpinnerVisible = true;
            await IncrementStimRateChange();
            await Task.Run(() => UpdateStimStatusGroup(false, false, true));
            StimSettingButtonsEnabled = true;
            IsSpinnerVisible = false;
        }
        private async Task IncrementStimRateChange()
        {
            if (theSummit == null)
                return;
            if (theSummit.IsDisposed)
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(StepRateValueInputBox) || !Double.TryParse(StepRateValueInputBox, out double nothing))
            {
                ShowMessageBox("Step value missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(StepRateValueInputBox) && Double.TryParse(StepRateValueInputBox, out double result))
            {
                try
                {
                    bufferReturnInfo = await Task.Run(() => theSummit.StimChangeStepFrequency(result, true, out currentValForRate));
                    Messages.Insert(0, DateTime.Now + ":: Increment Rate: " + bufferReturnInfo.Descriptor);
                }
                catch (Exception e)
                {
                    ShowMessageBox("Error calling summit system. Please fix and try again", "Error");
                    _log.Error(e);
                    return;
                }

                if (CheckForReturnError(bufferReturnInfo, "Increment Stim Rate", false))
                {
                    return;
                }
                try
                {
                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_RATE_UP_EVENT_ID + " Increment Rate: " + currentValForRate, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                    CheckForReturnErrorInLog(bufferReturnInfo, "Increment Rate");
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
        /// <summary>
        /// Sets the stim rate directly
        /// </summary>
        public async Task SetStimRateInputButton()
        {
            StimSettingButtonsEnabled = false;
            IsSpinnerVisible = true;
            await SetStimRateChange();
            await Task.Run(() => UpdateStimStatusGroup(false, false, true));
            StimSettingButtonsEnabled = true;
            IsSpinnerVisible = false;
        }
        private async Task SetStimRateChange()
        {
            if (theSummit == null)
                return;
            if (theSummit.IsDisposed)
            {
                return;
            }
            if (String.IsNullOrWhiteSpace(StimChangeRateInput) || !Double.TryParse(StimChangeRateInput, out double nothing))
            {
                ShowMessageBox("Stim value missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(StimChangeRateInput) && Double.TryParse(StimChangeRateInput, out double result))
            {
                double rateToChangeTo = 0;
                try
                {
                    rateToChangeTo = Math.Round(result + (-1 * stimModel.StimRate), 1);
                    //set stim rate
                    if (rateToChangeTo != 0)
                    {
                        bufferReturnInfo = await Task.Run(() => theSummit.StimChangeStepFrequency(rateToChangeTo, true, out currentValForRate));
                        Messages.Insert(0, DateTime.Now + ":: Change Rate: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Change Stim Rate", false))
                        {
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    ShowMessageBox("Error calling summit system. Please fix and try again", "Error");
                    _log.Error(e);
                    return;
                }
                try
                {
                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, CHANGE_RATE_EVENT_ID + " Change Rate: " + currentValForRate, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                    CheckForReturnErrorInLog(bufferReturnInfo, "Change Rate");
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
        #endregion

        #region Stim Pulse Width change Button Clicks
        /// <summary>
        /// Decrements Stim pulse width value by the value set in Step
        /// </summary>
        public async Task DecrementStimPWButton()
        {
            StimSettingButtonsEnabled = false;
            IsSpinnerVisible = true;
            await DecrementStimPWChange();
            await Task.Run(() => UpdateStimStatusGroup(false, false, true));
            StimSettingButtonsEnabled = true;
            IsSpinnerVisible = false;
        }
        private async Task DecrementStimPWChange()
        {
            if (theSummit == null)
                return;
            if (theSummit.IsDisposed)
            {
                return;
            }
            int result = 0;
            if (String.IsNullOrWhiteSpace(StepPWInputBox) || !Int32.TryParse(StepPWInputBox, out int nothing))
            {
                ShowMessageBox("Step value for Pulse Width missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(StepPWInputBox) && Int32.TryParse(StepPWInputBox, out result))
            {
                result = result * -1;
                try
                {
                    bufferReturnInfo = await Task.Run(() => theSummit.StimChangeStepPW((byte)ProgramOptions.IndexOf(SelectedProgram), result, out currentValueForPW));
                    Messages.Insert(0, DateTime.Now + ":: Decrement Pulse Width: " + bufferReturnInfo.Descriptor);
                }
                catch (Exception e)
                {
                    ShowMessageBox("Error calling summit system. Please fix and try again", "Error");
                    _log.Error(e);
                    return;
                }

                if (CheckForReturnError(bufferReturnInfo, "Decrement Stim Pulse Width", false))
                {
                    return;
                }
                try
                {
                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_PW_DOWN_EVENT_ID + " Decrement Pulse Width: " + currentValueForPW, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                    CheckForReturnErrorInLog(bufferReturnInfo, "Decrement Pulse Width");
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
        /// <summary>
        /// Increments Stim Pulse Width value by the value set in Step
        /// </summary>
        public async Task IncrementStimPWButton()
        {
            StimSettingButtonsEnabled = false;
            IsSpinnerVisible = true;
            await IncrementStimPWChange();
            await Task.Run(() => UpdateStimStatusGroup(false, false, true));
            StimSettingButtonsEnabled = true;
            IsSpinnerVisible = false;
        }
        private async Task IncrementStimPWChange()
        {
            if (theSummit == null)
                return;
            if (theSummit.IsDisposed)
            {
                return;
            }
            int result = 0;
            if (String.IsNullOrWhiteSpace(StepPWInputBox) || !Int32.TryParse(StepPWInputBox, out int nothing))
            {
                ShowMessageBox("Step value missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(StepPWInputBox) && Int32.TryParse(StepPWInputBox, out result))
            {
                try
                {
                    bufferReturnInfo = await Task.Run(() => theSummit.StimChangeStepPW((byte)ProgramOptions.IndexOf(SelectedProgram), result, out currentValueForPW));
                    Messages.Insert(0, DateTime.Now + ":: Increment Pulse Width: " + bufferReturnInfo.Descriptor);
                }
                catch (Exception e)
                {
                    ShowMessageBox("Error calling summit system. Please fix and try again", "Error");
                    _log.Error(e);
                    return;
                }

                if (CheckForReturnError(bufferReturnInfo, "Increment Stim Pulse Width", false))
                {
                    return;
                }
                try
                {
                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_PW_UP_EVENT_ID + " Increment Pulse Width: " + currentValueForPW, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                    CheckForReturnErrorInLog(bufferReturnInfo, "Increment Pulse Width");
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
        /// <summary>
        /// Sets the stim pulse width directly
        /// </summary>
        public async Task SetStimPWInputButton()
        {
            StimSettingButtonsEnabled = false;
            IsSpinnerVisible = true;
            await SetStimPWChange();
            await Task.Run(() => UpdateStimStatusGroup(false, false, true));
            StimSettingButtonsEnabled = true;
            IsSpinnerVisible = false;
        }
        private async Task SetStimPWChange()
        {
            if (theSummit == null)
                return;
            if (theSummit.IsDisposed)
            {
                return;
            }
            int result = 0;
            if (String.IsNullOrWhiteSpace(StimChangePWInput) || !Int32.TryParse(StimChangePWInput, out int nothing))
            {
                ShowMessageBox("Stim value missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(StimChangePWInput) && Int32.TryParse(StimChangePWInput, out result))
            {
                int pulseWidthToChangeTo = 0;
                try
                {
                    pulseWidthToChangeTo = result + (-1 * stimModel.PulseWidth);
                    //set stim rate
                    if (pulseWidthToChangeTo != 0)
                    {
                        bufferReturnInfo = await Task.Run(() => theSummit.StimChangeStepPW(0, pulseWidthToChangeTo, out currentValueForPW));
                        Messages.Insert(0, DateTime.Now + ":: Change Pulse Width: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Change Stim Pulse Width", false))
                        {
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    ShowMessageBox("Error calling summit system. Please fix and try again", "Error");
                    _log.Error(e);
                    return;
                }
                try
                {
                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, CHANGE_PW_EVENT_ID + " Change Pulse Width: " + currentValueForPW, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                    CheckForReturnErrorInLog(bufferReturnInfo, "Change Pulse Width");
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }

        #endregion

        #region Change Groups Button Clicks
        /// <summary>
        /// Binding button to switch to Group A
        /// </summary>
        public async Task GroupAButtonClick()
        {
            summitSensing.StopStreaming(theSummit, false);
            IsSpinnerVisible = true;
            await GroupAButtonChange();
            await Task.Run(() => UpdateStimStatusGroup(false, true, true));
            IsSpinnerVisible = false;
            summitSensing.StartStreaming(theSummit, senseConfig, false);
        }

        private async Task GroupAButtonChange()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    if (ActiveGroupDisplay.Equals("Group D"))
                    {
                        try
                        {
                            SensingState state;
                            theSummit.ReadSensingState(out state);
                            if (state.State.ToString().Contains("DetectionLd0") || state.State.ToString().Contains("DetectionLd1"))
                            {
                                bufferReturnInfo = await Task.Run(() => theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_EMBEDDED_OFF_EVENT_ID + " Turn Embedded off. Number: " + versionNumber, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt")));
                                CheckForReturnErrorInLog(bufferReturnInfo, "Embedded Therapy OFF");
                            }
                        }
                        catch (Exception e)
                        {
                            Messages.Insert(0, DateTime.Now + ":: ERROR: Could not determine if detection is on to log custom event.");
                            _log.Error(e);
                        }
                    }

                    try
                    {
                        //Turn Group to A and update all stim displays to show new change
                        bufferReturnInfo = await Task.Run(() => theSummit.StimChangeActiveGroup(ActiveGroup.Group0));
                        Console.WriteLine("Change to Group A: " + bufferReturnInfo.Descriptor);
                        Messages.Insert(0, DateTime.Now + ":: Change to Group A: " + bufferReturnInfo.Descriptor);
                        //Check if api return value is not an error
                        if (CheckForReturnError(bufferReturnInfo, "Group A", false))
                            return;
                        //Log event that we switched to Group A and check to make sure that event logging was successful
                        bufferReturnInfo = await Task.Run(() => theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_TO_GROUP_A_EVENT_ID + " Change group to A", DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt")));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Group A");
                        _log.Info("Group A switch success");
                        //Wait to let INS change
                        Thread.Sleep(300);
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: ERROR: Changing to group A");
                        _log.Error(e);
                    }
                }
                else
                {
                    _log.Warn("summit disposed in group A button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in group A button push");
            }
        }

        /// <summary>
        /// Binding button to switch to Group B
        /// </summary>
        public async Task GroupBButtonClick()
        {
            summitSensing.StopStreaming(theSummit, false);
            IsSpinnerVisible = true;
            await GroupBButtonChange();
            await Task.Run(() => UpdateStimStatusGroup(false, true, true));
            IsSpinnerVisible = false;
            summitSensing.StartStreaming(theSummit, senseConfig, false);
        }

        private async Task GroupBButtonChange()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    if (ActiveGroupDisplay.Equals("Group D"))
                    {
                        try
                        {
                            SensingState state;
                            theSummit.ReadSensingState(out state);
                            if (state.State.ToString().Contains("DetectionLd0") || state.State.ToString().Contains("DetectionLd1"))
                            {
                                bufferReturnInfo = await Task.Run(() => theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_EMBEDDED_OFF_EVENT_ID + " Turn Embedded off. Number: " + versionNumber, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt")));
                                CheckForReturnErrorInLog(bufferReturnInfo, "Embedded Therapy OFF");
                            }
                        }
                        catch (Exception e)
                        {
                            Messages.Insert(0, DateTime.Now + ":: ERROR: Could not determine if detection is on to log custom event.");
                            _log.Error(e);
                        }
                    }

                    try
                    {
                        //Turn Group to B and update all stim displays to show new change
                        bufferReturnInfo = await Task.Run(() => theSummit.StimChangeActiveGroup(ActiveGroup.Group1));
                        Console.WriteLine("Change to Group B: " + bufferReturnInfo.Descriptor);
                        Messages.Insert(0, DateTime.Now + ":: Change to Group B: " + bufferReturnInfo.Descriptor);
                        //Check if api return value is not an error
                        if (CheckForReturnError(bufferReturnInfo, "Group B", false))
                            return;
                        //Log event that we switched to Group B and check to make sure that event logging was successful
                        bufferReturnInfo = await Task.Run(() => theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_TO_GROUP_B_EVENT_ID + " Change group to B", DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt")));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Group B");
                        _log.Info("Group B switch success");
                        //Wait to let INS change
                        Thread.Sleep(300);
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: ERROR: Changing to group B");
                        _log.Error(e);
                    }
                }
                else
                {
                    _log.Warn("summit disposed in group B button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in group B button push");
            }
        }

        /// <summary>
        /// Binding button to switch to Group C
        /// </summary>
        public async Task GroupCButtonClick()
        {
            summitSensing.StopStreaming(theSummit, false);
            IsSpinnerVisible = true;
            await GroupCButtonChange();
            await Task.Run(() => UpdateStimStatusGroup(false, true, true));
            IsSpinnerVisible = false;
            summitSensing.StartStreaming(theSummit, senseConfig, false);
        }

        private async Task GroupCButtonChange()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    if (ActiveGroupDisplay.Equals("Group D"))
                    {
                        try
                        {
                            SensingState state;
                            theSummit.ReadSensingState(out state);
                            if (state.State.ToString().Contains("DetectionLd0") || state.State.ToString().Contains("DetectionLd1"))
                            {
                                bufferReturnInfo = await Task.Run(() => theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_EMBEDDED_OFF_EVENT_ID + " Turn Embedded off. Number: " + versionNumber, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt")));
                                CheckForReturnErrorInLog(bufferReturnInfo, "Embedded Therapy OFF");
                            }
                        }
                        catch (Exception e)
                        {
                            Messages.Insert(0, DateTime.Now + ":: ERROR: Could not determine if detection is on to log custom event.");
                            _log.Error(e);
                        }
                    }

                    try
                    {
                        //Turn Group to C and update all stim displays to show new change
                        bufferReturnInfo = await Task.Run(() => theSummit.StimChangeActiveGroup(ActiveGroup.Group2));
                        Console.WriteLine("Change to Group C: " + bufferReturnInfo.Descriptor);
                        Messages.Insert(0, DateTime.Now + ":: Change to Group C: " + bufferReturnInfo.Descriptor);
                        //Check if api return value is not an error
                        if (CheckForReturnError(bufferReturnInfo, "Group C", false))
                            return;
                        //Log event that we switched to Group C and check to make sure that event logging was successful
                        bufferReturnInfo = await Task.Run(() => theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_TO_GROUP_A_EVENT_ID + " Change group to C", DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt")));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Group C");
                        _log.Info("Group C switch success");
                        //Wait to let INS change
                        Thread.Sleep(300);
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: ERROR: Changing to group C");
                        _log.Error(e);
                    }
                }
                else
                {
                    _log.Warn("summit disposed in group C button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in group C button push");
            }
        }

        /// <summary>
        /// Binding button to switch to Group D
        /// </summary>
        public async Task GroupDButtonClick()
        {
            summitSensing.StopStreaming(theSummit, false);
            IsSpinnerVisible = true;
            await GroupDButtonChange();
            await Task.Run(() => UpdateStimStatusGroup(false, true, true));
            IsSpinnerVisible = false;
            summitSensing.StartStreaming(theSummit, senseConfig, false);
        }

        private async Task GroupDButtonChange()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        SensingState state;
                        theSummit.ReadSensingState(out state);
                        if (state.State.ToString().Contains("DetectionLd0") || state.State.ToString().Contains("DetectionLd1"))
                        {
                            bufferReturnInfo = await Task.Run(() => theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_EMBEDDED_ON_EVENT_ID + " Turn Embedded on. Number: " + versionNumber, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt")));
                            CheckForReturnErrorInLog(bufferReturnInfo, "Turn Embedded on");
                        }
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: ERROR: Could not determine if detection is on to log custom event.");
                        _log.Error(e);
                    }

                    try
                    {
                        //Turn Group to D and update all stim displays to show new change
                        bufferReturnInfo = await Task.Run(() => theSummit.StimChangeActiveGroup(ActiveGroup.Group3));
                        Console.WriteLine("Change to Group D: " + bufferReturnInfo.Descriptor);
                        Messages.Insert(0, DateTime.Now + ":: Change to Group D: " + bufferReturnInfo.Descriptor);
                        //Check if api return value is not an error
                        if (CheckForReturnError(bufferReturnInfo, "Group D", false))
                            return;
                        //Log event that we switched to Group D and check to make sure that event logging was successful
                        bufferReturnInfo = await Task.Run(() => theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_TO_GROUP_D_EVENT_ID + " Change group to D", DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt")));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Group D");
                        _log.Info("Group D switch success");
                        Thread.Sleep(1000);
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: ERROR: Changing to group D");
                        _log.Error(e);
                    }
                }
                else
                {
                    _log.Warn("summit disposed in group D button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in group D button push");
            }
        }
        #endregion

        #region Sense On Off Buttons
        /// <summary>
        /// Turn sensing on or off
        /// </summary>
        public async Task SenseStreamOnButton()
        {
            IsSpinnerVisible = true;
            await Task.Run(() => SenseOnChange());
            IsSpinnerVisible = false;
        }

        private void SenseOnChange()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        SensingState state;
                        int counterForLoop = 5;
                        do
                        {
                            theSummit.ReadSensingState(out state);
                            counterForLoop--;
                        } while (state == null && counterForLoop > 0);
                        if (state == null || counterForLoop == 0)
                        {
                            _log.Warn("Could not start sensing. State api call could not be made.");
                            MessageBox.Show("Could not start sensing. Please try again");
                            return;
                        }

                        initialCountForRollingMean = 0;
                        rollingMean.Clear();
                        senseConfig = jSONService.GetSenseModelFromFile(senseFileLocation);
                        if (senseConfig == null)
                        {
                            _log.Warn("Could not start sensing. sense config is null.");
                            return;
                        }
                        if (!CheckPacketLoss(senseConfig))
                        {
                            _log.Warn("Could not start sensing. Packet loss too great");
                            Messages.Insert(0, DateTime.Now + ":: ERROR - Please check sense config file.  Either Packet Loss is over the maximum level or the config file needs correcting.");
                            return;
                        }
                        if (state.State.ToString().Contains("DetectionLd0") || state.State.ToString().Contains("DetectionLd1"))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    MessageBox.Show(Application.Current.MainWindow, "Detection is enabled: Sense settings cannot be configured while detection is enabled.  If you would like to configure sense, please click 'aDBS Off' button and retry turning sense on. \n\nStream is being turned on with prior aDBS sense settings...", "Critical Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                                catch(Exception e)
                                {
                                    MessageBox.Show("Detection is enabled: Sense settings cannot be configured while detection is enabled.  If you would like to configure sense, please click 'aDBS Off' button and retry turning sense on. \n\nStream is being turned on with prior aDBS sense settings...", "Critical Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    _log.Warn("MessageBox.Show crashed while trying to let user know about detection being on when turning sense on");
                                    _log.Error(e);
                                }
                            });
                            _log.Info("Detection enabled. Start streaming without configuring/starting sense");
                            if (!summitSensing.StartStreaming(theSummit, senseConfig, true))
                            {
                                _log.Warn("Could not start streaming while turning sensing button on. Adaptive currently on");
                                Messages.Insert(0, DateTime.Now + ":: Could not start streaming while turning sensing button on");
                                return;
                            }
                            else
                            {
                                theSummit.DataReceivedDetectorHandler += TheSummit_dataReceivedDetector;
                            }
                        }
                        else
                        {
                            _log.Info("Detection not enabled. Configure/start sense/stream");
                            if (!summitSensing.StopSensing(theSummit, true))
                            {
                                _log.Warn("Could not stop sensing while turning sensing button on");
                                Messages.Insert(0, DateTime.Now + ":: Could not stop sensing while turning sensing button on");
                                return;
                            }
                            if (!summitSensing.SummitConfigureSensing(theSummit, senseConfig, true))
                            {
                                _log.Warn("Could not configure sensing while turning sensing button on");
                                Messages.Insert(0, DateTime.Now + ":: Could not configure sensing while turning sensing button on");
                                return;
                            }
                            if (!summitSensing.StartSensing(theSummit, senseConfig, true))
                            {
                                _log.Warn("Could not start sensing while turning sensing button on");
                                Messages.Insert(0, DateTime.Now + ":: Could not start sensing while turning sensing button on. Please check that LD0/LD1 is false in config file if not running adaptive.");
                                return;
                            }
                            if (!summitSensing.StartStreaming(theSummit, senseConfig, true))
                            {
                                _log.Warn("Could not start streaming while turning sensing button on");
                                Messages.Insert(0, DateTime.Now + ":: Could not start streaming while turning sensing button on");
                                return;
                            }
                        }
                        Messages.Insert(0, DateTime.Now + ":: Started sensing and streaming");

                        SenseStreamOffEnabled = true;
                        SenseStreamOnEnabled = false;

                        //Calculate fft bins for fft x values
                        CalculatePowerBins calculatePowerBins = new CalculatePowerBins(_log);
                        fftBins = calculatePowerBins.CalculateFFTBins(ConfigConversions.FftSizesConvert(senseConfig.Sense.FFT.FftSize), ConfigConversions.TDSampleRateConvert(senseConfig.Sense.TDSampleRate));
                        if (senseConfig.Sense.FFT.StreamSizeBins != 0)
                        {
                            CalculateNewFFTBins();
                        }

                        lowerPowerBinActualValues = calculatePowerBins.GetLowerPowerBinActualValues(senseConfig);
                        upperPowerBinActualValues = calculatePowerBins.GetUpperPowerBinActualValues(senseConfig);
                        //Calculate FFT overlap
                        FFTOverlapDisplay = CalculateFFTOverlap(senseConfig);
                        //Calculate FFT Time
                        FFTTimeDisplay = CalculateFFTTime(senseConfig);
                        //Display current FFT Channel
                        FFTCurrentChannel = "Ch. " + senseConfig.Sense.FFT.Channel.ToString();
                        //StartEventHandlers(); only start acc, td and power
                        theSummit.DataReceivedTDHandler += TheSummit_DataReceivedTDHandler;
                        theSummit.DataReceivedFFTHandler += TheSummit_DataReceivedFFTHandler;
                        theSummit.DataReceivedPowerHandler += TheSummit_PowerReceivedHandler;
                        theSummit.DataReceivedAccelHandler += TheSummit_DataReceivedAccelHandler;
                        //This sets the Channel options in drop down menu in visualization screen.
                        SetPowerChannelOptionsInDropDownMenu();
                        SetSTNTimeDomainChannelOptionsInDropDownMenu(senseConfig);
                        SetM1TimeDomainChannelOptionsInDropDownMenu(senseConfig);
                        _log.Info("Turn sensing on button success");

                        //Log event that sense was turned on and check to make sure that event logging was successful
                        bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_SENSE_ON_EVENT_ID + " Sense On, " + ActiveGroupDisplay, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Sense On");
                    }
                    catch (Exception e)
                    {
                        SenseStreamOffEnabled = false;
                        SenseStreamOnEnabled = true;
                        Messages.Insert(0, DateTime.Now + ":: Could not start sensing");
                        _log.Error(e);
                    }

                }
                else
                {
                    _log.Warn("summit disposed in sense on-off button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in sense on-off button push");
            }
        }
        /// <summary>
        /// Turns sense and stream off
        /// </summary>
        public async Task SenseStreamOffButton()
        {
            IsSpinnerVisible = true;
            await Task.Run(() => SenseOffChange());
            IsSpinnerVisible = false;
        }

        private void SenseOffChange()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        SensingState state;
                        int counterForLoop = 5;
                        do
                        {
                            theSummit.ReadSensingState(out state);
                            counterForLoop--;
                        } while (state == null && counterForLoop > 0);
                        if (state == null || counterForLoop == 0)
                        {
                            MessageBox.Show("Could not stop sensing. Please try again");
                            return;
                        }

                        if (state.State.ToString().Contains("DetectionLd0") || state.State.ToString().Contains("DetectionLd1"))
                        {
                            _log.Info("Detection enabled. Stop streaming only");
                            if (!summitSensing.StopStreaming(theSummit, true))
                            {
                                _log.Warn("Could not stop streaming while turning sensing button off");
                                return;
                            }
                            else
                            {
                                Messages.Insert(0, DateTime.Now + ":: Stopped streaming. Adaptive Therapy is on and did not stop sensing. If you wish to stop sensing, click aDBS off button first");
                            }
                        }
                        else
                        {
                            _log.Info("Detection not enabled. Stop streaming and sensing");
                            if (!summitSensing.StopSensing(theSummit, true))
                            {
                                _log.Warn("Could not stop sensing while turning sensing button off");
                                return;
                            }
                            else
                            {
                                Messages.Insert(0, DateTime.Now + ":: Stopped sensing and streaming");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: Error stopping sensing");
                        _log.Error(e);
                    }

                    SenseStreamOffEnabled = false;
                    SenseStreamOnEnabled = true;
                    UnRegisterEventHandlers();
                    try
                    {
                        //Log event that sense was turned off and check to make sure that event logging was successful
                        bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_SENSE_OFF_EVENT_ID + " Sense off, " + ActiveGroupDisplay, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Sense Off");

                        _log.Info("Turn sensing off button success");
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                        _log.Error(e);
                    }
                }
                else
                {
                    _log.Warn("summit disposed in stream off button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in stream off button push");
            }
        }
        #endregion

        #region Turn Embedded ON/OFF Button Clicks
        /// <summary>
        /// Bindable button click for Turning embedded Therapy off
        /// </summary>
        public void TurnOffEmbedded()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        UnRegisterEventHandlers();
                        //Turn stim therapy off. Must be off to turn embedded off
                        bufferReturnInfo = theSummit.StimChangeTherapyOff(false);
                        Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy OFF: " + bufferReturnInfo.Descriptor);
                        //Check if api return value is not an error
                        if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy off", false))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        Thread.Sleep(300);
                        _log.Info("Turn stim off success while turning embedded off button click");
                        //Turn adaptive embedded therapy off
                        bufferReturnInfo = theSummit.WriteAdaptiveMode(AdaptiveTherapyModes.Disabled);
                        Messages.Insert(0, DateTime.Now + ":: Turning Therapy Mode to disabled: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Disabling Embedded Therapy Mode", false))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Turn embedded off success while turning embedded off button click");
                        // Turn on Stim Therapy
                        bufferReturnInfo = theSummit.StimChangeTherapyOn();
                        Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy ON: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy On", false))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Turn stim on success while turning embedded off button click");
                        //Log event that Embedded has been turned off
                        bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_EMBEDDED_OFF_EVENT_ID + " Turn Embedded Therapy OFF. Number: " + versionNumber, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Embedded Therapy OFF");
                        // Reset POR if set.
                        if (bufferReturnInfo.RejectCodeType == typeof(MasterRejectCode)
                            && (MasterRejectCode)bufferReturnInfo.RejectCode == MasterRejectCode.ChangeTherapyPor)
                        {
                            resetPOR(theSummit);
                            bufferReturnInfo = theSummit.StimChangeTherapyOn();
                            _log.Info("Turn stim on success after reset POR while turning embedded off button click");
                        }
                        bufferReturnInfo = theSummit.WriteSensingState(SenseStates.None, 0x00);
                        Messages.Insert(0, DateTime.Now + ":: Turning off sensing: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Turn off Sensing", false)) { }
                        if (bufferReturnInfo.RejectCode == 0)
                        {
                            SenseStreamOffEnabled = false;
                            SenseStreamOnEnabled = true;
                        }

                        Thread.Sleep(300);
                        UpdateStimStatusGroup(true, true, true);
                        Messages.Insert(0, DateTime.Now + ":: Success turning embedded therapy OFF");
                        _log.Info("success while turning embedded off button click");
                    }
                    catch (Exception e)
                    {
                        UpdateStimStatusGroup(true, true, true);
                        //If unsuccessful, display to user and run error handling
                        Messages.Insert(0, DateTime.Now + ":: ERROR: Turning off embedded--");
                        SetEmbeddedOffGroupAStimOnWhenErrorOccurs();
                        _log.Error(e);
                    }
                }
                else
                {
                    _log.Warn("summit disposed in embedded off button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in embedded off button push");
            }
        }

        /// <summary>
        /// Bindable button click for Turning embedded Therapy ON
        /// </summary>
        public void UpdateDBSButtonClick()
        {
            //Load adaptive and sense config files
            adaptiveConfig = jSONService.GetAdaptiveModelFromFile(adaptiveFileLocation);
            if (adaptiveConfig == null)
            {
                return;
            }
            senseConfig = jSONService.GetSenseModelFromFile(senseFileLocation);
            if (senseConfig == null)
            {
                return;
            }
            if (!CheckPacketLoss(senseConfig))
            {
                Messages.Insert(0, DateTime.Now + ":: ERROR - Please check sense config file.  Either Packet Loss is over the maximum level or the config file needs correcting.");
                return;
            }
            //Start updateDBS thread so that UI doesn't freeze while updating.
            UpdateDBSThread = new Thread(new ThreadStart(UpdateDBSThreadCode));
            UpdateDBSThread.IsBackground = true;
            UpdateDBSThread.Start();
        }
        #endregion

        #region Medtronic Event Handlers
        //Time Domain Event Handler
        //Time domain new (unfinished) way of calculating x values
        /*private void TheSummit_DataReceivedTDHandler(object sender, SensingEventTD newData)
        {
            int channelValueForSTN = 0;
            int channelValueForM1 = 2;
            //Find the channel for each time domain channel for M1 and STN from the selected channel in the drop down menu for each
            //STN is either 0 or 1
            //M1 is either 2 or 3
            try
            {
                if (SelectedTimeDomainSTN != null)
                {
                    if (SelectedTimeDomainSTN.Equals(TimeDomainSTNDropDown[0]))
                    {
                        channelValueForSTN = 0;
                    }
                    else if (SelectedTimeDomainSTN.Equals(TimeDomainSTNDropDown[1]))
                    {
                        channelValueForSTN = 1;
                    }
                }
                if (SelectedTimeDomainM1 != null)
                {
                    if (SelectedTimeDomainM1.Equals(TimeDomainM1DropDown[0]))
                    {
                        channelValueForM1 = 2;
                    }
                    else if (SelectedTimeDomainM1.Equals(TimeDomainM1DropDown[1]))
                    {
                        channelValueForM1 = 3;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            try
            {
                if (TimeDomainSTNDropDown[0] != DISABLED || TimeDomainSTNDropDown[1] != DISABLED)
                {
                    //Add STN data into chart
                    List<double> stnDataList;
                    List<double> newSTNTdDataX = new List<double>();
                    if (newData.ChannelSamples.TryGetValue((SenseTimeDomainChannel)channelValueForSTN, out stnDataList))
                    {
                        List<double> newTdData1 = new List<double>(stnDataList.ToArray());
                        newTdData1.Reverse();
                        double timeSpace = 1.0 / (double)senseConfig.Sense.TDSampleRate;
                        if (isFirstSTNTDPacket)
                        {
                            newSTNTdDataX.Add(totalSTNTimeInSecondsOfLastPacket);
                            for (int index = 0; index < newTdData1.Count() - 1; index++)
                            {
                                totalSTNTimeInSecondsOfLastPacket += timeSpace;
                                tempTotalSTNTimeInSecondsOfLastPacket += timeSpace;
                                newSTNTdDataX.Add(totalSTNTimeInSecondsOfLastPacket);
                            }
                            isFirstSTNTDPacket = false;
                        }
                        else
                        {
                            if ((newData.Header.Timestamp.Seconds - prevSTNTDPacketTimestampSeconds) < 6)
                            {
                                double minTimeForNextPacket = 0.0;
                                ushort newPacketTicksTotalWithTimeGap = 0;
                                double newPacketSecondsTotalWithTimeGap = 0.0;
                                double newPacketSecondsTotalWithoutTimeGap = 0.0;
                                double timeLossBetweenNewOldPacketsInSeconds = 0.0;
                                if (newData.Header.SystemTick < prevSTNTDPacketTicks)
                                {
                                    newPacketTicksTotalWithTimeGap = (ushort)(65536 - prevSTNTDPacketTicks + newData.Header.SystemTick);
                                }
                                else if (newData.Header.SystemTick > prevSTNTDPacketTicks)
                                {
                                    newPacketTicksTotalWithTimeGap = (ushort)(newData.Header.SystemTick - prevSTNTDPacketTicks);
                                }
                                else
                                {
                                    return;
                                }
                                minTimeForNextPacket = Math.Floor(((1.0 / (double)senseConfig.Sense.TDSampleRate) + (newTdData1.Count() / (double)senseConfig.Sense.TDSampleRate)) * 100.0) /100.0;
                                newPacketSecondsTotalWithTimeGap = Math.Round((double)newPacketTicksTotalWithTimeGap / 10000.0, 2);
                                Console.WriteLine("newPacketSecondsTotalWithTimeGap: " + newPacketSecondsTotalWithTimeGap);
                                Console.WriteLine("minTimeForNextPacket: " + minTimeForNextPacket);
                                if (newPacketSecondsTotalWithTimeGap > minTimeForNextPacket)
                                {
                                    //Whatever is wrong is right here where we add the packet loss.  It subtracts instead of add or something
                                    newPacketSecondsTotalWithoutTimeGap = newTdData1.Count() / (double)senseConfig.Sense.TDSampleRate;
                                    timeLossBetweenNewOldPacketsInSeconds = newPacketSecondsTotalWithTimeGap - newPacketSecondsTotalWithoutTimeGap;
                                    //I changed it so that it isn't subtracting out the minTimeForNextpacket
                                    totalSTNTimeInSecondsOfLastPacket = Math.Round((totalSTNTimeInSecondsOfLastPacket + timeLossBetweenNewOldPacketsInSeconds), 4);
                                    if (totalSTNTimeInSecondsOfLastPacket <= tempTotalSTNTimeInSecondsOfLastPacket)
                                    {
                                        bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "PacketLoss: timespace: " + timeSpace + ". TimeStamp seconds new packet: " + newData.Header.Timestamp.Seconds + ". TimeStamp seconds last packet: " + prevSTNTDPacketTimestampSeconds + ". Previous X value in Seconds: " + tempTotalSTNTimeInSecondsOfLastPacket + ". Current X value in seconds" + totalSTNTimeInSecondsOfLastPacket, "SystemTicks new packet: " + newData.Header.SystemTick + ". SystemTicks last packet: " + prevSTNTDPacketTicks + ". DataTypeSequence New Packet: " + newData.Header.DataTypeSequence + ". DataTypeSequence Old Packet: " + dataSequenceOfLastPacket + ". Samples New Packet: " + newTdData1.Count() + ". Samples Old Packet: " + numberOfDataSamplesInLastPacket + ". newPacketSecondsTotalWithTimeGap: " + newPacketSecondsTotalWithTimeGap + ". minTimeForNextPacket: " + minTimeForNextPacket + ". timeLossBetweenNewOldPacketsInSeconds: " + timeLossBetweenNewOldPacketsInSeconds);
                                        tempTotalSTNTimeInSecondsOfLastPacket = totalSTNTimeInSecondsOfLastPacket;
                                    }
                                    else
                                    {
                                        tempTotalSTNTimeInSecondsOfLastPacket = Math.Round((totalSTNTimeInSecondsOfLastPacket + timeLossBetweenNewOldPacketsInSeconds), 4);

                                    }
                                    //Add STN time domain channel list data to chart
                                    try
                                    {
                                        _timeDomainSTNChart.Append(totalSTNTimeInSecondsOfLastPacket, 0);
                                    }
                                    catch (Exception e)
                                    {
                                        _log.Error(e);
                                    }
                                    //newSTNTdDataX.Add(totalSTNTimeInSecondsOfLastPacket);
                                    Console.WriteLine("Packet Loss: " + totalSTNTimeInSecondsOfLastPacket);
                                    for (int index = 0; index < newTdData1.Count(); index++)
                                    {
                                        totalSTNTimeInSecondsOfLastPacket += timeSpace;
                                        if(totalSTNTimeInSecondsOfLastPacket <= tempTotalSTNTimeInSecondsOfLastPacket)
                                        {
                                            bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "PacketLoss adding time: timespace: " + timeSpace + ". TimeStamp seconds new packet: " + newData.Header.Timestamp.Seconds + ". TimeStamp seconds last packet: " + prevSTNTDPacketTimestampSeconds + ". Previous X value in Seconds: " + tempTotalSTNTimeInSecondsOfLastPacket + ". Current X value in seconds" + totalSTNTimeInSecondsOfLastPacket, "SystemTicks new packet: " + newData.Header.SystemTick + ". SystemTicks last packet: " + prevSTNTDPacketTicks + ". DataTypeSequence New Packet: " + newData.Header.DataTypeSequence + ". DataTypeSequence Old Packet: " + dataSequenceOfLastPacket + ". Samples New Packet: " + newTdData1.Count() + ". Samples Old Packet: " + numberOfDataSamplesInLastPacket + ". newPacketSecondsTotalWithTimeGap: " + newPacketSecondsTotalWithTimeGap + ". minTimeForNextPacket: " + minTimeForNextPacket + ". timeLossBetweenNewOldPacketsInSeconds: " + timeLossBetweenNewOldPacketsInSeconds);
                                            tempTotalSTNTimeInSecondsOfLastPacket = totalSTNTimeInSecondsOfLastPacket;
                                        }
                                        else
                                        {
                                            tempTotalSTNTimeInSecondsOfLastPacket += timeSpace;

                                        }
                                        newSTNTdDataX.Add(totalSTNTimeInSecondsOfLastPacket);
                                        Console.WriteLine("Packet Loss Added timeSpace: " + totalSTNTimeInSecondsOfLastPacket);
                                    }
                                }
                                else if (newPacketSecondsTotalWithTimeGap == minTimeForNextPacket)
                                {
                                    for (int index = 0; index < newTdData1.Count(); index++)
                                    {
                                        totalSTNTimeInSecondsOfLastPacket += timeSpace;
                                        if (totalSTNTimeInSecondsOfLastPacket <= tempTotalSTNTimeInSecondsOfLastPacket)
                                        {
                                            bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "No Packet Loss: timespace: " + timeSpace + ".  TimeStamp seconds new packet: " + newData.Header.Timestamp.Seconds + ". TimeStamp seconds last packet: " + prevSTNTDPacketTimestampSeconds + ". Previous X value in Seconds: " + tempTotalSTNTimeInSecondsOfLastPacket + ". Current X value in seconds" + totalSTNTimeInSecondsOfLastPacket, "SystemTicks new packet: " + newData.Header.SystemTick + ". SystemTicks last packet: " + prevSTNTDPacketTicks + ". DataTypeSequence New Packet: " + newData.Header.DataTypeSequence + ". DataTypeSequence Old Packet: " + dataSequenceOfLastPacket + ". Samples New Packet: " + newTdData1.Count() + ". Samples Old Packet: " + numberOfDataSamplesInLastPacket);
                                            tempTotalSTNTimeInSecondsOfLastPacket = totalSTNTimeInSecondsOfLastPacket;
                                        }
                                        else
                                        {
                                            tempTotalSTNTimeInSecondsOfLastPacket += timeSpace;

                                        }
                                        newSTNTdDataX.Add(totalSTNTimeInSecondsOfLastPacket);
                                        Console.WriteLine("Added timeSpace: " + totalSTNTimeInSecondsOfLastPacket);
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        prevSTNTDPacketTicks = newData.Header.SystemTick;
                        prevSTNTDPacketTimestampSeconds = newData.Header.Timestamp.Seconds;
                        dataSequenceOfLastPacket = newData.Header.DataTypeSequence;
                        numberOfDataSamplesInLastPacket = newTdData1.Count();

                        //Add STN time domain channel list data to chart
                        try
                        {
                            _timeDomainSTNChart.Append(newSTNTdDataX, newTdData1.ToArray());
                        }
                        catch (Exception e)
                        {
                            _log.Error(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            try
            {
                if (TimeDomainM1DropDown[0] != DISABLED || TimeDomainM1DropDown[1] != DISABLED)
                {
                    //add M1 data into chart
                    List<double> m1DataList;
                    List<double> newM1TdDataX = new List<double>();
                    if (newData.ChannelSamples.TryGetValue((SenseTimeDomainChannel)channelValueForM1, out m1DataList))
                    {
                        List<double> newTdData1 = new List<double>(m1DataList.ToArray());
                        newTdData1.Reverse();

                        double timeSpace = 1.0 / (double)senseConfig.Sense.TDSampleRate;
                        if (isFirstM1TDPacket)
                        {
                            newM1TdDataX.Add(totalM1TimeInSecondsOfLastPacket);
                            for (int index = 0; index < newTdData1.Count() - 1; index++)
                            {
                                totalM1TimeInSecondsOfLastPacket += timeSpace;
                                newM1TdDataX.Add(totalM1TimeInSecondsOfLastPacket);
                            }
                            isFirstM1TDPacket = false;
                        }
                        else
                        {
                            if ((newData.Header.Timestamp.Seconds - prevM1TDPacketTimestampSeconds) < 6)
                            {
                                double minTimeForNextPacket = 0.0;
                                ushort newPacketTicksTotalWithTimeGap = 0;
                                double newPacketSecondsTotalWithTimeGap = 0.0;
                                double newPacketSecondsTotalWithoutTimeGap = 0.0;
                                double timeLossBetweenNewOldPacketsInSeconds = 0.0;
                                if (newData.Header.SystemTick < prevM1TDPacketTicks)
                                {
                                    newPacketTicksTotalWithTimeGap = (ushort)(65535 - prevM1TDPacketTicks + newData.Header.SystemTick);
                                }
                                else if (newData.Header.SystemTick > prevM1TDPacketTicks)
                                {
                                    newPacketTicksTotalWithTimeGap = (ushort)(newData.Header.SystemTick - prevM1TDPacketTicks);
                                }
                                else
                                {
                                    return;
                                }
                                minTimeForNextPacket = (1.0 / (double)senseConfig.Sense.TDSampleRate) + ((1.0 / (double)senseConfig.Sense.TDSampleRate) * newTdData1.Count());
                                newPacketSecondsTotalWithTimeGap = (double)newPacketTicksTotalWithTimeGap / 10000.0;
                                if (newPacketSecondsTotalWithTimeGap > minTimeForNextPacket)
                                {
                                    newPacketSecondsTotalWithoutTimeGap = ((1.0 / (double)senseConfig.Sense.TDSampleRate)) * newTdData1.Count();
                                    timeLossBetweenNewOldPacketsInSeconds = newPacketSecondsTotalWithTimeGap - newPacketSecondsTotalWithoutTimeGap;
                                    totalM1TimeInSecondsOfLastPacket = (totalM1TimeInSecondsOfLastPacket + timeLossBetweenNewOldPacketsInSeconds) - minTimeForNextPacket;
                                    newM1TdDataX.Add(totalM1TimeInSecondsOfLastPacket);
                                    for (int index = 0; index < newTdData1.Count() - 1; index++)
                                    {
                                        totalM1TimeInSecondsOfLastPacket += timeSpace;
                                        newM1TdDataX.Add(totalM1TimeInSecondsOfLastPacket);
                                    }
                                }
                                else if (newPacketSecondsTotalWithTimeGap == minTimeForNextPacket)
                                {
                                    for (int index = 0; index < newTdData1.Count(); index++)
                                    {
                                        totalM1TimeInSecondsOfLastPacket += timeSpace;
                                        newM1TdDataX.Add(totalM1TimeInSecondsOfLastPacket);
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        prevM1TDPacketTicks = newData.Header.SystemTick;
                        prevM1TDPacketTimestampSeconds = newData.Header.Timestamp.Seconds;

                        //Add M1 time domain channel list data to chart with same x values calculated for STN
                        try
                        {

                            if (newM1TdDataX.Count() == newTdData1.ToArray().Count())
                            {
                                _timeDomainM1Chart.Append(newM1TdDataX, newTdData1.ToArray());
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }


        }*/

        //Time domain old way of calculating x values
        private void TheSummit_DataReceivedTDHandler(object sender, SensingEventTD newData)
        {
            int channelValueForSTN = 0;
            int channelValueForM1 = 2;
            //Find the channel for each time domain channel for M1 and STN from the selected channel in the drop down menu for each
            //STN is either 0 or 1
            //M1 is either 2 or 3
            try
            {
                if (SelectedTimeDomainSTN != null)
                {
                    if (SelectedTimeDomainSTN.Equals(TimeDomainSTNDropDown[0]))
                    {
                        channelValueForSTN = 0;
                    }
                    else if (SelectedTimeDomainSTN.Equals(TimeDomainSTNDropDown[1]))
                    {
                        channelValueForSTN = 1;
                    }
                }
                if (SelectedTimeDomainM1 != null)
                {
                    if (SelectedTimeDomainM1.Equals(TimeDomainM1DropDown[0]))
                    {
                        channelValueForM1 = 2;
                    }
                    else if (SelectedTimeDomainM1.Equals(TimeDomainM1DropDown[1]))
                    {
                        channelValueForM1 = 3;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }


            try
            {
                if (TimeDomainSTNDropDown[0] != DISABLED || TimeDomainSTNDropDown[1] != DISABLED)
                {
                    //Add STN data into chart
                    List<double> stnDataList;
                    List<double> newSTNTdDataX = new List<double>();
                    if (newData.ChannelSamples.TryGetValue((SenseTimeDomainChannel)channelValueForSTN, out stnDataList))
                    {
                        List<double> newTdData1 = new List<double>(stnDataList.ToArray());
                        newTdData1.Reverse();
                        //create a list of x values the size of the y values
                        //Scichart only allows 2 same size arrays for x and y
                        for (int index = 0; index < newTdData1.Count(); index++)
                        {
                            newSTNTdDataX.Add(timeDomainSTNXvalue);
                            timeDomainSTNXvalue += 0.2;
                        }
                        //Add STN time domain channel list data to chart
                        try
                        {
                            _timeDomainSTNChart.Append(newSTNTdDataX, newTdData1.ToArray());
                        }
                        catch (Exception e)
                        {
                            _log.Error(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            try
            {
                if (TimeDomainM1DropDown[0] != DISABLED || TimeDomainM1DropDown[1] != DISABLED)
                {
                    //add M1 data into chart
                    List<double> m1DataList;
                    List<double> newM1TdDataX = new List<double>();
                    if (newData.ChannelSamples.TryGetValue((SenseTimeDomainChannel)channelValueForM1, out m1DataList))
                    {
                        List<double> newTdData1 = new List<double>(m1DataList.ToArray());
                        newTdData1.Reverse();
                        for (int index = 0; index < newTdData1.Count(); index++)
                        {
                            newM1TdDataX.Add(timeDomainM1Xvalue);
                            timeDomainM1Xvalue += 0.2;
                        }
                        //Add M1 time domain channel list data to chart with same x values calculated for STN
                        try
                        {

                            if (newM1TdDataX.Count() == newTdData1.ToArray().Count())
                            {
                                _timeDomainM1Chart.Append(newM1TdDataX, newTdData1.ToArray());
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }


        }

        //FFT Event Handler
        private void TheSummit_DataReceivedFFTHandler(object sender, SensingEventFFT newData)
        {
            rollingMean.Add(newData.FftOutput);
            if (initialCountForRollingMean < (userInputForFFTMean - 1))
            {
                initialCountForRollingMean++;
            }
            else
            {
                try
                {
                    List<double> tempList = new List<double>();
                    //find mean
                    for (int i = 0; i < newData.FftOutput.Count(); i++)
                    {
                        double mean = 0;
                        for (int j = 0; j < userInputForFFTMean; j++)
                        {
                            mean += rollingMean[j][i];
                        }
                        mean = mean / userInputForFFTMean;
                        tempList.Add(mean);
                    }
                    rollingMean.RemoveAt(0);
                    CheckForInfinityOrNegativeInfinityInAccHandler(ref tempList);
                    //insert into chart
                    if (fftBins.Count == newData.FftOutput.Count)
                    {
                        try
                        {
                            if (SelectedFFTScaleOption != null)
                            {
                                if (SelectedFFTScaleOption.Equals(fftAutoScaleChartOption))
                                {
                                    YAxesFFT[0].AxisTitle = "µV^2/Hz";
                                    YAxesFFT[0].AutoRange = AutoRange.Always;
                                }
                                else if (SelectedFFTScaleOption.Equals(fftNoneScaleChartOption))
                                {
                                    YAxesFFT[0].AxisTitle = "µV^2/Hz";
                                    YAxesFFT[0].AutoRange = AutoRange.Never;
                                }
                                else if (SelectedFFTScaleOption.Equals(fftLog10ScaleChartOption))
                                {
                                    YAxesFFT[0].AxisTitle = "Log10µV^2/Hz";
                                    YAxesFFT[0].AutoRange = AutoRange.Never;
                                    //go through templist and change to log10 for each value
                                    for (int i = 0; i < tempList.Count; i++)
                                    {
                                        tempList[i] = Math.Log10(tempList[i]);
                                    }
                                }
                            }
                            _fftChart.Clear();
                            _fftChart.Append(fftBins, tempList);
                        }
                        catch (Exception e)
                        {
                            _log.Error(e);
                        }
                    }
                    else
                    {
                        _log.Warn("fft Bin size not equal to incoming data for FFT handler");
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }

        }

        //Power Event Handler
        private void TheSummit_PowerReceivedHandler(object sender, SensingEventPower newData)
        {
            //newData.Header.SystemTick;
            //newData.Header.Timestamp.Seconds;
            //newData.Header.GlobalSequence

            //power variable is set to 0 just in case if/elseif statements never get called
            double power = 0;
            try
            {
                //Make sure the selected power channel in the drop down menu is set to something
                //Drop down menu is named PowerChannelOptions and SelectedPowerChannel are both binded inside VisualizationViewModel.cs
                if (SelectedPowerChannel != null)
                {
                    //Check to see which selected power channel is selected in the drop down menu and make sure it isn't a disabled power channel
                    //Disabled power channels are set when sense config file is read. 
                    //If power channel is disabled in config file, the drop down menu will have a "Disabled" value instead of an actual value.
                    //If user selected a non-Disabled channel, then get the data corresponding to the drop down menu value
                    if (!SelectedPowerChannel.Equals("Disabled") && SelectedPowerChannel.Equals(PowerChannelOptions[0]))
                    {
                        power = (double)newData.Bands[0];
                    }
                    else if (!SelectedPowerChannel.Equals("Disabled") && SelectedPowerChannel.Equals(PowerChannelOptions[1]))
                    {
                        power = (double)newData.Bands[1];
                    }
                    else if (!SelectedPowerChannel.Equals("Disabled") && SelectedPowerChannel.Equals(PowerChannelOptions[2]))
                    {
                        power = (double)newData.Bands[2];
                    }
                    else if (!SelectedPowerChannel.Equals("Disabled") && SelectedPowerChannel.Equals(PowerChannelOptions[3]))
                    {
                        power = (double)newData.Bands[3];
                    }
                    else if (!SelectedPowerChannel.Equals("Disabled") && SelectedPowerChannel.Equals(PowerChannelOptions[4]))
                    {
                        power = (double)newData.Bands[4];
                    }
                    else if (!SelectedPowerChannel.Equals("Disabled") && SelectedPowerChannel.Equals(PowerChannelOptions[5]))
                    {
                        power = (double)newData.Bands[5];
                    }
                    else if (!SelectedPowerChannel.Equals("Disabled") && SelectedPowerChannel.Equals(PowerChannelOptions[6]))
                    {
                        power = (double)newData.Bands[6];
                    }
                    else if (!SelectedPowerChannel.Equals("Disabled") && SelectedPowerChannel.Equals(PowerChannelOptions[7]))
                    {
                        power = (double)newData.Bands[7];
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            try
            {
                powerAndDetectorXValue = sw.ElapsedMilliseconds;
                //Both Threshold y values and power values are added to their respective charts. X axis value x is incremented.
                //If doing embedded then adaptive shouldn't be null, so use those values.  If not embedded and just sensing, just make 0
                if (adaptiveConfig != null)
                {

                    _b1ThresholdLine.Append(powerAndDetectorXValue, adaptiveConfig.Detection.LD0.B1);
                    _b0ThresholdLine.Append(powerAndDetectorXValue, adaptiveConfig.Detection.LD0.B0);
                }
                else
                {
                    _b1ThresholdLine.Append(powerAndDetectorXValue, 0);
                    _b0ThresholdLine.Append(powerAndDetectorXValue, 0);
                }
                _powerData.Append(powerAndDetectorXValue, power);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            try
            {
                //SelectedPowerScaleOption is implemented in VisualizationViewModel.cs and is responsible for how the chart is visualized
                //If the drop down menu for the selectedPowerScaleOption is equal to AutoScale, then use AutoRange
                if (SelectedPowerScaleOption.Equals(powerAutoScaleChartOption))
                {
                    YAxesPower[0].AutoRange = AutoRange.Always;
                }
                else if (SelectedPowerScaleOption.Equals(powerThresholdScaleChartOption))
                {
                    //If the drop down menu for the selectedPowerScaleOption is equal to Threshold, then snap to the lower and upper threshold values given and multiply upper limit by .4 to give buffer.
                    //Also, turn off auto-range
                    YAxesPower[0].AutoRange = AutoRange.Never;
                    if (adaptiveConfig != null)
                    {
                        YAxesPower[0].VisibleRange = new DoubleRange(adaptiveConfig.Detection.LD0.B0 - adaptiveConfig.Detection.LD0.B1 * .4, adaptiveConfig.Detection.LD0.B1 + adaptiveConfig.Detection.LD0.B1 * .4);
                    }
                }
                else if (SelectedPowerScaleOption.Equals(powerNoneScaleChartOption))
                {
                    YAxesPower[0].AutoRange = AutoRange.Never;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
        //Detector Event Handler
        private void TheSummit_dataReceivedDetector(object sender, AdaptiveDetectEvent newData)
        {
            try
            {
                powerAndDetectorXValue = sw.ElapsedMilliseconds;
                //get the detector for LD0 and append to chart. Increment z for x axis
                uint detectorData = newData.Ld0Status.Output;
                _detectorLD0Chart.Append(powerAndDetectorXValue, detectorData);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            try
            {
                //See power event handler above for explanation on SelectedPowerScaleOption
                //This is the same implementation for Detector
                if (SelectedPowerScaleOption.Equals(powerAutoScaleChartOption))
                {
                    YAxesPower[1].AutoRange = AutoRange.Always;
                }
                else if (SelectedPowerScaleOption.Equals(powerThresholdScaleChartOption))
                {
                    YAxesPower[1].AutoRange = AutoRange.Never;
                    YAxesPower[1].VisibleRange = new DoubleRange(adaptiveConfig.Detection.LD0.B0 - adaptiveConfig.Detection.LD0.B1 * .4, adaptiveConfig.Detection.LD0.B1 + adaptiveConfig.Detection.LD0.B1 * .4);

                }
                else if (SelectedPowerScaleOption.Equals(powerNoneScaleChartOption))
                {
                    YAxesPower[0].AutoRange = AutoRange.Never;
                }

                //Get adaptive state and add to chart. Update value in display to show new value change
                byte adaptiveState = newData.CurrentAdaptiveState;
                StimStateDisplay = adaptiveState.ToString();
                _adaptiveState.Append(y_coordinateForCurrState, adaptiveState);

                //Get adaptive amp and add to chart. Update value in display to show new value change
                double current = newData.CurrentProgramAmplitudesInMilliamps[0];
                StimAmpDisplay = current.ToString();
                _adaptiveCurrent.Append(y_coordinateForCurrState, current);

                //increment y value for x axis
                y_coordinateForCurrState += 0.2;
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
        //Accelerometer Event Handler
        private void TheSummit_DataReceivedAccelHandler(object sender, SensingEventAccel newData)
        {
            try
            {
                //calculate x values so that they are as big as the y values
                //start off at the same x value it left off at.
                List<double> newAccelXvalue = new List<double>();
                int count = newData.XSamples.ToArray().Count();
                //Save the count of each to get the mean
                countOfX += count;
                countOfY += newData.YSamples.ToArray().Count();
                countOfZ += newData.ZSamples.ToArray().Count();
                for (int index = 0; index < count; index++)
                {
                    newAccelXvalue.Add(accelerometerXvalue);
                    accelerometerXvalue += 0.2;
                }
                //Set samples to a new list
                List<double> newXValueSample = new List<double>(newData.XSamples.ToArray());
                List<double> newYValueSample = new List<double>(newData.YSamples.ToArray());
                List<double> newZValueSample = new List<double>(newData.ZSamples.ToArray());
                newXValueSample.Reverse();
                newYValueSample.Reverse();
                newZValueSample.Reverse();
                CheckForInfinityOrNegativeInfinityInAccHandler(ref newXValueSample);
                CheckForInfinityOrNegativeInfinityInAccHandler(ref newYValueSample);
                CheckForInfinityOrNegativeInfinityInAccHandler(ref newZValueSample);

                //loop through all lists and find mean. subtract mean to standardize values so they are on top of each other. 
                int xSize = newXValueSample.Count();
                double xMean = newXValueSample.Average();
                for (int i = 0; i < xSize; i++)
                {
                    newXValueSample[i] = newXValueSample[i] - xMean;
                }

                int ySize = newYValueSample.Count();
                double yMean = newYValueSample.Average();
                for (int i = 0; i < ySize; i++)
                {
                    newYValueSample[i] = newYValueSample[i] - yMean;
                }

                int zSize = newZValueSample.Count();
                double zMean = newZValueSample.Average();
                for (int i = 0; i < zSize; i++)
                {
                    newZValueSample[i] = newZValueSample[i] - zMean;
                }
                //Add data to charts. try catch in case x values list size doesn't match up to y value list size
                _accelerometryXChart.Append(newAccelXvalue, newXValueSample);
                _accelerometryYChart.Append(newAccelXvalue, newYValueSample);
                _accelerometryZChart.Append(newAccelXvalue, newZValueSample);

            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
        #endregion

        #region Error Checking/Handling Methods
        /// <summary>
        /// Checks for return error code from APIReturnInfo from Medtronic
        /// If there is an error, the method calls error handling method SetEmbeddedOffGroupAStimOnWhenErrorOccurs() to turn embedded off, change to group A and turn Stim ON
        /// The Error location and error descriptor from the returned API call are displayed to user in a message box.
        /// </summary>
        /// <param name="info">The APIReturnInfo value returned from the Medtronic API call</param>
        /// <param name="errorLocation">The location where the error is being check. Can be turning stim on, changing group, etc</param>
        /// /// <param name="runErrorHandling">If true, run the error handling process of turn stim on, group A, turn embedded off. If false, don't run error handling</param>
        /// <returns>True if there was an error or false if no error</returns>
        private bool CheckForReturnError(APIReturnInfo info, string errorLocation, bool runErrorHandling)
        {
            if (info.RejectCode != 0)
            {
                _log.Warn("Medtronic API Error: Reject code: " + info.RejectCode + ". Reject description: " + info.Descriptor + ". Error location: " + errorLocation);
                if (runErrorHandling)
                {
                    SetEmbeddedOffGroupAStimOnWhenErrorOccurs();
                }
                string messageBoxText = "Medtronic API Error: Reject code: " + info.RejectCode + ". Reject description: " + info.Descriptor + ". Error location: " + errorLocation;
                if (info.RejectCode == 6)
                {
                    messageBoxText += ". Possible error could be you are over/under the limits";
                }
                string caption = "ERROR";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBox.Show(messageBoxText, caption, button, icon);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method for error handling
        /// This method turns embedded therapy off, changes to group A and turns stim ON.
        /// If this fails for any of these steps, then an error message is displayed to user detailing what went wrong and the program is closed
        /// If the error handling is successful, then the error is logged in the event log and program execution continues
        /// </summary>
        private void SetEmbeddedOffGroupAStimOnWhenErrorOccurs()
        {
            try
            {
                _log.Info("Running Error handling...");
                //Turn everything back to normal: change to Group A, turn stim on, turn embedded off;
                bufferReturnInfo = theSummit.StimChangeTherapyOff(false);
                Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy OFF: " + bufferReturnInfo.Descriptor);
                if (bufferReturnInfo.RejectCode != 0)
                {
                    ShowMessageBox("Could not run error handling while turning therapy off to turn embedded therapy off. Reject Description: " + bufferReturnInfo.Descriptor + ". Please fix and try again.", "ERROR");
                    _log.Info("Turn stim off success while running Error handling");
                    return;
                }
                bufferReturnInfo = theSummit.WriteAdaptiveMode(AdaptiveTherapyModes.Disabled);
                Messages.Insert(0, DateTime.Now + ":: Disabling Therapy Mode: " + bufferReturnInfo.Descriptor);
                if (bufferReturnInfo.RejectCode != 0)
                {
                    ShowMessageBox("Could not run error handling while disabling embedded therapy. Reject Description: " + bufferReturnInfo.Descriptor + ". Please fix and try again.", "ERROR");
                    _log.Info("Turn embedded therapy off success while running Error handling");
                    return;
                }
                bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group0);
                Messages.Insert(0, DateTime.Now + ":: Changing to Group A: " + bufferReturnInfo.Descriptor);
                if (bufferReturnInfo.RejectCode != 0)
                {
                    ShowMessageBox("Could not run error handling while changing to Group A. Reject Description: " + bufferReturnInfo.Descriptor + ". Please fix and try again.", "ERROR");
                    _log.Info("Turn group A success while running Error handling");
                    return;
                }
                bufferReturnInfo = theSummit.StimChangeTherapyOn();
                Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy ON: " + bufferReturnInfo.Descriptor);
                if (bufferReturnInfo.RejectCode != 0)
                {
                    ShowMessageBox("Could not run error handling while turning Stim Therapy On. Reject Description: " + bufferReturnInfo.Descriptor + ". Please fix and try again.", "ERROR");
                    _log.Info("Turn stim on success while running Error handling");
                    return;
                }
                UpdateStimStatusGroup(true, true, true);
                bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, ERROR_IN_LOG_EVENT_ID + " RETURN ERROR FROM API (turn embedded off, change to group A, turn stim on): ", DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                CheckForReturnErrorInLog(bufferReturnInfo, "RETURN ERROR FROM API");
                _log.Info("Error handling success");
            }
            catch (Exception e)
            {
                _log.Error(e);
                ShowMessageBox("ERROR: Please fix and try again.", "ERROR");
            }
        }

        /// <summary>
        /// Similar to CheckForReturnError, this checks to see if there was a reject code error from the Medtronic API when logging an event
        /// If there is an error code, then display this to the user in the message box that the event log was unsuccessful
        /// </summary>
        /// <param name="info">APIReturnInfo from medtronic api call</param>
        /// <param name="errorLocation">Location where the error occurred such as turn stim on, change groups, etc.</param>
        private void CheckForReturnErrorInLog(APIReturnInfo info, string errorLocation)
        {
            if (info.RejectCode != 0)
            {
                _log.Warn("Medtronic API Error: Reject code not 0 while logging: " + info.RejectCode + ". Reject description: " + info.Descriptor + ". Error location: " + errorLocation);
                Messages.Insert(0, DateTime.Now + ":: Error writing event to log in " + errorLocation + ". Error description: " + info.Descriptor);
            }
        }
        #endregion

        #region UpdateDBSThread, WriteAdaptiveState parameters, WriteLD0DetectorConfiguration parameters, SummitConfigureSensing parameters
        /// <summary>
        /// Thread that is called from UpdateDBSButtonClick() to updateDBS
        /// </summary>
        private void UpdateDBSThreadCode()
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        SensingState state;
                        theSummit.ReadSensingState(out state);
                        if (state.State.ToString().Contains("DetectionLd0") || state.State.ToString().Contains("DetectionLd1"))
                        {
                            bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_EMBEDDED_OFF_EVENT_ID + " Turn Embedded Therapy OFF. Number: " + versionNumber, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                            CheckForReturnErrorInLog(bufferReturnInfo, "Embedded Therapy OFF");
                        }

                        //Start over fft rolling mean settings
                        initialCountForRollingMean = 0;
                        rollingMean.Clear();
                        //TODO check POR bit
                        Console.WriteLine("Turning off Stim and Sensing for Configuration...");
                        //Stim therapy must be off to setup embedded adaptive DBS
                        bufferReturnInfo = theSummit.StimChangeTherapyOff(false);
                        Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy OFF: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy off", true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        Thread.Sleep(300);
                        _log.Info("Turn stim off success in update DBS button click");
                        //Make sure embedded therapy is turned off while setting up parameters
                        bufferReturnInfo = theSummit.WriteAdaptiveMode(AdaptiveTherapyModes.Disabled);
                        Messages.Insert(0, DateTime.Now + ":: Turning Therapy Mode to disabled: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Disabling Embedded Therapy Mode", true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Turn embedded therapy off success in update DBS button click");
                        //Turn off streaming
                        bufferReturnInfo = theSummit.WriteSensingDisableStreams(true, true, true, true, true, true, true, true);
                        Messages.Insert(0, DateTime.Now + ":: Turning off streaming: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Disable write sensing", true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Turn streaming off success in update DBS button click");
                        //Ensure sensing is off before configuring.
                        bufferReturnInfo = theSummit.WriteSensingState(SenseStates.None, 0x00);
                        Messages.Insert(0, DateTime.Now + ":: Turning off sensing: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Turn off Sensing", true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Turn sensing off success in update DBS button click");
                        SenseStreamOffEnabled = false;
                        SenseStreamOnEnabled = true;
                        UnRegisterEventHandlers();
                        // ********************************** Sensing Settings **********************************
                        // Attempt to configure the INS sensing and write the LD0 Detector configurations
                        Console.WriteLine("Writing sensing configuration...");
                        if (!summitSensing.SummitConfigureSensing(theSummit, senseConfig, true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            _log.Warn("Could not configure sensing in update DBS button click");
                            return;
                        }
                        if (!WriteLD0DetectorConfiguration(adaptiveConfig, theSummit))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            _log.Warn("Could not configure detector for LD0 in update DBS button click");
                            return;
                        }

                        try
                        {
                            if (adaptiveConfig.Detection.LD1.IsEnabled)
                            {
                                //configure LD1
                                if (!WriteLD1DetectorConfiguration(adaptiveConfig, theSummit))
                                {
                                    UpdateStimStatusGroup(true, true, true);
                                    _log.Warn("Could not configure detector for LD1 in update DBS button click");
                                    return;
                                }
                            }
                        }
                        catch
                        {
                            Messages.Add("Error occurred checking if LD1 is enabled. Please check adaptive config file and try again...");
                            return;
                        }

                        // Clear settings
                        bufferReturnInfo = theSummit.WriteAdaptiveClearSettings(AdaptiveClearTypes.All, 0);
                        Messages.Insert(0, DateTime.Now + ":: Clearing Adaptive Settings: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Clear Apative Therapy Settings", true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Clear Adaptive Settings success in update DBS button click");
                        // Deltas - 0.1mA/second
                        AdaptiveDeltas[] embeddedDeltas = new AdaptiveDeltas[4];
                        embeddedDeltas[0] = new AdaptiveDeltas(adaptiveConfig.Adaptive.Program0.RiseTimes, adaptiveConfig.Adaptive.Program0.FallTimes);
                        embeddedDeltas[1] = new AdaptiveDeltas(0, 0);
                        embeddedDeltas[2] = new AdaptiveDeltas(0, 0);
                        embeddedDeltas[3] = new AdaptiveDeltas(0, 0);
                        bufferReturnInfo = theSummit.WriteAdaptiveDeltas(embeddedDeltas);
                        Messages.Insert(0, DateTime.Now + ":: Writing Adaptive Deltas (Rise/Fall times): " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Write Adaptive Deltas", true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Write Adaptive Deltas success in update DBS button click");
                        //Attempt to write adaptive states and settings
                        if (!WriteAdaptiveStates())
                        {
                            _log.Warn("Could not write adaptive states in update DBS button click");
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        // Make Group D Active
                        bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group3);
                        Messages.Insert(0, DateTime.Now + ":: Changing to Group D: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Change Stim Active Group to D", true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Change to group D success in update DBS button click");

                        if (!summitSensing.StartSensing(theSummit, senseConfig, true))
                        {
                            _log.Warn("Could not start sensingin update DBS button click");
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        if (!summitSensing.StartStreaming(theSummit, senseConfig, true))
                        {
                            _log.Warn("Could not start streaming in update DBS button click");
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        SenseStreamOffEnabled = true;
                        SenseStreamOnEnabled = false;
                        // Set the Stimulation Mode to Adaptive
                        bufferReturnInfo = theSummit.WriteAdaptiveMode(AdaptiveTherapyModes.Embedded);
                        Messages.Insert(0, DateTime.Now + ":: Turning Adaptive Therapy to Embedded: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Turn Adaptive Therapy to Embedded", true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Turn adaptive therapy on success in update DBS button click");

                        // Turn on Stim
                        bufferReturnInfo = theSummit.StimChangeTherapyOn();
                        Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy ON: " + bufferReturnInfo.Descriptor);

                        // Reset POR if set
                        if (bufferReturnInfo.RejectCodeType == typeof(MasterRejectCode)
                            && (MasterRejectCode)bufferReturnInfo.RejectCode == MasterRejectCode.ChangeTherapyPor)
                        {
                            resetPOR(theSummit);
                            bufferReturnInfo = theSummit.StimChangeTherapyOn();
                            _log.Info("Turn stim therapy on after resetPOR success in update DBS button click");
                        }
                        if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy On", true))
                        {
                            UpdateStimStatusGroup(true, true, true);
                            return;
                        }
                        _log.Info("Turn stim therapy on success in update DBS button click");
                        StartEventHandlers();

                        //Display the session/version number to the user to show which number of embedded therapy runs they're on
                        versionNumber++;
                        SessionDisplay = versionNumber.ToString("000");

                        //Make a log that embedded was turned on in the event log
                        bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_EMBEDDED_ON_EVENT_ID + " Turn Embedded Therapy ON. Number: " + versionNumber, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                        CheckForReturnErrorInLog(bufferReturnInfo, "Embedded Therapy ON");

                        //Write the adaptive and sense config files to the json log directory
                        //This is where the patientID and deviceID are needed for the path.
                        //If this was unsuccessful, then report to user in the message box
                        JSONService jsonServiceToWriteFiles = new JSONService(deviceID, patientID, PROJECT_ID, basePathForMedtronicFiles, _log);
                        if (!jsonServiceToWriteFiles.WriteAdaptiveConfigToFile(adaptiveConfig, versionNumber))
                        {
                            Messages.Insert(0, DateTime.Now + ":: --Failure to write Adaptive Config file to Log--");
                            _log.Warn("Could not write adaptive config file in update DBS button click");
                        }
                        if (!jsonServiceToWriteFiles.WriteSenseConfigToFile(senseConfig, versionNumber))
                        {
                            Messages.Insert(0, DateTime.Now + ":: --Failure to write Sense Config file to Log--");
                            _log.Warn("Could not write sense config file in update DBS button click");
                        }
                        Thread.Sleep(1000);
                        //Update all stim display settings to user
                        UpdateStimStatusGroup(true, true, true);
                        //Set y axis for current/state from config file
                        //This checks to see if either state 0 or state 2 is bigger.
                        //If one is bigger than the other, then the bigger one will have the upper limit on y axis
                        if (adaptiveConfig.Adaptive.Program0.State2AmpInMilliamps > adaptiveConfig.Adaptive.Program0.State0AmpInMilliamps)
                        {
                            YAxes[1].VisibleRange = new DoubleRange(0, adaptiveConfig.Adaptive.Program0.State2AmpInMilliamps);
                        }
                        else
                        {
                            YAxes[1].VisibleRange = new DoubleRange(0, adaptiveConfig.Adaptive.Program0.State0AmpInMilliamps);
                        }
                        //Calculate fft bins for FFT x values
                        CalculatePowerBins calculatePowerBins = new CalculatePowerBins(_log);
                        fftBins = calculatePowerBins.CalculateFFTBins(ConfigConversions.FftSizesConvert(senseConfig.Sense.FFT.FftSize), ConfigConversions.TDSampleRateConvert(senseConfig.Sense.TDSampleRate));
                        fftBins = calculatePowerBins.CalculateFFTBins(ConfigConversions.FftSizesConvert(senseConfig.Sense.FFT.FftSize), ConfigConversions.TDSampleRateConvert(senseConfig.Sense.TDSampleRate));
                        if (senseConfig.Sense.FFT.StreamSizeBins != 0)
                        {
                            CalculateNewFFTBins();
                        }
                        lowerPowerBinActualValues = calculatePowerBins.GetLowerPowerBinActualValues(senseConfig);
                        upperPowerBinActualValues = calculatePowerBins.GetUpperPowerBinActualValues(senseConfig);
                        //Calculate FFT overlap
                        FFTOverlapDisplay = CalculateFFTOverlap(senseConfig);
                        //Calculate FFT Time
                        FFTTimeDisplay = CalculateFFTTime(senseConfig);
                        //Display current FFT Channel
                        FFTCurrentChannel = "Ch. " + senseConfig.Sense.FFT.Channel.ToString();
                        //This sets the Channel options in drop down menu in visualization screen.
                        SetPowerChannelOptionsInDropDownMenu();
                        SetSTNTimeDomainChannelOptionsInDropDownMenu(senseConfig);
                        SetM1TimeDomainChannelOptionsInDropDownMenu(senseConfig);
                        //This sets the selected option to whichever power value is set to true in adaptive config file
                        SetPowerChannelSelectedOptionInDropDownMenu();
                        Messages.Insert(0, DateTime.Now + ":: --Update embedded aDBS successful--");
                        _log.Info("Success updating DBS button click");
                    }
                    catch (Exception e)
                    {
                        SenseStreamOffEnabled = false;
                        SenseStreamOnEnabled = true;
                        UpdateStimStatusGroup(true, true, true);
                        //If failed, then run error handling
                        Messages.Insert(0, DateTime.Now + ":: --ERROR: Update embedded aDBS NOT successful-- ERROR");
                        SetEmbeddedOffGroupAStimOnWhenErrorOccurs();
                        _log.Error(e);
                    }
                }
                else
                {
                    _log.Warn("summit disposed in embedded on button push");
                }
            }
            else
            {
                _log.Warn("summit is null or is not connected in embedded on button push");
            }
        }

        /// <summary>
        /// Starts summit event handlers
        /// </summary>
        private void StartEventHandlers()
        {
            //Start Event Handlers
            theSummit.DataReceivedTDHandler += TheSummit_DataReceivedTDHandler;
            theSummit.DataReceivedFFTHandler += TheSummit_DataReceivedFFTHandler;
            theSummit.DataReceivedPowerHandler += TheSummit_PowerReceivedHandler;
            theSummit.DataReceivedDetectorHandler += TheSummit_dataReceivedDetector;
            theSummit.DataReceivedAccelHandler += TheSummit_DataReceivedAccelHandler;
        }

        private void UnRegisterEventHandlers()
        {
            theSummit.DataReceivedTDHandler -= TheSummit_DataReceivedTDHandler;
            theSummit.DataReceivedFFTHandler -= TheSummit_DataReceivedFFTHandler;
            theSummit.DataReceivedPowerHandler -= TheSummit_PowerReceivedHandler;
            theSummit.DataReceivedAccelHandler -= TheSummit_DataReceivedAccelHandler;
            theSummit.DataReceivedDetectorHandler -= TheSummit_dataReceivedDetector;
        }

        /// <summary>
        /// Setup adaptive states
        /// </summary>
        /// <returns>true if successfully write adaptive states or false if unsuccessful</returns>
        private bool WriteAdaptiveStates()
        {
            try
            {
                //For now, this just sets up state 0-2 and sets the other states at 25.5 which is the value to hold the current
                AdaptiveState aState = new AdaptiveState();
                aState.Prog0AmpInMilliamps = adaptiveConfig.Adaptive.Program0.State0AmpInMilliamps;
                aState.Prog1AmpInMilliamps = 0;
                aState.Prog2AmpInMilliamps = 0;
                aState.Prog3AmpInMilliamps = 0;
                aState.RateTargetInHz = adaptiveConfig.Adaptive.Program0.RateTargetInHz; // Hold Rate
                bufferReturnInfo = theSummit.WriteAdaptiveState(0, aState);
                Messages.Insert(0, DateTime.Now + ":: Writing Adaptive State 0: " + bufferReturnInfo.Descriptor);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing adaptive state", true))
                    return false;
                aState.Prog0AmpInMilliamps = adaptiveConfig.Adaptive.Program0.State1AmpInMilliamps;
                bufferReturnInfo = theSummit.WriteAdaptiveState(1, aState);
                Messages.Insert(0, DateTime.Now + ":: Writing Adaptive State 1: " + bufferReturnInfo.Descriptor);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing adaptive state", true))
                    return false;
                aState.Prog0AmpInMilliamps = adaptiveConfig.Adaptive.Program0.State2AmpInMilliamps;
                bufferReturnInfo = theSummit.WriteAdaptiveState(2, aState);
                Messages.Insert(0, DateTime.Now + ":: Writing Adaptive State 2: " + bufferReturnInfo.Descriptor);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing adaptive state", true))
                    return false;
                aState.Prog0AmpInMilliamps = adaptiveConfig.Adaptive.Program0.State3AmpInMilliamps;
                bufferReturnInfo = theSummit.WriteAdaptiveState(3, aState);
                Messages.Insert(0, DateTime.Now + ":: Writing Adaptive State 3: " + bufferReturnInfo.Descriptor);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing adaptive state", true))
                    return false;
                aState.Prog0AmpInMilliamps = adaptiveConfig.Adaptive.Program0.State4AmpInMilliamps;
                bufferReturnInfo = theSummit.WriteAdaptiveState(4, aState);
                Messages.Insert(0, DateTime.Now + ":: Writing Adaptive State 4: " + bufferReturnInfo.Descriptor);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing adaptive state", true))
                    return false;
                aState.Prog0AmpInMilliamps = adaptiveConfig.Adaptive.Program0.State5AmpInMilliamps;
                bufferReturnInfo = theSummit.WriteAdaptiveState(5, aState);
                Messages.Insert(0, DateTime.Now + ":: Writing Adaptive State 5: " + bufferReturnInfo.Descriptor);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing adaptive state", true))
                    return false;
                aState.Prog0AmpInMilliamps = adaptiveConfig.Adaptive.Program0.State6AmpInMilliamps;
                bufferReturnInfo = theSummit.WriteAdaptiveState(6, aState);
                Messages.Insert(0, DateTime.Now + ":: Writing Adaptive State 6: " + bufferReturnInfo.Descriptor);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing adaptive state", true))
                    return false;
                aState.Prog0AmpInMilliamps = adaptiveConfig.Adaptive.Program0.State7AmpInMilliamps;
                bufferReturnInfo = theSummit.WriteAdaptiveState(7, aState);
                Messages.Insert(0, DateTime.Now + ":: Writing Adaptive State 7: " + bufferReturnInfo.Descriptor);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing adaptive state", true))
                    return false;
                aState.Prog0AmpInMilliamps = adaptiveConfig.Adaptive.Program0.State8AmpInMilliamps;
                bufferReturnInfo = theSummit.WriteAdaptiveState(8, aState);
                Messages.Insert(0, DateTime.Now + ":: Writing Adaptive State 8: " + bufferReturnInfo.Descriptor);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing adaptive state", true))
                    return false;
                _log.Info("Write Adaptive States success");
            }
            catch (Exception e)
            {
                Messages.Insert(0, DateTime.Now + ":: --ERROR: Writing Adaptive States-- Please check the adaptive config file is correct");
                _log.Error(e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Write Detector Configuration for LD0
        /// </summary>
        /// <returns>true if successfully write LD detector settings or false if unsuccessful</returns>
        private bool WriteLD0DetectorConfiguration(AdaptiveModel adaptiveConfig, SummitSystem localSummit)
        {
            APIReturnInfo bufferReturnInfo;
            // ********************************** Detector Settings **********************************`

            // Create a LD configuration and write it down to the device for both LD.
            LinearDiscriminantConfiguration configLd = new LinearDiscriminantConfiguration();
            try
            {
                // Enable dual threshold mode - LD output can be high, in-range, or below the two thresholds defined in the bias term.
                // If DualThreshold is not enabled, set it to None.
                if (adaptiveConfig.Detection.LD0.DualThreshold && adaptiveConfig.Detection.LD0.BlankBothLD)
                {
                    configLd.DetectionEnable = DetectionEnables.DualThresholdEnabled | DetectionEnables.BlankBoth;
                }
                else if (adaptiveConfig.Detection.LD0.DualThreshold && !adaptiveConfig.Detection.LD0.BlankBothLD)
                {
                    configLd.DetectionEnable = DetectionEnables.DualThresholdEnabled;
                }
                else if (!adaptiveConfig.Detection.LD0.DualThreshold && adaptiveConfig.Detection.LD0.BlankBothLD)
                {
                    configLd.DetectionEnable = DetectionEnables.BlankBoth;
                }
                else
                {
                    configLd.DetectionEnable = DetectionEnables.None;
                }
                // Convert the Detection inputs based on the config file values
                configLd.DetectionInputs = ConfigConversions.DetectionInputsConvert(
                    adaptiveConfig.Detection.LD0.Inputs.Ch0Band0,
                    adaptiveConfig.Detection.LD0.Inputs.Ch0Band1,
                    adaptiveConfig.Detection.LD0.Inputs.Ch1Band0,
                    adaptiveConfig.Detection.LD0.Inputs.Ch1Band1,
                    adaptiveConfig.Detection.LD0.Inputs.Ch2Band0,
                    adaptiveConfig.Detection.LD0.Inputs.Ch2Band1,
                    adaptiveConfig.Detection.LD0.Inputs.Ch3Band0,
                    adaptiveConfig.Detection.LD0.Inputs.Ch3Band1);
                // Update LD state 
                configLd.UpdateRate = adaptiveConfig.Detection.LD0.UpdateRate;
                // Set other timing parameters
                configLd.OnsetDuration = adaptiveConfig.Detection.LD0.OnsetDuration;
                configLd.TerminationDuration = adaptiveConfig.Detection.LD0.TerminationDuration;
                configLd.HoldoffTime = adaptiveConfig.Detection.LD0.HoldOffOnStartupTime;
                configLd.BlankingDurationUponStateChange = adaptiveConfig.Detection.LD0.StateChangeBlankingUponStateChange;
                // Set the fixed point value
                configLd.FractionalFixedPointValue = adaptiveConfig.Detection.LD0.FractionalFixedPointValue;
                double FFVP = Math.Pow(2, adaptiveConfig.Detection.LD0.FractionalFixedPointValue);
                // Set the weight vectors for the power inputs, since only one channel is used rest can be zero.
                configLd.Features[0].WeightVector = (uint) (adaptiveConfig.Detection.LD0.WeightVector[0] * FFVP);
                configLd.Features[1].WeightVector = (uint) (adaptiveConfig.Detection.LD0.WeightVector[1] * FFVP);
                configLd.Features[2].WeightVector = (uint) (adaptiveConfig.Detection.LD0.WeightVector[2] * FFVP);
                configLd.Features[3].WeightVector = (uint) (adaptiveConfig.Detection.LD0.WeightVector[3] * FFVP);
                // Set the normalization vectors for the power inputs, since only one channel is used rest can be zero. 
                configLd.Features[0].NormalizationMultiplyVector = (uint)(adaptiveConfig.Detection.LD0.NormalizationMultiplyVector[0] * FFVP);
                configLd.Features[1].NormalizationMultiplyVector = (uint)(adaptiveConfig.Detection.LD0.NormalizationMultiplyVector[1] * FFVP);
                configLd.Features[2].NormalizationMultiplyVector = (uint)(adaptiveConfig.Detection.LD0.NormalizationMultiplyVector[2] * FFVP);
                configLd.Features[3].NormalizationMultiplyVector = (uint)(adaptiveConfig.Detection.LD0.NormalizationMultiplyVector[3] * FFVP);
                // Set the normalization subtract vectors for the power inputs
                configLd.Features[0].NormalizationSubtractVector = adaptiveConfig.Detection.LD0.NormalizationSubtractVector[0];
                configLd.Features[1].NormalizationSubtractVector = adaptiveConfig.Detection.LD0.NormalizationSubtractVector[1];
                configLd.Features[2].NormalizationSubtractVector = adaptiveConfig.Detection.LD0.NormalizationSubtractVector[2];
                configLd.Features[3].NormalizationSubtractVector = adaptiveConfig.Detection.LD0.NormalizationSubtractVector[3];
                // Set the thresholds
                configLd.BiasTerm[0] = adaptiveConfig.Detection.LD0.B0;
                configLd.BiasTerm[1] = adaptiveConfig.Detection.LD0.B1;
                
            }
            catch
            {
                Messages.Insert(0, DateTime.Now + ":: --ERROR: Writing LD0 Detector Parameters. Please check that adaptive config file is correct.--");
                return false;
            }

            // Write the detector down to the INS
            try
            {
                //Attempt to write the detection parameters to medtronic api
                bufferReturnInfo = localSummit.WriteAdaptiveDetectionParameters(0, configLd);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing Detection Parameters", true))
                    return false;
            }
            catch
            {
                Messages.Insert(0, DateTime.Now + ":: --ERROR: Writing LD0 Detector Parameters--");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Write Detector Configuration for LD0
        /// </summary>
        /// <returns>true if successfully write LD detector settings or false if unsuccessful</returns>
        private bool WriteLD1DetectorConfiguration(AdaptiveModel adaptiveConfig, SummitSystem localSummit)
        {
            APIReturnInfo bufferReturnInfo;
            // ********************************** Detector Settings **********************************`

            // Create a LD configuration and write it down to the device for both LD.
            LinearDiscriminantConfiguration configLd = new LinearDiscriminantConfiguration();
            try
            {
                // Enable dual threshold mode - LD1 output can be high, in-range, or below the two thresholds defined in the bias term.
                // If DualThreshold is not enabled, set it to None.
                if (adaptiveConfig.Detection.LD1.DualThreshold && adaptiveConfig.Detection.LD1.BlankBothLD)
                {
                    configLd.DetectionEnable = DetectionEnables.DualThresholdEnabled | DetectionEnables.BlankBoth;
                }
                else if (adaptiveConfig.Detection.LD1.DualThreshold && !adaptiveConfig.Detection.LD1.BlankBothLD)
                {
                    configLd.DetectionEnable = DetectionEnables.DualThresholdEnabled;
                }
                else if (!adaptiveConfig.Detection.LD1.DualThreshold && adaptiveConfig.Detection.LD1.BlankBothLD)
                {
                    configLd.DetectionEnable = DetectionEnables.BlankBoth;
                }
                else
                {
                    configLd.DetectionEnable = DetectionEnables.None;
                }
                // Convert the Detection inputs based on the config file values
                configLd.DetectionInputs = ConfigConversions.DetectionInputsConvert(
                    adaptiveConfig.Detection.LD1.Inputs.Ch0Band0,
                    adaptiveConfig.Detection.LD1.Inputs.Ch0Band1,
                    adaptiveConfig.Detection.LD1.Inputs.Ch1Band0,
                    adaptiveConfig.Detection.LD1.Inputs.Ch1Band1,
                    adaptiveConfig.Detection.LD1.Inputs.Ch2Band0,
                    adaptiveConfig.Detection.LD1.Inputs.Ch2Band1,
                    adaptiveConfig.Detection.LD1.Inputs.Ch3Band0,
                    adaptiveConfig.Detection.LD1.Inputs.Ch3Band1);
                // Update LD state 
                configLd.UpdateRate = adaptiveConfig.Detection.LD1.UpdateRate;
                // Set other timing parameters
                configLd.OnsetDuration = adaptiveConfig.Detection.LD1.OnsetDuration;
                configLd.TerminationDuration = adaptiveConfig.Detection.LD1.TerminationDuration;
                configLd.HoldoffTime = adaptiveConfig.Detection.LD1.HoldOffOnStartupTime;
                configLd.BlankingDurationUponStateChange = adaptiveConfig.Detection.LD1.StateChangeBlankingUponStateChange;
                // Set the weight vectors for the power inputs, since only one channel is used rest can be zero.
                configLd.Features[0].WeightVector = adaptiveConfig.Detection.LD1.WeightVector[0];
                configLd.Features[1].WeightVector = adaptiveConfig.Detection.LD1.WeightVector[1];
                configLd.Features[2].WeightVector = adaptiveConfig.Detection.LD1.WeightVector[2];
                configLd.Features[3].WeightVector = adaptiveConfig.Detection.LD1.WeightVector[3];
                // Set the normalization vectors for the power inputs, since only one channel is used rest can be zero. 
                configLd.Features[0].NormalizationMultiplyVector = adaptiveConfig.Detection.LD1.NormalizationMultiplyVector[0];
                configLd.Features[1].NormalizationMultiplyVector = adaptiveConfig.Detection.LD1.NormalizationMultiplyVector[1];
                configLd.Features[2].NormalizationMultiplyVector = adaptiveConfig.Detection.LD1.NormalizationMultiplyVector[2];
                configLd.Features[3].NormalizationMultiplyVector = adaptiveConfig.Detection.LD1.NormalizationMultiplyVector[3];
                // Set the normalization subtract vectors for the power inputs
                configLd.Features[0].NormalizationSubtractVector = adaptiveConfig.Detection.LD1.NormalizationSubtractVector[0];
                configLd.Features[1].NormalizationSubtractVector = adaptiveConfig.Detection.LD1.NormalizationSubtractVector[1];
                configLd.Features[2].NormalizationSubtractVector = adaptiveConfig.Detection.LD1.NormalizationSubtractVector[2];
                configLd.Features[3].NormalizationSubtractVector = adaptiveConfig.Detection.LD1.NormalizationSubtractVector[3];
                // Set the thresholds
                configLd.BiasTerm[0] = adaptiveConfig.Detection.LD1.B0;
                configLd.BiasTerm[1] = adaptiveConfig.Detection.LD1.B1;
                // Set the fixed point value
                configLd.FractionalFixedPointValue = adaptiveConfig.Detection.LD1.FractionalFixedPointValue;
            }
            catch
            {
                Messages.Insert(0, DateTime.Now + ":: --ERROR: Writing LD1 Detector Parameters. Please check that adaptive config file is correct.--");
                return false;
            }

            // Write the detector down to the INS
            try
            {
                //Attempt to write the detection parameters to medtronic api
                bufferReturnInfo = localSummit.WriteAdaptiveDetectionParameters(1, configLd);
                if (CheckForReturnError(bufferReturnInfo, "Error Writing Detection Parameters", true))
                    return false;
            }
            catch
            {
                Messages.Insert(0, DateTime.Now + ":: --ERROR: Writing LD1 Detector Parameters--");
                return false;
            }
            return true;
        }
        #endregion

        #region Sets up Drop down menu options for Power Channel Options and Selected Power Channel Options and TimeDomain Channel option
        /// <summary>
        /// Method to set all availabe power options in the drop down menu in Visualization Tab
        /// This sets the variable PowerChannelOptions drop down menu collection in the VisualizationViewModel
        /// </summary>
        private void SetPowerChannelOptionsInDropDownMenu()
        {
            //Set the power channel options to show the time domain inputs and power channel values
            //Time domain Channel must be enabled for Power Band to be enabled
            //If the power band is not enabled, set to disabled to let user know they can't use it.
            BindableCollection<string> temp = new BindableCollection<string>();
            //Check if power band enabled in config file AND time domain channel enabled in config file
            //If so, add it to collection else set to "Disabled"
            //Do for all power bands
            if (senseConfig.Sense.PowerBands[0].IsEnabled && senseConfig.Sense.TimeDomains[0].IsEnabled)
            {
                temp.Add("+" + senseConfig.Sense.TimeDomains[0].Inputs[0] + "-" + senseConfig.Sense.TimeDomains[0].Inputs[1] + " " + lowerPowerBinActualValues[0] + "-" + upperPowerBinActualValues[0] + "Hz");
            }
            else
            {
                temp.Add(DISABLED);
            }
            if (senseConfig.Sense.PowerBands[1].IsEnabled && senseConfig.Sense.TimeDomains[0].IsEnabled)
            {
                temp.Add("+" + senseConfig.Sense.TimeDomains[0].Inputs[0] + "-" + senseConfig.Sense.TimeDomains[0].Inputs[1] + " " + lowerPowerBinActualValues[1] + "-" + upperPowerBinActualValues[1] + "Hz");
            }
            else
            {
                temp.Add(DISABLED);
            }
            if (senseConfig.Sense.PowerBands[2].IsEnabled && senseConfig.Sense.TimeDomains[1].IsEnabled)
            {
                temp.Add("+" + senseConfig.Sense.TimeDomains[1].Inputs[0] + "-" + senseConfig.Sense.TimeDomains[1].Inputs[1] + " " + lowerPowerBinActualValues[2] + "-" + upperPowerBinActualValues[2] + "Hz");
            }
            else
            {
                temp.Add(DISABLED);
            }
            if (senseConfig.Sense.PowerBands[3].IsEnabled && senseConfig.Sense.TimeDomains[1].IsEnabled)
            {
                temp.Add("+" + senseConfig.Sense.TimeDomains[1].Inputs[0] + "-" + senseConfig.Sense.TimeDomains[1].Inputs[1] + " " + lowerPowerBinActualValues[3] + "-" + upperPowerBinActualValues[3] + "Hz");
            }
            else
            {
                temp.Add(DISABLED);
            }
            if (senseConfig.Sense.PowerBands[4].IsEnabled && senseConfig.Sense.TimeDomains[2].IsEnabled)
            {
                temp.Add("+" + senseConfig.Sense.TimeDomains[2].Inputs[0] + "-" + senseConfig.Sense.TimeDomains[2].Inputs[1] + " " + lowerPowerBinActualValues[4] + "-" + upperPowerBinActualValues[4] + "Hz");
            }
            else
            {
                temp.Add(DISABLED);
            }
            if (senseConfig.Sense.PowerBands[5].IsEnabled && senseConfig.Sense.TimeDomains[2].IsEnabled)
            {
                temp.Add("+" + senseConfig.Sense.TimeDomains[2].Inputs[0] + "-" + senseConfig.Sense.TimeDomains[2].Inputs[1] + " " + lowerPowerBinActualValues[5] + "-" + upperPowerBinActualValues[5] + "Hz");
            }
            else
            {
                temp.Add(DISABLED);
            }
            if (senseConfig.Sense.PowerBands[6].IsEnabled && senseConfig.Sense.TimeDomains[3].IsEnabled)
            {
                temp.Add("+" + senseConfig.Sense.TimeDomains[3].Inputs[0] + "-" + senseConfig.Sense.TimeDomains[3].Inputs[1] + " " + lowerPowerBinActualValues[6] + "-" + upperPowerBinActualValues[6] + "Hz");
            }
            else
            {
                temp.Add(DISABLED);
            }
            if (senseConfig.Sense.PowerBands[7].IsEnabled && senseConfig.Sense.TimeDomains[3].IsEnabled)
            {
                temp.Add("+" + senseConfig.Sense.TimeDomains[3].Inputs[0] + "-" + senseConfig.Sense.TimeDomains[3].Inputs[1] + " " + lowerPowerBinActualValues[7] + "-" + upperPowerBinActualValues[7] + "Hz");
            }
            else
            {
                temp.Add(DISABLED);
            }
            //PowerChannelOptions is set in VisualizationVieModel.cs. 
            //Set temp to PowerChannelOptions
            PowerChannelOptions = temp;
        }

        /// <summary>
        /// This sets the PowerChannelOption to whichever power band is enabled in Adaptive config file
        /// If mulitple are enabled, then the first one is selected.
        /// Makes it so that user doesn't have to keep reselecting proper power from drop down menu
        /// SelectedPowerChannel is the variable set and is found in VisualizationViewModel
        /// </summary>
        private void SetPowerChannelSelectedOptionInDropDownMenu()
        {
            if (adaptiveConfig.Detection.LD0.Inputs.Ch0Band0)
            {
                SelectedPowerChannel = PowerChannelOptions[0];
            }
            else if (adaptiveConfig.Detection.LD0.Inputs.Ch0Band1)
            {
                SelectedPowerChannel = PowerChannelOptions[1];
            }
            else if (adaptiveConfig.Detection.LD0.Inputs.Ch1Band0)
            {
                SelectedPowerChannel = PowerChannelOptions[2];
            }
            else if (adaptiveConfig.Detection.LD0.Inputs.Ch1Band1)
            {
                SelectedPowerChannel = PowerChannelOptions[3];
            }
            else if (adaptiveConfig.Detection.LD0.Inputs.Ch2Band0)
            {
                SelectedPowerChannel = PowerChannelOptions[4];
            }
            else if (adaptiveConfig.Detection.LD0.Inputs.Ch2Band1)
            {
                SelectedPowerChannel = PowerChannelOptions[5];
            }
            else if (adaptiveConfig.Detection.LD0.Inputs.Ch3Band0)
            {
                SelectedPowerChannel = PowerChannelOptions[6];
            }
            else if (adaptiveConfig.Detection.LD0.Inputs.Ch3Band1)
            {
                SelectedPowerChannel = PowerChannelOptions[7];
            }
        }

        private void SetSTNTimeDomainChannelOptionsInDropDownMenu(SenseModel localModel)
        {
            BindableCollection<string> temp = new BindableCollection<string>();
            if (localModel.Sense.TimeDomains[0].IsEnabled)
            {
                temp.Add(leadLocation1 + " +" + localModel.Sense.TimeDomains[0].Inputs[0] + "-" + localModel.Sense.TimeDomains[0].Inputs[1]);
                SelectedTimeDomainSTN = temp[0];
            }
            else
            {
                temp.Add(DISABLED);
            }
            if (localModel.Sense.TimeDomains[1].IsEnabled)
            {
                temp.Add(leadLocation1 + " +" + localModel.Sense.TimeDomains[1].Inputs[0] + "-" + localModel.Sense.TimeDomains[1].Inputs[1]);
                SelectedTimeDomainSTN = temp[1];
            }
            else
            {
                temp.Add(DISABLED);
            }
            TimeDomainSTNDropDown = temp;
        }

        private void SetM1TimeDomainChannelOptionsInDropDownMenu(SenseModel localModel)
        {
            BindableCollection<string> temp = new BindableCollection<string>();
            if (localModel.Sense.TimeDomains[2].IsEnabled)
            {
                temp.Add(leadLocation2 + " +" + localModel.Sense.TimeDomains[2].Inputs[0] + "-" + localModel.Sense.TimeDomains[2].Inputs[1]);
                SelectedTimeDomainM1 = temp[0];
            }
            else
            {
                temp.Add(DISABLED);
            }
            if (localModel.Sense.TimeDomains[3].IsEnabled)
            {
                temp.Add(leadLocation2 + " +" + localModel.Sense.TimeDomains[3].Inputs[0] + "-" + localModel.Sense.TimeDomains[3].Inputs[1]);
                SelectedTimeDomainM1 = temp[1];
            }
            else
            {
                temp.Add(DISABLED);
            }
            TimeDomainM1DropDown = temp;
        }
        #endregion

        #region Stim Status Display Methods
        /// <summary>
        /// Method gets the status from StimulationData class for active group and stim therapy status
        /// Also calls methods GetStimParamsBasedOnGroup to get active group
        /// All data is displayed to user through binded variables
        /// </summary>
        private void UpdateStimStatusGroup(bool updateStimTherapy, bool updateGroup, bool updateStimStats)
        {
            if (theSummit != null)
            {
                if (!theSummit.IsDisposed)
                {
                    if (updateStimTherapy)
                    {
                        StimActiveDisplay = stimData.GetTherapyStatus(ref theSummit);
                        if (StimActiveDisplay.Equals("TherapyOff"))
                        {
                            StimOnButtonEnabled = true;
                            StimOffButtonEnabled = false;
                            TherapyStatusBackground = Brushes.LightGray;
                        }
                        else if (StimActiveDisplay.Equals("TherapyActive"))
                        {
                            StimOnButtonEnabled = false;
                            StimOffButtonEnabled = true;
                            TherapyStatusBackground = Brushes.ForestGreen;
                        }
                        else
                        {
                            return;
                        }
                    }
                    if (updateGroup)
                    {
                        ActiveGroupDisplay = stimData.GetActiveGroup(ref theSummit);
                        UpdateStimLimitsAndElectrodes(ActiveGroupDisplay);
                    }
                    if (updateStimStats)
                    {
                        stimModel = GetStimParamsBasedOnGroup(ActiveGroupDisplay);
                        //Keep trying to get the stim values until get actual values.
                        int stimGroupCounter = 5;
                        while ((stimModel.StimAmp == -1 || stimModel.StimRate == -1 || stimModel.PulseWidth == -1) && stimGroupCounter > 0)
                        {
                            stimModel = GetStimParamsBasedOnGroup(ActiveGroupDisplay);
                            stimGroupCounter--;
                        }
                        if (stimGroupCounter <= 0)
                        {
                            MessageBox.Show("Could not get Stim Values. Current Values incorrect.  You may try changing groups to update stim stats or create a new session by pressing the New Session button.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        if (stimModel != null)
                        {
                            StimAmpDisplay = stimModel.StimAmp.ToString();

                            StimRateDisplay = stimModel.StimRate.ToString();

                            StimPWDisplay = stimModel.PulseWidth.ToString();

                        }
                    }
                }
            }
            IsSpinnerVisible = false;
        }

        /// <summary>
        /// Gets the group stim params based on the group that was read from the device.
        /// ie: if Group b was read from the device, then it gets the params for that specific group.
        /// </summary>
        /// <param name="group">String that is the group for getting the params. Example: "Group A", "Group B", etc.</param>
        /// <returns>StimParameterModel that contains the stim rate, stim amp and pulse width data if successful or null if unsuccessful</returns>
        private StimParameterModel GetStimParamsBasedOnGroup(string group)
        {
            if (theSummit == null)
            {
                if (!theSummit.IsDisposed)
                {
                    return null;
                }
            }
            StimParameterModel stimParam = new StimParameterModel(0, 0, 0);
            if (string.IsNullOrEmpty(group))
            {
                return stimParam;
            }
            switch (group)
            {
                case "Group A":
                    stimParam = stimData.GetStimParameterModelGroupA(ref theSummit, ProgramOptions.IndexOf(SelectedProgram));
                    GroupAButtonEnabled = false;
                    GroupBButtonEnabled = true;
                    GroupCButtonEnabled = true;
                    GroupDButtonEnabled = true;
                    UpdateStimLimitsAndElectrodes("Group A");
                    break;
                case "Group B":
                    stimParam = stimData.GetStimParameterModelGroupB(ref theSummit, ProgramOptions.IndexOf(SelectedProgram));
                    GroupAButtonEnabled = true;
                    GroupBButtonEnabled = false;
                    GroupCButtonEnabled = true;
                    GroupDButtonEnabled = true;
                    UpdateStimLimitsAndElectrodes("Group B");
                    break;
                case "Group C":
                    stimParam = stimData.GetStimParameterModelGroupC(ref theSummit, ProgramOptions.IndexOf(SelectedProgram));
                    GroupAButtonEnabled = true;
                    GroupBButtonEnabled = true;
                    GroupCButtonEnabled = false;
                    GroupDButtonEnabled = true;
                    UpdateStimLimitsAndElectrodes("Group C");
                    break;
                case "Group D":
                    stimParam = stimData.GetStimParameterModelGroupD(ref theSummit, ProgramOptions.IndexOf(SelectedProgram));
                    GroupAButtonEnabled = true;
                    GroupBButtonEnabled = true;
                    GroupCButtonEnabled = true;
                    GroupDButtonEnabled = false;
                    UpdateStimLimitsAndElectrodes("Group D");
                    break;
                default:
                    break;
            }
            return stimParam;
        }

        private void UpdateStimLimitsAndElectrodes(string group)
        {
            switch (group)
            {
                case "Group A":
                    switch (SelectedProgram)
                    {
                        case "Program 0":
                            AmpLowerLimit = GroupATherapyLimits.StimAmpLowerLimitProg0;
                            AmpUpperLimit = GroupATherapyLimits.StimAmpUpperLimitProg0;
                            StimElectrode = GroupATherapyLimits.StimElectrodesProg0;
                            break;
                        case "Program 1":
                            AmpLowerLimit = GroupATherapyLimits.StimAmpLowerLimitProg1;
                            AmpUpperLimit = GroupATherapyLimits.StimAmpUpperLimitProg1;
                            StimElectrode = GroupATherapyLimits.StimElectrodesProg1;
                            break;
                        case "Program 2":
                            AmpLowerLimit = GroupATherapyLimits.StimAmpLowerLimitProg2;
                            AmpUpperLimit = GroupATherapyLimits.StimAmpUpperLimitProg2;
                            StimElectrode = GroupATherapyLimits.StimElectrodesProg2;
                            break;
                        case "Program 3":
                            AmpLowerLimit = GroupATherapyLimits.StimAmpLowerLimitProg3;
                            AmpUpperLimit = GroupATherapyLimits.StimAmpUpperLimitProg3;
                            StimElectrode = GroupATherapyLimits.StimElectrodesProg3;
                            break;
                    }
                    RateLowerLimit = GroupATherapyLimits.StimRateLowerLimit;
                    RateUpperLimit = GroupATherapyLimits.StimRateUpperLimit;
                    PWLowerLimit = GroupATherapyLimits.PulseWidthLowerLimit;
                    PWUpperLimit = GroupATherapyLimits.PulseWidthUpperLimit;
                    ActiveRechargeStatus = GroupATherapyLimits.ActiveRechargeStatus;
                    break;
                case "Group B":
                    switch (SelectedProgram)
                    {
                        case "Program 0":
                            AmpLowerLimit = GroupBTherapyLimits.StimAmpLowerLimitProg0;
                            AmpUpperLimit = GroupBTherapyLimits.StimAmpUpperLimitProg0;
                            StimElectrode = GroupBTherapyLimits.StimElectrodesProg0;
                            break;
                        case "Program 1":
                            AmpLowerLimit = GroupBTherapyLimits.StimAmpLowerLimitProg1;
                            AmpUpperLimit = GroupBTherapyLimits.StimAmpUpperLimitProg1;
                            StimElectrode = GroupBTherapyLimits.StimElectrodesProg1;
                            break;
                        case "Program 2":
                            AmpLowerLimit = GroupBTherapyLimits.StimAmpLowerLimitProg2;
                            AmpUpperLimit = GroupBTherapyLimits.StimAmpUpperLimitProg2;
                            StimElectrode = GroupBTherapyLimits.StimElectrodesProg2;
                            break;
                        case "Program 3":
                            AmpLowerLimit = GroupBTherapyLimits.StimAmpLowerLimitProg3;
                            AmpUpperLimit = GroupBTherapyLimits.StimAmpUpperLimitProg3;
                            StimElectrode = GroupBTherapyLimits.StimElectrodesProg3;
                            break;
                    }
                    RateLowerLimit = GroupBTherapyLimits.StimRateLowerLimit;
                    RateUpperLimit = GroupBTherapyLimits.StimRateUpperLimit;
                    PWLowerLimit = GroupBTherapyLimits.PulseWidthLowerLimit;
                    PWUpperLimit = GroupBTherapyLimits.PulseWidthUpperLimit;
                    ActiveRechargeStatus = GroupBTherapyLimits.ActiveRechargeStatus;
                    break;
                case "Group C":
                    switch (SelectedProgram)
                    {
                        case "Program 0":
                            AmpLowerLimit = GroupCTherapyLimits.StimAmpLowerLimitProg0;
                            AmpUpperLimit = GroupCTherapyLimits.StimAmpUpperLimitProg0;
                            StimElectrode = GroupCTherapyLimits.StimElectrodesProg0;
                            break;
                        case "Program 1":
                            AmpLowerLimit = GroupCTherapyLimits.StimAmpLowerLimitProg1;
                            AmpUpperLimit = GroupCTherapyLimits.StimAmpUpperLimitProg1;
                            StimElectrode = GroupCTherapyLimits.StimElectrodesProg1;
                            break;
                        case "Program 2":
                            AmpLowerLimit = GroupCTherapyLimits.StimAmpLowerLimitProg2;
                            AmpUpperLimit = GroupCTherapyLimits.StimAmpUpperLimitProg2;
                            StimElectrode = GroupCTherapyLimits.StimElectrodesProg2;
                            break;
                        case "Program 3":
                            AmpLowerLimit = GroupCTherapyLimits.StimAmpLowerLimitProg3;
                            AmpUpperLimit = GroupCTherapyLimits.StimAmpUpperLimitProg3;
                            StimElectrode = GroupCTherapyLimits.StimElectrodesProg3;
                            break;
                    }
                    RateLowerLimit = GroupCTherapyLimits.StimRateLowerLimit;
                    RateUpperLimit = GroupCTherapyLimits.StimRateUpperLimit;
                    PWLowerLimit = GroupCTherapyLimits.PulseWidthLowerLimit;
                    PWUpperLimit = GroupCTherapyLimits.PulseWidthUpperLimit;
                    ActiveRechargeStatus = GroupCTherapyLimits.ActiveRechargeStatus;
                    break;
                case "Group D":
                    switch (SelectedProgram)
                    {
                        case "Program 0":
                            AmpLowerLimit = GroupDTherapyLimits.StimAmpLowerLimitProg0;
                            AmpUpperLimit = GroupDTherapyLimits.StimAmpUpperLimitProg0;
                            StimElectrode = GroupDTherapyLimits.StimElectrodesProg0;
                            break;
                        case "Program 1":
                            AmpLowerLimit = GroupDTherapyLimits.StimAmpLowerLimitProg1;
                            AmpUpperLimit = GroupDTherapyLimits.StimAmpUpperLimitProg1;
                            StimElectrode = GroupDTherapyLimits.StimElectrodesProg1;
                            break;
                        case "Program 2":
                            AmpLowerLimit = GroupDTherapyLimits.StimAmpLowerLimitProg2;
                            AmpUpperLimit = GroupDTherapyLimits.StimAmpUpperLimitProg2;
                            StimElectrode = GroupDTherapyLimits.StimElectrodesProg2;
                            break;
                        case "Program 3":
                            AmpLowerLimit = GroupDTherapyLimits.StimAmpLowerLimitProg3;
                            AmpUpperLimit = GroupDTherapyLimits.StimAmpUpperLimitProg3;
                            StimElectrode = GroupDTherapyLimits.StimElectrodesProg3;
                            break;
                    }
                    RateLowerLimit = GroupDTherapyLimits.StimRateLowerLimit;
                    RateUpperLimit = GroupDTherapyLimits.StimRateUpperLimit;
                    PWLowerLimit = GroupDTherapyLimits.PulseWidthLowerLimit;
                    PWUpperLimit = GroupDTherapyLimits.PulseWidthUpperLimit;
                    ActiveRechargeStatus = GroupDTherapyLimits.ActiveRechargeStatus;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Helper Methods: Check packet loss, CheckForNegativeInfinity, resetPOR, Calculate FFT overlap/time
        /// <summary>
        /// Checks the packet loss.
        /// </summary>
        /// <param name="localSenseModel">The local sense model.</param>
        /// <returns>True if there is no packet loss or false if there was an error calculating due to config file error or over the packet loss amount</returns>
        private bool CheckPacketLoss(SenseModel localSenseModel)
        {
            //Calculate number of time domain channels enabled
            int numberOfTDChannels = 0;
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    if (localSenseModel.Sense.TimeDomains[i].IsEnabled)
                    {
                        numberOfTDChannels++;
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    return false;
                }

            }
            //Calculate timedomain if stream is set to true.  Otherwise leave it at 0
            double TD = 0;
            try
            {
                if (localSenseModel.StreamEnables.TimeDomain)
                {
                    TD = (1000 / localSenseModel.Sense.Misc.StreamingRate * 14 + (numberOfTDChannels * 2 * localSenseModel.Sense.TDSampleRate));
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                return false;
            }

            //Calculate FFT if stream is set to true. Otherwise leave at 0
            double FFT = 0;
            try
            {
                if (localSenseModel.StreamEnables.FFT)
                {
                    if (localSenseModel.Sense.FFT.StreamSizeBins != 0)
                    {
                        int fftSizeAfterSizeAndOffset = localSenseModel.Sense.FFT.StreamSizeBins * 2;
                        FFT = ((14 + fftSizeAfterSizeAndOffset) * 1000 / localSenseModel.Sense.FFT.FftInterval);
                    }
                    else
                    {
                        FFT = ((14 + localSenseModel.Sense.FFT.FftSize) * 1000 / localSenseModel.Sense.FFT.FftInterval);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                return false;
            }

            //Calculate Power if stream is set to true. Otherwise leave at 0
            double Power = 0;
            try
            {
                if (localSenseModel.StreamEnables.Power)
                {
                    Power = (46 * (1000 / localSenseModel.Sense.FFT.FftInterval));
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                return false;
            }

            //Calculate Detection if stream is set to true. Otherwise leave at 0
            double Detection = 0;
            try
            {
                if (localSenseModel.StreamEnables.AdaptiveTherapy)
                {
                    Detection = (89 * (1000 / localSenseModel.Sense.FFT.FftInterval));
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                return false;
            }

            //Calculate Acceleromotry if stream is set to true. Otherwise leave at 0
            double ACC = 0;
            try
            {
                if (localSenseModel.StreamEnables.Accelerometry)
                {
                    ACC = (78 * localSenseModel.Sense.Accelerometer.SampleRate / 8);
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                return false;
            }

            //Calculate Time if stream is set to true. Otherwise leave at 0
            double TimeStamp = 0;
            try
            {
                if (localSenseModel.StreamEnables.TimeStamp)
                {
                    TimeStamp = 14;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                return false;
            }

            double total = TD + FFT + Power + Detection + ACC + TimeStamp;

            //If mode 3 - max is 4500; if mode 4 - max is 6000
            try
            {
                if (localSenseModel.Mode != 3 && localSenseModel.Mode != 4)
                {
                    _log.Warn("Checking packet loss method.  Variable in config file: Mode - not set to 3 or 4. Variable set to: " + localSenseModel.Mode);
                    return false;
                }
                if (localSenseModel.Mode == 3 && total >= 4500)
                {
                    return false;
                }
                else if (localSenseModel.Mode == 4 && total >= 6000)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                return false;
            }

            return true;
        }
        /// <summary>
        /// Checks if any data in accelerometer list is infinite or negative infinite and fixes it
        /// </summary>
        /// <param name="newTdData1">List of accelerometer data</param>
        private void CheckForInfinityOrNegativeInfinityInAccHandler(ref List<double> newTdData1)
        {
            // Ensure new data does not contain infinity or negative infinity
            if (newTdData1.Contains(double.PositiveInfinity) || newTdData1.Contains(double.NegativeInfinity))
            {
                for (int i = 0; i < newTdData1.Count; i++)
                {
                    if (newTdData1[i] == double.PositiveInfinity)
                    {
                        newTdData1[i] = double.MaxValue;
                    }

                    if (newTdData1[i] == double.NegativeInfinity)
                    {
                        newTdData1[i] = double.MinValue;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the FFT Overlap in percent. Adds percent sign and sets value to 2 decimal places
        /// </summary>
        /// <param name="localConfig">Sense config file for getting sample rate, fft interval and fft size</param>
        /// <returns>String that is the fft overlap with percent sign to 2 decimal places</returns>
        private string CalculateFFTOverlap(SenseModel localConfig)
        {
            double result = 1.0 - (((double)localConfig.Sense.TDSampleRate * (double)localConfig.Sense.FFT.FftInterval) / 1000.0) / (double)localConfig.Sense.FFT.FftSize;
            result = result * 100.0;
            return result.ToString("0.##") + "%";
        }

        /// <summary>
        /// Calculates the FFt Time
        /// </summary>
        /// <param name="localConfig">Sense config file for getting fft interval</param>
        /// <returns>String that is the fft time with added ms after it showing it is in milliseconds</returns>
        private string CalculateFFTTime(SenseModel localConfig)
        {
            double result = userInputForFFTMean * (double)localConfig.Sense.FFT.FftInterval;
            return result.ToString() + "ms";
        }

        private void CalculateNewFFTBins()
        {
            List<double> tempFFTBins = new List<double>();
            int valueToGoUpTo = senseConfig.Sense.FFT.StreamOffsetBins + senseConfig.Sense.FFT.StreamSizeBins;
            for (int i = senseConfig.Sense.FFT.StreamOffsetBins; i < valueToGoUpTo; i++)
            {
                tempFFTBins.Add(fftBins[i]);
            }
            fftBins.Clear();
            fftBins.AddRange(tempFFTBins);
        }

        /// <summary>
        /// Resets the POR bit if it was set
        /// </summary>
        /// <param name="theSummit">SummitSystem for the api call</param>
        private void resetPOR(SummitSystem theSummit)
        {
            _log.Info("POR was set, resetting...");
            Messages.Insert(0, DateTime.Now + ":: -POR was set, resetting...");
            try
            {
                // reset POR
                bufferReturnInfo = theSummit.ResetErrorFlags(Medtronic.NeuroStim.Olympus.DataTypes.Core.StatusBits.Por);
                if (!CheckForReturnError(bufferReturnInfo, "Reset POR", false))
                {
                    return;
                }

                // check battery
                BatteryStatusResult theStatus;
                theSummit.ReadBatteryLevel(out theStatus);
                if (!CheckForReturnError(bufferReturnInfo, "Checking Battery Level", false))
                {
                    return;
                }
                // perform interrogate command and check if therapy is enabled.s
                GeneralInterrogateData interrogateBuffer;
                theSummit.ReadGeneralInfo(out interrogateBuffer);
                if (interrogateBuffer.IsTherapyUnavailable)
                {
                    Messages.Insert(0, DateTime.Now + ":: Therapy still unavailable after reset");
                    return;
                }
            }
            catch (Exception e)
            {
                Messages.Insert(0, DateTime.Now + ":: --ERROR: Reset POR bit");
                _log.Error(e);
            }
        }


        private void ShowMessageBox(string message, string title)
        {
            string messageBoxText = message;
            string caption = title;
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText, caption, button, icon);
        }
        #endregion
    }
    /// <summary>
    /// Class for the MessageBox
    /// </summary>
    public class MyMessage : INotifyPropertyChanged
    {
        private string testMessage;
        /// <summary>
        /// Message for displaying to screen to user
        /// </summary>
        public string TestMessage
        {
            get { return testMessage; }
            set
            {
                testMessage = value;
                this.OnPropertyChanged("TestMessage");
            }
        }
        /// <summary>
        /// Property change for Message to display to user
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
