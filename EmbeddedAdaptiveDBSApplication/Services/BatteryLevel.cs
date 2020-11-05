using Caliburn.Micro;
using Medtronic.NeuroStim.Olympus.DataTypes.PowerManagement;
using Medtronic.SummitAPI.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedAdaptiveDBSApplication.Services
{
    class BatteryLevel
    {
        private string INSBatteryLevel = "Not Connected";
        /// <summary>
        /// Gets the battery level for the INS
        /// </summary>
        /// <param name="theSummit">Summit System</param>
        /// <param name="_log">Caliburn Micro Logger</param>
        /// <returns>String that tells the battery status of the INS</returns>
        public string GetINSBatteryLevel(SummitSystem theSummit, ILog _log)
        {
            BatteryStatusResult outputBuffer = null;
            APIReturnInfo returnInfo = new APIReturnInfo();

            //Return Not Connected if summit is null
            if (theSummit == null)
            {
                return "Not Connected";
            }
            if (theSummit.IsDisposed)
            {
                return "Not Connected";
            }

            try
            {
                returnInfo = theSummit.ReadBatteryLevel(out outputBuffer);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            // Ensure the command was successful before using the result
            try
            {
                //Check if command was successful
                if (returnInfo.RejectCode == 0)
                {
                    // Retrieve the battery level from the output buffer
                    if (outputBuffer != null)
                        INSBatteryLevel = outputBuffer.BatteryLevelPercent.ToString();
                }
                else
                {
                    INSBatteryLevel = "";
                }
            }
            catch (Exception e)
            {
                INSBatteryLevel = "";
                _log.Error(e);
            }
            //Return either battery level, empty string or Not Connected
            return INSBatteryLevel;
        }
    }
}
