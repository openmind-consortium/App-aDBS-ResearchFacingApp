/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace EmbeddedAdaptiveDBSApplication.Models
{
    /// <summary>
    /// Class used to valdidate json strings, get model from json strings, and write json files from models
    /// </summary>
    public class JSONService
    {
        #region Variables and Constructors
        //deviceIDForPath, patientIDForPath, projectIDForPath are used when writing files. Needed for path to Medtronic directory
        //filepath is the path to the medtronic json directory found in GetDirectoryPathForCurrentSession()
        private string deviceIDForPath, patientIDForPath, projectIDForPath, filepath, basePathForMedtronicFiles;
        private ILog _log;

        /// <summary>
        /// Constructor for not writing files
        /// </summary>
        /// <param name="_log">Caliburn Micro Logger</param>
        public JSONService(ILog _log)
        {
            this._log = _log;
        }

        /// <summary>
        /// This constructor is used if writing config files
        /// The path to the medtronic directory is needed so each variable is used to find the path
        /// </summary>
        /// <param name="deviceID">Found from SummitSystem.DeviceID from API Call</param>
        /// <param name="patientID">Found from: SubjectInfo subjectInfo; SummitSystem.FlashReadSubjectInfo(out subjectInfo); patientID = subjectInfo.ID; from API Call</param>
        /// <param name="projectID">Project ID that was assigned when assigning summit manager: new SummitManager(projectID, 200, true);</param>
        /// <param name="basePathForMedtronicFiles">Base path matching the registry overwrite for the ORCA files</param>
        /// <param name="_log">Caliburn micro logger</param>
        public JSONService(string deviceID, string patientID, string projectID, string basePathForMedtronicFiles, ILog _log)
        {
            this._log = _log;
            deviceIDForPath = deviceID;
            patientIDForPath = patientID;
            projectIDForPath = projectID;
            this.basePathForMedtronicFiles = basePathForMedtronicFiles;
            GetDirectoryPathForCurrentSession();
        }
        #endregion

        /// <summary>
        /// Validates that the json matches the schema provided
        /// </summary>
        /// <param name="jsonToValidate">json string from the config file</param>
        /// <param name="schema">schema text from SchemaModel.cs that matches the appropriate jsonToValidate string structure</param>
        /// <returns>true is valid and false if not</returns>
        public bool ValidateJSON(string jsonToValidate, string schema)
        {
            //Set to false as default
            bool isValid = false;
            //Messages for anything wrong with json validation. Not used 
            IList<string> messages;
            try
            {
                JSchema jsonSchema = JSchema.Parse(schema);
                Newtonsoft.Json.Linq.JObject person = JObject.Parse(jsonToValidate);
                //Only set to true if json is valid
                isValid = person.IsValid(jsonSchema, out messages);
                if (!isValid)
                {
                    MessageBox.Show("Error messages after failing to validate json file: " + String.Join(",", messages));
                }
            }
            catch(Exception e)
            {
                _log.Error(e);
            }
            return isValid;
        }

        #region Get Sense, Adaptive and Report, Montage and Stim sweep Config files
        /// <summary>
        /// Gets the file from the filepath, validates the file, and converts it to the sense model
        /// </summary>
        /// <param name="filePath">File path for the sense_config.json file to be used to convert</param>
        /// <returns>SenseModel if successful or null if unsuccessful</returns>
        public SenseModel GetSenseModelFromFile(string filePath)
        {
            SenseModel model = null;
            string json = null;
            try
            {
                //read sense config file into string
                using (StreamReader sr = new StreamReader(filePath))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The sense config file could not be read from the file. Please check that it exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _log.Error(e);
                return model;
            }
            if (string.IsNullOrEmpty(json))
            {
                MessageBox.Show("Sense JSON file is empty. Please check that the sense config is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Warn("Sense JSON file is empty after loading file.");
                return model;
            }
            else
            {
                SchemaModel schemaModel = new SchemaModel();
                if (ValidateJSON(json, schemaModel.GetSenseSchema()))
                {
                    //if correct json format, write it into SenseModel
                    try
                    {
                        model = JsonConvert.DeserializeObject<SenseModel>(json);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Could not convert sense config file. Please be sure that sense config file is correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _log.Error(e);
                        return model;
                    }
                }
                else
                {
                    MessageBox.Show("Could not validate sense config file. Please be sure that sense config file is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _log.Warn("Could not validate sense config file.");
                    return model;
                }
            }
            return model;
        }

        /// <summary>
        /// Gets the file from the filepath, validates the file, and converts it to the adaptive model
        /// </summary>
        /// <param name="filePath">File path for the adaptive_config.json file to be used to convert</param>
        /// <returns>AdaptiveModel if successful or null if unsuccessful</returns>
        public AdaptiveModel GetAdaptiveModelFromFile(string filePath)
        {
            AdaptiveModel model = null;
            string json = null;
            try
            {
                //read sense config file into string
                using (StreamReader sr = new StreamReader(filePath))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The adaptive config file could not be read from the file. Please check that it exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _log.Error(e);
                return model;
            }
            if (string.IsNullOrEmpty(json))
            {
                MessageBox.Show("Adaptive JSON file is empty. Please check that the adaptive config is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Warn("Adaptive JSON file is empty after loading file.");
                return model;
            }
            else
            {
                SchemaModel schemaModel = new SchemaModel();
                if (ValidateJSON(json, schemaModel.GetAdaptiveSchema()))
                {
                    //if correct json format, write it into SenseModel
                    try
                    {
                        model = JsonConvert.DeserializeObject<AdaptiveModel>(json);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Could not convert adaptive config file. Please be sure that adaptive config file is correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _log.Error(e);
                        return model;
                    }
                }
                else
                {
                    MessageBox.Show("Could not validate adaptive config file. Please be sure that adaptive config file is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _log.Warn("Could not validate adaptive config file.");
                    return model;
                }
            }
            return model;
        }

        /// <summary>
        /// Gets the file from the filepath, validates the file, and converts it to the report model
        /// </summary>
        /// <param name="filePath">File path for the report_config.json file to be used to convert</param>
        /// <returns>ReportModel if successful or null if unsuccessful</returns>
        public ReportModel GetReportModelFromFile(string filePath)
        {
            ReportModel model = null;
            string json = null;
            try
            {
                //read report config file into string
                using (StreamReader sr = new StreamReader(filePath))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The report config file could not be read from the file. Please check that it exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _log.Error(e);
                return model;
            }
            if (string.IsNullOrEmpty(json))
            {
                MessageBox.Show("Report JSON file is empty. Please check that the report config is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Warn("Report JSON file is empty after loading file.");
                return model;
            }
            else
            {
                SchemaModel schemaModel = new SchemaModel();
                if (ValidateJSON(json, schemaModel.GetReportSchema()))
                {
                    //if correct json format, write it into reportModel
                    try
                    {
                        model = JsonConvert.DeserializeObject<ReportModel>(json);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Could not convert report config file. Please be sure that report config file is correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _log.Error(e);
                        return model;
                    }
                }
                else
                {
                    MessageBox.Show("Could not validate report config file. Please be sure that report config file is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _log.Warn("Could not validate report config file.");
                    return model;
                }
            }
            return model;
        }
        /// <summary>
        /// Gets the file from the filepath, validates the file, and converts it to the application model
        /// </summary>
        /// <param name="filePath">File path for the application_config.json file to be used to convert</param>
        /// <returns>ApplicationModel if successful or null if unsuccessful</returns>
        public AppModel GetApplicationModelFromFile(string filePath)
        {
            AppModel model = null;
            string json = null;
            try
            {
                //read application config file into string
                using (StreamReader sr = new StreamReader(filePath))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The application config file could not be read from the file. Please check that it exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _log.Error(e);
                return model;
            }
            if (string.IsNullOrEmpty(json))
            {
                MessageBox.Show("Application JSON file is empty. Please check that the application config is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Warn("Application JSON file is empty after loading file.");
                return model;
            }
            else
            {
                SchemaModel schemaModel = new SchemaModel();
                if (ValidateJSON(json, schemaModel.GetApplicationSchema()))
                {
                    //if correct json format, write it into applicationModel
                    try
                    {
                        model = JsonConvert.DeserializeObject<AppModel>(json);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Could not convert application config file. Please be sure that application config file is correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _log.Error(e);
                        return model;
                    }
                }
                else
                {
                    MessageBox.Show("Could not validate application config file. Please be sure that application config file is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _log.Warn("Could not validate application config file.");
                    return model;
                }
            }
            return model;
        }
        /// <summary>
        /// Gets the montage model from the montage config
        /// </summary>
        /// <param name="filePath">File path to the montage config file</param>
        /// <returns>Montage model if success or null if unsuccessful</returns>
        public MontageModel GetMontageModelFromFile(string filePath)
        {
            MontageModel model = null;
            string json = null;
            try
            {
                //read MontageModel config file into string
                using (StreamReader sr = new StreamReader(filePath))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The montage config file could not be read from the file. Please check that it exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _log.Error(e);
                return model;
            }
            if (string.IsNullOrEmpty(json))
            {
                MessageBox.Show("Montage JSON file is empty. Please check that the Montage config is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Warn("Montage JSON file is empty after loading file.");
                return model;
            }
            else
            {
                SchemaModel schemaModel = new SchemaModel();
                if (ValidateJSON(json, schemaModel.GetMontageSchema()))
                {
                    //if correct json format, write it into master switchModel
                    try
                    {
                        model = JsonConvert.DeserializeObject<MontageModel>(json);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Could not convert montage config file. Please be sure that montage config file is correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _log.Error(e);
                        return model;
                    }
                }
                else
                {
                    MessageBox.Show("Could not validate montage config file. Please be sure that montage config file is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _log.Warn("Could not validate montage config file.");
                    return model;
                }
            }
            return model;
        }
        /// <summary>
        /// Converts the JSON string to a Config class Model for stim sweep
        /// </summary>
        /// <param name="filepath">file path for the stim sweep config file</param>
        /// <returns>StimSweepModel if successful and null if not</returns>
        public StimSweepModel GetStimSweepModelFromFile(string filepath)
        {
            StimSweepModel model = null;
            string json = null;
            try
            {
                //read MontageModel config file into string
                using (StreamReader sr = new StreamReader(filepath))
                {
                    json = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The stim sweep config file could not be read from the file. Please check that it exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _log.Error(e);
                return model;
            }
            if (string.IsNullOrEmpty(json))
            {
                MessageBox.Show("Stim Sweep JSON file is empty. Please check that the Stim Sweep config is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Warn("Stim Sweep JSON file is empty after loading file.");
                return model;
            }
            else
            {
                SchemaModel schemaModel = new SchemaModel();
                if (ValidateJSON(json, schemaModel.GetStimSweepSchema()))
                {
                    //if correct json format, write it into master switchModel
                    try
                    {
                        model = JsonConvert.DeserializeObject<StimSweepModel>(json);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Could not convert Stim Sweep config file. Please be sure that Stim Sweep config file is correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        _log.Error(e);
                        return model;
                    }
                }
                else
                {
                    MessageBox.Show("Could not validate Stim Sweep config file. Please be sure that Stim Sweep config file is correct.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _log.Warn("Could not validate Stim Sweep config file.");
                    return model;
                }
            }
            return model;
        }
        #endregion

        #region Write Adaptive and Sense Config Files
        /// <summary>
        /// Writes the Sense Model to a json config file in specified path
        /// </summary>
        /// <param name="senseModel">Model to be written to json file</param>
        /// <param name="path">path where the file goes including filename and extension</param>
        /// <returns>true if success and false if unsuccessful</returns>
        public bool WriteSenseConfigToFile(SenseModel senseModel, string path)
        {
            bool success = false;
            try
            {
                using (StreamWriter file = File.CreateText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, senseModel);
                    success = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not write sense config to file at the path: " + path + ". Please save your current sense config file if you would like them saved for later use.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Error(e);
                success = false;
            }
            return success;
        }
        /// <summary>
        /// Writes the Adaptive Model to a json config file in the medtronic directory path for the current session
        /// </summary>
        /// <param name="senseModel">Model to be written to json file</param>
        /// <param name="version">version number to be prepended to front of filename</param>
        /// <returns>true if success and false if unsuccessful</returns>
        public bool WriteSenseConfigToFile(SenseModel senseModel, int version)
        {
            bool success = false;
            try
            {
                //prepend the version number to front with date and time
                string path = String.Concat(version.ToString("000"), "_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss_tt") + "_sense.json");
                //Add filepath found in constructor to beginning and path at end. ConfigLogFiles is a directory made inside medtronic directory
                path = filepath + "\\ConfigLogFiles\\" + path;
                //Check if path exists and if not then create the path
                CheckIfDirectoryExistsOtherwiseCreateIt(path);
                //write the file to path and set success
                using (StreamWriter file = File.CreateText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, senseModel);
                    success = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not write sense config to file. Please be sure the application_config.json BasePathToJSONFiles has the same path as DataDirectory in the Registry Editor for the path Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Medtronic\\ORCA. Please save your current sense config files if you would like them saved for later use. Please fix and restart application", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Error(e);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Writes the adaptive Model to a json config file in the medtronic directory path for the current session
        /// </summary>
        /// <param name="adaptiveModel">Model to be written to json file</param>
        /// <param name="version">version number to be prepended to front of filename</param>
        /// <returns>true if success and false if unsuccessful</returns>
        public bool WriteAdaptiveConfigToFile(AdaptiveModel adaptiveModel, int version)
        {
            bool success = false;
            try
            {
                //prepend the version number to front with date and time
                string path = String.Concat(version.ToString("000"), "_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss_tt") + "_adaptive.json");
                //Add filepath found in constructor to beginning and path at end. ConfigLogFiles is a directory made inside medtronic directory
                path = filepath + "\\ConfigLogFiles\\" + path;
                //Check if path exists and if not then create the path
                CheckIfDirectoryExistsOtherwiseCreateIt(path);
                using (StreamWriter file = File.CreateText(path))
                {
                    //write the file to path and set success
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, adaptiveModel);
                    success = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not write adaptive config to file. Please be sure the application_config.json BasePathToJSONFiles has the same path as DataDirectory in the Registry Editor for the path Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Medtronic\\ORCA. Please save your current adaptive config files if you would like them saved for later use. Please fix and restart application", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Error(e);
                success = false;
            }
            return success;
        }
        /// <summary>
        /// Writes the Adaptive Model to a json config file in the medtronic directory path for the current session
        /// </summary>
        /// <param name="adaptiveModel">Model to be written to json file</param>
        /// <param name="path">filepath to where to write the file</param>
        /// <returns>true if success and false if unsuccessful</returns>
        public bool WriteAdaptiveConfigToFile(AdaptiveModel adaptiveModel, string path)
        {
            bool success = false;
            try
            {
                using (StreamWriter file = File.CreateText(path))
                {
                    //write the file to path and set success
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, adaptiveModel);
                    success = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not write adaptive config to file. Please be sure the adaptive_config.json is located in C:\\AdaptiveDBS\\adaptive_config.json", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Error(e);
                success = false;
            }
            return success;
        }

        #endregion

        #region Helper Methods for Getting directory path, Checking if path exists or creating it
        /// <summary>
        /// Gets the path to the medtronic json files directory base on projectID, deviceID, and patientID
        /// Sets this path to filepath variable for use for later.
        /// This method is used in the constructor when creating the class
        /// </summary>
        private void GetDirectoryPathForCurrentSession()
        {
            try
            {
                //This gets the directories in the summitData directory and sort it
                //This is because we want the most recent directory and directories in there are sorted by linux timestamp
                //once sorted, we can find the most recent one (last one) and return the name of that directory to add to the filepath
                string[] folders = Directory.GetDirectories(basePathForMedtronicFiles + "\\SummitData\\" + projectIDForPath + "\\" + patientIDForPath);
                Array.Sort(folders);
                filepath = folders[folders.Length - 1] + "\\" + "Device" + deviceIDForPath;
            }
            catch(Exception e)
            {
                MessageBox.Show("Could not find current session directory path. Please be sure the application_config.json BasePathToJSONFiles has the same path as DataDirectory in the Registry Editor for the path Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Medtronic\\ORCA. Please fix and restart application", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Error(e);
            }
        }

        /// <summary>
        /// Checks to see if the path already. If it does not, it will write the directory path for placing files into
        /// </summary>
        /// <param name="path">This is the final path where the files will be written and checked to make sure it is valid or else writes it</param>
        private void CheckIfDirectoryExistsOtherwiseCreateIt(string path)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    Directory.CreateDirectory(fileInfo.Directory.FullName);
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not create directory for writing files. Please be sure the application_config.json BasePathToJSONFiles has the same path as DataDirectory in the Registry Editor for the path Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Medtronic\\ORCA. Please fix and restart application", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _log.Error(e);
            }
        }
        #endregion
    }
}
