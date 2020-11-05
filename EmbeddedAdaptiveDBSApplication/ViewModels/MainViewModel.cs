/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using Medtronic.SummitAPI.Classes;
using EmbeddedAdaptiveDBSApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using SciChart.Charting.ViewportManagers;
using SciChart.Charting.Model.DataSeries;
using System.Windows.Media;
using SciChart.Data.Model;
using SciChart.Charting.Model.ChartSeries;
using System.Collections.ObjectModel;
using SciChart.Charting.Visuals.Axes;
using Medtronic.SummitAPI.Flash;
using Medtronic.NeuroStim.Olympus.DataTypes.Sensing;
using Medtronic.NeuroStim.Olympus.DataTypes.DeviceManagement;
using System.Diagnostics;
using System.Reflection;
using EmbeddedAdaptiveDBSApplication.Services;
using NAudio.Wave;
using System.Windows.Controls;
using Medtronic.NeuroStim.Olympus.DataTypes.Therapy;
using Medtronic.TelemetryM;
using System.Timers;

namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    /// <summary>
    /// Main class that controls the connection, connect button/display, window closing, disposing summit system/manager, and constructor
    /// This is in general the original class with the main window and all other classes are partial to this class
    /// This class is also tied to MainPageViewModel since they both have Bindings from the same Main tab.
    /// </summary>
    public partial class MainViewModel : Screen
    {
        #region MainViewModel VARIABLES:
        private static readonly string applicationFileLocation = @"C:\AdaptiveDBS\application_config.json";
        //logger
        private static readonly ILog _log = LogManager.GetLog(typeof(MainViewModel));
        private IWindowManager manager = new WindowManager();
        private static readonly string PROJECT_ID = "StarrLab";
        private SummitConnect connect = new SummitConnect();
        private static SenseModel senseConfig = null;
        private static SenseModel senseConfigFromUI = null;
        private static SummitManager theSummitManager = null;
        private static SummitSystem theSummit = null;
        private static Thread workerThread;
        //INS battery levels
        private static System.Timers.Timer batteryTimer = new System.Timers.Timer();
        private BatteryLevel INSbatteryLevel = new BatteryLevel();
        private string _iNSBatteryLevel;
        //Thread for doing the align process
        private static Thread alignThread;
        //Variable to stop worker thread when window is closing. Cleaner than just aborting thread, but aborting happens anyway
        private static volatile bool _shouldStopWorkerThread = false;
        //Connection variables. Implementation below in class under Connect button bindings region
        private static bool isConnected = false;
        private static bool _isSpinnerVisible = false;
        private static bool _stimSettingButtonsEnabled = false;
        private string _connectButtonText;
        private Brush _connectButtonColor, _therapyStatusBackground, _ratioBorderColor, _modeBorderColor;
        private bool _stimOnButtonEnabled, _stimOffButtonEnabled, _groupAButtonEnabled, _groupBButtonEnabled, _groupCButtonEnabled, _groupDButtonEnabled;
        private Stopwatch sw = new Stopwatch();
        private string _titleText = "";
        private AppModel appConfigModel = null;
        private string basePathForMedtronicFiles;
        //Variables for writing an event for a beep
        WaveIn waveIn;
        int signalOnValue = 10;
        bool previousOnFlag = false;
        bool currentOnFlag = false;
        //Program options is the drop down for the medtronic program selection
        private BindableCollection<string> _programOptions = new BindableCollection<string>();
        private string _selectedProgram;
        private readonly string program0Option = "Program 0";
        private readonly string program1Option = "Program 1";
        private readonly string program2Option = "Program 2";
        private readonly string program3Option = "Program 3";
        //Combobox for mode and ratio
        private int _borderThicknessForAllCB = 3;
        private Brush comboboxChangedBrush = Brushes.Red;
        private Brush comboboxNotChangedBrush = Brushes.Transparent;
        private ushort _selectedMode;
        private byte _selectedRatio;
        private BindableCollection<ushort> _modeCB = new BindableCollection<ushort>();
        private BindableCollection<byte> _ratioCB = new BindableCollection<byte>();
        //CTM connection
        private bool _connectWithSelectedCTM = false;
        private BindableCollection<string> _cTMCB = new BindableCollection<string>();
        private string _selectedCTM;
        //Patient ID display variable
        private string _devicePatientID;
        #endregion

        #region VisualizationViewModel VARIABLES:
        /// <summary>
        /// Below variable is implemented mainly in VisualizationViewModel.cs for Binding to front end MVVM
        /// Yaxes is the Binded yaxis for current/state
        /// This is used to separate the y axis for each chart
        /// </summary>
        public ObservableCollection<IAxisViewModel> YAxes { get { return _yAxes; } }
        /// <summary>
        ///  variable is implemented mainly in VisualizationViewModel.cs for Binding to front end MVVM
        /// yAxesPower is for power/detector
        /// This is used to separate the y axis for each chart
        /// </summary>
        public ObservableCollection<IAxisViewModel> YAxesPower { get { return _yAxesPower; } }
        private ObservableCollection<IAxisViewModel> _yAxes = new ObservableCollection<IAxisViewModel>();
        private ObservableCollection<IAxisViewModel> _yAxesPower = new ObservableCollection<IAxisViewModel>();
        //IDataSeris is the actual line chart for adding data to be plotted
        private IDataSeries<double, double> _powerData = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _b1ThresholdLine = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _b0ThresholdLine = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _detectorLD0Chart = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _adaptiveState = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _adaptiveCurrent = new XyDataSeries<double, double>();
        //_powerChannelOptions and _powerScaleOption variables are Binded in VisualizationViewModel. This is actual variable
        private BindableCollection<string> _powerChannelOptions = new BindableCollection<string>();
        private BindableCollection<string> _powerScaleOptions = new BindableCollection<string>();
        //Gives the options for drop down menu for how to view the power/LD0 chart 
        private readonly string powerAutoScaleChartOption = "AutoScale";
        private readonly string powerThresholdScaleChartOption = "Threshold";
        private readonly string powerNoneScaleChartOption = "None";
        //All variables below are used in VisualizationViewModel.cs for binding to the front end
        private string _selectedPowerChannel, _selectedPowerScaleOption;
        //Variable for binding for the UI to set the visible range for the Y value for Power
        private IRange _powerYAxisVisibleRange;
        private int DEFAULT_DATA_POINTS_POWER_DETECTOR = 100;
        #endregion

        #region VisualizationSenseViewModel VARIABLES:
        /// <summary>
        /// Y axis implementation similar to the VisualizationViewModel Charts. 
        /// This one collection will be used for time domain STN charts in the VisualizationSenseViewModel
        /// </summary>
        public ObservableCollection<IAxisViewModel> YAxesTimeDomainSTN { get { return _yAxesTimeDomainSTN; } }
        private ObservableCollection<IAxisViewModel> _yAxesTimeDomainSTN = new ObservableCollection<IAxisViewModel>();
        /// <summary>
        /// Y axis implementation similar to the VisualizationViewModel Charts. 
        /// This one collection will be used for time domain M1 charts in the VisualizationSenseViewModel
        /// </summary>
        public ObservableCollection<IAxisViewModel> YAxesTimeDomainM1 { get { return _yAxesTimeDomainM1; } }
        private ObservableCollection<IAxisViewModel> _yAxesTimeDomainM1 = new ObservableCollection<IAxisViewModel>();
        /// <summary>
        /// Y axis implementation similar to the VisualizationViewModel Charts. 
        /// This one collection will be used for accelerometer charts in the VisualizationSenseViewModel
        /// </summary>
        public ObservableCollection<IAxisViewModel> YAxesAccelerometer { get { return _yAxesAccelerometer; } }
        private ObservableCollection<IAxisViewModel> _yAxesAccelerometer = new ObservableCollection<IAxisViewModel>();
        //IDataSeris is the actual line chart for adding data to be plotted
        private IDataSeries<double, double> _timeDomainSTNChart = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _timeDomainM1Chart = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _accelerometryXChart = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _accelerometryYChart = new XyDataSeries<double, double>();
        private IDataSeries<double, double> _accelerometryZChart = new XyDataSeries<double, double>();
        //Binded collection for the drop down menu to choose which time domain key for each timedomain chart
        private BindableCollection<string> _timeDomainSTNDropDown = new BindableCollection<string>();
        private BindableCollection<string> _timeDomainM1DropDown = new BindableCollection<string>();
        //Variable for the selected item in each binded collection for the drop down menu for time domain key
        private string _selectedTimeDomainM1, _selectedTimeDomainSTN;
        private readonly int FONTSIZE = 20;
        private int DEFAULT_DATA_POINTS_TD = 1000;
        //Drop Down menu for the FFT scaling
        private BindableCollection<string> _fftScaleOptions = new BindableCollection<string>();
        private string _selectedFFTScaleOption;
        private readonly string fftAutoScaleChartOption = "AutoScale";
        private readonly string fftLog10ScaleChartOption = "Log10";
        private readonly string fftNoneScaleChartOption = "None";

        /// <summary>
        /// Y axis for FFT allows me to change it in code
        /// </summary>
        public ObservableCollection<IAxisViewModel> YAxesFFT { get { return _yAxesFFT; } }
        private ObservableCollection<IAxisViewModel> _yAxesFFT = new ObservableCollection<IAxisViewModel>();
        private IDataSeries<double, double> _fftChart = new XyDataSeries<double, double>();
        #endregion

        #region MainPageViewModel VARIABLES:
        //lower/upper BinActualValues are for storing the actual values from the power
        //User adds in the estimated values in config file and actual values are calculated based on these values
        //The actual values are stored in these arrays to display to user in Visualization Tab.  Implementation for storing in MainPageViewModel.cs.
        private double[] lowerPowerBinActualValues = new double[8];
        private double[] upperPowerBinActualValues = new double[8];
        //deviceId and patientID are used to find the path to the medtronic json files.
        //Later we use this path to store config files in the same directory
        //sessionDisplay shows what number of session we are on or what number of times we have run updateDBS in MainPageViewModel
        private string deviceID, patientID, _sessionDisplay, leadLocation1, leadLocation2;
        #endregion

        #region ReportWindow VARIABLES:
        private JSONService jSONService;
        #endregion

        /// <summary>
        /// Contains the Constructors for MainViewModel, MainPageViewModel, VisualizationViewModel and ReportWindowModel
        /// </summary>
        public MainViewModel()
        {
            #region ReportViewModel Constructor
            jSONService = new JSONService(_log);
            stimData = new StimulationData(_log);
            summitSensing = new SummitSensing(_log);
            //Medication and Condition list for report window.
            //Both of these collection data come from the same json file
            MedicationList = new ObservableCollection<MedicationCheckBoxClass>();
            ConditionList = new ObservableCollection<ConditionCheckBoxClass>();
            //These two methods are implemented in ReportViewModel.cs
            reportConfig = jSONService?.GetReportModelFromFile(REPORT_FILEPATH);
            if (reportConfig == null)
            {
                MessageBox.Show("Report Config could not be loaded. Please check that it exists or has the correct format", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                isReportConfigFileFound = true;
                GetListOfMedicationsConditionsFromConfig();
            }
            #endregion

            #region Visualization Constructor
            //Give series a name so legend in each chart shows a name
            _adaptiveCurrent.SeriesName = "Current";
            _adaptiveState.SeriesName = "State";
            _powerData.SeriesName = "Power";
            _detectorLD0Chart.SeriesName = "LDO";
            _b0ThresholdLine.SeriesName = "B0";
            _b1ThresholdLine.SeriesName = "B1";
            //Fifo capacity sets the number of values in the x axis shown
            ChangeDataSeriesForChart(DEFAULT_DATA_POINTS_POWER_DETECTOR);
            //Add to dropdown menu the options for power/detector chart view
            _powerScaleOptions.Add(powerAutoScaleChartOption);
            _powerScaleOptions.Add(powerThresholdScaleChartOption);
            _powerScaleOptions.Add(powerNoneScaleChartOption);
            //Select the default option in power/detector dropdown menu
            _selectedPowerScaleOption = powerThresholdScaleChartOption;
            //Setup Y axis programatically for current/state and power/detector
            //This binds this y axis with the actual chart y axis. Makes it easier to change in code for MVVM
            YAxes.Add(new NumericAxisViewModel()
            {
                AutoRange = AutoRange.Never,
                VisibleRange = new DoubleRange(0, 2),
                GrowBy = new DoubleRange(0.1, 0.1),
                AxisTitle = "State",
                Id = "StateAxisID",
                FontSize = FONTSIZE,
                AxisAlignment = AxisAlignment.Left,
            });

            YAxes.Add(new NumericAxisViewModel()
            {
                AutoRange = AutoRange.Never,
                AxisTitle = "Current",
                Id = "CurrentAxisID",
                FontSize = FONTSIZE,
                VisibleRange = new DoubleRange(0, 2),
                GrowBy = new DoubleRange(0.1, 0.1),
                AxisAlignment = AxisAlignment.Right,
            });

            YAxesPower.Add(new NumericAxisViewModel()
            {
                AutoRange = AutoRange.Never,
                AxisTitle = "Power",
                Id = "PowerAxisID",
                FontSize = FONTSIZE,
                VisibleRange = new DoubleRange(0, 2),
                GrowBy = new DoubleRange(0.1, 0.1),
                AxisAlignment = AxisAlignment.Left,
            });

            YAxesPower.Add(new NumericAxisViewModel()
            {
                AutoRange = AutoRange.Always,
                AxisTitle = "Detector",
                Id = "DetectorAxisID",
                FontSize = FONTSIZE,
                VisibleRange = new DoubleRange(0, 2),
                GrowBy = new DoubleRange(0.1, 0.1),
                AxisAlignment = AxisAlignment.Right,
            });
            #endregion

            #region Visualization Sense Constructor
            //Give series a name so legend in each chart shows a name
            _timeDomainSTNChart.SeriesName = "Time Domain";
            _timeDomainM1Chart.SeriesName = "Time Domain";
            _accelerometryXChart.SeriesName = "X";
            _accelerometryYChart.SeriesName = "Y";
            _accelerometryZChart.SeriesName = "Z";
            _fftChart.SeriesName = "FFT";
            //Fifo capacity sets the number of values in the x axis shown
            ChangeDataSeriesForVisualizationSenseChart(DEFAULT_DATA_POINTS_TD);
            //Add to the fft scale drop down menu
            _fftScaleOptions.Add(fftNoneScaleChartOption);
            _fftScaleOptions.Add(fftAutoScaleChartOption);
            _fftScaleOptions.Add(fftLog10ScaleChartOption);
            //default Selected FFT Scale Option
            _selectedFFTScaleOption = fftNoneScaleChartOption;
            //Setup Y axis programatically for visualization sense charts
            _yAxesTimeDomainSTN.Add(new NumericAxisViewModel()
            {
                AutoRange = AutoRange.Always,
                VisibleRange = new DoubleRange(0, 2),
                GrowBy = new DoubleRange(0.1, 0.1),
                AxisTitle = "TD",
                TitleFontSize = FONTSIZE,
                Id = "TimeDomainSTNID",
                FontSize = FONTSIZE,
                AxisAlignment = AxisAlignment.Right,
            });
            _yAxesTimeDomainM1.Add(new NumericAxisViewModel()
            {
                AutoRange = AutoRange.Always,
                VisibleRange = new DoubleRange(0, 2),
                GrowBy = new DoubleRange(0.1, 0.1),
                AxisTitle = "TD",
                TitleFontSize = FONTSIZE,
                Id = "TimeDomainM1ID",
                FontSize = FONTSIZE,
                AxisAlignment = AxisAlignment.Right,
            });
            _yAxesAccelerometer.Add(new NumericAxisViewModel()
            {
                AutoRange = AutoRange.Always,
                VisibleRange = new DoubleRange(0, 2),
                GrowBy = new DoubleRange(0.1, 0.1),
                AxisTitle = "Acc",
                Id = "AccelerometryID",
                FontSize = FONTSIZE,
                TitleFontSize = FONTSIZE,
                AxisAlignment = AxisAlignment.Right,
            });
            _yAxesFFT.Add(new NumericAxisViewModel()
            {
                AutoRange = AutoRange.Always,
                VisibleRange = new DoubleRange(0, 0.5),
                GrowBy = new DoubleRange(0.1, 0.1),
                AxisTitle = "FFT",
                Id = "FFTID",
                FontSize = FONTSIZE,
                AxisAlignment = AxisAlignment.Left,
            });
            #endregion

            #region MainView Constructor
            senseConfig = jSONService.GetSenseModelFromFile(senseFileLocation);
            if (senseConfig == null)
            {
                MessageBox.Show("Sense Config could not be loaded. Please check that it exists or has the correct format", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            sw.Start();
            Assembly assem = Assembly.GetExecutingAssembly();
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            TitleText = "UCSF Starr Lab Clinician Program. Version: " + ver;
            //Load the application model. This will determine what UI elements to show and if bilateral
            appConfigModel = jSONService?.GetApplicationModelFromFile(applicationFileLocation);
            //Application config if required. If user is missing it (hence null) then they can't move on
            if (appConfigModel == null)
            {
                MessageBox.Show("You are missing the application config file. This allows files to be added to your current session if the path was changed in the Summit ORCA registry editor. Proceed if path is still C:\\ProgramData\\Medtronic ORCA", "Warning", MessageBoxButton.OK, MessageBoxImage.Hand);
                basePathForMedtronicFiles = "C:\\ProgramData\\Medtronic ORCA";
            }
            else
            {
                basePathForMedtronicFiles = appConfigModel?.BasePathToJSONFiles;
            }
            //Initialize to listen for a beep noise.
            if (appConfigModel.LogBeepEvent)
            {
                try
                {
                    waveIn = new WaveIn();
                    waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(waveIn_data);
                    waveIn.WaveFormat = new WaveFormat(48000, 8, 1);
                    waveIn.BufferMilliseconds = 20;
                    waveIn.NumberOfBuffers = 2;
                    waveIn.StartRecording();
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
            ProgramOptions.Add(program0Option);
            ProgramOptions.Add(program1Option);
            ProgramOptions.Add(program2Option);
            ProgramOptions.Add(program3Option);
            SelectedProgram = program0Option;

            ModeCB.Add(3);
            ModeCB.Add(4);
            for (byte i = 1; i < 33; i++)
            {
                RatioCB.Add(i);
            }
            SelectedMode = senseConfig.Mode;
            SelectedRatio = senseConfig.Ratio;
            if (theSummitManager == null)
            {
                _log.Info("Initializing Summit Manager");
                theSummitManager = new SummitManager(PROJECT_ID, 200, false);
                CTMCB.AddRange(theSummitManager.GetKnownTelemetry().Select(c => c.SerialNumber).ToList());
            }

            #endregion

            #region SenseSettings Constructor
            senseConfigFromUI = Clone<SenseModel>(senseConfig);
            PopulateComboBoxes(senseConfigFromUI);
            LoadValuesFromSenseCongifToUI(senseConfigFromUI);
            #endregion
        }

        #region UI Bindings for buttons, textboxes, etc
        /// <summary>
        /// Binding for the INS battery level display
        /// </summary>
        public string DevicePatientID
        {
            get { return _devicePatientID; }
            set
            {
                _devicePatientID = value;
                NotifyOfPropertyChange(() => DevicePatientID);
            }
        }
        /// <summary>
        /// Binding for the INS battery level display
        /// </summary>
        public string INSBatteryLevel
        {
            get { return _iNSBatteryLevel; }
            set
            {
                _iNSBatteryLevel = value;
                NotifyOfPropertyChange(() => INSBatteryLevel);
            }
        }
        /// <summary>
        /// Binding for the actual option selected in the drop down menu for CTM
        /// </summary>
        public string SelectedCTM
        {
            get { return _selectedCTM; }
            set
            {
                _selectedCTM = value;
                NotifyOfPropertyChange(() => SelectedCTM);
            }
        }
        /// <summary>
        /// Combo box drop down list for CTM list
        /// </summary>
        public BindableCollection<string> CTMCB
        {
            get { return _cTMCB; }
            set
            {
                _cTMCB = value;
                NotifyOfPropertyChange(() => CTMCB);
            }
        }
        /// <summary>
        /// If true, then connect with selected CTM in combobox
        /// </summary>
        public bool ConnectWithSelectedCTM
        {
            get { return _connectWithSelectedCTM; }
            set
            {
                _connectWithSelectedCTM = value;
                NotifyOfPropertyChange(() => ConnectWithSelectedCTM);
            }
        }
        /// <summary>
        /// Adjusts border thickness for all comboboxes in sense settings and mode ratio
        /// </summary>
        public int BorderThicknessForAllCB
        {
            get { return _borderThicknessForAllCB; }
            set
            {
                _borderThicknessForAllCB = value;
                NotifyOfPropertyChange(() => BorderThicknessForAllCB);
            }
        }
        /// <summary>
        /// Changes Mode border color so that user knows when a change from normal has occurred. 
        /// </summary>
        public Brush ModeBorderColor
        {
            get { return _modeBorderColor ?? (_modeBorderColor = comboboxNotChangedBrush); }
            set
            {
                _modeBorderColor = value;
                NotifyOfPropertyChange(() => ModeBorderColor);
            }
        }
        /// <summary>
        /// Changes ratio border color so that user knows when a change from normal has occurred.  
        /// </summary>
        public Brush RatioBorderColor
        {
            get { return _ratioBorderColor ?? (_ratioBorderColor = comboboxNotChangedBrush); }
            set
            {
                _ratioBorderColor = value;
                NotifyOfPropertyChange(() => RatioBorderColor);
            }
        }
        /// <summary>
        /// Combo box drop down list for Mode for sense config
        /// </summary>
        public BindableCollection<ushort> ModeCB
        {
            get { return _modeCB; }
            set
            {
                _modeCB = value;
                NotifyOfPropertyChange(() => ModeCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for ModeCB
        /// </summary>
        public ushort SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                _selectedMode = value;
                NotifyOfPropertyChange(() => SelectedMode);
                if (SelectedMode != senseConfig.Mode)
                {
                    ModeBorderColor = comboboxChangedBrush;
                }
                else
                {
                    ModeBorderColor = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for Mode for sense config
        /// </summary>
        public BindableCollection<byte> RatioCB
        {
            get { return _ratioCB; }
            set
            {
                _ratioCB = value;
                NotifyOfPropertyChange(() => RatioCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for RatioCB
        /// </summary>
        public byte SelectedRatio
        {
            get { return _selectedRatio; }
            set
            {
                _selectedRatio = value;
                NotifyOfPropertyChange(() => SelectedRatio);
                if (SelectedRatio != senseConfig.Ratio)
                {
                    RatioBorderColor = comboboxChangedBrush;
                }
                else
                {
                    RatioBorderColor = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Turns stim settings such as stim amp/rate/pw buttons up or down to disable or enabled 
        /// </summary>
        public bool StimSettingButtonsEnabled
        {
            get { return _stimSettingButtonsEnabled; }
            set
            {
                _stimSettingButtonsEnabled = value;
                NotifyOfPropertyChange(() => StimSettingButtonsEnabled);
            }
        }
        /// <summary>
        /// Binding for the pulse width lower limit
        /// </summary>
        public int PWLowerLimit
        {
            get { return _pWLowerLimit; }
            set
            {
                _pWLowerLimit = value;
                NotifyOfPropertyChange(() => PWLowerLimit);
            }
        }
        /// <summary>
        /// Binding for the pulse width upper limit
        /// </summary>
        public int PWUpperLimit
        {
            get { return _pWUpperLimit; }
            set
            {
                _pWUpperLimit = value;
                NotifyOfPropertyChange(() => PWUpperLimit);
            }
        }
        /// <summary>
        /// Binding for the rate lower limit
        /// </summary>
        public double RateLowerLimit
        {
            get { return _rateLowerLimit; }
            set
            {
                _rateLowerLimit = value;
                NotifyOfPropertyChange(() => RateLowerLimit);
            }
        }
        /// <summary>
        /// Binding for the rate upper limit
        /// </summary>
        public double RateUpperLimit
        {
            get { return _rateUpperLimit; }
            set
            {
                _rateUpperLimit = value;
                NotifyOfPropertyChange(() => RateUpperLimit);
            }
        }
        /// <summary>
        /// Binding for the amp lower limit
        /// </summary>
        public double AmpLowerLimit
        {
            get { return _ampLowerLimit; }
            set
            {
                _ampLowerLimit = value;
                NotifyOfPropertyChange(() => AmpLowerLimit);
            }
        }
        /// <summary>
        /// Binding for the amp upper limit
        /// </summary>
        public double AmpUpperLimit
        {
            get { return _ampUpperLimit; }
            set
            {
                _ampUpperLimit = value;
                NotifyOfPropertyChange(() => AmpUpperLimit);
            }
        }
        /// <summary>
        /// Combo box drop down list for the medtronic program options 0-3
        /// </summary>
        public BindableCollection<string> ProgramOptions
        {
            get { return _programOptions; }
            set
            {
                _programOptions = value;
                NotifyOfPropertyChange(() => ProgramOptions);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for ProgramOptions
        /// </summary>
        public string SelectedProgram
        {
            get { return _selectedProgram; }
            set
            {
                _selectedProgram = value;
                NotifyOfPropertyChange(() => SelectedProgram);
                summitSensing.StopStreaming(theSummit, false);
                UpdateStimStatusGroup(true, true, true);
                summitSensing.StartStreaming(theSummit, senseConfig, false);
            }
        }
        /// <summary>
        /// Message collection to keep appending messages for showing status to users.
        ///Shows up in Main tab and Report tab as message box
        /// </summary>
        public BindableCollection<string> Messages
        {
            get { return _message; }
            set { _message = value; }
        }

        /// <summary>
        /// Display the Session to user to show Version number to show to user how many times the aDBS has been updated. 
        ///This is also used to prepend to front of adaptive/sense config files that are saved in the Medtronic json file directory
        /// </summary>
        public string SessionDisplay
        {
            get { return _sessionDisplay; }
            set
            {
                _sessionDisplay = value;
                NotifyOfPropertyChange(() => SessionDisplay);
            }
        }

        /// <summary>
        /// Binding that Shows stim rate to user
        /// </summary>
        public string StimRateDisplay
        {
            get { return _stimRate; }
            set
            {
                _stimRate = value;
                NotifyOfPropertyChange(() => StimRateDisplay);
            }
        }
        /// <summary>
        /// Binding that shows active recharge status
        /// </summary>
        public string ActiveRechargeStatus
        {
            get { return _activeRechargeStatus; }
            set
            {
                _activeRechargeStatus = value;
                NotifyOfPropertyChange(() => ActiveRechargeStatus);
            }
        }
        /// <summary>
        /// Binding that shows the electrodes that are stimming
        /// </summary>
        public string StimElectrode
        {
            get { return _stimElectrode; }
            set
            {
                _stimElectrode = value;
                NotifyOfPropertyChange(() => StimElectrode);
            }
        }
        /// <summary>
        /// Binding that Shows stim amp to user
        /// </summary>
        public string StimAmpDisplay
        {
            get { return _stimAmp; }
            set
            {
                _stimAmp = value;
                NotifyOfPropertyChange(() => StimAmpDisplay);
            }
        }
        /// <summary>
        /// Binding that Shows stim pulse width to user
        /// </summary>
        public string StimPWDisplay
        {
            get { return _stimPW; }
            set
            {
                _stimPW = value;
                NotifyOfPropertyChange(() => StimPWDisplay);
            }
        }
        /// <summary>
        /// Binding that Shows if stim is on or off to user
        /// </summary>
        public string StimActiveDisplay
        {
            get { return _stimActive; }
            set
            {
                _stimActive = value;
                NotifyOfPropertyChange(() => StimActiveDisplay);
            }
        }
        /// <summary>
        /// Binding that Shows which group is active to user
        /// </summary>
        public string ActiveGroupDisplay
        {
            get { return _activeGroup; }
            set
            {
                _activeGroup = value;
                NotifyOfPropertyChange(() => ActiveGroupDisplay);
            }
        }
        /// <summary>
        /// Binding that Shows stim state to user
        /// </summary>
        public string StimStateDisplay
        {
            get { return _stimState; }
            set
            {
                _stimState = value;
                NotifyOfPropertyChange(() => StimStateDisplay);
            }
        }
        /// <summary>
        /// Binding for the step value to change stim up or down
        /// </summary>
        public string StepValueInputBox
        {
            get { return _stepValueInputBox; }
            set
            {
                _stepValueInputBox = value;
                NotifyOfPropertyChange(() => StepValueInputBox);
            }
        }
        /// <summary>
        /// Binding for stim value to change to when clicking Go
        /// </summary>
        public string StimChangeValueInput
        {
            get { return _stimChangeValueInput; }
            set
            {
                _stimChangeValueInput = value;
                NotifyOfPropertyChange(() => StimChangeValueInput);
            }
        }
        /// <summary>
        /// Binding for stim rate value to change to when clicking Go
        /// </summary>
        public string StimChangeRateInput
        {
            get { return _stimChangeRateInput; }
            set
            {
                _stimChangeRateInput = value;
                NotifyOfPropertyChange(() => StimChangeRateInput);
            }
        }
        /// <summary>
        /// Binding for stim rate value to change to when clicking Go
        /// </summary>
        public string StepRateValueInputBox
        {
            get { return _stepRateValueInputBox; }
            set
            {
                _stepRateValueInputBox = value;
                NotifyOfPropertyChange(() => StepRateValueInputBox);
            }
        }
        /// <summary>
        /// Binding for the step value to change stim pulse width up or down
        /// </summary>
        public string StepPWInputBox
        {
            get { return _stepPWInputBox; }
            set
            {
                _stepPWInputBox = value;
                NotifyOfPropertyChange(() => StepPWInputBox);
            }
        }
        /// <summary>
        /// Binding for stim value to change pulse width to when clicking Go
        /// </summary>
        public string StimChangePWInput
        {
            get { return _stimChangePWInput; }
            set
            {
                _stimChangePWInput = value;
                NotifyOfPropertyChange(() => StimChangePWInput);
            }
        }
        /// <summary>
        /// Changes the title of the program
        /// </summary>
        public string TitleText
        {
            get { return _titleText; }
            set
            {
                _titleText = value;
                NotifyOfPropertyChange(() => TitleText);
            }
        }
        /// <summary>
        /// Turns stim on
        /// </summary>
        public bool StimOnButtonEnabled
        {
            get { return _stimOnButtonEnabled; }
            set
            {
                _stimOnButtonEnabled = value;
                NotifyOfPropertyChange(() => StimOnButtonEnabled);
            }
        }
        /// <summary>
        /// Turns stim off
        /// </summary>
        public bool StimOffButtonEnabled
        {
            get { return _stimOffButtonEnabled; }
            set
            {
                _stimOffButtonEnabled = value;
                NotifyOfPropertyChange(() => StimOffButtonEnabled);
            }
        }
        /// <summary>
        /// Changes background to show if therapy is on or off. 
        /// </summary>
        public Brush TherapyStatusBackground
        {
            get { return _therapyStatusBackground ?? (_therapyStatusBackground = Brushes.LightGray); }
            set
            {
                _therapyStatusBackground = value;
                NotifyOfPropertyChange(() => TherapyStatusBackground);
            }
        }
        /// <summary>
        /// Group A button enabled/disabled
        /// </summary>
        public bool GroupAButtonEnabled
        {
            get { return _groupAButtonEnabled; }
            set
            {
                _groupAButtonEnabled = value;
                NotifyOfPropertyChange(() => GroupAButtonEnabled);
            }
        }
        /// <summary>
        /// Group B button enabled/disabled
        /// </summary>
        public bool GroupBButtonEnabled
        {
            get { return _groupBButtonEnabled; }
            set
            {
                _groupBButtonEnabled = value;
                NotifyOfPropertyChange(() => GroupBButtonEnabled);
            }
        }
        /// <summary>
        /// Group C button enabled/disabled
        /// </summary>
        public bool GroupCButtonEnabled
        {
            get { return _groupCButtonEnabled; }
            set
            {
                _groupCButtonEnabled = value;
                NotifyOfPropertyChange(() => GroupCButtonEnabled);
            }
        }
        /// <summary>
        /// Group D button enabled/disabled
        /// </summary>
        public bool GroupDButtonEnabled
        {
            get { return _groupDButtonEnabled; }
            set
            {
                _groupDButtonEnabled = value;
                NotifyOfPropertyChange(() => GroupDButtonEnabled);
            }
        }
        /// <summary>
        /// Binding used to change the color of the connect button/displays
        /// </summary>
        public Brush ConnectButtonColor
        {
            get { return _connectButtonColor ?? (_connectButtonColor = SystemColors.WindowBrush); }
            set
            {
                _connectButtonColor = value;
                NotifyOfPropertyChange(() => ConnectButtonColor);
            }
        }
        /// <summary>
        /// Binding used to change the text of the connect button/displays
        /// </summary>
        public string ConnectButtonText
        {
            get { return _connectButtonText ?? (_connectButtonText = "Connect"); }
            set
            {
                _connectButtonText = value;
                NotifyOfPropertyChange(() => ConnectButtonText);
            }
        }
        /// <summary>
        /// Enables or disables the stream on button
        /// </summary>
        public bool SenseStreamOnEnabled
        {
            get { return _senseStreamOnEnabled; }
            set
            {
                _senseStreamOnEnabled = value;
                NotifyOfPropertyChange(() => SenseStreamOnEnabled);
            }
        }

        /// <summary>
        /// Enables or disables the stream off button
        /// </summary>
        public bool SenseStreamOffEnabled
        {
            get { return _senseStreamOffEnabled; }
            set
            {
                _senseStreamOffEnabled = value;
                NotifyOfPropertyChange(() => SenseStreamOffEnabled);
            }
        }
        /// <summary>
        /// Determines if spinner is visible or not
        /// </summary>
        public bool IsSpinnerVisible
        {
            get { return _isSpinnerVisible; }
            set
            {
                _isSpinnerVisible = value;
                NotifyOfPropertyChange(() => IsSpinnerVisible);
            }
        }
        /// <summary>
        /// Binding that is broken but should disable connect button when already connected
        /// Used to show connection status as well
        /// </summary>
        /// <returns>bool</returns>
        public bool CanConnect()
        {
            return !isConnected;
        }
        #endregion

        #region Button Clicks for Connect and New Session
        /// <summary>
        /// Actual Connect button pressed by user and calls the connection WorkerThread
        /// WorkerThread used so not to freeze UI
        /// </summary>
        public void Connect()
        {
            //If we're not connected already, start the worker thread to connect
            if (!isConnected)
            {
                Messages.Insert(0, DateTime.Now + ":: Connecting");
                if (CheckIfModeRatioChangedInUI(senseConfig))
                {
                    senseConfig = ChangeModeRatioAndSaveToFile(senseConfig);
                }
                workerThread = new Thread(new ThreadStart(WorkerThread));
                workerThread.IsBackground = true;
                workerThread.Start();
            }
            else
            {
                Messages.Insert(0, DateTime.Now + ":: Already Connected. If you would like to connect to a different device, please restart application and retry connecting.");
            }
        }

        /// <summary>
        /// Connection wrapper that connects to CTM the INS
        /// </summary>
        /// <param name="theSummitManager">Summit Manager</param>
        /// <returns>True if connected and false if couldn't connect</returns>
        private bool SummitConnectWrapper(SummitManager theSummitManager)
        {
            _log.Info("Connecting");
            ConnectButtonText = "Connecting CTM";
            ConnectButtonColor = Brushes.LightGoldenrodYellow;
            if (!connect.ConnectCTM(theSummitManager, ref theSummit, senseConfig, appConfigModel, _log, ConnectWithSelectedCTM, CTMCB.IndexOf(SelectedCTM)))
            {
                return false;
            }
            else
            {
                ConnectButtonText = "Connecting INS";
                ConnectButtonColor = Brushes.Yellow;
            }
            if (!connect.ConnectINS(ref theSummit, _log))
            {
                return false;
            }
            else
            {
                _log.Info("Connection Successful");
                //Successful connection
                Messages.Insert(0, DateTime.Now + ":: Connected");
                isConnected = true;
                ConnectButtonText = "Connected";
                ConnectButtonColor = Brushes.ForestGreen;
            }
            return true;
        }

        /// <summary>
        /// Creates a new session directory by disconnecting and reconnecting
        /// </summary>
        public void NewSessionButton()
        {
            IsSpinnerVisible = true;
            deviceID = null;
            patientID = null;
            DevicePatientID = "";
            INSBatteryLevel = "";
            TherapyStatusBackground = Brushes.LightGray;
            senseConfig = jSONService.GetSenseModelFromFile(senseFileLocation);
            if (senseConfig == null)
            {
                MessageBox.Show("Sense Config could not be loaded. Please check that it exists or has the correct format", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            //Check mode and ratio to see if they changed in the UI and change them in config file if so
            if (CheckIfModeRatioChangedInUI(senseConfig))
            {
                senseConfig = ChangeModeRatioAndSaveToFile(senseConfig);
            }
            _shouldStopWorkerThread = true;
            try
            {
                if (workerThread != null)
                {
                    workerThread.Abort();
                    // make sure null so next operation can be executed
                    workerThread = null;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
            if (theSummit != null)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        theSummit.WriteSensingState(SenseStates.None, 0x00);
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);
                    }
                }
            }
            DisposeSummitSystem();
            isConnected = false;
            _shouldStopWorkerThread = false;
            Connect();
            versionNumber = 0;
            SessionDisplay = "";
            ReloadConfigButton();
        }
        #endregion

        #region Align Button Bindings
        /// <summary>
        /// Button click for the align.  Moves to group B, changes stim on/off 4 times and moves to group A
        /// </summary>
        public void AlignButtonClick()
        {
            if (theSummit != null)
            {
                if (!theSummit.IsDisposed)
                {
                    if (isConnected)
                    {
                        alignThread = new Thread(new ThreadStart(AlignThreadCode));
                        alignThread.IsBackground = true;
                        alignThread.Start();
                    }
                }
            }
        }

        private void AlignThreadCode()
        {
            IsSpinnerVisible = true;
            int counter;
            _log.Info("Running align");
            Messages.Insert(0, DateTime.Now + ":: Running Align... *****Please wait until finished before clicking anything else*****");
            
            if (StimActiveDisplay.Equals("TherapyActive"))
            {
                //Change to group B
                try
                {
                    counter = 5;
                    summitSensing.StopStreaming(theSummit, false);
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group1);
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                    Thread.Sleep(300);
                    UpdateStimStatusGroup(false, true, true);
                    IsSpinnerVisible = true;
                    Thread.Sleep(800);
                    summitSensing.StartStreaming(theSummit, senseConfig, false);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error moving to Group B. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error moving to Group B. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                ActiveGroupDisplay = "Group B";
                GroupAButtonEnabled = true;
                GroupBButtonEnabled = false;
                GroupCButtonEnabled = true;
                GroupDButtonEnabled = true;

                //Turn stim off
                try
                {
                    counter = 5;
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeTherapyOff(false);
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error turning stim therapy off. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error turning stim therapy off. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                StimActiveDisplay = "TherapyOff";
                StimOnButtonEnabled = true;
                StimOffButtonEnabled = false;
                TherapyStatusBackground = Brushes.LightGray;
                Thread.Sleep(3000);

                //Turn stim on
                try
                {
                    counter = 5;
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeTherapyOn();
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error turning stim therapy on. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error turning stim therapy on. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                StimActiveDisplay = "TherapyActive";
                StimOnButtonEnabled = false;
                StimOffButtonEnabled = true;
                TherapyStatusBackground = Brushes.ForestGreen;
                Thread.Sleep(4000);

                //Turn stim off
                try
                {
                    counter = 5;
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeTherapyOff(false);
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error turning stim therapy off. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error turning stim therapy off. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                StimActiveDisplay = "TherapyOff";
                StimOnButtonEnabled = true;
                StimOffButtonEnabled = false;
                TherapyStatusBackground = Brushes.LightGray;
                Thread.Sleep(3000);

                //Turn stim on
                try
                {
                    counter = 5;
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeTherapyOn();
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error turning stim therapy on. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error turning stim therapy on. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                StimActiveDisplay = "TherapyActive";
                StimOnButtonEnabled = false;
                StimOffButtonEnabled = true;
                TherapyStatusBackground = Brushes.ForestGreen;
                Thread.Sleep(2000);

                //Change to group A
                try
                {
                    counter = 5;
                    summitSensing.StopStreaming(theSummit, false);
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group0);
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                    Thread.Sleep(300);
                    UpdateStimStatusGroup(true, true, true);
                    summitSensing.StartStreaming(theSummit, senseConfig, false);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error moving to group A. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error moving to group A. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                ActiveGroupDisplay = "Group A";
                GroupAButtonEnabled = false;
                GroupBButtonEnabled = true;
                GroupCButtonEnabled = true;
                GroupDButtonEnabled = true;
            }
            else if (StimActiveDisplay.Equals("TherapyOff"))
            {
                //Change to group B
                try
                {
                    counter = 5;
                    summitSensing.StopStreaming(theSummit, false);
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group1);
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                    Thread.Sleep(300);
                    UpdateStimStatusGroup(false, true, true);
                    Thread.Sleep(800);
                    IsSpinnerVisible = true;
                    summitSensing.StartStreaming(theSummit, senseConfig, false);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error moving to group B. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error moving to group B. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                ActiveGroupDisplay = "Group B";
                GroupAButtonEnabled = true;
                GroupBButtonEnabled = false;
                GroupCButtonEnabled = true;
                GroupDButtonEnabled = true;

                //Turn stim on
                try
                {
                    counter = 5;
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeTherapyOn();
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error turning stim therapy on. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error turning stim therapy on. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                StimActiveDisplay = "TherapyActive";
                StimOnButtonEnabled = false;
                StimOffButtonEnabled = true;
                TherapyStatusBackground = Brushes.ForestGreen;
                Thread.Sleep(3000);

                //Turn stim off
                try
                {
                    counter = 5;
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeTherapyOff(false);
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error turning stim therapy off. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error turning stim therapy off. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                StimActiveDisplay = "TherapyOff";
                StimOnButtonEnabled = true;
                StimOffButtonEnabled = false;
                TherapyStatusBackground = Brushes.LightGray;
                Thread.Sleep(4000);

                //Turn stim on
                try
                {
                    counter = 5;
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeTherapyOn();
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error turning stim therapy on. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error turning stim therapy on. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                StimActiveDisplay = "TherapyActive";
                StimOnButtonEnabled = false;
                StimOffButtonEnabled = true;
                TherapyStatusBackground = Brushes.ForestGreen;
                Thread.Sleep(3000);

                //Turn stim off
                try
                {
                    counter = 5;
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeTherapyOff(false);
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error turning stim therapy off. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error turning stim therapy off. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                StimActiveDisplay = "TherapyOff";
                StimOnButtonEnabled = true;
                StimOffButtonEnabled = false;
                TherapyStatusBackground = Brushes.LightGray;
                Thread.Sleep(2000);

                //Change to group A
                try
                {
                    counter = 5;
                    summitSensing.StopStreaming(theSummit, false);
                    do
                    {
                        bufferReturnInfo = theSummit.StimChangeActiveGroup(ActiveGroup.Group0);
                        counter--;
                    } while (bufferReturnInfo.RejectCode != 0 && counter > 5);
                    Thread.Sleep(300);
                    UpdateStimStatusGroup(true, true, true);
                    Thread.Sleep(500);
                    summitSensing.StartStreaming(theSummit, senseConfig, false);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    MessageBox.Show("Error moving to group A. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                if (!CheckReturnCodeForClinician(bufferReturnInfo) || counter == 0)
                {
                    MessageBox.Show("Error moving to group A. Please try again. Settings may not be in original state. Be sure to correct settings before moving forward.");
                    IsSpinnerVisible = false;
                    UpdateStimStatusGroup(true, true, true);
                    return;
                }
                ActiveGroupDisplay = "Group A";
                GroupAButtonEnabled = false;
                GroupBButtonEnabled = true;
                GroupCButtonEnabled = true;
                GroupDButtonEnabled = true;
            }
            else
            {
                _log.Warn("Could not run align");
                MessageBox.Show("Could not determine if Stim Therapy is already on or off. Please Fix and try again.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                IsSpinnerVisible = false;
                UpdateStimStatusGroup(true, true, true);
                return;
            }
            //Log event that align has been clicked
            bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_ALIGN_EVENT_ID + " Align Button Clicked", DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
            CheckForReturnErrorInLog(bufferReturnInfo, "Align Button Clicked");
            Messages.Insert(0, DateTime.Now + ":: Align Finished Running.");
            IsSpinnerVisible = false;
        }


        private bool CheckReturnCodeForClinician(APIReturnInfo info)
        {
            if (info.RejectCode != 0)
            {
                _log.Warn("Reject code not 0: " + info.RejectCode + ". Reject description: " + info.Descriptor + " in align button click");
                MessageBox.Show("Error from Medtronic API: Reject Description: " + info.Descriptor + ". Reject Code: " + info.RejectCode, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        #endregion

        #region WorkerThread for Summit, event handler for listen for beep, Mode and ratio drop down code
        /// <summary>
        /// WorkerThread that continually runs and checks for connection. Keeps a continuous connection
        /// </summary>
        private void WorkerThread()
        {
            //Keep going until _shouldStopWorkerThread is switched. This is done in closing the window
            while (!_shouldStopWorkerThread)
            {
                IsSpinnerVisible = true;
                if (theSummitManager == null)
                {
                    IsSpinnerVisible = false;
                    _log.Warn("Summit Manager null");
                    isConnected = false;
                    ConnectButtonText = "Not Connected";
                    ConnectButtonColor = SystemColors.WindowBrush;
                    return;
                }
                //Connect using the SummitConnect wrapper. Return true if connected and false if failed connection
                if (!SummitConnectWrapper(theSummitManager))
                {
                    IsSpinnerVisible = false;
                    _log.Warn("Failure to connect, reattempting connection in 5 seconds...");
                    isConnected = false;
                    ConnectButtonText = "Not Connected";
                    ConnectButtonColor = SystemColors.WindowBrush;
                    if (_shouldStopWorkerThread)
                        break;
                    Messages.Insert(0, DateTime.Now + ":: Failure to connect, reattempting connection in 2 seconds...");
                    Thread.Sleep(2000);
                }
                else
                {
                    //Get device ID and Patient Id to find path to the medtronic json files
                    //Keep checking until we have both of them.  
                    //This is added since it has errored out in the past and we need them to save the adaptive and sense config files on each updateDBS
                    while (deviceID == null || patientID == null)
                    {
                        //Need Id's to get the full path write JSON files when updating DBS
                        if (deviceID == null)
                        {
                            try
                            {
                                deviceID = theSummit.DeviceID;
                                Messages.Insert(0, DateTime.Now + ":: deviceID: " + deviceID);
                                bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Device ID", deviceID);
                            }
                            catch (Exception error)
                            {
                                Messages.Insert(0, DateTime.Now + ":: Failed to get deviceID");
                                _log.Error(error);
                            }

                        }
                        if (patientID == null)
                        {
                            try
                            {
                                SubjectInfo subjectInfo;
                                theSummit.FlashReadSubjectInfo(out subjectInfo);
                                if (subjectInfo == null)
                                {
                                    continue;
                                }
                                patientID = subjectInfo.ID;
                                DevicePatientID = patientID;
                                leadLocation1 = subjectInfo.LeadTargets[0].ToString();
                                leadLocation2 = subjectInfo.LeadTargets[2].ToString();

                                Messages.Insert(0, DateTime.Now + ":: patientID: " + patientID);
                                Messages.Insert(0, DateTime.Now + ":: Lead Locations: " + leadLocation1 + ", " + leadLocation2);
                                bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Patient ID", patientID);
                                bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Lead Location 1", leadLocation1);
                                bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Lead Location 2", leadLocation2);
                            }
                            catch (Exception error)
                            {
                                Messages.Insert(0, DateTime.Now + ":: Failed to get patientID");
                                _log.Error(error);
                            }
                        }

                    }
                    GroupATherapyLimits = stimData.GetTherapyLimitsStimContactsForGroup(theSummit, GroupNumber.Group0);
                    GroupBTherapyLimits = stimData.GetTherapyLimitsStimContactsForGroup(theSummit, GroupNumber.Group1);
                    GroupCTherapyLimits = stimData.GetTherapyLimitsStimContactsForGroup(theSummit, GroupNumber.Group2);
                    GroupDTherapyLimits = stimData.GetTherapyLimitsStimContactsForGroup(theSummit, GroupNumber.Group3);
                    //Update UI for statuses
                    UpdateStimStatusGroup(true, true, true);
                    //Set Chart titles
                    _timeDomainSTNChart.SeriesName = "TD " + leadLocation1;
                    TimeDomainSTNChartTitle = "Time Domain " + leadLocation1;
                    _timeDomainM1Chart.SeriesName = "TD " + leadLocation2;
                    TimeDomainM1ChartTitle = "Time Domain " + leadLocation2;
                    YAxesTimeDomainSTN[0].AxisTitle = "TD " + leadLocation1;
                    YAxesTimeDomainM1[0].AxisTitle = "TD " + leadLocation2;

                    IsSpinnerVisible = false;
                    StimSettingButtonsEnabled = true;
                    INSBatteryLevel = INSbatteryLevel.GetINSBatteryLevel(theSummit, _log);
                    // Create the timer to periodically retrieve battery status
                    Console.WriteLine("Setup Timer for battery check");
                    TimeSpan interval = new TimeSpan(0, 0, 180);
                    batteryTimer.Interval = interval.TotalMilliseconds;
                    batteryTimer.AutoReset = true;
                    batteryTimer.Elapsed += BatteryLevelTimedHandler;
                    batteryTimer.Enabled = true;
                    Thread.Sleep(300);
                    if (ActiveGroupDisplay.Equals("Group D"))
                    {
                        try
                        {
                            SensingState state;
                            theSummit.ReadSensingState(out state);
                            if (state.State.ToString().Contains("DetectionLd0") || state.State.ToString().Contains("DetectionLd1"))
                            {
                                bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, TURN_EMBEDDED_ON_EVENT_ID + " Turn Embedded on. Number: " + versionNumber, DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                                CheckForReturnErrorInLog(bufferReturnInfo, "Logging Embedded Therapy ON");
                            }
                        }
                        catch (Exception e)
                        {
                            Messages.Insert(0, DateTime.Now + ":: ERROR: Could not determine if detection is on to log custom event.");
                            _log.Error(e);
                        }
                    }
                    Thread.Sleep(300);

                    //Start sensing
                    SenseStreamOnButton();

                    // Worker thread to loop in inner loop while system is connected
                    _log.Info("Inside the worker loop");
                    //_shouldStopDoWhileLoop will stop the loop if theSummit is disposed and main while loop will run summitConnect again to reconnect.
                    bool _shouldStopDoWhileLoop = false;
                    do
                    {
                        Thread.Sleep(1000);
                        if (theSummit != null)
                        {
                            if (theSummit.IsDisposed)
                            {
                                _log.Warn("summit disposed in worker loop");
                                //if summit is disposed, reconnect
                                _shouldStopDoWhileLoop = true;
                            }
                        }
                        else
                        {
                            _log.Warn("summit null in worker loop");
                            //if summit is null, reconnect
                            _shouldStopDoWhileLoop = true;
                        }
                    } while (!_shouldStopDoWhileLoop && !_shouldStopWorkerThread);
                    _log.Info("Exit Do-while loop in Worker thread");
                    isConnected = false;
                    ConnectButtonText = "Not Connected";
                    ConnectButtonColor = SystemColors.WindowBrush;
                }
            }
            IsSpinnerVisible = false;
            _log.Info("Exit Worker thread");
            Messages.Insert(0, DateTime.Now + ":: Exit Worker Thread.");
            DisposeSummitSystem();
        }

        private void waveIn_data(object sender, WaveInEventArgs e)
        {
            //find mean
            int mean = 0;
            for (int i = 0; i < e.BytesRecorded; i++)
            {
                mean += e.Buffer[i];
            }
            mean = mean / e.BytesRecorded;

            //subtract mean from each value in array and use absolute value
            for (int i = 0; i < e.BytesRecorded; i++)
            {
                e.Buffer[i] = (byte)Math.Abs((e.Buffer[i] - mean));
            }

            //Check if the max value in the array is above threshold value
            if (e.Buffer.Max() > signalOnValue)
            {
                currentOnFlag = true;
            }
            else
            {
                currentOnFlag = false;
            }

            if (!previousOnFlag && currentOnFlag)
            {
                if (theSummit != null && isConnected)
                {
                    if (!theSummit.IsDisposed)
                    {
                        try
                        {
                            //Log event that stim was turned on and check to make sure that event logging was successful
                            bufferReturnInfo = theSummit.LogCustomEvent(DateTime.Now, DateTime.Now, "Log Beep", DateTime.Now.ToString("MM_dd_yyyy hh:mm:ss tt"));
                            CheckForReturnErrorInLog(bufferReturnInfo, "Log Beep");
                            Messages.Insert(0, DateTime.Now + ":: Beep Logged");
                            _log.Info("Beep Logged");
                        }
                        catch (Exception error)
                        {
                            _log.Error(error);
                        }
                    }
                }
            }
            previousOnFlag = currentOnFlag;
        }

        private bool CheckIfModeRatioChangedInUI(SenseModel localSense)
        {
            if (localSense.Mode != SelectedMode || localSense.Ratio != SelectedRatio)
            {
                return true;
            }
            return false;
        }

        private SenseModel ChangeModeRatioAndSaveToFile(SenseModel currentSenseModel)
        {
            if (currentSenseModel.Mode != SelectedMode)
            {
                ushort tempMode = currentSenseModel.Mode;
                currentSenseModel.Mode = SelectedMode;
                if (!CheckPacketLoss(currentSenseModel))
                {
                    MessageBox.Show("Packet Loss over maximum. Please check config file settings and adjust to lower bandwidth to avoid major packet loss.", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    currentSenseModel.Mode = tempMode;
                    return currentSenseModel;
                }
            }
            if (currentSenseModel.Ratio != SelectedRatio)
            {
                currentSenseModel.Ratio = SelectedRatio;
            }
            //save to file
            JSONService jSONService = new JSONService(_log);
            if (jSONService.WriteSenseConfigToFile(currentSenseModel, senseFileLocation))
            {
                Messages.Insert(0, DateTime.Now + ":: Success writing Sense Config to file");
                ModeBorderColor = comboboxNotChangedBrush;
                RatioBorderColor = comboboxNotChangedBrush;
                senseConfigFromUI.Mode = currentSenseModel.Mode;
                senseConfigFromUI.Ratio = currentSenseModel.Ratio;
            }
            return currentSenseModel;
        }
        #endregion

        #region Battery Level for INS
        /// <summary>
        /// Timer Elapsed Event Handler for querying the SummitSystem for battery information at regular intervals.
        /// </summary>
        /// <param name="sender">Required field for C# event handlers</param>
        /// <param name="e">Required field for C# event handlers, specific class for timers</param>
        private void BatteryLevelTimedHandler(object sender, ElapsedEventArgs e)
        {
            INSBatteryLevel = INSbatteryLevel.GetINSBatteryLevel(theSummit, _log);
        }
        #endregion

        #region Functions to Dispose of Summit System/Manager or Both and Window Closing
        /// <summary>
        /// Event called when window is closed by user originally called in Bootstrapper upon closing
        /// </summary>
        public static void WindowClosing()
        {
            if (theSummit != null)
            {
                if (!theSummit.IsDisposed)
                {
                    try
                    {
                        theSummit.WriteSensingState(SenseStates.None, 0x00);
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);
                    }
                }
            }
            // perform clean up. Abort continuous connection worker thread and dispose of summit system/manager
            _shouldStopWorkerThread = true;
            DisposeSummitManagerAndSystem();
            Environment.Exit(0);
        }
        /// <summary>
        /// Disposes of both summit manager and summit system
        /// </summary>
        public static void DisposeSummitManagerAndSystem()
        {
            DisposeSummitSystem();
            DisposeSummitManager();
        }
        /// <summary>
        /// Dispose just the SummitManager
        /// Called after disposing of summit system
        /// </summary>
        private static void DisposeSummitManager()
        {
            try
            {
                if (theSummitManager != null)
                {
                    theSummitManager.Dispose();
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
        /// <summary>
        /// Dispose just the SummitSystem
        /// </summary>
        private static void DisposeSummitSystem()
        {
            try
            {
                if (theSummitManager != null)
                {
                    if (theSummit != null)
                    {
                        if (!theSummit.IsDisposed)
                        {
                            Console.WriteLine("Disposing of Summit System");
                            theSummitManager.DisposeSummit(theSummit);
                        }
                        theSummit = null;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
        #endregion
    }
}
