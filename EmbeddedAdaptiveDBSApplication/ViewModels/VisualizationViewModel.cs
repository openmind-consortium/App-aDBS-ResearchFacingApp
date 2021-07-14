/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.Models;
using Medtronic.SummitAPI.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciChart.Charting.Model.DataSeries;
using SciChart.Core.Utility;
using System.Timers;
using System.Windows;
using SciChart.Charting.ViewportManagers;
using System.Windows.Input;
using SciChart.Charting.Common.Helpers;
using SciChart.Core.Framework;
using SciChart.Data.Model;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Services;
using System.Text.RegularExpressions;

/// <summary>
/// This class is part of the MainViewModel class and it implements the code for the visualization tab.
/// Includes most of the buttons and bindings for the visualization page.
/// Some variable instantiations and initializations are made in MainViewModel and its constructor.
/// </summary>
namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    public partial class MainViewModel : Screen
    {
        #region Variables
        //IUpdateSuspender variables allow the scichart to be paused and started back up.
        //Used below for StartButton() and PauseButton()
        IUpdateSuspender suspenderPower = null;
        IUpdateSuspender suspenderPowerTwo = null;
        IUpdateSuspender suspenderState = null;
        IUpdateSuspender suspenderCurrentProgram0 = null;
        IUpdateSuspender suspenderCurrentProgram1 = null;
        IUpdateSuspender suspenderCurrentProgram2 = null;
        IUpdateSuspender suspenderCurrentProgram3 = null;
        IUpdateSuspender suspenderB1 = null;
        IUpdateSuspender suspenderB0 = null;
        IUpdateSuspender suspenderDetector = null;
        IUpdateSuspender suspenderPowerLD1 = null;
        IUpdateSuspender suspenderPowerTwoLD1 = null;
        //String to get number of data points for the graphs from user. Implemented below under DataPoints
        private string _dataPoints;
        //Vars for the start stop button
        private bool isChartPaused = false;
        private string _startPauseButtonText = "Pause Chart";
        #endregion

        /// <summary>
        /// Binding for the UI to set the visible range for the Y value for Power
        /// </summary>
        public IRange PowerYAxisVisibleRange
        {
            get { return _powerYAxisVisibleRange; }
            set
            {
                _powerYAxisVisibleRange = value;
                NotifyOfPropertyChange(() => PowerYAxisVisibleRange);
            }
        }

        #region Drop Down Menu for Power Options LD1 Chart
        /// <summary>
        /// Binding to drop down menu for either Auto-Scale or Threshold for chart for Power/LD1
        /// Allows user to select which view they want the chart have
        /// </summary>
        public BindableCollection<string> PowerLD1ScaleOptions
        {
            get { return _powerLD1ScaleOptions; }
            set
            {
                _powerLD1ScaleOptions = value;
                NotifyOfPropertyChange(() => PowerLD1ScaleOptions);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu in PowerLD1ScaleOptions
        /// </summary>
        public string SelectedPowerLD1ScaleOption
        {
            get { return _selectedPowerLD1ScaleOption; }
            set
            {
                _selectedPowerLD1ScaleOption = value;
                NotifyOfPropertyChange(() => SelectedPowerLD1ScaleOption);
            }
        }

        /// <summary>
        /// Binding to drop down menu to that lists the power channel options available. 
        /// This is implemented in MainPageViewModel and if power channel is disabled, it states disabled
        /// </summary>
        public BindableCollection<string> PowerLD1ChannelOptions
        {
            get { return _powerLD1ChannelOptions; }
            set
            {
                _powerLD1ChannelOptions = value;
                NotifyOfPropertyChange(() => PowerLD1ChannelOptions);
            }
        }

        /// <summary>
        /// This is the binding for the actual option selected in the drop down menu in PowerLD1ChannelOptions
        /// </summary>
        public string SelectedPowerLD1Channel
        {
            get { return _selectedPowerLD1Channel; }
            set
            {
                _selectedPowerLD1Channel = value;
                NotifyOfPropertyChange(() => SelectedPowerLD1Channel);
            }
        }

        /// <summary>
        /// Binding to drop down menu to that lists the power channel two options available. 
        /// This is implemented in MainPageViewModel and if power channel is disabled, it states disabled
        /// </summary>
        public BindableCollection<string> PowerLD1ChannelOptionsTwo
        {
            get { return _powerLD1ChannelOptionsTwo; }
            set
            {
                _powerLD1ChannelOptionsTwo = value;
                NotifyOfPropertyChange(() => PowerLD1ChannelOptionsTwo);
            }
        }

        /// <summary>
        /// This is the binding for the actual option selected in the drop down menu in PowerChannelOptionsTwo
        /// </summary>
        public string SelectedPowerLD1ChannelTwo
        {
            get { return _selectedPowerLD1ChannelTwo; }
            set
            {
                _selectedPowerLD1ChannelTwo = value;
                NotifyOfPropertyChange(() => SelectedPowerLD1ChannelTwo);
            }
        }

        /// <summary>
        /// This is the binding for the Start pause button text
        /// </summary>
        public string StartPauseButtonText
        {
            get { return _startPauseButtonText; }
            set
            {
                _startPauseButtonText = value;
                NotifyOfPropertyChange(() => StartPauseButtonText);
            }
        }
        #endregion

        #region Drop Down Menu for Power Options
        /// <summary>
        /// Binding to drop down menu for either Auto-Scale or Threshold for chart for Power/LD0
        /// Allows user to select which view they want the chart have
        /// </summary>
        public BindableCollection<string> PowerScaleOptions
        {
            get { return _powerScaleOptions; }
            set
            {
                _powerScaleOptions = value;
                NotifyOfPropertyChange(() => PowerScaleOptions);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu in PowerScaleOptions
        /// </summary>
        public string SelectedPowerScaleOption
        {
            get { return _selectedPowerScaleOption; }
            set
            {
                _selectedPowerScaleOption = value;
                NotifyOfPropertyChange(() => SelectedPowerScaleOption);
            }
        }

        /// <summary>
        /// Binding to drop down menu to that lists the power channel options available. 
        /// This is implemented in MainPageViewModel and if power channel is disabled, it states disabled
        /// </summary>
        public BindableCollection<string> PowerChannelOptions
        {
            get { return _powerChannelOptions; }
            set
            {
                _powerChannelOptions = value;
                NotifyOfPropertyChange(() => PowerChannelOptions);
                SelectedPowerChannel = PowerChannelOptions[0];
            }
        }

        /// <summary>
        /// This is the binding for the actual option selected in the drop down menu in PowerChannelOptions
        /// </summary>
        public string SelectedPowerChannel
        {
            get { return _selectedPowerChannel; }
            set
            {
                _selectedPowerChannel = value;
                NotifyOfPropertyChange(() => SelectedPowerChannel);
            }
        }

        /// <summary>
        /// Binding to drop down menu to that lists the power channel two options available. 
        /// This is implemented in MainPageViewModel and if power channel is disabled, it states disabled
        /// </summary>
        public BindableCollection<string> PowerChannelOptionsTwo
        {
            get { return _powerChannelOptionsTwo; }
            set
            {
                _powerChannelOptionsTwo = value;
                NotifyOfPropertyChange(() => PowerChannelOptionsTwo);
                SelectedPowerChannelTwo = PowerChannelOptionsTwo[0];
            }
        }

        /// <summary>
        /// This is the binding for the actual option selected in the drop down menu in PowerChannelOptionsTwo
        /// </summary>
        public string SelectedPowerChannelTwo
        {
            get { return _selectedPowerChannelTwo; }
            set
            {
                _selectedPowerChannelTwo = value;
                NotifyOfPropertyChange(() => SelectedPowerChannelTwo);
            }
        }
        #endregion

        #region Set Data Points
        /// <summary>
        /// This allows user to input how many data points they want shown on chart.  
        /// This string is later converted to int and fed into scichart as number of datapoints
        /// Conversion is below when user presses SetDataPointsButton()
        /// </summary>
        public string DataPoints
        {
            get { return _dataPoints ?? (_dataPoints = DEFAULT_DATA_POINTS_POWER_DETECTOR.ToString()); }
            set
            {
                _dataPoints = value;
                NotifyOfPropertyChange(() => DataPoints);
            }
        }

        /// <summary>
        /// Sets the data points for the charts for all charts under visualization: power/LD0/current/state
        /// If user inputs a non-integer, then it will not change the data points
        /// ChangeDataSeriesForChart() does the actual changing of data points and is implemented below
        /// </summary>
        public void SetDataPointsButton()
        {
            //i is the acutal number of data points assuming the user gave an integer
            int i = 0;
            bool result = int.TryParse(DataPoints, out i);

            if (result)
            {
                if (ChangeDataSeriesForChart(i))
                {
                    Messages.Insert(0, DateTime.Now + ":: Changed Data Series to " + i);
                }
                else
                {
                    Messages.Insert(0, DateTime.Now + ":: ERROR: Could NOT change data points");
                }
            }
        }

        /// <summary>
        /// This changes the acutal data series for all charts under visualization tab.
        /// Original/default data point is set with this method in constructor of MainViewModel
        /// </summary>
        /// <param name="fifoCapacity">The value of data points for the chart on the x axis</param>
        /// <returns>True if successful and false if unsuccessful</returns>
        public bool ChangeDataSeriesForChart(int fifoCapacity)
        {
            bool canDo = true;
            try
            {
                _detectorLD0Chart.FifoCapacity = fifoCapacity;
                _detectorLD1Chart.FifoCapacity = fifoCapacity;
                _powerData.FifoCapacity = fifoCapacity;
                _powerDataTwo.FifoCapacity = fifoCapacity;
                //_powerLD1DataChart.FifoCapacity = fifoCapacity;
                //_powerLD1DataChartTwo.FifoCapacity = fifoCapacity;
                _b0ThresholdLine.FifoCapacity = fifoCapacity;
                _b1ThresholdLine.FifoCapacity = fifoCapacity;
                _b0LD1ThresholdLine.FifoCapacity = fifoCapacity;
                _b1LD1ThresholdLine.FifoCapacity = fifoCapacity;
                _adaptiveState.FifoCapacity = fifoCapacity;
                _adaptiveCurrentProgram0Chart.FifoCapacity = fifoCapacity;
                _adaptiveCurrentProgram1Chart.FifoCapacity = fifoCapacity;
                _adaptiveCurrentProgram2Chart.FifoCapacity = fifoCapacity;
                _adaptiveCurrentProgram3Chart.FifoCapacity = fifoCapacity;
            }
            catch
            {
                canDo = false;
            }
            return canDo;
        }
        #endregion

        #region Start/Stop Chart Buttons
        /// <summary>
        /// Button that starts and pauses the charts
        /// This is scichart implementation on how to start/pause the charts.
        /// </summary>
        public void StartPauseButton()
        {
            if (isChartPaused)
            {
                try
                {
                    if (suspenderPower != null)
                        suspenderPower.Dispose();
                    if (suspenderPowerTwo != null)
                        suspenderPowerTwo.Dispose();
                    if (suspenderPowerLD1 != null)
                        suspenderPowerLD1.Dispose();
                    if (suspenderPowerTwoLD1 != null)
                        suspenderPowerLD1.Dispose();
                    if (suspenderState != null)
                        suspenderState.Dispose();
                    if (suspenderCurrentProgram0 != null)
                        suspenderCurrentProgram0.Dispose();
                    if (suspenderCurrentProgram1 != null)
                        suspenderCurrentProgram1.Dispose();
                    if (suspenderCurrentProgram2 != null)
                        suspenderCurrentProgram2.Dispose();
                    if (suspenderCurrentProgram3 != null)
                        suspenderCurrentProgram3.Dispose();
                    if (suspenderB0 != null)
                        suspenderB0.Dispose();
                    if (suspenderB1 != null)
                        suspenderB1.Dispose();
                    if (suspenderDetector != null)
                        suspenderDetector.Dispose();

                    isChartPaused = false;
                    StartPauseButtonText = "Pause Chart";
                }
                catch (Exception e)
                {
                    Messages.Insert(0, DateTime.Now + ":: Could not start chart");
                    _log.Error(e);
                }
            }
            else
            {
                try
                {
                    suspenderPower = _powerData.ParentSurface.SuspendUpdates();
                    suspenderPowerTwo = _powerDataTwo.ParentSurface.SuspendUpdates();
                    //suspenderPowerLD1 = PowerLD1DataChart.ParentSurface.SuspendUpdates();
                    //suspenderPowerTwoLD1 = PowerLD1DataChartTwo.ParentSurface.SuspendUpdates();
                    suspenderState = _adaptiveState.ParentSurface.SuspendUpdates();
                    suspenderCurrentProgram0 = _adaptiveCurrentProgram0Chart.ParentSurface.SuspendUpdates();
                    suspenderCurrentProgram1 = _adaptiveCurrentProgram1Chart.ParentSurface.SuspendUpdates();
                    suspenderCurrentProgram2 = _adaptiveCurrentProgram2Chart.ParentSurface.SuspendUpdates();
                    suspenderCurrentProgram3 = _adaptiveCurrentProgram3Chart.ParentSurface.SuspendUpdates();
                    suspenderB1 = _b1ThresholdLine.ParentSurface.SuspendUpdates();
                    suspenderB0 = _b0ThresholdLine.ParentSurface.SuspendUpdates();
                    suspenderDetector = _detectorLD0Chart.ParentSurface.SuspendUpdates();

                    isChartPaused = true;
                    StartPauseButtonText = "Start Chart";
                }
                catch (Exception e)
                {
                    Messages.Insert(0, DateTime.Now + ":: Could not pause chart");
                    _log.Error(e);
                }
            }
        }
        #endregion

        #region Charts Binding
        /// <summary>
        /// Binding for the line chart for LD1 Detector. Actual input values added in MainPageViewModel under Detector Event for this chart
        /// </summary>
        public IDataSeries<double, double> DetectorLD1Chart
        {
            get { return _detectorLD1Chart; }
            set
            {
                _detectorLD1Chart = value;
                NotifyOfPropertyChange("DetectorLD1Chart");
            }
        }
        /// <summary>
        /// Binding for the line chart for LDO Detector. Actual input values added in MainPageViewModel under Detector Event for this chart
        /// </summary>
        public IDataSeries<double, double> DetectorLD0Chart
        {
            get { return _detectorLD0Chart; }
            set
            {
                _detectorLD0Chart = value;
                NotifyOfPropertyChange("DetectorLD0Chart");
            }
        }

        /// <summary>
        /// Binding for the line chart for Power. Actual input values added in MainPageViewModel under Power Event for this chart
        /// </summary>
        public IDataSeries<double, double> PowerDataChart
        {
            get { return _powerData; }
            set
            {
                _powerData = value;
                NotifyOfPropertyChange("PowerDataChart");
            }
        }

        /// <summary>
        /// Binding for the line chart for Power. Actual input values added in MainPageViewModel under Power Event for this chart
        /// </summary>
        public IDataSeries<double, double> PowerDataChartTwo
        {
            get { return _powerDataTwo; }
            set
            {
                _powerDataTwo = value;
                NotifyOfPropertyChange("PowerDataChartTwo");
            }
        }

        /// <summary>
        /// Binding for the line chart for Power in ld1 chart. Actual input values added in MainPageViewModel under Power Event for this chart
        /// </summary>
        //public IDataSeries<double, double> PowerLD1DataChart
        //{
        //    get { return _powerLD1DataChart; }
        //    set
        //    {
        //        _powerLD1DataChart = value;
        //        NotifyOfPropertyChange("PowerLD1DataChart");
        //    }
        //}

        /// <summary>
        /// Binding for the line chart for Power in ld1 chart. Actual input values added in MainPageViewModel under Power Event for this chart
        /// </summary>
        //public IDataSeries<double, double> PowerLD1DataChartTwo
        //{
        //    get { return _powerLD1DataChartTwo; }
        //    set
        //    {
        //        _powerLD1DataChartTwo = value;
        //        NotifyOfPropertyChange("PowerLD1DataChartTwo");
        //    }
        //}

        /// <summary>
        /// Binding for the line chart for B1 Threshold. 
        /// Usage in MainPageViewModel after user updates aDBS
        /// B1 upper threshold value taken from config file and drawn as a line on power/LD0 chart to show upper threshold
        /// </summary>
        public IDataSeries<double, double> B1ThresholdLine
        {
            get { return _b1ThresholdLine; }
            set
            {
                _b1ThresholdLine = value;
                NotifyOfPropertyChange("B1ThresholdLine");
            }
        }

        /// <summary>
        /// Binding for the line chart for B0 Threshold. 
        /// Usage in MainPageViewModel after user updates aDBS
        /// B0 lower threshold value taken from config file and drawn as a line on power/LD0 chart to show lower threshold
        /// </summary>
        public IDataSeries<double, double> B0ThresholdLine
        {
            get { return _b0ThresholdLine; }
            set
            {
                _b0ThresholdLine = value;
                NotifyOfPropertyChange("B0ThresholdLine");
            }
        }

        /// <summary>
        /// Binding for the line chart for B1 LD1 Threshold. 
        /// Usage in MainPageViewModel after user updates aDBS
        /// B1 upper threshold value taken from config file and drawn as a line on power/LD1 chart to show upper threshold
        /// </summary>
        public IDataSeries<double, double> B1LD1ThresholdLine
        {
            get { return _b1LD1ThresholdLine; }
            set
            {
                _b1LD1ThresholdLine = value;
                NotifyOfPropertyChange("B1LD1ThresholdLine");
            }
        }

        /// <summary>
        /// Binding for the line chart for B0 LD1 Threshold. 
        /// Usage in MainPageViewModel after user updates aDBS
        /// B1 upper threshold value taken from config file and drawn as a line on power/LD1 chart to show upper threshold
        /// </summary>
        public IDataSeries<double, double> B0LD1ThresholdLine
        {
            get { return _b0LD1ThresholdLine; }
            set
            {
                _b0LD1ThresholdLine = value;
                NotifyOfPropertyChange("B0LD1ThresholdLine");
            }
        }

        /// <summary>
        /// Binding for the line chart for Adaptive State. Actual input values added in MainPageViewModel under Detector Event for this chart
        /// </summary>
        public IDataSeries<double, double> AdaptiveStateChart
        {
            get { return _adaptiveState; }
            set
            {
                _adaptiveState = value;
                NotifyOfPropertyChange("AdaptiveStateChart");
            }
        }

        /// <summary>
        /// Binding for the line chart for Current. Actual input values added in MainPageViewModel under Detector Event for this chart
        /// </summary>
        public IDataSeries<double, double> AdaptiveCurrentProgram0Chart
        {
            get { return _adaptiveCurrentProgram0Chart; }
            set
            {
                _adaptiveCurrentProgram0Chart = value;
                NotifyOfPropertyChange("AdaptiveCurrentProgram0Chart");
            }
        }

        /// <summary>
        /// Binding for the line chart for Current Program 1. Actual input values added in MainPageViewModel under Detector Event for this chart
        /// </summary>
        public IDataSeries<double, double> AdaptiveCurrentProgram1Chart
        {
            get { return _adaptiveCurrentProgram1Chart; }
            set
            {
                _adaptiveCurrentProgram1Chart = value;
                NotifyOfPropertyChange("AdaptiveCurrentProgram1Chart");
            }
        }
        /// <summary>
        /// Binding for the line chart for Current Program 2. Actual input values added in MainPageViewModel under Detector Event for this chart
        /// </summary>
        public IDataSeries<double, double> AdaptiveCurrentProgram2Chart
        {
            get { return _adaptiveCurrentProgram2Chart; }
            set
            {
                _adaptiveCurrentProgram2Chart = value;
                NotifyOfPropertyChange("AdaptiveCurrentProgram2Chart");
            }
        }
        /// <summary>
        /// Binding for the line chart for Current Program 3. Actual input values added in MainPageViewModel under Detector Event for this chart
        /// </summary>
        public IDataSeries<double, double> AdaptiveCurrentProgram3Chart
        {
            get { return _adaptiveCurrentProgram3Chart; }
            set
            {
                _adaptiveCurrentProgram3Chart = value;
                NotifyOfPropertyChange("AdaptiveCurrentProgram3Chart");
            }
        }
        #endregion

    }
}


