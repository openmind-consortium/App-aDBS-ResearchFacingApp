/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.Models;
using Medtronic.NeuroStim.Olympus.DataTypes.Sensing;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    public partial class MainViewModel : Screen
    {
        private string _dataPointsForVisualizationSense;
        private string _fFTMean, _fFTTimeDisplay, _fFTCurrentChannel, _fFTOverlapDisplay;
        private int userInputForFFTMean = 10;
        private string _timeDomainM1ChartTitle, _timeDomainSTNChartTitle;

        #region Charts Binding
        /// <summary>
        /// Binding for the line chart for TimeDomainSTNChart. Actual input values added in MainPageViewModel under Time Domain Event Handler for this chart
        /// </summary>
        public IDataSeries<double, double> TimeDomainSTNChart
        {
            get { return _timeDomainSTNChart; }
            set
            {
                _timeDomainSTNChart = value;
                NotifyOfPropertyChange("TimeDomainSTNChart");
            }
        }

        /// <summary>
        /// Binding for the line chart for TimeDomainM1Chart. Actual input values added in MainPageViewModel under Time Domain Event Handler for this chart
        /// </summary>
        public IDataSeries<double, double> TimeDomainM1Chart
        {
            get { return _timeDomainM1Chart; }
            set
            {
                _timeDomainM1Chart = value;
                NotifyOfPropertyChange("TimeDomainM1Chart");
            }
        }

        /// <summary>
        /// Binding for the line chart for AccelerometryXChart. Actual input values added in MainPageViewModel under Acc Event Handler for this chart
        /// </summary>
        public IDataSeries<double, double> AccelerometryXChart
        {
            get { return _accelerometryXChart; }
            set
            {
                _accelerometryXChart = value;
                NotifyOfPropertyChange("AccelerometryXChart");
            }
        }

        /// <summary>
        /// Binding for the line chart for AccelerometryYChart. Actual input values added in MainPageViewModel under Acc Event Handler for this chart
        /// </summary>
        public IDataSeries<double, double> AccelerometryYChart
        {
            get { return _accelerometryYChart; }
            set
            {
                _accelerometryYChart = value;
                NotifyOfPropertyChange("AccelerometryYChart");
            }
        }

        /// <summary>
        /// Binding for the line chart for AccelerometryZChart. Actual input values added in MainPageViewModel under Acc Event Handler for this chart
        /// </summary>
        public IDataSeries<double, double> AccelerometryZChart
        {
            get { return _accelerometryZChart; }
            set
            {
                _accelerometryZChart = value;
                NotifyOfPropertyChange("AccelerometryZChart");
            }
        }

        /// <summary>
        /// Binding for the line chart for FFTChart. Actual input values added in MainPageViewModel under FFT Event Handler for this chart
        /// </summary>
        public IDataSeries<double, double> FFTChart
        {
            get { return _fftChart; }
            set
            {
                _fftChart = value;
                NotifyOfPropertyChange("FFTChart");
            }
        }
        #endregion

        #region Drop Down Menu Bindings
        /// <summary>
        /// Binding for the drop down menu for STN timedomain options
        /// </summary>
        public BindableCollection<string> TimeDomainSTNDropDown
        {
            get { return _timeDomainSTNDropDown; }
            set
            {
                _timeDomainSTNDropDown = value;
                NotifyOfPropertyChange(() => TimeDomainSTNDropDown);
            }
        }
        /// <summary>
        /// Binding for the drop down menu for M1 timedomain options
        /// </summary>
        public BindableCollection<string> TimeDomainM1DropDown
        {
            get { return _timeDomainM1DropDown; }
            set
            {
                _timeDomainM1DropDown = value;
                NotifyOfPropertyChange(() => TimeDomainM1DropDown);
            }
        }

        /// <summary>
        /// Binding for the drop down menu for fft options
        /// </summary>
        public BindableCollection<string> FFTScaleOptions
        {
            get { return _fftScaleOptions; }
            set
            {
                _fftScaleOptions = value;
                NotifyOfPropertyChange(() => FFTScaleOptions);
            }
        }
        /// <summary>
        /// Binding for selected option for the drop down menu for M1 timedomain options
        /// </summary>
        public string SelectedTimeDomainM1
        {
            get { return _selectedTimeDomainM1; }
            set
            {
                _selectedTimeDomainM1 = value;
                NotifyOfPropertyChange(() => SelectedTimeDomainM1);
            }
        }
        /// <summary>
        /// Binding for selected option for the drop down menu for STN timedomain options
        /// </summary>
        public string SelectedTimeDomainSTN
        {
            get { return _selectedTimeDomainSTN; }
            set
            {
                _selectedTimeDomainSTN = value;
                NotifyOfPropertyChange(() => SelectedTimeDomainSTN);
            }
        }

        /// <summary>
        /// Binding for selected option for the drop down menu for FFT options
        /// </summary>
        public string SelectedFFTScaleOption
        {
            get { return _selectedFFTScaleOption; }
            set
            {
                _selectedFFTScaleOption = value;
                NotifyOfPropertyChange(() => SelectedFFTScaleOption);
            }
        }

        /// <summary>
        /// Binding to change chart title for lead 2
        /// </summary>
        public string TimeDomainM1ChartTitle
        {
            get { return _timeDomainM1ChartTitle; }
            set
            {
                _timeDomainM1ChartTitle = value;
                NotifyOfPropertyChange(() => TimeDomainM1ChartTitle);
            }
        }
        /// <summary>
        /// Binding to change chart title for lead 1
        /// </summary>
        public string TimeDomainSTNChartTitle
        {
            get { return _timeDomainSTNChartTitle; }
            set
            {
                _timeDomainSTNChartTitle = value;
                NotifyOfPropertyChange(() => TimeDomainSTNChartTitle);
            }
        }
        #endregion

        #region Data Points
        /// <summary>
        /// This changes the acutal data series for all charts under visualization tab.
        /// Original/default data point is set with this method in constructor of MainViewModel
        /// </summary>
        /// <param name="fifoCapacity">The value of data points for the chart on the x axis</param>
        /// <returns>True if successful and false if unsuccessful</returns>
        public bool ChangeDataSeriesForVisualizationSenseChart(int fifoCapacity)
        {
            bool canDo = true;
            try
            {
                _timeDomainSTNChart.FifoCapacity = fifoCapacity;
                _timeDomainM1Chart.FifoCapacity = fifoCapacity;
                _accelerometryXChart.FifoCapacity = fifoCapacity;
                _accelerometryYChart.FifoCapacity = fifoCapacity;
                _accelerometryZChart.FifoCapacity = fifoCapacity;
                _fftChart.FifoCapacity = fifoCapacity;
            }
            catch
            {
                canDo = false;
            }
            return canDo;
        }

        /// <summary>
        /// This allows user to input how many data points they want shown on chart.  
        /// This string is later converted to int and fed into scichart as number of datapoints
        /// Conversion is below when user presses SetDataPointsForSenseVisualizationButton()
        /// </summary>
        public string DataPointsForVisualizationSense
        {
            get { return _dataPointsForVisualizationSense ?? (_dataPointsForVisualizationSense = DEFAULT_DATA_POINTS_TD.ToString()); }
            set
            {
                _dataPointsForVisualizationSense = value;
                NotifyOfPropertyChange(() => DataPointsForVisualizationSense);
            }
        }

        /// <summary>
        ///  Sets the data points for the charts for all charts under sense visualization: time domain M1/STN and Accelerometer
        /// If user inputs a non-integer, then it will not change the data points
        /// ChangeDataSeriesForVisualizationSenseChart() does the actual changing of data points and is implemented below
        /// </summary>
        public void SetDataPointsForSenseVisualizationButton()
        {
            //i is the acutal number of data points assuming the user gave an integer
            int i = 0;
            bool result = int.TryParse(DataPointsForVisualizationSense, out i);

            if (result)
            {
                if (ChangeDataSeriesForVisualizationSenseChart(i))
                {
                    Messages.Insert(0, DateTime.Now + ":: Changed Sense Data Series to " + i);
                }
                else
                {
                    Messages.Insert(0, DateTime.Now + ":: ERROR: Could NOT change data points for sense visualization");
                }
            }
        }
        #endregion

        #region FFT Mean
        /// <summary>
        /// Button to set the FFT mean value
        /// </summary>
        public void FFTMeanButton()
        {
            //i is the acutal number of data points assuming the user gave an integer
            int i = 0;
            bool result = int.TryParse(FFTMean, out i);

            if (result)
            {
                initialCountForRollingMean = 0;
                rollingMean.Clear();
                userInputForFFTMean = i;
                //Calculate FFT Time
                FFTTimeDisplay = CalculateFFTTime(senseConfig);
            }
        }

        /// <summary>
        /// The input should be an integer. This is the actual value for the fft mean. If not an integer, then nothing will happen.
        /// </summary>
        public string FFTMean
        {
            get { return _fFTMean; }
            set
            {
                _fFTMean = value;
                NotifyOfPropertyChange(() => FFTMean);
            }
        }

        /// <summary>
        /// Display for the fft time to user
        /// </summary>
        public string FFTTimeDisplay
        {
            get { return _fFTTimeDisplay; }
            set
            {
                _fFTTimeDisplay = value;
                NotifyOfPropertyChange(() => FFTTimeDisplay);
            }
        }

        /// <summary>
        /// Display for the FFT Overlap to user
        /// </summary>
        public string FFTOverlapDisplay
        {
            get { return _fFTOverlapDisplay; }
            set
            {
                _fFTOverlapDisplay = value;
                NotifyOfPropertyChange(() => FFTOverlapDisplay);
            }
        }
        #endregion

        #region FFT Channel Changer
        /// <summary>
        /// Displays the current FFT Channel
        /// </summary>
        public string FFTCurrentChannel
        {
            get { return _fFTCurrentChannel; }
            set
            {
                _fFTCurrentChannel = value;
                NotifyOfPropertyChange(() => FFTCurrentChannel);
            }
        }
        /// <summary>
        /// Changes the FFT channel to channel 0
        /// </summary>
        public void FFTChannel0Button()
        {
            if (!ChangeFFTChannelCode(SenseTimeDomainChannel.Ch0))
            {
                Messages.Insert(0, DateTime.Now + ":: Please fix FFT settings and try again." + bufferReturnInfo.Descriptor);
            }
            else
            {
                FFTCurrentChannel = "Ch. 0";
            }
        }
        /// <summary>
        /// Changes the FFT channel to channel 1
        /// </summary>
        public void FFTChannel1Button()
        {
            if (!ChangeFFTChannelCode(SenseTimeDomainChannel.Ch1))
            {
                Messages.Insert(0, DateTime.Now + ":: Please fix FFT settings and try again." + bufferReturnInfo.Descriptor);
            }
            else
            {
                FFTCurrentChannel = "Ch. 1";
            }
        }
        /// <summary>
        /// Changes the FFT channel to channel 2
        /// </summary>
        public void FFTChannel2Button()
        {
            if (!ChangeFFTChannelCode(SenseTimeDomainChannel.Ch2))
            {
                Messages.Insert(0, DateTime.Now + ":: Please fix FFT settings and try again." + bufferReturnInfo.Descriptor);
            }
            else
            {
                FFTCurrentChannel = "Ch. 2";
            }
        }
        /// <summary>
        /// Changes the FFT channel to channel 3
        /// </summary>
        public void FFTChannel3Button()
        {
            if (!ChangeFFTChannelCode(SenseTimeDomainChannel.Ch3))
            {
                Messages.Insert(0, DateTime.Now + ":: Please fix FFT settings and try again." + bufferReturnInfo.Descriptor);
            }
            else
            {
                FFTCurrentChannel = "Ch. 3";
            }
        }

        private bool ChangeFFTChannelCode(SenseTimeDomainChannel channel)
        {
            if (theSummit != null && isConnected)
            {
                if (!theSummit.IsDisposed && senseConfig != null)
                {
                    try
                    {
                        if (!summitSensing.StopSensing(theSummit, false))
                        {
                            Messages.Insert(0, DateTime.Now + ":: Could not stop sensing: " + bufferReturnInfo.Descriptor);
                            return false;
                        }
                        senseConfig.Sense.FFT.Channel = (int)channel;
                        if (!summitSensing.StartSensing(theSummit, senseConfig, false))
                        {
                            Messages.Insert(0, DateTime.Now + ":: Could not stop sensing: " + bufferReturnInfo.Descriptor);
                            return false;
                        }
                        if (!summitSensing.StartStreaming(theSummit, senseConfig, false))
                        {
                            Messages.Insert(0, DateTime.Now + ":: Could not stop sensing: " + bufferReturnInfo.Descriptor);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);
                        return false;
                    }
                }
                else
                {
                    Messages.Insert(0, DateTime.Now + ":: Summit disposed or config file empty: " + bufferReturnInfo.Descriptor);
                    return false;
                }
            }
            else
            {
                Messages.Insert(0, DateTime.Now + ":: Summit is null or no connection: " + bufferReturnInfo.Descriptor);
                return false;
            }
            return true;
        }
        #endregion
    }
}
