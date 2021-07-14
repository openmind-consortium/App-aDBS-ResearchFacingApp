/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.Models;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Core.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    public partial class MainViewModel : Screen
    {
        #region Variables
        /// <summary>
        /// Yaxis for the state chart
        /// </summary>
        private const uint DEFAULT_SET_BUFFER = 50; 
        private const uint DEFAULT_SET_B0_B1_LENGTH = 20000;
        /// <summary>
        /// Y axis for the state space chart
        /// </summary>
        public ObservableCollection<IAxisViewModel> YAxesState { get { return _yAxesState; } }
        private ObservableCollection<IAxisViewModel> _yAxesState = new ObservableCollection<IAxisViewModel>();
        private IDataSeries<double, double> _stateSpaceChart = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _b1StateThresholdLine = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _b0StateThresholdLine = new XyDataSeries<double, double>();
        IUpdateSuspender suspenderStateSpace = null;
        IUpdateSuspender suspenderB0StateSpace = null;
        IUpdateSuspender suspenderB1StateSpace = null;
        private string _weightOneTB, _weightTwoTB, _biasTerm0TB, _biasTerm1TB, _fFPVTB, _setStateChartBufferTB, _setB0B1LengthTB, _updateRateTB;
        private string _x1ValueFromLineDrawStateSpaceText, _y1ValueFromLineDrawStateSpaceText, _x2ValueFromLineDrawStateSpaceText, _y2ValueFromLineDrawStateSpaceText;
        private double _weightOneValue, _weightTwoValue;
        private int _biasTerm0Value, _biasTerm1Value;
        private uint stateChartBufferValue = DEFAULT_SET_BUFFER;
        private byte _fFPVValue;
        private static uint _b0B1Length = 0;
        private ushort _updateRateValue;
        private double _x1ValueFromLineDrawStateSpaceValue, _y1ValueFromLineDrawStateSpaceValue, _x2ValueFromLineDrawStateSpaceValue, _y2ValueFromLineDrawStateSpaceValue;
        #endregion

        #region Chart Bindings
        /// <summary>
        /// Binding for the line chart for FFTChart. Actual input values added in MainPageViewModel under FFT Event Handler for this chart
        /// </summary>
        public IDataSeries<double, double> StateSpaceChart
        {
            get { return _stateSpaceChart; }
            set
            {
                _stateSpaceChart = value;
                NotifyOfPropertyChange("StateSpaceChart");
            }
        }

        /// <summary>
        /// Binding for the line chart for B1 Threshold in state chart. 
        /// B1 upper threshold value taken from config file and drawn as a line on upper threshold
        /// </summary>
        public IDataSeries<double, double> B1StateThresholdLine
        {
            get { return _b1StateThresholdLine; }
            set
            {
                _b1StateThresholdLine = value;
                NotifyOfPropertyChange("B1StateThresholdLine");
            }
        }

        /// <summary>
        /// Binding for the line chart for B0 Threshold in state chart. 
        /// B0 lower threshold value taken from config file and drawn as a line to show lower threshold
        /// </summary>
        public IDataSeries<double, double> B0StateThresholdLine
        {
            get { return _b0StateThresholdLine; }
            set
            {
                _b0StateThresholdLine = value;
                NotifyOfPropertyChange("B0StateThresholdLine");
            }
        }
        #endregion

        #region Start/Stop Chart Buttons
        /// <summary>
        /// Start Button that starts the charts after being paused by the user
        /// This is scichart implementation on how to start the charts after being paused.
        /// </summary>
        public void StartStateChartButton()
        {
            try
            {
                if (suspenderStateSpace != null)
                    suspenderStateSpace.Dispose();
                if (suspenderB0StateSpace != null)
                    suspenderB0StateSpace.Dispose();
                if (suspenderB1StateSpace != null)
                    suspenderB1StateSpace.Dispose();
            }
            catch (Exception e)
            {
                Messages.Insert(0, DateTime.Now + ":: Could not start chart");
                _log.Error(e);
            }
        }

        /// <summary>
        /// Pause Button that pauses all charts after user presses this button.
        /// This is scichart implementation on how to pause a chart
        /// </summary>
        public void PauseStateChartButton()
        {
            try
            {
                suspenderStateSpace = StateSpaceChart.ParentSurface.SuspendUpdates();
                suspenderB0StateSpace = B0StateThresholdLine.ParentSurface.SuspendUpdates();
                suspenderB1StateSpace = B1StateThresholdLine.ParentSurface.SuspendUpdates();
            }
            catch (Exception e)
            {
                Messages.Insert(0, DateTime.Now + ":: Could not pause chart");
                _log.Error(e);
            }
        }
        #endregion

        #region TextBoxes and Textblocks Bindings
        /// <summary>
        /// Binding for Set B0 B1 Length text box
        /// </summary>
        public string SetB0B1LengthTB
        {
            get
            {
                return _setB0B1LengthTB ?? (_setB0B1LengthTB = DEFAULT_SET_B0_B1_LENGTH.ToString());
            }
            set
            {
                _setB0B1LengthTB = value;
                NotifyOfPropertyChange(() => SetB0B1LengthTB);
            }
        }
        /// <summary>
        /// Binding for Set Buffer text box
        /// </summary>
        public string SetStateChartBufferTB
        {
            get { return _setStateChartBufferTB  ?? (_setStateChartBufferTB = DEFAULT_SET_BUFFER.ToString()); 
        }
            set
            {
                _setStateChartBufferTB = value;
                NotifyOfPropertyChange(() => SetStateChartBufferTB);
            }
        }
        /// <summary>
        /// Binding for WeightOneTB text box
        /// </summary>
        public string WeightOneTB
        {
            get { return _weightOneTB; }
            set
            {
                _weightOneTB = value;
                NotifyOfPropertyChange(() => WeightOneTB);
            }
        }
        /// <summary>
        /// Binding for WeightTwoTB text box
        /// </summary>
        public string WeightTwoTB
        {
            get { return _weightTwoTB; }
            set
            {
                _weightTwoTB = value;
                NotifyOfPropertyChange(() => WeightTwoTB);
            }
        }
        /// <summary>
        /// Binding for BiasTerm0TB text box
        /// </summary>
        public string BiasTerm0TB
        {
            get { return _biasTerm0TB; }
            set
            {
                _biasTerm0TB = value;
                NotifyOfPropertyChange(() => BiasTerm0TB);
            }
        }
        /// <summary>
        /// Binding for BiasTerm1TB text box
        /// </summary>
        public string BiasTerm1TB
        {
            get { return _biasTerm1TB; }
            set
            {
                _biasTerm1TB = value;
                NotifyOfPropertyChange(() => BiasTerm1TB);
            }
        }
        /// <summary>
        /// Binding for FFPVTB text box
        /// </summary>
        public string FFPVTB
        {
            get { return _fFPVTB; }
            set
            {
                _fFPVTB = value;
                NotifyOfPropertyChange(() => FFPVTB);
            }
        }
        /// <summary>
        /// Binding for Update rate text box
        /// </summary>
        public string UpdateRateTB
        {
            get { return _updateRateTB; }
            set
            {
                _updateRateTB = value;
                NotifyOfPropertyChange(() => UpdateRateTB);
            }
        }
        /// <summary>
        /// Binding for the X1 Data Point display
        /// </summary>
        public string X1ValueFromLineDrawStateSpaceText
        {
            get { return _x1ValueFromLineDrawStateSpaceText; }
            set
            {
                _x1ValueFromLineDrawStateSpaceText = value;
                NotifyOfPropertyChange(() => X1ValueFromLineDrawStateSpaceText);
            }
        }
        /// <summary>
        /// Binding for the Y1 Data Point display
        /// </summary>
        public string Y1ValueFromLineDrawStateSpaceText
        {
            get { return _y1ValueFromLineDrawStateSpaceText; }
            set
            {
                _y1ValueFromLineDrawStateSpaceText = value;
                NotifyOfPropertyChange(() => Y1ValueFromLineDrawStateSpaceText);
            }
        }
        /// <summary>
        /// Binding for the X2 Data Point display
        /// </summary>
        public string X2ValueFromLineDrawStateSpaceText
        {
            get { return _x2ValueFromLineDrawStateSpaceText; }
            set
            {
                _x2ValueFromLineDrawStateSpaceText = value;
                NotifyOfPropertyChange(() => X2ValueFromLineDrawStateSpaceText);
            }
        }
        /// <summary>
        /// Binding for the Y2 Data Point display
        /// </summary>
        public string Y2ValueFromLineDrawStateSpaceText
        {
            get { return _y2ValueFromLineDrawStateSpaceText; }
            set
            {
                _y2ValueFromLineDrawStateSpaceText = value;
                NotifyOfPropertyChange(() => Y2ValueFromLineDrawStateSpaceText);
            }
        }
        #endregion

        #region Button Clicks
        /// <summary>
        /// Starts the matlab chart window
        /// </summary>
        public void StartMatlabChartButton()
        {
            try
            {
                InitFigureSample1.Setup();
                InitFigureSample1.InitFigureSample();
                PlotDataSample1.Setup();
            }
            catch(Exception e)
            {
                _log.Error(e);
            }

            plotDataForMatlabStateSpace = true;
        }
        /// <summary>
        /// Gets the values from the matlab chart after drawing the line
        /// This also hides the matlab chart window so the window doesn't keep popping up when trying to use the reasearch app
        /// </summary>
        public void HideMatlabChartGetDrawValuesButton()
        {
            try
            {
                GetLineCoordinatesSample1.Setup();
                Object[] drawLineValues = GetLineCoordinatesSample1.GetLineCoordinatesSample(_log);
                if(drawLineValues != null)
                {
                    if(drawLineValues[0] != null)
                    {
                        SplitValuesFromMatlab(drawLineValues[0].ToString());
                        SetAndCheckDoubleValuesFromMatlab();
                        DrawLineFromMatlabToSciChart();
                    }
                }
                HideFigureSample1.Setup();
                HideFigureSample1.HideFigureSample(_log);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
        private void SplitValuesFromMatlab(string value)
        {
            string actualName = Regex.Replace(value, @"\r\n?|\n|\s+", "-");
            string[] partsOfName = actualName.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if(7 < partsOfName.Length)
            {
                X1ValueFromLineDrawStateSpaceText = Math.Round(Convert.ToDecimal(partsOfName[2]), 2).ToString();
                Y1ValueFromLineDrawStateSpaceText = Math.Round(Convert.ToDecimal(partsOfName[6]), 2).ToString();
                X2ValueFromLineDrawStateSpaceText = Math.Round(Convert.ToDecimal(partsOfName[3]), 2).ToString();
                Y2ValueFromLineDrawStateSpaceText = Math.Round(Convert.ToDecimal(partsOfName[7]), 2).ToString();
            }
        }
        private void SetAndCheckDoubleValuesFromMatlab()
        {
            //Check that values are correct and not missing from textboxes
            Tuple<bool, double> result;
            result = ChecknGetDoubleTextBlockValue(X1ValueFromLineDrawStateSpaceText, "X1Value");
            if (result.Item1)
            {
                _x1ValueFromLineDrawStateSpaceValue = result.Item2;
            }
            else
            {
                return;
            }
            result = ChecknGetDoubleTextBlockValue(Y1ValueFromLineDrawStateSpaceText, "Y1Value");
            if (result.Item1)
            {
                _y1ValueFromLineDrawStateSpaceValue = result.Item2;
            }
            else
            {
                return;
            }
            result = ChecknGetDoubleTextBlockValue(X2ValueFromLineDrawStateSpaceText, "X2Value");
            if (result.Item1)
            {
                _x2ValueFromLineDrawStateSpaceValue = result.Item2;
            }
            else
            {
                return;
            }
            result = ChecknGetDoubleTextBlockValue(Y2ValueFromLineDrawStateSpaceText, "Y2Value");
            if (result.Item1)
            {
                _y2ValueFromLineDrawStateSpaceValue = result.Item2;
            }
            else
            {
                return;
            }
        }
        private void DrawLineFromMatlabToSciChart()
        {
            _b0StateThresholdLine.Clear();
            _b0StateThresholdLine.Append(_x1ValueFromLineDrawStateSpaceValue, _y1ValueFromLineDrawStateSpaceValue);
            _b0StateThresholdLine.Append(_x2ValueFromLineDrawStateSpaceValue, _y2ValueFromLineDrawStateSpaceValue);
        }
        /// <summary>
        /// After hiding the matlab window, this shows it again
        /// </summary>
        public void ShowMatlabChartButton()
        {
            try
            {
                ShowFigureSample1.Setup();
                ShowFigureSample1.ShowFigureSample(_log);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
        /// <summary>
        /// Button sets length of B0/B1 lines
        /// </summary>
        public void SetB0B1LengthButton()
        {
            Tuple<bool, int> result;
            result = ChecknGetIntTextBoxValue(SetB0B1LengthTB, "Set B0/B1 Length");
            if (result.Item1)
            {
                _b0B1Length = (uint)result.Item2;
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// Sets the buffer for the state chart
        /// </summary>
        public void SetStateChartBufferButton()
        {
            lock (_bufferXQueueLock) lock (_bufferYQueueLock) lock (_bufferXListLock) lock (_bufferYListLock)
            {
                            stateSpaceXValueQueue.Clear();
                            stateSpaceYValueQueue.Clear();
                            powerStateSpaceXValueList.Clear();
                            powerStateSpaceYValueList.Clear();
            }

            Tuple<bool, int> result;
            result = ChecknGetIntTextBoxValue(SetStateChartBufferTB, "Set Buffer");
            if (result.Item1)
            {
                stateChartBufferValue = (uint)result.Item2;
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// Button that sets the adaptive config values
        /// </summary>
        public void SetAdaptiveValueInputButton()
        {
            //Check that values are correct and not missing from textboxes
            Tuple<bool, int> result;
            Tuple<bool, double> resultDouble;
            resultDouble = ChecknGetDoubleTextBlockValue(WeightOneTB, "W1");
            if (resultDouble.Item1)
            {
                _weightOneValue = resultDouble.Item2;
            }
            else
            {
                return;
            }
            resultDouble = ChecknGetDoubleTextBlockValue(WeightTwoTB, "W2");
            if (resultDouble.Item1)
            {
                _weightTwoValue = resultDouble.Item2;
            }
            else
            {
                return;
            }
            result = ChecknGetIntTextBoxValue(BiasTerm0TB, "B0");
            if (result.Item1)
            {
                _biasTerm0Value = result.Item2;
            }
            else
            {
                return;
            }
            result = ChecknGetIntTextBoxValue(BiasTerm1TB, "B1");
            if (result.Item1)
            {
                _biasTerm1Value = result.Item2;
            }
            else
            {
                return;
            }
            result = ChecknGetIntTextBoxValue(FFPVTB, "FFPV");
            if (result.Item1)
            {
                _fFPVValue = (byte)result.Item2;
            }
            else
            {
                return;
            }
            result = ChecknGetIntTextBoxValue(UpdateRateTB, "UpdateRate");
            if (result.Item1)
            {
                _updateRateValue = (ushort)result.Item2;
            }
            else
            {
                return;
            }
            //Get current adaptive config file
            JSONService jService = new JSONService(_log);
            adaptiveConfig = jService.GetAdaptiveModelFromFile(adaptiveFileLocation);
            if (adaptiveConfig == null)
            {
                return;
            }
            //Set values to current adaptive config file
            adaptiveConfig.Detection.LD0.WeightVector[0] = _weightOneValue;
            adaptiveConfig.Detection.LD0.WeightVector[1] = _weightTwoValue;
            adaptiveConfig.Detection.LD0.B0 = _biasTerm0Value;
            adaptiveConfig.Detection.LD0.B1 = _biasTerm1Value;
            adaptiveConfig.Detection.LD0.FractionalFixedPointValue = _fFPVValue;
            adaptiveConfig.Detection.LD0.UpdateRate = _updateRateValue;
            //Write the config model to file
            if (jService.WriteAdaptiveConfigToFile(adaptiveConfig, adaptiveFileLocation))
            {
                Messages.Insert(0, DateTime.Now + ":: Success writing Adaptive Config to file");
            }
            else
            {
                ErrorMessageToUser("Error writing adaptive config to file. Please try again");
                return;
            }
            //Show success box!
            AutoClosingMessageBox.Show("Save was successful", "Success!", 1500);
        }

        #endregion
        private void PopulateStateSpaceTextBoxes()
        {
            adaptiveConfig = jSONService.GetAdaptiveModelFromFile(adaptiveFileLocation);
            WeightOneTB = adaptiveConfig.Detection.LD0.WeightVector[0].ToString();
            WeightTwoTB = adaptiveConfig.Detection.LD0.WeightVector[1].ToString();
            BiasTerm0TB = adaptiveConfig.Detection.LD0.B0.ToString();
            BiasTerm1TB = adaptiveConfig.Detection.LD0.B1.ToString();
            FFPVTB = adaptiveConfig.Detection.LD0.FractionalFixedPointValue.ToString();
            UpdateRateTB = adaptiveConfig.Detection.LD0.UpdateRate.ToString();

            //ensure the temp application values are set in case of discrepency
            _weightOneValue = adaptiveConfig.Detection.LD0.WeightVector[0];
            _weightTwoValue = adaptiveConfig.Detection.LD0.WeightVector[1];
            _biasTerm0Value = adaptiveConfig.Detection.LD0.B0;
            _biasTerm1Value = adaptiveConfig.Detection.LD0.B1;
            _fFPVValue = adaptiveConfig.Detection.LD0.FractionalFixedPointValue;
            _updateRateValue = adaptiveConfig.Detection.LD0.UpdateRate;
        }
        /// <summary>
        /// helper function that checks int textbox values and returns them
        /// </summary>
        /// <param name="TB">Textbox value as string</param>
        /// <param name="nameOfTB">Name or title of the text box for user</param>
        /// <returns>Tuple(bool,int) where bool is true if success and int is actual value from textbox</returns>
        private Tuple<bool, int> ChecknGetIntTextBoxValue(string TB, string nameOfTB)
        {
            if (String.IsNullOrWhiteSpace(TB) || !Int32.TryParse(TB, out int nothing))
            {
                ShowMessageBox(nameOfTB + " is missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(TB) && Int32.TryParse(TB, out int Result))
            {
                return new Tuple<bool, int>(true, Result);
            }
            return new Tuple<bool, int>(false, 0);
        }
        /// <summary>
        /// helper function that checks int textblock values and returns them
        /// </summary>
        /// <param name="TB">Textbox value as string</param>
        /// <param name="nameOfTB">Name or title of the text box for user</param>
        /// <returns>Tuple(bool,double) where bool is true if success and double is actual value from textblock</returns>
        private Tuple<bool, double> ChecknGetDoubleTextBlockValue(string TB, string nameOfTB)
        {
            if (String.IsNullOrWhiteSpace(TB) || !Double.TryParse(TB, out double nothing))
            {
                ShowMessageBox(nameOfTB + " is missing or incorrect format. Please fix and try again", "Error");
            }
            else if (!String.IsNullOrWhiteSpace(TB) && Double.TryParse(TB, out double Result))
            {
                return new Tuple<bool, double>(true, Result);
            }
            return new Tuple<bool, double>(false, 0);
        }
    }
}
