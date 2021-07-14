/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/

namespace EmbeddedAdaptiveDBSApplication.Models
{
    /// <summary>
    /// Model for the config file for the application. Sets whether bilateral or the switch is enabled.
    /// </summary>
    public class AppModel
    {
        /// <summary>
        /// Sets the base path to the Medtronic JSON files. 
        /// </summary>
        public string BasePathToJSONFiles { get; set; }
        /// <summary>
        /// Turns on the capability to log events when a beep or noise detected
        /// </summary>
        public bool LogBeepEvent { get; set; }
        /// <summary>
        /// Class that allows user to change the beep noise
        /// </summary>
        public CTMBeepEnables CTMBeepEnables { get; set; }
    }

    /// <summary>
    /// Class that allows user to change the beep noise
    /// </summary>
    public class CTMBeepEnables
    {
        /// <summary>
        /// Comment 
        /// </summary>
        public string comment { get; set; }
        /// <summary>
        /// No beep
        /// </summary>
        public bool None { get; set; }
        /// <summary>
        /// General alert beep
        /// </summary>
        public bool GeneralAlert { get; set; }
        /// <summary>
        /// Tememetry Completed Beep
        /// </summary>
        public bool TelMCompleted { get; set; }
        /// <summary>
        /// Device discovered beep
        /// </summary>
        public bool DeviceDiscovered { get; set; }
        /// <summary>
        /// No device discovered beep
        /// </summary>
        public bool NoDeviceDiscovered { get; set; }
        /// <summary>
        /// Tel lost beep
        /// </summary>
        public bool TelMLost { get; set; }
    }
}
