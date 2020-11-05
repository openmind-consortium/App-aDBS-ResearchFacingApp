/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.ViewModels;
using Medtronic.NeuroStim.Olympus.DataTypes.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EmbeddedAdaptiveDBSApplication.Models
{
    /// <summary>
    /// Calculates power bins from fft bins. 
    /// FFt bins are calculated from SampleRate and FFT Size
    /// Allows user to get the actual power values from estimated power values and upper and lower index based on the estimated power value which are used for the api call
    /// </summary>
    class CalculatePowerBins
    {
        ILog _log;
        //used to calculate the fft bins which are used to get the upper and lower power bins
        private List<double> bins = new List<double>();
        //Upper and Lower power bins are the result of calculating the fft bins and adding binwidth/2 for upper and subtracting binwidth/2 for lower from fft bins.
        private List<double> upperPowerBins = new List<double>();
        private List<double> lowerPowerBins = new List<double>();
        private double[] lowerPowerBinActualValues = new double[8];
        private double[] upperPowerBinActualValues = new double[8];
        //size of the smallest value or bin[1]-bin[0]
        private double binWidth;
        /// <summary>
        /// Variable for upper power value that are set with actual power values. Must be retrieved each time the index is calculated
        /// </summary>
        private double ActualUpperPowerValue { get; set; }
        /// <summary>
        /// Variable for lower power value that are set with actual power values. Must be retrieved each time the index is calculated
        /// </summary>
        private double ActualLowerPowerValue { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_log">Caliburn Micro Logger</param>
        public CalculatePowerBins(ILog _log)
        {
            this._log = _log;
        }

        /// <summary>
        /// Gets the power band bins and actual hz values. Program shuts down if can't compute
        /// </summary>
        /// <param name="localModel">SenseModel</param>
        /// <returns>List of PowerBandModel class</returns>
        public List<PowerBandModel> GetPowerBands(SenseModel localModel)
        {
            List<PowerBandModel> powerBandsList = new List<PowerBandModel>();
            try
            {
                bins.Clear();
                bins = CalculateFFTBins(ConfigConversions.FftSizesConvert(localModel.Sense.FFT.FftSize), ConfigConversions.TDSampleRateConvert(localModel.Sense.TDSampleRate));
                binWidth = bins[1] - bins[0];
                CalculateUpperBins();
                CalculateLowerBins();
            }
            catch (Exception e)
            {
                HandleException(e);
            }

            for (int i = 0; i < 8;)
            {
                try
                {
                    PowerBandModel powerBandModel = new PowerBandModel();
                    //Gets the lower index value and upper index value based from the config file value.
                    powerBandModel.lowerIndexBand0 = GetLowerIndex(localModel.Sense.PowerBands[i].ChannelPowerBand[0]);
                    powerBandModel.upperIndexBand0 = GetUpperIndex(localModel.Sense.PowerBands[i].ChannelPowerBand[1]);
                    
                    //This checks to make sure that the upper power bin index is bigger than the lower; cannot have upper index that is less than lower index
                    //if the upper index value is not bigger than the lower, then the upper index is set to lower index for error handling
                    powerBandModel.upperIndexBand0 = CheckThatUpperPowerBandGreaterThanLowerPowerBand(powerBandModel.lowerIndexBand0, powerBandModel.upperIndexBand0);
                    //Actual lower and upper power values are calculated from the users estimated power values from the config file
                    //This is done in the CalculatePowerBins class.  Below saves the actual lower and upper power values in each array to 2 decimal places.
                    //These arrays are instantiated in MainViewModel.cs
                    powerBandModel.lowerActualValueHzBand0 = Math.Round(ActualLowerPowerValue, 2);
                    lowerPowerBinActualValues[i] = powerBandModel.lowerActualValueHzBand0;
                    powerBandModel.UpperActualValueHzBand0 = Math.Round(ActualUpperPowerValue, 2);
                    upperPowerBinActualValues[i] = powerBandModel.UpperActualValueHzBand0;
                    //increment i so that the next power band can be set. There are 8 (0-7). There are 4 power channels (0-3);
                    i++;
                    //Repeat this step for next power band
                    powerBandModel.lowerIndexBand1 = GetLowerIndex(localModel.Sense.PowerBands[i].ChannelPowerBand[0]);
                    powerBandModel.upperIndexBand1 = GetUpperIndex(localModel.Sense.PowerBands[i].ChannelPowerBand[1]);
                    
                    powerBandModel.upperIndexBand1 = CheckThatUpperPowerBandGreaterThanLowerPowerBand(powerBandModel.lowerIndexBand1, powerBandModel.upperIndexBand1);
                    powerBandModel.lowerActualValueHzBand1 = Math.Round(ActualLowerPowerValue, 2);
                    lowerPowerBinActualValues[i] = powerBandModel.lowerActualValueHzBand1;
                    powerBandModel.upperActualValueHzBand1 = Math.Round(ActualUpperPowerValue, 2);
                    upperPowerBinActualValues[i] = powerBandModel.upperActualValueHzBand1;
                    i++;
                    powerBandsList.Add(powerBandModel);
                }
                catch (Exception e)
                {
                    HandleException(e);
                }
            }
            return powerBandsList;
        }

        /// <summary>
        /// Gets the lower power bins in Hz for the actual values used by program
        /// </summary>
        /// <param name="localModel">sense model</param>
        /// <returns>Double array containing values</returns>
        public double[] GetLowerPowerBinActualValues(SenseModel localModel)
        {
            GetPowerBands(localModel);
            return lowerPowerBinActualValues;
        }
        /// <summary>
        /// Gets the upper power bins in Hz for the actual values used by program
        /// </summary>
        /// <param name="localModel">sense model</param>
        /// <returns>Double array containing values</returns>
        public double[] GetUpperPowerBinActualValues(SenseModel localModel)
        {
            GetPowerBands(localModel);
            return upperPowerBinActualValues;
        }
        /// <summary>
        /// Gets the lower index number used for the Medtronic API call. 
        /// The estimated value from the config file in Hz are used to find the nearest actual power value.
        /// The index of this power value is used for the lower index are returned.
        /// The Acutal Lower Power Value is also set each time the lower index is found. 
        /// A call to ActualLowerPowerValue will retrieve this value and must be made after each GetLowerIndex call
        /// </summary>
        /// <param name="lower">Estimated lower power band value in Hz</param>
        /// <returns>Index of estimated power value used for medtronic powerband api call</returns>
        private ushort GetLowerIndex(double lower)
        {
            ushort lowerIndex = 0;
            try
            {
                //ActualLowerPowerValue is the actual power value used from the estimated power value from user
                //This is set so that user can get the actual value if they want it.
                ActualLowerPowerValue = lowerPowerBins.Aggregate((x, y) => Math.Abs(x - lower) < Math.Abs(y - lower) ? x : y);
                lowerIndex = (ushort)lowerPowerBins.IndexOf(ActualLowerPowerValue);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
            return lowerIndex;
        }

        /// <summary>
        /// Gets the upper index number used for the Medtronic API call. 
        /// The estimated value from the config file in Hz are used to find the nearest actual power value.
        /// The index of this power value is used for the upper index are returned.
        /// The Acutal Upper Power Value is also set each time the upper index is found. 
        /// A call to ActualUpperPowerValue will retrieve this value and must be made after each GetUpperIndex call
        /// </summary>
        /// <param name="upper">Estimated upper power band value in Hz</param>
        /// <returns>Index of estimated power value used for medtronic powerband api call</returns>
        private ushort GetUpperIndex(double upper)
        {
            ushort upperIndex = 0;
            try
            {
                //ActualUpperPowerValue is the actual power value used from the estimated power value from user
                //This is set so that user can get the actual value if they want it.
                ActualUpperPowerValue = upperPowerBins.Aggregate((x, y) => Math.Abs(x - upper) < Math.Abs(y - upper) ? x : y);
                upperIndex = (ushort)upperPowerBins.IndexOf(ActualUpperPowerValue);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
            return upperIndex;
        }

        /// <summary>
        /// Calculate lower power bins from the binwidth and fft bins
        /// each lower power bin is the fft bin[i] - binwidth/2
        /// this is different than the upper power bin
        /// </summary>
        private void CalculateLowerBins()
        {
            //Add the 0 indexed bin to the lower power bin since it's the same with fft bin and lower power bin
            //This is different for upper power bin since it is not the same as fft bin
            lowerPowerBins.Add(bins[0]);
            try
            {
                for (int i = 1; i < bins.Count(); i++)
                {
                    lowerPowerBins.Add(Math.Round(bins[i] - binWidth / 2, 2));
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        /// <summary>
        /// Calculate upper power bins from the binwidth and fft bins
        /// each upper power bin is the fft bin[i] + binwidth/2
        /// this is different than the lower power bin
        /// </summary>
        private void CalculateUpperBins()
        {
            try
            {
                for (int i = 0; i < bins.Count(); i++)
                {
                    upperPowerBins.Add(Math.Round(bins[i] + binWidth / 2, 2));
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
        public List<double> GetUpperBinsInHz(SenseModel localModel)
        {
            GetPowerBands(localModel);
            return upperPowerBins;
        }
        public List<double> GetLowerBinsInHz(SenseModel localModel)
        {
            GetPowerBands(localModel);
            return lowerPowerBins;
        }
        /// <summary>
        /// Calculates the fft bins.
        /// This code is taken from the config ui training code to calculate fft bins
        /// </summary>
        /// <param name="fftSize">Size of the FFT.</param>
        /// <param name="timeRate">The time rate.</param>
        /// <returns>a list of fft bin boundaries</returns>
        public List<double> CalculateFFTBins(FftSizes fftSize, TdSampleRates timeRate)
        {
            List<double> tempBins = new List<double>();
            int numBins = 0;
            switch (fftSize)
            {
                case FftSizes.Size0064:
                    numBins = 64 / 2;
                    break;
                case FftSizes.Size0256:
                    numBins = 256 / 2;
                    break;
                case FftSizes.Size1024:
                    numBins = 1024 / 2;
                    break;
                default:
                    ConfigConversions.DisplayErrorMessageAndClose("Could not convert FFT Size for FFT bins");
                    break;
            }
            int rate = 0;
            switch (timeRate)
            {
                case TdSampleRates.Sample0250Hz:
                    rate = 250;
                    break;
                case TdSampleRates.Sample0500Hz:
                    rate = 500;
                    break;
                case TdSampleRates.Sample1000Hz:
                    rate = 1000;
                    break;
                default:
                    ConfigConversions.DisplayErrorMessageAndClose("Could not convert TDSampleRate for FFT bins");
                    break;
            }

            double binWidth = rate / 2.0 / numBins;

            for (int i = 0; i < numBins; i++)
            {
                tempBins.Add(i * binWidth);
            }
            return tempBins;
        }
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
        /// Handles an exception in CalculatePowerBins class. Logs error, displays error to user and shuts program down properly. 
        /// </summary>
        /// <param name="e">Exception</param>
        private void HandleException(Exception e)
        {
            _log.Error(e);
            MessageBox.Show("Error calculating power bins in CaclulatePowerBins. Program shutting down...", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            MainViewModel.DisposeSummitManagerAndSystem();
            Environment.Exit(0);
        }
    }
}
