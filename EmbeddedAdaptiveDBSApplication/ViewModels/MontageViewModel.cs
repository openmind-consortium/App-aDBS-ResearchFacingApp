/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.Models;
using Medtronic.NeuroStim.Olympus.DataTypes.DeviceManagement;
using Medtronic.NeuroStim.Olympus.DataTypes.Sensing;
using Medtronic.NeuroStim.Olympus.DataTypes.Therapy.Adaptive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using System.Text.RegularExpressions;
using Medtronic.NeuroStim.Olympus.Commands;
using Medtronic.SummitAPI.Events;
using Medtronic.NeuroStim.Olympus.DataTypes.Therapy;

namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    /// <summary>
    /// Class that runs montage sweep
    /// </summary>
    public partial class MainViewModel : Screen
    {
        private readonly string MONTAGE_FILE_LOCATION = @"C:\AdaptiveDBS\Montage\montage_config.json";
        private readonly string STIMSWEEP_FILE_LOCATION = @"C:\AdaptiveDBS\Montage\stim_sweep_config.json";
        private readonly string MONTAGE_CONFIG_LOCATION_PREFIX = @"C:\AdaptiveDBS\Montage\";
        private static Thread RunMontageSweepThread;
        private static Thread RunStimSweepThread;
        private MontageModel montageModel = null;
        private StimSweepModel stimSweepModel = null;
        private List<SenseModel> montageSweepConfigList = new List<SenseModel>();
        private static System.Timers.Timer aTimer;
        private volatile int timeToRun = 0;
        private double timeToRunForStimSweep = 0;
        private int totalTimeForMontage = 0;
        private int totalTimeLeftForMontage = 0;
        private readonly int timeBeforeBeginAndEndMontage = 5;
        private double timeBeforeBeginAndEndStimSweep = 10;
        private double? currentValForAmp = 0, currentValForRate = 0;
        private int? currentValueForPW = 0;
        //settings before stim sweep started
        private int originalPulseWidth;
        private double originalStimAmp, originalStimRate;
        private string originalTherapyStatus, originalGroup;
        private double totalTimeLeftForStimSweep = 0;
        private volatile bool flagToTurnLinkStatusEventHandlerOn = false;
        private readonly object counterLock = new object();
        private int counter = 5;

        #region Montage Sweep
        /// <summary>
        /// Button to run Montage sweep
        /// </summary>
        public void ButtonRunMontage()
        {
            Messages.Insert(0, DateTime.Now + ":: RUNNING MONTAGE SWEEP: Please wait for it to finish before running anything else");
            //Load montage config file first
            montageModel = jSONService.GetMontageModelFromFile(MONTAGE_FILE_LOCATION);
            if (montageModel == null)
            {
                return;
            }
            //Load all config files for montage
            if (!LoadSenseJSONFilesForMontage())
            {
                return;
            }
            //Start thread for montage sweep on all config files
            RunMontageSweepThread = new Thread(new ThreadStart(RunMontageSweepCode));
            RunMontageSweepThread.IsBackground = true;
            RunMontageSweepThread.Start();
        }

        /// <summary>
        /// Button that reads the montage configurations and prints them to user
        /// </summary>
        public void ButtonReadMontage()
        {
            Messages.Insert(0, DateTime.Now + ":: Reading Montage configurations and times...");

            if (!PrintFileInfoToMessageScreen())
            {
                Messages.Insert(0, DateTime.Now + ":: Could not get file info.  Please check config files and try again");
            }
            if (!GetTotalTimeForMontage())
            {
                Messages.Insert(0, DateTime.Now + ":: Could not get total runtime.  Please check config files and try again");
            }
            //Convert the seconds into hour/min/seconds format
            TimeSpan time = TimeSpan.FromSeconds(totalTimeForMontage);
            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            string str = time.ToString(@"hh\:mm\:ss");

            Messages.Insert(0, DateTime.Now + ":: Total Runtime for montage is: " + str);
        }

        /// <summary>
        /// Thread that does the main work to run montage sweep.
        /// In thread so doesn't freeze UI
        /// </summary>
        private void RunMontageSweepCode()
        {
            Messages.Insert(0, DateTime.Now + ":: Starting Sweep");
            if (!GetTotalTimeForMontage())
            {
                Messages.Insert(0, DateTime.Now + ":: Could not get total runtime.  Please check config files and try again");
            }
            else
            {
                totalTimeLeftForMontage = totalTimeForMontage;
            }
            if (theSummit != null)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        //Make sure embedded therapy is turned off while setting up parameters
                        bufferReturnInfo = theSummit.WriteAdaptiveMode(AdaptiveTherapyModes.Disabled);
                        Messages.Insert(0, DateTime.Now + ":: Turning Therapy Mode to disabled: " + bufferReturnInfo.Descriptor);
                        if (CheckForReturnError(bufferReturnInfo, "Disabling Embedded Therapy Mode", false))
                            return;
                        if (!summitSensing.StopSensing(theSummit, false))
                        {
                            Messages.Insert(0, DateTime.Now + ":: Could not stop sensing, try turning off Embedded Therapy. Please try again.");
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: Could not stop sensing, try turning off Embedded Therapy. Please try again.");
                        _log.Error(e);
                        return;
                    }

                    int montageIndex = 0;
                    foreach (SenseModel localSenseModel in montageSweepConfigList)
                    {
                        //Configure sensing
                        Messages.Insert(0, DateTime.Now + ":: Configuring sensing for: " + montageModel.MontageFiles[montageIndex].Filename);
                        if (!summitSensing.SummitConfigureSensing(theSummit, localSenseModel, false))
                        {
                            Messages.Insert(0, DateTime.Now + ":: Error - could not configure sensing in file: " + montageModel.MontageFiles[montageIndex].Filename);
                            return;
                        }
                        Messages.Insert(0, DateTime.Now + ":: Begin Sense and Stream for Config file name: " + montageModel.MontageFiles[montageIndex].Filename);
                        //Start sensing and streaming
                        if (!summitSensing.StartSensing(theSummit, localSenseModel, false))
                        {
                            Messages.Insert(0, DateTime.Now + ":: Error - could not start sensing in file: " + montageModel.MontageFiles[montageIndex].Filename);
                            return;
                        }
                        if (!summitSensing.StartStreaming(theSummit, localSenseModel, false))
                        {
                            Messages.Insert(0, DateTime.Now + ":: Error - could not start streaming in file: " + montageModel.MontageFiles[montageIndex].Filename);
                            return;
                        }

                        //Set timer to run for timeToRun amount
                        timeToRun = montageModel.MontageFiles[montageIndex].TimeToRunInSeconds;
                        if (timeToRun <= 10)
                        {
                            Messages.Insert(0, DateTime.Now + ":: ERROR: TimeToRunInSeconds in montage_config.json must be greater than 10 seconds");
                            return;
                        }
                        //Set timer to go off every second and decrement the timeToRun
                        aTimer = new System.Timers.Timer();
                        aTimer.Interval = 1000;
                        aTimer.AutoReset = true;
                        aTimer.Elapsed += (sender, e) => OnTimedEvent(sender, e, montageIndex);
                        aTimer.Enabled = true;

                        //This gives 10 seconds into the stream to log start time
                        int tenSecondTimeMark = timeToRun - timeBeforeBeginAndEndMontage;
                        //This tells us that we have already run this command. Needed since while loop will log a bunch all in one second, so we are limiting it to one log
                        bool startTimeHasRun = false;
                        bool stopTimeHasRun = false;
                        //Run until full time is up
                        while (timeToRun > 0)
                        {
                            if (tenSecondTimeMark == timeToRun && !startTimeHasRun)
                            {
                                try
                                {
                                    Messages.Insert(0, DateTime.Now + ":: Logging Start Time");
                                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Start : " + montageModel.MontageFiles[montageIndex].Filename, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                                    CheckForReturnErrorInLog(bufferReturnInfo, "Start Sensing for config file");
                                    startTimeHasRun = true;
                                }
                                catch (Exception e)
                                {
                                    Messages.Insert(0, DateTime.Now + ":: ERROR: Could not Log start event.");
                                    _log.Error(e);
                                }
                            }
                            if (timeToRun == timeBeforeBeginAndEndMontage && !stopTimeHasRun)
                            {
                                try
                                {
                                    Messages.Insert(0, DateTime.Now + ":: Logging Stop Time");
                                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Stop : " + montageModel.MontageFiles[montageIndex].Filename, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                                    CheckForReturnErrorInLog(bufferReturnInfo, "Start Sensing for config file");
                                    stopTimeHasRun = true;
                                }
                                catch (Exception e)
                                {
                                    Messages.Insert(0, DateTime.Now + ":: ERROR: Could not Log stop event.");
                                    _log.Error(e);
                                }
                            }
                        }
                        //stop timer so it doesn't get out of control
                        aTimer.Enabled = false;
                        //Stop Sensing
                        if (!summitSensing.StopSensing(theSummit, false))
                        {
                            Messages.Insert(0, DateTime.Now + ":: Could not stop sensing. Please try Montage again.");
                            return;
                        }
                        montageIndex++;
                    }
                    //show that sense is off now
                    SenseStreamOffButton();
                    ShowMessageBox("Montage Complete", "Success");
                }
                else
                {
                    _log.Warn("Summit System is disposed while trying to run montage");
                    Messages.Insert(0, DateTime.Now + ":: Error: Summit System is disposed. Please try again.");
                    return;
                }
            }
            else
            {
                _log.Warn("Summit System is null while trying to run montage");
                Messages.Insert(0, DateTime.Now + ":: Error: Summit System is null. Please try again.");
                return;
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e, int montageIndex)
        {
            totalTimeLeftForMontage--;
            timeToRun--;
            int percentageDoneForCurrentFile = 100 - (int)Math.Round((double)(100 * timeToRun) / montageModel.MontageFiles[montageIndex].TimeToRunInSeconds);
            int precentageDoneForTotalMontage = 100 - (int)Math.Round((double)(100 * totalTimeLeftForMontage) / totalTimeForMontage);
            //Convert the seconds into hour/min/seconds format
            TimeSpan time = TimeSpan.FromSeconds(totalTimeLeftForMontage);
            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            string str = time.ToString(@"hh\:mm\:ss");
            Messages.Insert(0, montageModel.MontageFiles[montageIndex].Filename + ": montage " + (montageIndex + 1) + " out of " + montageSweepConfigList.Count + " (" + timeToRun + " seconds left " + percentageDoneForCurrentFile + "% complete) " + precentageDoneForTotalMontage + "% of montage sweep " + str + " left to complete full sweep");
        }

        /// <summary>
        /// Reads the config file and gets the total amount of time to run 
        /// </summary>
        /// <returns>true if success and false if unsuccessful</returns>
        private bool GetTotalTimeForMontage()
        {
            montageModel = jSONService.GetMontageModelFromFile(MONTAGE_FILE_LOCATION);
            if (montageModel == null)
            {
                return false;
            }
            //Go through the files
            //Add the total amount of time for the whole thing and save in local variable to get use later
            totalTimeForMontage = 0;
            foreach (MontageFile fileInfo in montageModel.MontageFiles)
            {
                totalTimeForMontage += fileInfo.TimeToRunInSeconds;
            }
            return true;
        }

        /// <summary>
        /// Prints the file name and time for each run to the Messages List Box
        /// </summary>
        /// <returns>True if success and false if unsuccessful</returns>
        private bool PrintFileInfoToMessageScreen()
        {
            montageModel = jSONService.GetMontageModelFromFile(MONTAGE_FILE_LOCATION);
            if (montageModel == null)
            {
                return false;
            }
            try
            {
                //Go through the files and print each filename and time run
                foreach (MontageFile fileInfo in montageModel.MontageFiles)
                {
                    Messages.Insert(0, DateTime.Now + ":: " + fileInfo.Filename + " will run for " + fileInfo.TimeToRunInSeconds + " seconds.");
                }
            }
            catch (Exception e)
            {
                Messages.Insert(0, DateTime.Now + ":: Could not read files.  Please check that all config files are in the correct directory and that they have the correct format.");
                _log.Error(e);
            }

            return true;
        }
        #endregion

        #region Stim Sweep
        /// <summary>
        /// Runs the Stim Sweep code Button
        /// </summary>
        public void ReadStimSweepButton()
        {
            stimSweepModel = jSONService.GetStimSweepModelFromFile(STIMSWEEP_FILE_LOCATION);
            if (stimSweepModel == null)
            {
                Messages.Insert(0, DateTime.Now + ":: Could not load stim sweep config file.  Please fix and try again.");
                return;
            }
            //Check to make sure lists in config file are same size. If not then they need to fix and try again.
            if (stimSweepModel.AmpInmA.Count() != stimSweepModel.RateInHz.Count() || stimSweepModel.AmpInmA.Count() != stimSweepModel.PulseWidthInMicroSeconds.Count() || stimSweepModel.AmpInmA.Count() != stimSweepModel.TimeToRunInSeconds.Count() || stimSweepModel.AmpInmA.Count() != stimSweepModel.GroupABCD.Count())
            {
                Messages.Insert(0, DateTime.Now + ":: Stim Sweep Config arrays are not the same size.  Please fix array sizes in config file and try again.");
                _log.Warn("Stim Sweep Config arrays are not the same size");
                return;
            }
            PrintEachStimSweepRunToUser(stimSweepModel);
            Messages.Insert(0, DateTime.Now + ":: Total time to run stim sweep: " + GetTotalTimeForStimSweep(stimSweepModel) + " seconds");
        }

        private void PrintEachStimSweepRunToUser(StimSweepModel localModel)
        {
            try
            {
                for (int i = 0; i < localModel.AmpInmA.Count(); i++)
                {
                    double localTimeToRun = localModel.TimeToRunInSeconds[i];
                    double rateValue = localModel.RateInHz[i];
                    double ampValue = localModel.AmpInmA[i];
                    string groupValue = localModel.GroupABCD[i];
                    int pwValue = localModel.PulseWidthInMicroSeconds[i];
                    Messages.Insert(0, DateTime.Now + ":: Sweep Number: " + (i + 1) + ". TimeToRun: " + localTimeToRun + " seconds. Amp: " + ampValue + "mA. Rate: " + rateValue + "Hz. Pulse Width: " + pwValue + "μs. Group " + groupValue);
                }
            }
            catch (Exception e)
            {
                Messages.Insert(0, DateTime.Now + ":: --ERROR: Getting value from config file. Please fix config file and try again.");
                _log.Error(e);
            }
        }

        private double GetTotalTimeForStimSweep(StimSweepModel localModel)
        {
            double totalTimeForRunToDisplayToUser = 0;
            try
            {
                for (int i = 0; i < localModel.AmpInmA.Count(); i++)
                {
                    double localTimeToRun = localModel.TimeToRunInSeconds[i];
                    totalTimeForRunToDisplayToUser += localTimeToRun;
                }
            }
            catch (Exception e)
            {
                Messages.Insert(0, DateTime.Now + ":: --ERROR: Getting value from config file. Please fix config file and try again.");
                _log.Error(e);
                return totalTimeForRunToDisplayToUser;
            }
            return totalTimeForRunToDisplayToUser;
        }
        /// <summary>
        /// Reads the Stim Sweep code Button
        /// </summary>
        public void RunStimSweepButton()
        {
            UpdateStimStatusGroup(true, true, true);
            originalPulseWidth = stimModel.PulseWidth;
            originalStimAmp = stimModel.StimAmp;
            originalStimRate = stimModel.StimRate;
            originalTherapyStatus = StimActiveDisplay;
            originalGroup = ActiveGroupDisplay;

            Messages.Insert(0, DateTime.Now + ":: RUNNING STIM SWEEP: Please wait for it to finish before running anything else");
            //Load stim sweep config file first
            stimSweepModel = jSONService.GetStimSweepModelFromFile(STIMSWEEP_FILE_LOCATION);
            if(stimSweepModel == null)
            {
                return;
            }
            //load sense config
            senseConfig = jSONService.GetSenseModelFromFile(senseFileLocation);
            if (senseConfig == null)
            {
                return;
            }
            lock (counterLock)
            {
                totalTimeLeftForStimSweep = GetTotalTimeForStimSweep(stimSweepModel);
            }
            //Start thread for montage sweep on all config files
            RunStimSweepThread = new Thread(new ThreadStart(RunStimSweepCode));
            RunStimSweepThread.IsBackground = false;
            RunStimSweepThread.Start();
        }
        /// <summary>
        ///Thread that runs the stim sweep so not to freeze UI
        /// </summary>
        private void RunStimSweepCode()
        {
            if (theSummit != null)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        //Check to make sure lists in config file are same size. If not then they need to fix and try again.
                        if (stimSweepModel.AmpInmA.Count() != stimSweepModel.RateInHz.Count() || stimSweepModel.AmpInmA.Count() != stimSweepModel.PulseWidthInMicroSeconds.Count() || stimSweepModel.AmpInmA.Count() != stimSweepModel.TimeToRunInSeconds.Count() || stimSweepModel.AmpInmA.Count() != stimSweepModel.GroupABCD.Count())
                        {
                            Messages.Insert(0, DateTime.Now + ":: Stim Sweep Config arrays are not the same size.  Please fix array sizes in config file and try again.");
                            _log.Warn("Stim Sweep Config arrays are not the same size");
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: Could not read list count from config file. Please fix and try sweep again.");
                        _log.Error(e);
                        return;
                    }

                    try
                    {
                        if (!StimActiveDisplay.Equals("TherapyActive"))
                        {
                            //Turn stim on in case it's off
                            Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy ON: " + bufferReturnInfo.Descriptor);
                            bufferReturnInfo = theSummit.StimChangeTherapyOn();
                            if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy On", false))
                                return;

                            // Reset POR if set
                            if (bufferReturnInfo.RejectCodeType == typeof(MasterRejectCode)
                                && (MasterRejectCode)bufferReturnInfo.RejectCode == MasterRejectCode.ChangeTherapyPor)
                            {
                                resetPOR(theSummit);
                                bufferReturnInfo = theSummit.StimChangeTherapyOn();
                                _log.Info("Turn stim therapy on after resetPOR success in Stim Sweep button click");
                            }
                            _log.Info("Turn stim therapy on success in Stim Sweep button click");
                            UpdateStimStatusGroup(true, false, false);
                        }
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: Could not turn therapy on. Please try sweep again.");
                        _log.Error(e);
                        return;
                    }

                    try
                    {
                        timeBeforeBeginAndEndStimSweep = stimSweepModel.EventMarkerDelayTimeInSeconds;
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: Could not get EventMarkerDelayTimeInSeconds from config file. Please fix and try again.");
                        _log.Error(e);
                        return;
                    }

                    //Configure-start sensing and streaming
                    if (!summitSensing.StopSensing(theSummit, false))
                    {
                        _log.Warn("Could not stop sensing");
                        Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                        return;
                    }
                    //Configure sensing
                    if (!summitSensing.SummitConfigureSensing(theSummit, senseConfig, false))
                    {
                        _log.Warn("Could not configure sensing");
                        Messages.Insert(0, DateTime.Now + ":: Could not configure sensing.  Please fix and try again.");
                        return;
                    }
                    //Start sensing and streaming
                    if (!summitSensing.StartSensing(theSummit, senseConfig, false))
                    {
                        Messages.Insert(0, DateTime.Now + ":: Error - could not start sensing");
                        return;
                    }
                    if (!summitSensing.StartStreaming(theSummit, senseConfig, false))
                    {
                        Messages.Insert(0, DateTime.Now + ":: Error - could not start streaming");
                        return;
                    }
                    else
                    {
                        //start the unexpected link handler to check for OOR
                        theSummit.UnexpectedLinkStatusHandler += TheSummit_UnexpectedLinkStatusHandler;
                    }

                    double ampValueReadFromDevice = stimModel.StimAmp;
                    double rateValueReadFromDevice = stimModel.StimRate;
                    int pwReadFromDevice = stimModel.PulseWidth;
                    //Run stim sweep
                    for (int i = 0; i < stimSweepModel.AmpInmA.Count(); i++)
                    {
                        //Change to group from config file to next group
                        try
                        {
                            counter = 5;
                            while (counter > 0)
                            {
                                summitSensing.StopStreaming(theSummit, false);
                                bufferReturnInfo = theSummit.StimChangeActiveGroup(ConvertStimModelGroupToAPIGroup(stimSweepModel.GroupABCD[i]));
                                summitSensing.StartStreaming(theSummit, senseConfig, false);
                                if(bufferReturnInfo.RejectCode == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    counter--;
                                    Thread.Sleep(300);
                                }
                            }
                            if (counter == 0)
                            {
                                ShowMessageBox("Could not change groups. Error from Medtronic API: " + bufferReturnInfo.Descriptor + ". Please try again.", "Error");
                                if (!summitSensing.StopSensing(theSummit, false))
                                {
                                    _log.Warn("Could not stop sensing");
                                    Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                                }
                                SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                                return;
                            }
                            UpdateStimStatusGroup(false, false, true);
                        }
                        catch (Exception e)
                        {
                            ShowMessageBox("Could not change groups.  Please try again.", "Error");
                            _log.Error(e);
                            if (!summitSensing.StopSensing(theSummit, false))
                            {
                                _log.Warn("Could not stop sensing");
                                Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                            }
                            SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                            return;
                        }
                        Thread.Sleep(300);
                        GeneralInterrogateData insGeneralInfo = null;
                        string activeGroup = "";
                        try
                        {
                            counter = 5;
                            while (counter > 0)
                            {
                                bufferReturnInfo = theSummit.ReadGeneralInfo(out insGeneralInfo);
                                if (bufferReturnInfo.RejectCode == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    counter--;
                                    Thread.Sleep(300);
                                }
                            }
                            if (counter == 0)
                            {
                                ShowMessageBox("Could not get Active Group. Error from Medtronic API: " + bufferReturnInfo.Descriptor + ". Please try again.", "Error");
                                _log.Warn("ERROR: Could not get Active Group.");
                                if (!summitSensing.StopSensing(theSummit, false))
                                {
                                    _log.Warn("Could not stop sensing");
                                    Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                                }
                                SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                                return;
                            }
                            if (insGeneralInfo != null)
                                activeGroup = insGeneralInfo.TherapyStatusData.ActiveGroup.ToString();
                        }
                        catch (Exception e)
                        {
                            ShowMessageBox("Could not get Active Group.  Please try again.", "Error");
                            _log.Error(e);
                            if (!summitSensing.StopSensing(theSummit, false))
                            {
                                _log.Warn("Could not stop sensing");
                                Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                            }
                            SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                            return;
                        }
                        //Sleep each time to make sure other thread completes
                        double ampValue = 0;
                        double rateValue = 0;
                        int pwValue = 0;
                        
                        try
                        {
                            lock (counterLock)
                            {
                                timeToRunForStimSweep = stimSweepModel.TimeToRunInSeconds[i];
                            }
                            rateValue = stimSweepModel.RateInHz[i];
                            ampValue = stimSweepModel.AmpInmA[i];
                            pwValue = stimSweepModel.PulseWidthInMicroSeconds[i];
                        }
                        catch (Exception e)
                        {
                            ShowMessageBox("Getting value from config file. Please fix config file and try again.", "Error");
                            _log.Error(e);
                            if (!summitSensing.StopSensing(theSummit, false))
                            {
                                _log.Warn("Could not stop sensing");
                                Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                            }
                            SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                            return;
                        }

                        TherapyGroup insStateGroup = null;
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(activeGroup))
                            {
                                counter = 5;
                                //Get the group from the api call
                                while (counter > 0)
                                {
                                    switch (activeGroup)
                                    {
                                        case "Group0":
                                            bufferReturnInfo = theSummit.ReadStimGroup(GroupNumber.Group0, out insStateGroup);
                                            ActiveGroupDisplay = "Group A";
                                            break;
                                        case "Group1":
                                            bufferReturnInfo = theSummit.ReadStimGroup(GroupNumber.Group1, out insStateGroup);
                                            ActiveGroupDisplay = "Group B";
                                            break;
                                        case "Group2":
                                            bufferReturnInfo = theSummit.ReadStimGroup(GroupNumber.Group2, out insStateGroup);
                                            ActiveGroupDisplay = "Group C";
                                            break;
                                        case "Group3":
                                            bufferReturnInfo = theSummit.ReadStimGroup(GroupNumber.Group3, out insStateGroup);
                                            ActiveGroupDisplay = "Group D";
                                            break;
                                    }
                                    if (bufferReturnInfo.RejectCode == 0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        counter--;
                                        Thread.Sleep(300);
                                    }
                                }
                                if (counter == 0)
                                {
                                    ShowMessageBox("Could not read stim group from INS. Error from Medtronic API: " + bufferReturnInfo.Descriptor + ". Please try again", "Error");
                                    _log.Warn("Could not read stim group from INS");
                                    if (!summitSensing.StopSensing(theSummit, false))
                                    {
                                        _log.Warn("Could not stop sensing");
                                        Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                                    }
                                    SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                                    return;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ShowMessageBox("Could not read stim group from INS. Please try again", "Error");
                            _log.Error(e);
                            if (!summitSensing.StopSensing(theSummit, false))
                            {
                                _log.Warn("Could not stop sensing");
                                Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                            }
                            SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                            return;
                        }

                        try
                        {
                            if (insStateGroup != null)
                            {
                                ampValueReadFromDevice = insStateGroup.Programs[0].AmplitudeInMilliamps;
                                rateValueReadFromDevice = insStateGroup.RateInHz;
                                pwReadFromDevice = insStateGroup.Programs[0].PulseWidthInMicroseconds;
                            }
                            else
                            {
                                Messages.Insert(0, DateTime.Now + ":: Please check that values are correct on this run.  Values were not able to be retreived and verified from the device.");
                            }
                        }
                        catch (Exception e)
                        {
                            ShowMessageBox("Could not read stim values (amp, pw, rate) from data. Please try again", "Error");
                            _log.Error(e);
                            if (!summitSensing.StopSensing(theSummit, false))
                            {
                                _log.Warn("Could not stop sensing");
                                Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                            }
                            SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                            return;
                        }
                        StimParameterModel stimParameterModel = new StimParameterModel();
                        double ampToChangeTo = 0;  
                        try
                        {
                            ampToChangeTo = Math.Round(ampValue + (-1 * ampValueReadFromDevice), 1);
                            Messages.Insert(0, DateTime.Now + ":: Amp Delta Change: " + ampToChangeTo);
                            if (ampToChangeTo != 0)
                            {
                                counter = 5;
                                while (counter > 0)
                                {
                                    bufferReturnInfo = theSummit.StimChangeStepAmp(0, ampToChangeTo, out currentValForAmp);
                                    if (bufferReturnInfo.RejectCode == 0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        counter--;
                                        Thread.Sleep(300);
                                    }
                                }   
                                if (counter == 0)
                                {
                                    ShowMessageBox("Could not change stim amp. Error from Medtronic API: " + bufferReturnInfo.Descriptor + ". Please try again", "Error");
                                    _log.Warn("ERROR: Could not change stim amp.");
                                    if (!summitSensing.StopSensing(theSummit, false))
                                    {
                                        _log.Warn("Could not stop sensing");
                                        Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                                    }
                                    SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                                    return;
                                }
                                if(currentValForAmp != null)
                                {
                                    stimParameterModel.StimAmp = (double)currentValForAmp;
                                    StimAmpDisplay = currentValForAmp.ToString();
                                }
                                else
                                {
                                    stimParameterModel.StimAmp = ampValueReadFromDevice;
                                    StimAmpDisplay = ampValueReadFromDevice.ToString();
                                }
                            }
                            else
                            {
                                stimParameterModel.StimAmp = ampValueReadFromDevice;
                                StimAmpDisplay = ampValueReadFromDevice.ToString();
                            }
                        }
                        catch (Exception e)
                        {
                            ShowMessageBox("Error changing stim amp. Please try again", "Error");
                            _log.Error(e);
                            if (!summitSensing.StopSensing(theSummit, false))
                            {
                                _log.Warn("Could not stop sensing");
                                Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                            }
                            SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                            return;
                        }
                        Thread.Sleep(200);
                        double rateToChangeTo = 0;
                        try
                        {
                            //set stim rate
                            rateToChangeTo = Math.Round(rateValue + (-1 * rateValueReadFromDevice), 1);
                            Messages.Insert(0, DateTime.Now + ":: Rate Delta Change: " + rateToChangeTo);
                            if (rateToChangeTo != 0)
                            {
                                counter = 5;
                                while (counter > 0)
                                {
                                    //Allow reject code 5 because the sense friendly is on and if its close to sense friendly rate that is already active then it will throw the reject code of 5. 
                                    //We just want to leave it.
                                    bufferReturnInfo = theSummit.StimChangeStepFrequency(rateToChangeTo, true, out currentValForRate);
                                    if (bufferReturnInfo.RejectCode == 0 || bufferReturnInfo.RejectCode == 5)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        counter--;
                                        Thread.Sleep(300);
                                    }
                                }
                                if (counter == 0)
                                {
                                    ShowMessageBox("Could not change stim rate. Error from Medtronic API: " + bufferReturnInfo.Descriptor + ". Please try again", "Error");
                                    _log.Warn("ERROR: Could not change stim rate.");
                                    if (!summitSensing.StopSensing(theSummit, false))
                                    {
                                        _log.Warn("Could not stop sensing");
                                        Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                                    }
                                    SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                                    return;
                                }
                                if(currentValForRate != null)
                                {
                                    stimParameterModel.StimRate = (double)currentValForRate;
                                    StimRateDisplay = currentValForRate.ToString();
                                }
                                else
                                {
                                    stimParameterModel.StimRate = rateValueReadFromDevice;
                                    StimRateDisplay = rateValueReadFromDevice.ToString();
                                }
                            }
                            else
                            {
                                stimParameterModel.StimRate = rateValueReadFromDevice;
                                StimRateDisplay = rateValueReadFromDevice.ToString();
                            }
                        }
                        catch (Exception e)
                        {
                            ShowMessageBox("Error changing stim rate. Please try again", "Error");
                            _log.Error(e);
                            if (!summitSensing.StopSensing(theSummit, false))
                            {
                                _log.Warn("Could not stop sensing");
                                Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                            }
                            SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                            return;
                        }
                        Thread.Sleep(200);
                        int pwToChangeTo = 0;
                        try
                        {
                            //set stim pulse width
                            pwToChangeTo = pwValue + (-1 * pwReadFromDevice);
                            Messages.Insert(0, DateTime.Now + ":: Pulse Width Delta Change: " + pwToChangeTo);
                            if (pwToChangeTo != 0)
                            {
                                counter = 5;
                                while (counter > 0)
                                {
                                    bufferReturnInfo = theSummit.StimChangeStepPW(0, pwToChangeTo, out currentValueForPW);
                                    if (bufferReturnInfo.RejectCode == 0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        counter--;
                                        Thread.Sleep(300);
                                    }
                                }
                                if (counter == 0)
                                {
                                    ShowMessageBox("Could not change pulse width. Error from Medtronic API: " + bufferReturnInfo.Descriptor + ". Please try again", "Error");
                                    _log.Warn("ERROR: Could not change pulse width.");
                                    if (!summitSensing.StopSensing(theSummit, false))
                                    {
                                        _log.Warn("Could not stop sensing");
                                        Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                                    }
                                    SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                                    return;
                                }
                                if(currentValueForPW != null)
                                {
                                    stimParameterModel.PulseWidth = (int)currentValueForPW;
                                }
                                else
                                {
                                    stimParameterModel.PulseWidth = pwReadFromDevice;
                                }
                            }
                            else
                            {
                                stimParameterModel.PulseWidth = pwReadFromDevice;
                            }
                            UpdateStimStatusGroup(false, false, true);
                        }
                        catch (Exception e)
                        {
                            ShowMessageBox("Error changing pulse width. Please try again", "Error");
                            _log.Error(e);
                            if (!summitSensing.StopSensing(theSummit, false))
                            {
                                _log.Warn("Could not stop sensing");
                                Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                            }
                            SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                            return;
                        }
                        Thread.Sleep(200);
                        double tenSecondTimeMark;
                        lock (counterLock)
                        {
                            tenSecondTimeMark = timeToRunForStimSweep - timeBeforeBeginAndEndStimSweep;
                            Messages.Insert(0, DateTime.Now + ":: Time to run: " + timeToRunForStimSweep + " seconds. Stim amp: " + stimParameterModel.StimAmp + "mA. Stim Rate: " + stimParameterModel.StimRate + "Hz. Pulse Width: " + stimParameterModel.PulseWidth + "μs. " + ActiveGroupDisplay);
                        }


                        flagToTurnLinkStatusEventHandlerOn = true;
                        bool startTimeHasRun = false;
                        bool stopTimeHasRun = false;
                        if (tenSecondTimeMark < timeBeforeBeginAndEndStimSweep)
                        {
                            startTimeHasRun = true;
                            stopTimeHasRun = true;
                        }

                        while (timeToRunForStimSweep > 0)
                        {
                            if (Math.Round(tenSecondTimeMark, 2) == Math.Round(timeToRunForStimSweep, 2) && !startTimeHasRun)
                            {
                                try
                                {
                                    Messages.Insert(0, DateTime.Now + ":: Logging Event Start Time");
                                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Start Stim Run. Stim amp: " + stimParameterModel.StimAmp + "mA. Stim Rate: " + stimParameterModel.StimRate + "Hz. Pulse Width: " + stimParameterModel.PulseWidth + "μs. " + ActiveGroupDisplay, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                                    CheckForReturnErrorInLog(bufferReturnInfo, "Start Sensing for config file");
                                    startTimeHasRun = true;
                                }
                                catch (Exception e)
                                {
                                    ShowMessageBox("Could not log start event marker. Please try again", "Error");
                                    _log.Error(e);
                                    if (!summitSensing.StopSensing(theSummit, false))
                                    {
                                        _log.Warn("Could not stop sensing");
                                        Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                                    }
                                    SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                                    return;
                                }
                            }

                            if (Math.Round(timeToRunForStimSweep, 2) == Math.Round(timeBeforeBeginAndEndStimSweep, 2) && !stopTimeHasRun)
                            {
                                try
                                {
                                    Messages.Insert(0, DateTime.Now + ":: Logging Event Stop Time");
                                    bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Stop Stim Run. Stim amp: " + stimParameterModel.StimAmp + "mA. Stim Rate: " + stimParameterModel.StimRate + "Hz. Pulse Width: " + stimParameterModel.PulseWidth + "μs. " + ActiveGroupDisplay, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                                    CheckForReturnErrorInLog(bufferReturnInfo, "Start Sensing for config file");
                                    stopTimeHasRun = true;
                                }
                                catch (Exception e)
                                {
                                    ShowMessageBox("Could not log stop event marker. Please try again", "Error");
                                    _log.Error(e);
                                    if (!summitSensing.StopSensing(theSummit, false))
                                    {
                                        _log.Warn("Could not stop sensing");
                                        Messages.Insert(0, DateTime.Now + ":: Could not stop sensing");
                                    }
                                    SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                                    return;
                                }

                            }
                            Thread.Sleep(100);
                            if(Math.Round(timeToRunForStimSweep, 2) % 1 == 0)
                            {
                                Messages.Insert(0, DateTime.Now + ":: Time left in current sweep: " + Math.Round(timeToRunForStimSweep) + ". Total time left for sweep: " + Math.Round(totalTimeLeftForStimSweep));
                            }
                            timeToRunForStimSweep = timeToRunForStimSweep - 0.1;
                            totalTimeLeftForStimSweep = totalTimeLeftForStimSweep - 0.1;
                        }
                        flagToTurnLinkStatusEventHandlerOn = false;
                    }
                    if (!summitSensing.StopSensing(theSummit, false))
                    {
                        _log.Warn("Could not stop sensing");
                        ShowMessageBox("Could not stop sensing. Please try again", "Error");
                        SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                        return;
                    }
                    SetBackToOriginalSettings(originalPulseWidth, originalStimRate, originalStimAmp, originalGroup, originalTherapyStatus);
                    ShowMessageBox("Stim Sweep Complete", "Success");
                }
                else
                {
                    _log.Warn("Summit System is disposed while trying to run stim sweep");
                    Messages.Insert(0, DateTime.Now + ":: Error: Summit System is disposed. Please try again.");
                }
            }
            else
            {
                _log.Warn("Summit System is null while trying to run stim sweep");
                Messages.Insert(0, DateTime.Now + ":: Error: Summit System is null. Please try again.");
            }
        }

        private void TheSummit_UnexpectedLinkStatusHandler(object sender, UnexpectedLinkStatusEvent incomingData)
        {
            if (incomingData.TheLinkStatus.OOR && flagToTurnLinkStatusEventHandlerOn)
            {
                Messages.Insert(0, DateTime.Now + ":: Error: OOR has occurred.");
            }
        }

        private bool ResetStimAmp(double ampValue)
        {
            bool success = true;

            double ampToChangeTo = 0;
            try
            {
                ampToChangeTo = Math.Round(ampValue + (-1 * stimModel.StimAmp), 1);
                if (ampToChangeTo != 0)
                {
                    bufferReturnInfo = theSummit.StimChangeStepAmp(0, ampToChangeTo, out currentValForAmp);
                    if (CheckForReturnError(bufferReturnInfo, "Change Stim Amp Setting", false))
                        success = false;
                    UpdateStimStatusGroup(false, false, true);
                }
            }
            catch (Exception e)
            {
                success = false;
                Messages.Insert(0, DateTime.Now + ":: ERROR: Could not reset stim amp.");
                _log.Error(e);
            }
            return success;
        }

        private bool ResetStimRate(double rateValue)
        {
            double rateToChangeTo = 0;
            bool success = true;
            try
            {
                //set stim rate
                rateToChangeTo = Math.Round(rateValue + (-1 * stimModel.StimRate), 1);
                if (rateToChangeTo != 0)
                {
                    bufferReturnInfo = theSummit.StimChangeStepFrequency(rateToChangeTo, true, out currentValForRate);
                    if (CheckForReturnError(bufferReturnInfo, "Change Stim Rate Setting", false))
                        success = false;
                    UpdateStimStatusGroup(false, false, true);
                }
            }
            catch (Exception e)
            {
                success = false;
                Messages.Insert(0, DateTime.Now + ":: ERROR: Could not reset stim rate.");
                _log.Error(e);
            }
            return success;
        }

        private bool ResetStimPulseWidth(int pwValue)
        {
            int pwToChangeTo = 0;
            bool success = true;
            try
            {
                //set stim pulse width
                pwToChangeTo = pwValue + (-1 * stimModel.PulseWidth);
                if (pwToChangeTo != 0)
                {
                    bufferReturnInfo = theSummit.StimChangeStepPW(0, pwToChangeTo, out currentValueForPW);
                    if (CheckForReturnError(bufferReturnInfo, "Change Pulse Width Setting", false))
                        success = false;
                    UpdateStimStatusGroup(false, false, true);
                }
            }
            catch (Exception e)
            {
                success = false;
                Messages.Insert(0, DateTime.Now + ":: ERROR: Could not reset stim pulse width.");
                _log.Error(e);
            }
            return success;
        }

        #endregion

        #region Load Sense Config Files
        /// <summary>
        /// Method that loads all of the sense config files based on what was loaded from montage config file
        /// </summary>
        /// <returns>true if successful and false if unsuccessful</returns>
        private bool LoadSenseJSONFilesForMontage()
        {
            bool loadedCorrectly = true;
            Messages.Insert(0, DateTime.Now + ":: Loading Sense config files");
            montageSweepConfigList.Clear();

            foreach (MontageFile montageFileObject in montageModel.MontageFiles)
            {
                SenseModel tempModel = jSONService.GetSenseModelFromFile(MONTAGE_CONFIG_LOCATION_PREFIX + montageFileObject?.Filename + ".json");
                if (tempModel != null)
                {
                    montageSweepConfigList.Add(tempModel);
                }
                else
                {
                    loadedCorrectly = false;
                    break;
                }
            }

            return loadedCorrectly;
        }
        #endregion

        #region Helper methods
        private void SetBackToOriginalSettings(int localOriginalPW, double localOriginalRate, double localOriginalAmp, string localOriginalGroup, string localOriginalTherapyStatus)
        {
            //reset group
            try
            {
                switch (originalGroup)
                {
                    case "Group A":
                        summitSensing.StopStreaming(theSummit, false);
                        bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group0);
                        summitSensing.StartStreaming(theSummit, senseConfig, false);
                        Messages.Insert(0, DateTime.Now + ":: Changed to original Group A");
                        if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy off", false))
                            Messages.Insert(0, DateTime.Now + ":: Error changing to Group A: " + bufferReturnInfo.Descriptor);
                        break;
                    case "Group B":
                        summitSensing.StopStreaming(theSummit, false);
                        bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group1);
                        summitSensing.StartStreaming(theSummit, senseConfig, false);
                        Messages.Insert(0, DateTime.Now + ":: Changed to original Group B");
                        if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy off", false))
                            Messages.Insert(0, DateTime.Now + ":: Error changing to Group B: " + bufferReturnInfo.Descriptor);
                        break;
                    case "Group C":
                        summitSensing.StopStreaming(theSummit, false);
                        bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group2);
                        summitSensing.StartStreaming(theSummit, senseConfig, false);
                        Messages.Insert(0, DateTime.Now + ":: Changed to original Group C");
                        if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy off", false))
                            Messages.Insert(0, DateTime.Now + ":: Error changing to Group C: " + bufferReturnInfo.Descriptor);
                        break;
                    case "Group D":
                        summitSensing.StopStreaming(theSummit, false);
                        bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group3);
                        summitSensing.StartStreaming(theSummit, senseConfig, false);
                        Messages.Insert(0, DateTime.Now + ":: Changed to original Group D");
                        if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy off", false))
                            Messages.Insert(0, DateTime.Now + ":: Error changing to Group D: " + bufferReturnInfo.Descriptor);
                        break;
                    default:
                        Messages.Insert(0, DateTime.Now + ":: Error: could not change groups. Please use RLP or PTM to switch back to original settings.");
                        return;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                Messages.Insert(0, DateTime.Now + ":: Error: could not change groups. Please use RLP or PTM to switch back to original settings.");
                return;
            }
            UpdateStimStatusGroup(true, true, true);
            Thread.Sleep(500);
            //reset amp
            if (!ResetStimAmp(localOriginalAmp))
            {
                Messages.Insert(0, DateTime.Now + ":: ERROR: Could not set stim amp to original setting. Please exit the application adjust with the PTM or RLP");
            }
            else
            {
                Messages.Insert(0, DateTime.Now + ":: Successfully set stim amp back to original settings");
            }
            Thread.Sleep(500);
            //reset rate
            if (!ResetStimRate(localOriginalRate))
            {
                Messages.Insert(0, DateTime.Now + ":: ERROR: Could not set stim rate to original setting. Please exit the application adjust with the PTM or RLP");
            }
            else
            {
                Messages.Insert(0, DateTime.Now + ":: Successfully set stim rate back to original settings");
            }
            Thread.Sleep(500);
            //reset pw
            if (!ResetStimPulseWidth(localOriginalPW))
            {
                Messages.Insert(0, DateTime.Now + ":: ERROR: Could not set pulse width to original setting. Please exit the application adjust with the PTM or RLP");
            }
            else
            {
                Messages.Insert(0, DateTime.Now + ":: Successfully set pulse width back to original settings");
            }
            Thread.Sleep(500);
            //reset therapy
            if (localOriginalGroup.Equals("TherapyActive") && StimActiveDisplay.Equals("TherapyOff"))
            {
                try
                {
                    //Turn stim on in case it's off
                    bufferReturnInfo = theSummit.StimChangeTherapyOn();
                    Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy ON: " + bufferReturnInfo.Descriptor);
                    if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy On", false))
                    {
                        Messages.Insert(0, DateTime.Now + ":: Error turning Stim Therapy ON: " + bufferReturnInfo.Descriptor);
                    }
                    else
                    {
                        Messages.Insert(0, DateTime.Now + ":: Successfully set stim therapy back to original settings");
                    }
                    // Reset POR if set
                    if (bufferReturnInfo.RejectCodeType == typeof(MasterRejectCode)
                        && (MasterRejectCode)bufferReturnInfo.RejectCode == MasterRejectCode.ChangeTherapyPor)
                    {
                        resetPOR(theSummit);
                        bufferReturnInfo = theSummit.StimChangeTherapyOn();
                        _log.Info("Turn stim therapy on after resetPOR success in Stim Sweep button click");
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    Messages.Insert(0, DateTime.Now + ":: Error: turning Stim Therapy ON");
                }
            }
            else if (localOriginalGroup.Equals("TherapyOff") && StimActiveDisplay.Equals("TherapyActive"))
            {
                try
                {
                    Messages.Insert(0, DateTime.Now + ":: Turning Stim Therapy OFF");
                    _log.Info("Turning Stim Therapy OFF");
                    bufferReturnInfo = theSummit.StimChangeTherapyOff(false);
                    if (CheckForReturnError(bufferReturnInfo, "Turn Stim Therapy off", false))
                    {
                        _log.Info("Error: Turning Stim Therapy OFF: " + bufferReturnInfo.Descriptor);
                        Messages.Insert(0, DateTime.Now + ":: Error turning Stim Therapy OFF: " + bufferReturnInfo.Descriptor);
                    }
                    else
                    {
                        Messages.Insert(0, DateTime.Now + ":: Successfully set stim therapy back to original settings");
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    Messages.Insert(0, DateTime.Now + ":: Error: turning Stim Therapy OFF");
                }
            }
            UpdateStimStatusGroup(true, true, true);
        }
        private double ChangeStringToDouble(string val)
        {
            return Convert.ToDouble(val);
        }

        private ActiveGroup ConvertStimModelGroupToAPIGroup(string groupValue)
        {
            ActiveGroup activeGroup = ActiveGroup.Group0;
            if (groupValue == null)
            {
                Messages.Insert(0, DateTime.Now + ":: --ERROR: Group Value is null. Group set to default group A");
                return activeGroup;
            }
            switch (groupValue)
            {
                case "A":
                    activeGroup = ActiveGroup.Group0;
                    break;
                case "B":
                    activeGroup = ActiveGroup.Group1;
                    break;
                case "C":
                    activeGroup = ActiveGroup.Group2;
                    break;
                case "D":
                    activeGroup = ActiveGroup.Group3;
                    break;
                default:
                    Messages.Insert(0, DateTime.Now + ":: --ERROR: Group Value incorrect. Group set to default group A");
                    break;
            }
            return activeGroup;
        }
        #endregion
    }
}
