/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.ViewModels;
using Medtronic.SummitAPI.Classes;
using Medtronic.TelemetryM;
using Medtronic.TelemetryM.CtmProtocol.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmbeddedAdaptiveDBSApplication.Models
{
    /// <summary>
    /// Class that makes the connection to the CTM and to the INS
    /// </summary>
    class SummitConnect
    {
        #region Variables
        //skipDiscovery is if the batteries weren't pulled out, then we don't have to discover the INS because it was already found. We can just connect to it
        private bool skipDiscovery = false;
        #endregion

        /// <summary>
        /// Connects to the INS
        /// </summary>
        /// <param name="theSummit">SummitSystem</param>
        /// /// <param name="_log">Caliburn Micro logger</param>
        /// <returns>True if connected or false if not connected</returns>
        public bool ConnectINS(ref SummitSystem theSummit, ILog _log)
        {
            ConnectReturn theWarnings;
            APIReturnInfo connectReturn;
            //Check if you can skip discovery, if successful, return immediately, if not proceed to discover
            //If it has already discovered the INS before then we can skip discovery. 
            //If battery taken out or new connection then we need to discover
            if (skipDiscovery)
            {
                try
                {
                    int count = 5;
                    do
                    {
                        connectReturn = theSummit.StartInsSession(null, out theWarnings, true);
                        _log.Info("Skip Discovery: Start INS session reject code == " + connectReturn.RejectCode + "\r\nReject Code Type: " + connectReturn.RejectCodeType.ToString());
                        if (connectReturn.RejectCode == 0)
                        {
                            return true;
                        }
                        count--;
                    } while (connectReturn.RejectCodeType != typeof(APIRejectCodes) && connectReturn.RejectCode != 12 && count >= 0);
                    skipDiscovery = false;
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
            // Discovery INS with the connected CTM, loop until a device has been discovered
            List<DiscoveredDevice> discoveredDevices;
            int countForDiscoveredDevices = 5;
            try
            {
                do
                {
                    Thread.Sleep(100);
                    theSummit.OlympusDiscovery(out discoveredDevices);
                    _log.Info("Discovery INS with the connected CTM, loop until a device has been discovered");
                    countForDiscoveredDevices--;
                    if (countForDiscoveredDevices <= 0)
                    {
                        _log.Warn("count for discovered device ran out");
                        return false;
                    }
                } while ((discoveredDevices == null || discoveredDevices.Count == 0));

                _log.Info("Olympi found: Creating Summit Interface.");
                // Connect to a device
                int countToAvoidInfiniteLoop = 5;
                do
                {
                    Thread.Sleep(2000); //Add short delay here for connection problems
                    connectReturn = theSummit.StartInsSession(discoveredDevices[0], out theWarnings, true);
                    _log.Info("Discovery: Start INS session reject code == " + connectReturn.RejectCode.ToString() + "\r\nReject Code Type: " + connectReturn.RejectCodeType.ToString());
                    if (connectReturn.RejectCodeType == typeof(InstrumentReturnCode) && (InstrumentReturnCode)connectReturn.RejectCode == InstrumentReturnCode.InvalidDiscoveredCount)
                    {
                        _log.Info("Start INS Session");
                        break;
                    }
                    countToAvoidInfiniteLoop--;
                    if (countToAvoidInfiniteLoop <= 0)
                    {
                        _log.Warn("count for ins connect ran out");
                        return false;
                    }
                } while (theWarnings.HasFlag(ConnectReturn.InitializationError));

                // Write out the final result of the example
                if (connectReturn.RejectCode != 0)
                {
                    _log.Warn("Summit Initialization: INS failed to connect");
                    return false;
                }
                else
                {
                    // Write out the warnings if they exist
                    _log.Info("Summit Initialization: INS connected, warnings: " + theWarnings.ToString());
                    skipDiscovery = true;
                    return true;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Connects to CTM 
        /// </summary>
        /// <param name="theSummitManager">Summit manager</param>
        /// <param name="theSummit">Summit System</param>
        /// <param name="senseModel">Model for sense from config file</param>
        /// <param name="appModel">Model for application from config file</param>
        /// <param name="_log">Caliburn Micro Logger</param>
        /// <param name="useCTMAtIndex">If true use the index of ctm to connect to otherwise connect to first ctm available</param>
        /// <param name="index">index of ctm to connect to</param>
        /// <returns>true if connected or false if not connected</returns>
        public bool ConnectCTM(SummitManager theSummitManager, ref SummitSystem theSummit, SenseModel senseModel, AppModel appModel, ILog _log, bool useCTMAtIndex, int index)
        {
            _log.Info("Checking USB for unbonded CTMs. Please make sure they are powered on.");
            theSummitManager?.GetUsbTelemetry();

            // Retrieve a list of known and bonded telemetry
            List<InstrumentInfo> knownTelemetry = theSummitManager?.GetKnownTelemetry();

            // Check if any CTMs are currently bonded, poll the USB if not so that the user can be prompted to plug in a CTM over USB
            if (knownTelemetry?.Count == 0)
            {
                do
                {
                    // Inform user we will loop until a CTM is found on USBs
                    _log.Warn("No bonded CTMs found, please plug a CTM in via USB...");
                    Thread.Sleep(2000);

                    // Bond with any CTMs plugged in over USB
                    knownTelemetry = theSummitManager?.GetUsbTelemetry();
                } while (knownTelemetry?.Count == 0);
            }

            SummitSystem tempSummit = null;
            if (useCTMAtIndex && index >= 0)
            {
                try
                {
                    ManagerConnectStatus connectReturn;
                    if (appModel == null)
                    {
                        connectReturn = theSummitManager.CreateSummit(out tempSummit, theSummitManager.GetKnownTelemetry()[index], InstrumentPhysicalLayers.Any, senseModel.Mode, senseModel.Ratio, CtmBeepEnables.None);
                    }
                    else if (appModel?.CTMBeepEnables == null)
                    {
                        connectReturn = theSummitManager.CreateSummit(out tempSummit, theSummitManager.GetKnownTelemetry()[index], InstrumentPhysicalLayers.Any, senseModel.Mode, senseModel.Ratio, CtmBeepEnables.None);
                    }
                    else if (!appModel.CTMBeepEnables.DeviceDiscovered && !appModel.CTMBeepEnables.GeneralAlert && !appModel.CTMBeepEnables.NoDeviceDiscovered && !appModel.CTMBeepEnables.TelMCompleted && !appModel.CTMBeepEnables.TelMLost)
                    {
                        connectReturn = theSummitManager.CreateSummit(out tempSummit, theSummitManager.GetKnownTelemetry()[index], InstrumentPhysicalLayers.Any, senseModel.Mode, senseModel.Ratio, CtmBeepEnables.None);
                    }
                    else
                    {
                        connectReturn = theSummitManager.CreateSummit(out tempSummit, theSummitManager.GetKnownTelemetry()[index], InstrumentPhysicalLayers.Any, senseModel.Mode, senseModel.Ratio, ConfigConversions.BeepEnablesConvert(appModel));
                    }

                    // Write out the result
                    _log.Info("Create Summit Result: " + connectReturn.ToString());
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
            else
            {
                // Connect to the first CTM available, then try others if it fails
                try
                {
                    for (int i = 0; i < theSummitManager.GetKnownTelemetry().Count; i++)
                    {
                        ManagerConnectStatus connectReturn;
                        if (appModel == null)
                        {
                            connectReturn = theSummitManager.CreateSummit(out tempSummit, theSummitManager.GetKnownTelemetry()[i], InstrumentPhysicalLayers.Any, senseModel.Mode, senseModel.Ratio, CtmBeepEnables.None);
                        }
                        else if (appModel?.CTMBeepEnables == null)
                        {
                            connectReturn = theSummitManager.CreateSummit(out tempSummit, theSummitManager.GetKnownTelemetry()[i], InstrumentPhysicalLayers.Any, senseModel.Mode, senseModel.Ratio, CtmBeepEnables.None);
                        }
                        else if (!appModel.CTMBeepEnables.DeviceDiscovered && !appModel.CTMBeepEnables.GeneralAlert && !appModel.CTMBeepEnables.NoDeviceDiscovered && !appModel.CTMBeepEnables.TelMCompleted && !appModel.CTMBeepEnables.TelMLost)
                        {
                            connectReturn = theSummitManager.CreateSummit(out tempSummit, theSummitManager.GetKnownTelemetry()[i], InstrumentPhysicalLayers.Any, senseModel.Mode, senseModel.Ratio, CtmBeepEnables.None);
                        }
                        else
                        {
                            connectReturn = theSummitManager.CreateSummit(out tempSummit, theSummitManager.GetKnownTelemetry()[i], InstrumentPhysicalLayers.Any, senseModel.Mode, senseModel.Ratio, ConfigConversions.BeepEnablesConvert(appModel));
                        }

                        // Write out the result
                        _log.Info("Create Summit Result: " + connectReturn.ToString());

                        // Break if it failed successful
                        if (tempSummit != null && connectReturn.HasFlag(ManagerConnectStatus.Success))
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
            

            // Make sure telemetry was connected to, if not fail
            if (tempSummit == null)
            {
                // inform user that CTM was not successfully connected to
                _log.Warn("Failed to connect to CTM...");
                return false;
            }
            else
            {
                theSummit = tempSummit;
                return true;
            }

        }
    }
}
