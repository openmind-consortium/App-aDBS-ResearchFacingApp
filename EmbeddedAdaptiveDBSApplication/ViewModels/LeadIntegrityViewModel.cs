/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using Medtronic.NeuroStim.Olympus.DataTypes.Measurement;
using Medtronic.SummitAPI.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    public partial class MainViewModel : Screen
    {
        private string _caseInputBox = "16";
        private byte caseValue = 16;
        private bool _leadIntegrityButtonEnabled = true;

        /// <summary>
        /// Binding for the case value for lead integrity
        /// </summary>
        public string CaseInputBox
        {
            get { return _caseInputBox; }
            set
            {
                _caseInputBox = value;
                NotifyOfPropertyChange(() => CaseInputBox);
            }
        }
        /// <summary>
        /// Enables/disables lead integrity button
        /// </summary>
        public bool LeadIntegrityButtonEnabled
        {
            get { return _leadIntegrityButtonEnabled; }
            set
            {
                _leadIntegrityButtonEnabled = value;
                NotifyOfPropertyChange(() => LeadIntegrityButtonEnabled);
            }
        }
        /// <summary>
        /// Run a flattened lead integrity test
        /// </summary>
        public async Task RunLeadIntegrityTestButton()
        {
            LeadIntegrityButtonEnabled = false;
            IsSpinnerVisible = true;
            await Task.Run(() => RunLeadIntegrity());
            IsSpinnerVisible = false;
            LeadIntegrityButtonEnabled = true;
        }

        private void RunLeadIntegrity()
        {
            if (theSummit != null)
            {
                if (!theSummit.IsDisposed)
                {
                    Messages.Clear();
                    Messages.Insert(0, DateTime.Now + ":: New impedance test:");
                    if (String.IsNullOrWhiteSpace(CaseInputBox) || !byte.TryParse(CaseInputBox, out byte nothing))
                    {
                        ShowMessageBox("Case value missing or incorrect format. Please fix and try again", "Error");
                        return;
                    }
                    else if (!String.IsNullOrWhiteSpace(CaseInputBox) && byte.TryParse(CaseInputBox, out byte result))
                    {
                        if (result <= 16 && result >= 0)
                        {
                            caseValue = result;
                        }
                        else
                        {
                            ShowMessageBox("Case value outside of bounds. Please enter a valid case number and try again", "Error");
                            return;
                        }
                    }
                    try
                    {
                        Messages.Add("Running " + leadLocation1);
                        LeadIntegrityTestResult testResultBuffer;
                        APIReturnInfo testReturnInfo;
                        testReturnInfo = theSummit.LeadIntegrityTest(
                                        new List<Tuple<byte, byte>> {
                                            new Tuple<byte, byte>(0, caseValue),
                                            new Tuple<byte, byte>(1, caseValue),
                                            new Tuple<byte, byte>(2, caseValue),
                                            new Tuple<byte, byte>(3, caseValue),
                                            new Tuple<byte, byte>(0, 1),
                                            new Tuple<byte, byte>(0, 2),
                                            new Tuple<byte, byte>(0, 3),
                                            new Tuple<byte, byte>(1, 2),
                                            new Tuple<byte, byte>(1, 3),
                                            new Tuple<byte, byte>(2, 3)
                    },
                    out testResultBuffer);
                        // Make sure returned structure isn't null
                        if (testReturnInfo.RejectCode == 0 && testResultBuffer != null)
                        {
                            // Write out result to the console
                            Messages.Add("Test Result Impedance (0, " + caseValue + "): " + testResultBuffer.PairResults[0].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(0," + caseValue + ")", testResultBuffer.PairResults[0].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (1, " + caseValue + "): " + testResultBuffer.PairResults[1].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(1," + caseValue + ")", testResultBuffer.PairResults[1].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (2, " + caseValue + "): " + testResultBuffer.PairResults[2].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(2," + caseValue + ")", testResultBuffer.PairResults[2].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (3, " + caseValue + "): " + testResultBuffer.PairResults[3].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(3," + caseValue + ")", testResultBuffer.PairResults[3].Impedance.ToString());
                            Messages.Add("Test Result Impedance (0, 1): " + testResultBuffer.PairResults[4].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(0,1)", testResultBuffer.PairResults[4].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (0, 2): " + testResultBuffer.PairResults[5].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(0,2)", testResultBuffer.PairResults[5].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (0, 3): " + testResultBuffer.PairResults[6].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(0,3)", testResultBuffer.PairResults[6].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (1, 2): " + testResultBuffer.PairResults[7].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(1,2)", testResultBuffer.PairResults[7].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (1, 3): " + testResultBuffer.PairResults[8].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(1,3)", testResultBuffer.PairResults[8].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (2, 3): " + testResultBuffer.PairResults[9].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(2,3)", testResultBuffer.PairResults[9].Impedance.ToString());
                        }
                        else
                        {
                            Messages.Insert(0, DateTime.Now + ":: --ERROR from Medtronic API. Reject Description: " + testReturnInfo.Descriptor + ". Reject Code: " + testReturnInfo.RejectCode);
                        }

                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: --ERROR: Could not run Lead Integrity Test");
                        _log.Error(e);
                    }

                    Messages.Add("-----------------------------------------------");

                    try
                    {
                        Messages.Add("Running " + leadLocation2);
                        LeadIntegrityTestResult testResultBuffer;
                        APIReturnInfo testReturnInfo;
                        testReturnInfo = theSummit.LeadIntegrityTest(
                                        new List<Tuple<byte, byte>> {
                                            new Tuple<byte, byte>(8, caseValue),
                                            new Tuple<byte, byte>(9, caseValue),
                                            new Tuple<byte, byte>(10, caseValue),
                                            new Tuple<byte, byte>(11, caseValue),
                                            new Tuple<byte, byte>(8, 9),
                                            new Tuple<byte, byte>(8, 10),
                                            new Tuple<byte, byte>(8, 11),
                                            new Tuple<byte, byte>(9, 10),
                                            new Tuple<byte, byte>(9, 11),
                                            new Tuple<byte, byte>(10, 11)
                    },
                    out testResultBuffer);
                        // Make sure returned structure isn't null
                        if (testReturnInfo.RejectCode == 0 && testResultBuffer != null)
                        {
                            // Write out result to the console
                            Messages.Add("Test Result Impedance: (8, " + caseValue + "): " + testResultBuffer.PairResults[0].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(8," + caseValue + ")", testResultBuffer.PairResults[0].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (9, " + caseValue + "): " + testResultBuffer.PairResults[1].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(9," + caseValue + ")", testResultBuffer.PairResults[1].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (10, " + caseValue + "): " + testResultBuffer.PairResults[2].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(10," + caseValue + ")", testResultBuffer.PairResults[2].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (11, " + caseValue + "): " + testResultBuffer.PairResults[3].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(11," + caseValue + ")", testResultBuffer.PairResults[3].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (8, 9): " + testResultBuffer.PairResults[4].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(8,9)", testResultBuffer.PairResults[4].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (8, 10): " + testResultBuffer.PairResults[5].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(8,10)", testResultBuffer.PairResults[5].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (8, 11): " + testResultBuffer.PairResults[6].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(8,11)", testResultBuffer.PairResults[6].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (9, 10): " + testResultBuffer.PairResults[7].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(9,10)", testResultBuffer.PairResults[7].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (9, 11): " + testResultBuffer.PairResults[8].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(9,11)", testResultBuffer.PairResults[8].Impedance.ToString());
                            Messages.Add("Test Result Impedance: (10, 11): " + testResultBuffer.PairResults[9].Impedance.ToString());
                            LogLeadIntegrityAsEvent("(10,11)", testResultBuffer.PairResults[9].Impedance.ToString());
                        }
                        else
                        {
                            Messages.Insert(0, DateTime.Now + ":: --ERROR from Medtronic API. Reject Description: " + testReturnInfo.Descriptor + ". Reject Code: " + testReturnInfo.RejectCode);
                        }
                    }
                    catch (Exception e)
                    {
                        Messages.Insert(0, DateTime.Now + ":: --ERROR: Could not run Lead Integrity Test");
                        _log.Error(e);
                    }
                }
            }
        }

        private void LogLeadIntegrityAsEvent(string pairs, string result)
        {
            try
            {
                bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Lead Integrity" , pairs + " --- " + result);
            }
            catch(Exception e)
            {
                ShowMessageBox("Could not log event. If you would like all lead integrity results logged, please try again.", "Error Logging");
                _log.Error(e);
            }
            if(bufferReturnInfo.RejectCode != 0)
            {
                ShowMessageBox("Could not log event. If you would like all lead integrity results logged, please try again.", "Error Logging");
                _log.Warn("Could not log lead integrity event. Reject code: " + bufferReturnInfo.RejectCode + ". Reject description: " + bufferReturnInfo.Descriptor);
            }
        }
    }
}
