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
        IUpdateSuspender suspenderState = null;
        IUpdateSuspender suspenderCurrent = null;
        IUpdateSuspender suspenderB1 = null;
        IUpdateSuspender suspenderB0 = null;
        IUpdateSuspender suspenderDetector = null;
        //String to get number of data points for the graphs from user. Implemented below under DataPoints
        private string _dataPoints;
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
                _powerData.FifoCapacity = fifoCapacity;
                _b0ThresholdLine.FifoCapacity = fifoCapacity;
                _b1ThresholdLine.FifoCapacity = fifoCapacity;
                _adaptiveState.FifoCapacity = fifoCapacity;
                _adaptiveCurrent.FifoCapacity = fifoCapacity;
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
        /// Start Button that starts the charts after being paused by the user
        /// This is scichart implementation on how to start the charts after being paused.
        /// </summary>
        public void StartButton()
        {
            try
            {
                if (suspenderPower != null)
                    suspenderPower.Dispose();
                if (suspenderState != null)
                    suspenderState.Dispose();
                if (suspenderCurrent != null)
                    suspenderCurrent.Dispose();
                if (suspenderB0 != null)
                    suspenderB0.Dispose();
                if (suspenderB1 != null)
                    suspenderB1.Dispose();
                if (suspenderDetector != null)
                    suspenderDetector.Dispose();
            }
            catch(Exception e)
            {
                Messages.Insert(0, DateTime.Now + ":: Could not start chart");
                _log.Error(e);
            }
        }

        /// <summary>
        /// Pause Button that pauses all charts after user presses this button.
        /// This is scichart implementation on how to pause a chart
        /// </summary>
        public void PauseButton()
        {
            try
            {
                suspenderPower = _powerData.ParentSurface.SuspendUpdates();
                suspenderState = _adaptiveState.ParentSurface.SuspendUpdates();
                suspenderCurrent = _adaptiveCurrent.ParentSurface.SuspendUpdates();
                suspenderB1 = _b1ThresholdLine.ParentSurface.SuspendUpdates();
                suspenderB0 = _b0ThresholdLine.ParentSurface.SuspendUpdates();
                suspenderDetector = _detectorLD0Chart.ParentSurface.SuspendUpdates();
            }
            catch (Exception e)
            {
                Messages.Insert(0, DateTime.Now + ":: Could not pause chart");
                _log.Error(e);
            }
        }
        #endregion

        #region Charts Binding
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
        public IDataSeries<double, double> AdaptiveCurrentChart
        {
            get { return _adaptiveCurrent; }
            set
            {
                _adaptiveCurrent = value;
                NotifyOfPropertyChange("AdaptiveCurrentChart");
            }
        }
        #endregion

    }
}


