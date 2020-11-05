/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.Models;
using Medtronic.SummitAPI.Classes;

/// <summary>
/// This class is part of the MainViewModel class and it implements the code for the Report tab.
/// Includes the bindings for the report tab.
/// Some variable instantiations and initializations are made in MainPageViewModel and its constructor.
/// </summary>
namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    public partial class MainViewModel : Screen
    {
        #region Variables
        private readonly string REPORT_FILEPATH = @"C:\AdaptiveDBS\report_config.json";
        //MEDICATION used for storing in event log that we are storing medication data
        private readonly static string MEDICATION = "medication";
        private readonly static string CONDITIONS = "conditions";
        private readonly static string EXTRA_COMMENTS = "extra_comments";
        private static ReportModel reportConfig = null;        
        //isReportConfigFileFound is used to make sure we have successfully loaded the report config file
        //if successful, then we can add the data. If not, don't add data since report config file invalid
        private static bool isReportConfigFileFound = false;
        //variable to add additional comments from the report box. Used in AdditionalCommentsForReportBox
        private string _additionalCommentsForReportBox;
        //Binding for the medication list and condition list selectable boxes
        private ObservableCollection<MedicationCheckBoxClass> _medicationList = new ObservableCollection<MedicationCheckBoxClass>();
        private ObservableCollection<ConditionCheckBoxClass> _conditionList = new ObservableCollection<ConditionCheckBoxClass>();
        /// <summary>
        /// Binding for the Medication time
        /// </summary>
        public string MedicationTime { get; set; }
        #endregion

        #region Binds Collections for MedicationList, ConditionList and Additional Comments
        /// <summary>
        /// The list of medications that are displayed to user
        /// </summary>
        public ObservableCollection<MedicationCheckBoxClass> MedicationList
        {
            get { return _medicationList; }
            set
            {
                _medicationList = value;
                NotifyOfPropertyChange(() => MedicationList);
            }
        }
        /// <summary>
        /// The list of conditions that are displayed to user
        /// </summary>
        public ObservableCollection<ConditionCheckBoxClass> ConditionList
        {
            get { return _conditionList; }
            set
            {
                _conditionList = value;
                NotifyOfPropertyChange(() => ConditionList);
            }
        }
        /// <summary>
        /// Text box for any additional information user wants to input
        /// </summary>
        public string AdditionalCommentsForReportBox
        {
            get { return _additionalCommentsForReportBox; }
            set
            {
                _additionalCommentsForReportBox = value;
                NotifyOfPropertyChange(() => AdditionalCommentsForReportBox);
            }
        }
        #endregion

        #region Report and Reset Button Clicks
        /// <summary>
        /// Report button click that reports data to Event log in medtronic json file
        /// </summary>
        public void ReportClick()
        {
            //this adds conditions to string
            string conditions = "";
            foreach(ConditionCheckBoxClass itemToAddIfChecked in ConditionList)
            {
                if (itemToAddIfChecked.IsSelected)
                    conditions += itemToAddIfChecked.Condition + ", ";
            }
            //This adds medications to string
            string medications = "";
            foreach (MedicationCheckBoxClass itemToCheckIfChecked in MedicationList)
            {
                if (itemToCheckIfChecked.IsSelected)
                    medications += itemToCheckIfChecked.Medication + ", ";
            }
            //This adds both medications and conditions to event log
            if (theSummit != null)
            {
                if (!theSummit.IsDisposed)
                {
                    //Make sure there is a medication checked before writing. No need to write if nothing there.
                    if (!string.IsNullOrEmpty(medications))
                    {
                        //add time to medications only if available
                        if (MedicationTime != null)
                        {
                            medications += " --- " + MedicationTime.ToString();
                            medications = medications.Replace('/', '_');
                        }
                        try
                        {
                            //Adds data to event log
                            APIReturnInfo result = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, MEDICATION, medications);
                            if (result.RejectCode != 0)
                            {
                                Messages.Insert(0, DateTime.Now + ":: Error writing medications report: " + result.Descriptor);
                            }
                            else
                            {
                                Messages.Insert(0, DateTime.Now + ":: Medications Reported Successully: " + medications);
                            }
                        }catch(Exception e)
                        {
                            Messages.Insert(0, DateTime.Now + ":: Error writing medications report: " + e.Message);
                            _log.Error(e);
                        }
                    }
                    //Make sure there is data for conditions checked
                    //If no data, then nothing to write
                    if (!string.IsNullOrEmpty(conditions))
                    {
                        try
                        {
                            //Adds data to event log
                            APIReturnInfo result = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, CONDITIONS, conditions);
                            if (result.RejectCode != 0)
                            {
                                Messages.Insert(0, DateTime.Now + ":: Error writing conditions report: " + result.Descriptor);
                            }
                            else
                            {
                                Messages.Insert(0, DateTime.Now + ":: Conditions Reported Successully: " + conditions);
                            }
                        }
                        catch (Exception e)
                        {
                            Messages.Insert(0, DateTime.Now + ":: Error writing conditions report: " + e.Message);
                            _log.Error(e);
                        }
                    }
                    //Make sure there is data inside the additional comments box
                    //If no data, then nothing to write
                    if (!string.IsNullOrEmpty(AdditionalCommentsForReportBox))
                    {
                        try
                        {
                            //Adds data to event log
                            APIReturnInfo result = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, EXTRA_COMMENTS, AdditionalCommentsForReportBox);
                            if (result.RejectCode != 0)
                            {
                                Messages.Insert(0, DateTime.Now + ":: Error writing extra comments report: " + result.Descriptor);
                            }
                            else
                            {
                                Messages.Insert(0, DateTime.Now + ":: Extra Comments Reported Successully: " + AdditionalCommentsForReportBox);
                            }
                        }
                        catch (Exception e)
                        {
                            Messages.Insert(0, DateTime.Now + ":: Error writing extra comments report: " + e.Message);
                            _log.Error(e);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset button click resets info and unchecks all data
        /// This also reloads the report_config.json file in case there are any changes
        /// //Allows for real-time update of report lists for medications and conditions
        /// </summary>
        public void ResetClick()
        {
            //Reset all values and gets the data from the report config file again in case there was a change
            //Allows for real-time update of report lists for medications and conditions
            AdditionalCommentsForReportBox = "";
            ConditionList.Clear();
            MedicationList.Clear();
            reportConfig = jSONService?.GetReportModelFromFile(REPORT_FILEPATH);
            GetListOfMedicationsConditionsFromConfig();
        }
        #endregion

        #region Gets Data from Report Config file
        /// <summary>
        /// Fills the MedicationList and ConditionList with the data from the reportConfig.Medications and reportCofig.Symptoms variables, respectively.
        /// reportConfig variable is set from GetReportJSONFile()
        /// </summary>
        public void GetListOfMedicationsConditionsFromConfig()
        {
            //add to list of medications and conditions from the config file
            int index = 1;
            if (isReportConfigFileFound)
            {
                foreach (string medication in reportConfig.Medications)
                {
                    MedicationList.Add(new MedicationCheckBoxClass { Medication = medication, Index = index, IsSelected = false });
                    index++;
                }
                index = 1;
                foreach (string condition in reportConfig.Symptoms)
                {
                    ConditionList.Add(new ConditionCheckBoxClass { Condition = condition, Index = index, IsSelected = false });
                    index++;
                }
            }
        }
        #endregion
    }

    #region ConditionCheckBoxClass, MedicationCheckBoxClass
    /// <summary>
    /// Classes for the conditions list. Allows them to be selectable
    /// </summary>
    public class ConditionCheckBoxClass
    {
        /// <summary>
        /// Condition name
        /// </summary>
        public string Condition { get; set; }
        /// <summary>
        /// What number in the list it is
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// If it is selected or not
        /// </summary>
        public bool IsSelected { get; set; }
    }
    /// <summary>
    /// Classes for the medications list. Allows them to be selectable
    /// </summary>
    public class MedicationCheckBoxClass
    {
        /// <summary>
        /// Medication name
        /// </summary>
        public string Medication { get; set; }
        /// <summary>
        /// What number in the list it is
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// If it is selected or not
        /// </summary>
        public bool IsSelected { get; set; }
    }
    #endregion
}
