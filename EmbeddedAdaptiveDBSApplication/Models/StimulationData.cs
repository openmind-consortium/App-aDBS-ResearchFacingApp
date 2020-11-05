/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using Medtronic.NeuroStim.Olympus.DataTypes.DeviceManagement;
using Medtronic.NeuroStim.Olympus.DataTypes.Therapy;
using Medtronic.SummitAPI.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EmbeddedAdaptiveDBSApplication.Models
{
    /// <summary>
    /// Class that handles getting the simulation data from the INS
    /// This data is used to report status to user in display
    /// </summary>
    class StimulationData
    {
        #region Variables
        //APIReturnInfo for checking return values from api
        APIReturnInfo bufferInfo = new APIReturnInfo();
        //stim status variables set to empty in case could not get data
        //This will just set it with empty string instead of null
        private string activeGroup = "";
        private int pulseWidth = 0;
        private int pulseWidthLowerLimit = 0;
        private int pulseWidthUpperLimit = 0;
        private double stimAmp = 0;
        private double stimAmpLowerLimit = 0;
        private double stimAmpUpperLimit = 0;
        private double stimRate = 0;
        private double stimRateLowerLimit = 0;
        private double stimRateUpperLimit = 0;
        private string stimState = "";
        private string stimElectrodes = "";
        private ILog _log;
        #endregion

        public StimulationData(ILog _log)
        {
            this._log = _log;
        }

        #region Get Active Group and Stim Therapy Status from API
        /// <summary>
        /// Gets the Active group from the api
        /// </summary>
        /// <param name="theSummit">SummitSystem to make api calls to INS</param>
        /// <returns>The active group in the format Group A instead of the format returned from medtonic such as Group0 or empty string if an error occurred</returns>
        public string GetActiveGroup(ref SummitSystem theSummit)
        {
            if(theSummit == null)
            {
                if (!theSummit.IsDisposed)
                {
                    return "";
                }
            }
            GeneralInterrogateData insGeneralInfo = null;
            try
            {
                //Get the group from the api call
                do
                {
                    bufferInfo = theSummit.ReadGeneralInfo(out insGeneralInfo);
                    if (insGeneralInfo == null)
                    {
                        continue;
                    }
                    else
                    {
                        activeGroup = insGeneralInfo.TherapyStatusData.ActiveGroup.ToString();
                    }

                } while (!insGeneralInfo.TherapyStatusData.TherapyStatus.ToString().Equals("TherapyActive") && !insGeneralInfo.TherapyStatusData.TherapyStatus.ToString().Equals("TherapyOff"));
                
                

                _log.Info("Ins active group:" + activeGroup);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            //This returns the converted group from something like Group0 to Group A
            return ConvertActiveGroupToReadableForm(activeGroup);
        }

        /// <summary>
        /// Gets the Stim Therapy Status from the API
        /// </summary>
        /// <param name="theSummit">SummitSystem for making the call to the API to the INS</param>
        /// <returns>String showing Therapy Active or Therapy Inactive or empty string if error occurred</returns>
        public string GetTherapyStatus(ref SummitSystem theSummit)
        {
            if (theSummit == null)
            {
                if (!theSummit.IsDisposed)
                {
                    return "";
                }
            }
            GeneralInterrogateData insGeneralInfo = null;
            try
            {
                //Add a sleep in there to allow status of therapy to get pass Therapy Transitioning state
                //Allows it to either be Active or InActive and not inbetween
                Thread.Sleep(200);
                //Get data from api
                bufferInfo = theSummit.ReadGeneralInfo(out insGeneralInfo);
                if (!CheckReturnFromAPI("Reading General Info from INS", bufferInfo))
                {
                    return "";
                }
                //parse insGeneralInfo to get stim therapy status
                if (insGeneralInfo != null)
                    stimState = insGeneralInfo.TherapyStatusData.TherapyStatus.ToString();

                _log.Info("Ins active group:" + stimState);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            return stimState;
        }
        #endregion

        #region Gets Stim parameters based on group. This is called from outside the class
        /// <summary>
        /// Gets the Stim parameters for Group A
        /// </summary>
        /// <param name="theSummit">SummitSystem for making the api call to INS</param>
        /// <param name="programNumber">Group Program number</param>
        /// <returns>StimParameterModel that contains stim amp, stim rate and pulse width</returns>
        public StimParameterModel GetStimParameterModelGroupA(ref SummitSystem theSummit, int programNumber)
        {
            if (theSummit == null)
            {
                if (!theSummit.IsDisposed)
                {
                    return null;
                }
            }
            // Read the stimulation settings from the device
            StimParameterModel StimParameterModel = GetStimParameterModel(ref theSummit, GroupNumber.Group0, programNumber);
            _log.Info("STIM PARAMS GROUP A: pulse width = " + StimParameterModel.PulseWidth + ", stim rate = " + StimParameterModel.StimRate + ", stim amp = " + StimParameterModel.StimAmp);
            return StimParameterModel;
        }

        /// <summary>
        /// Gets the Stim parameters for Group B
        /// </summary>
        /// <param name="theSummit">SummitSystem for making the api call to INS</param>
        /// <param name="programNumber">Group Program number</param>
        /// <returns>StimParameterModel that contains stim amp, stim rate and pulse width</returns>
        public StimParameterModel GetStimParameterModelGroupB(ref SummitSystem theSummit, int programNumber)
        {
            if (theSummit == null)
            {
                if (!theSummit.IsDisposed)
                {
                    return null;
                }
            }
            // Read the stimulation settings from the device
            StimParameterModel StimParameterModel = GetStimParameterModel(ref theSummit, GroupNumber.Group1, programNumber);
            _log.Info("STIM PARAMS GROUP B: pulse width = " + StimParameterModel.PulseWidth + ", stim rate = " + StimParameterModel.StimRate + ", stim amp = " + StimParameterModel.StimAmp);
            return StimParameterModel;
        }

        /// <summary>
        /// Gets the Stim parameters for Group C
        /// </summary>
        /// <param name="theSummit">SummitSystem for making the api call to INS</param>
        /// <param name="programNumber">Group Program number</param>
        /// <returns>StimParameterModel that contains stim amp, stim rate and pulse width</returns>
        public StimParameterModel GetStimParameterModelGroupC(ref SummitSystem theSummit, int programNumber)
        {
            if (theSummit == null)
            {
                if (!theSummit.IsDisposed)
                {
                    return null;
                }
            }
            // Read the stimulation settings from the device
            StimParameterModel StimParameterModel = GetStimParameterModel(ref theSummit, GroupNumber.Group2, programNumber);
            _log.Info("STIM PARAMS GROUP C: pulse width = " + StimParameterModel.PulseWidth + ", stim rate = " + StimParameterModel.StimRate + ", stim amp = " + StimParameterModel.StimAmp);
            return StimParameterModel;
        }

        /// <summary>
        /// Gets the Stim parameters for Group D
        /// </summary>
        /// <param name="theSummit">SummitSystem for making the api call to INS</param>
        /// <param name="programNumber">Group Program number</param>
        /// <returns>StimParameterModel that contains stim amp, stim rate and pulse width</returns>
        public StimParameterModel GetStimParameterModelGroupD(ref SummitSystem theSummit, int programNumber)
        {
            if (theSummit == null)
            {
                if (!theSummit.IsDisposed)
                {
                    return null;
                }
            }
            // Read the stimulation settings from the device
            StimParameterModel StimParameterModel = GetStimParameterModel(ref theSummit, GroupNumber.Group3, programNumber);
            _log.Info("STIM PARAMS GROUP D: pulse width = " + StimParameterModel.PulseWidth + ", stim rate = " + StimParameterModel.StimRate + ", stim amp = " + StimParameterModel.StimAmp);
            return StimParameterModel;
        }
        #endregion

        #region Helper Functions for Converting Group Format and Getting Stim Parameters from API
        /// <summary>
        /// Gets the stim parameters based on group from the actual API
        /// </summary>
        /// <param name="theSummit">SummitSystem for making the API call to INS</param>
        /// <param name="groupNumber">Group number corresponding to which group we want to get stim parameters from such as Group0, Group1, etc</param>
        /// <param name="programNumber">Group Program number</param>
        /// <returns>StimParameterModel that contains stim amp, stim rate and pulse width</returns>
        private StimParameterModel GetStimParameterModel(ref SummitSystem theSummit, GroupNumber groupNumber, int programNumber)
        {
            if (theSummit == null)
            {
                if (!theSummit.IsDisposed)
                {
                    return null;
                }
            }
            TherapyGroup insStateGroup = null;
            StimParameterModel StimParameterModel;
            try
            {
                int counter = 5;
                //Get the data from the api
                do
                {
                    bufferInfo = theSummit.ReadStimGroup(groupNumber, out insStateGroup);
                    counter--;
                } while (bufferInfo.RejectCode != 0 && counter > 0);
                
                if (!CheckReturnFromAPI("Reading Stim Group", bufferInfo) || counter == 0)
                {
                    StimParameterModel = new StimParameterModel();
                    return StimParameterModel;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            try
            {
                //parse the data to get the pulsewidth
                if (insStateGroup != null)
                {
                    pulseWidth = insStateGroup.Programs[programNumber].PulseWidthInMicroseconds;
                    stimRate = insStateGroup.RateInHz;
                    stimAmp = insStateGroup.Programs[programNumber].AmplitudeInMilliamps;
                }   
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            //Set the Model with these values and return model
            StimParameterModel = new StimParameterModel(pulseWidth, stimRate, stimAmp);
            return StimParameterModel;
        }

        public TherapyLimitsForGroupModel GetTherapyLimitsStimContactsForGroup(SummitSystem theSummit, GroupNumber groupNumber)
        {
            if (theSummit == null)
            {
                if (!theSummit.IsDisposed)
                {
                    return null;
                }
            }
            TherapyLimitsForGroupModel therapyLimits = new TherapyLimitsForGroupModel();
            TherapyGroup insStateGroup = null;
            AmplitudeLimits ampLimits = null;
            try
            {
                //Get the data from the api
                int counter = 5;
                do
                {
                    bufferInfo = theSummit.ReadStimGroup(groupNumber, out insStateGroup);
                    counter--;
                } while ((insStateGroup == null || bufferInfo.RejectCode != 0) && counter > 0);
                if(counter == 0)
                {
                    MessageBox.Show("Error reading stim group. Please try again", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return therapyLimits;
                }
                counter = 5;
                do
                {
                    bufferInfo = theSummit.ReadStimAmplitudeLimits(groupNumber, out ampLimits);
                    counter--;
                } while ((ampLimits == null || bufferInfo.RejectCode != 0) && counter > 0);
                if (counter == 0)
                {
                    MessageBox.Show("Error reading stim amp limits. Please try again", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return therapyLimits;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            try
            {
                if (insStateGroup != null)
                {
                    therapyLimits.PulseWidthLowerLimit = insStateGroup.PulseWidthLowerLimitInMicroseconds;
                    therapyLimits.PulseWidthUpperLimit = insStateGroup.PulseWidthUpperLimitInMicroseconds;
                    therapyLimits.StimRateLowerLimit = insStateGroup.RateLowerLimitInHz;
                    therapyLimits.StimRateUpperLimit = insStateGroup.RateUpperLimitInHz;
                    therapyLimits.StimElectrodesProg0 = FindStimElectrodes(insStateGroup, 0);
                    therapyLimits.StimElectrodesProg1 = FindStimElectrodes(insStateGroup, 1);
                    therapyLimits.StimElectrodesProg2 = FindStimElectrodes(insStateGroup, 2);
                    therapyLimits.StimElectrodesProg3 = FindStimElectrodes(insStateGroup, 3);
                    ActiveRechargeRatios activeRechargeActive = insStateGroup.Programs[0].MiscSettings.ActiveRechargeRatio;
                    if (activeRechargeActive.Equals(ActiveRechargeRatios.PassiveOnly))
                    {
                        therapyLimits.ActiveRechargeStatus = "ActiveRechargeOff";
                    }
                    else
                    {
                        therapyLimits.ActiveRechargeStatus = "ActiveRechargeOn";
                    }
                }
                if(ampLimits != null)
                {
                    therapyLimits.StimAmpLowerLimitProg0 = ampLimits.Prog0LowerInMilliamps;
                    therapyLimits.StimAmpUpperLimitProg0 = ampLimits.Prog0UpperInMilliamps;
                    therapyLimits.StimAmpLowerLimitProg1 = ampLimits.Prog1LowerInMilliamps;
                    therapyLimits.StimAmpUpperLimitProg1 = ampLimits.Prog1UpperInMilliamps;
                    therapyLimits.StimAmpLowerLimitProg2 = ampLimits.Prog2LowerInMilliamps;
                    therapyLimits.StimAmpUpperLimitProg2 = ampLimits.Prog2UpperInMilliamps;
                    therapyLimits.StimAmpLowerLimitProg3 = ampLimits.Prog3LowerInMilliamps;
                    therapyLimits.StimAmpUpperLimitProg3 = ampLimits.Prog3UpperInMilliamps;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            return therapyLimits;
        }

        private string FindStimElectrodes(TherapyGroup therapyGroup, int programNumber)
        {
            if(programNumber > 3 || programNumber < 0)
            {
                return "Incorrect Program Number";
            }
            string electrodesStimming = "";
            if (therapyGroup.Valid)
            {
                if (therapyGroup.Programs[programNumber].Valid)
                {
                    for(int i = 0; i < 17; i++)
                    {
                        if (!therapyGroup.Programs[programNumber].Electrodes[i].IsOff)
                        {
                            //Case is 16 so it gets a C. Otherwise give the number
                            if(i == 16)
                            {
                                electrodesStimming += "C";
                            }
                            else
                            {
                                electrodesStimming += i.ToString();
                            }
                            
                            // What type of electrode is it?
                            switch (therapyGroup.Programs[programNumber].Electrodes[i].ElectrodeType)
                            {
                                case ElectrodeTypes.Cathode:
                                    electrodesStimming += "-";
                                    break;
                                case ElectrodeTypes.Anode:
                                    electrodesStimming += "+";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    return "Invalid Program";
                }
            }
            else
            {
                return "Invalid Group";
            }
            return electrodesStimming;
        }

        /// <summary>
        /// This methods converts the groups that are read from the device and converts them to a readable form for displaying to user
        /// </summary>
        /// <param name="group">Medtronic API call format such as Group0, Group1, Group2 or Group3</param>
        /// <returns>Group A, Group B, Group C or Group D</returns>
        private string ConvertActiveGroupToReadableForm(string group)
        {
            string tempGroup = "";
            if (string.IsNullOrEmpty(group))
            {
                return tempGroup;
            }
            switch (group)
            {
                case "Group0":
                    tempGroup = "Group A";
                    break;
                case "Group1":
                    tempGroup = "Group B";
                    break;
                case "Group2":
                    tempGroup = "Group C";
                    break;
                case "Group3":
                    tempGroup = "Group D";
                    break;
                default:
                    tempGroup = "";
                    break;
            }
            return tempGroup;
        }

        private bool CheckReturnFromAPI(string location, APIReturnInfo returnInfo)
        {
            if (returnInfo.RejectCode != 0)
            {
                MessageBox.Show("Error from Medtronic API in " + location + ". Reject Code: " + bufferInfo.RejectCode + ". Reject description: " + bufferInfo.Descriptor, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _log.Warn("Error reading stim data: " + bufferInfo.Descriptor.ToString() + "\r\n" + "Reject Code: " + bufferInfo.RejectCode);
                return false;
            }
            return true;
        }
        #endregion
    }
}
