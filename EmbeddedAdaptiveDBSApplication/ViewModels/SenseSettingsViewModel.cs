/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace EmbeddedAdaptiveDBSApplication.ViewModels
{
    public partial class MainViewModel : Screen
    {
        //Sense setting general variables
        private Brush buttonChangedBrush = Brushes.Red;
        private Brush buttonNotChangedBrush = Brushes.Transparent;
        private bool _senseButtonEnabled = true;
        //Text and textboxes
        private string _timeDomainCh0SenseText, _timeDomainCh1SenseText, _timeDomainCh2SenseText, _timeDomainCh3SenseText,
                _fFTIntervalTB, _successMessageInSenseSettings;
        private Brush _fFTIntervalTBBorder;
        //Button colors
        private Brush _tDCh0SenseButtonColor, _tDCh0PB0SenseButtonColor, _tDCh0PB1SenseButtonColor, _tDSenseButtonColor, _tDStreamButtonColor,
                _fFTSenseButtonColor, _fFTStreamButtonColor, _powerSenseButtonColor, _powerStreamButtonColor, _tDCh1SenseButtonColor,
                _lD1SenseButtonColor, _accStreamButtonColor, _ch1PB0SenseButtonColor, _ch1PB1SenseButtonColor, _lD0SenseButtonColor,
                _adaptherapyStreamButtonColor, _adaptiveStateSenseButtonColor, _adaptiveStateStreamButtonColor, _loopRecSenseButtonColor,
                _eventStreamButtonColor, _timeStampStreamButtonColor, _tDCh2SenseButtonColor, _tDCh2PB0ButtonColor, _tDCh2PB1ButtonColor,
                _accelOnOffButtonColor, _tDCh3SenseButtonColor, _tDCh3PB0ButtonColor, _tDCh3PB1ButtonColor, _windowEnabledButtonColor;
        //Button border colors
        private Brush _timeDomainCh0SenseBorder, _tDCh0PowerBand0SenseBorder, _tDCh0PowerBand1SenseBorder, _timeDomainSenseBorder, _timeDomainStreamBorder,
                _fFTSenseBorder, _fFTStreamBorder, _powerSenseBorder, _powerStreamBorder, _timeDomainCh1SenseBorder, _lD0SenseBorder,
                _accStreamBorder, _tDCh1PowerBand0SenseBorder, _tDCh1PowerBand1SenseBorder, _lD1SenseBorder, _adaptiveStreamBorder,
                _adaptiveStateSenseBorder, _adaptiveStateStreamBorder, _loopRecSenseBorder, _eventMarkerStreamBorder, _timeStampStreamBorder,
                _timeDomainCh2SenseBorder, _tDCh2PowerBand0SenseBorder, _tDCh2PowerBand1SenseBorder, _accelOnOffBorder, _timeDomainCh3SenseBorder,
                _tDCh3PowerBand0SenseBorder, _tDCh3PowerBand1SenseBorder, _windowEnabledFFTBorder;
        //Combobox border colors
        private Brush _tDSampleRateCBBorder, _tDCh0PosInputCBBorder, _tDCh0HPF1InputCBBorder, _tDCh0LPF1InputCBBorder,
                _tDCh0PB0LowInputCBBorder, _tDCh0PB1LowInputCBBorder, _tDCh0NegInputCBBorder, _tDCh0LPF2InputCBBorder,
                _tDCh0PB0UpperInputCBBorder, _tDCh0PB1UpperInputCBBorder, _tDCh1PosInputCBBorder, _tDCh1HPF1InputCBBorder,
                _tDCh1LPF1InputCBBorder, _tDCh1PB0LowerInputCBBorder, _tDCh1PB1LowerInputCBBorder, _tDCh1NegInputCBBorder,
                _tDCh1LPF2InputCBBorder, _tDCh1PB0UpperInputCBBorder, _tDCh1PB1UpperInputCBBorder, _tDCh2PosInputCBBorder,
                _tDCh02HPF1InputCBBorder, _tDCh2LPF1InputCBBorder, _tDCh2PB0LowerInputCBBorder, _tDCh2PB1LowerInputCBBorder,
                _fFTShiftCBBorder, _fFTChannelCBBorder, _tDCh2NegInputCBBorder, _tDCh2LPF2InputCBBorder, _tDCh2PB0UpperInputCBBorder,
                _tDCh2PB1UpperInputCBBorder, _fFTSizeCBBorder, _miscSampleRateCBBorder, _fFTWindowLoadCBBorder, _accSampleRateCBBorder,
                _tDCh3PosInputCBBorder, _tDCh3HPF1InputCBBorder, _tDCh3LPF1InputCBBorder, _tDCh3PB0LowerInputCBBorder,
                _tDCh3PB1LowerInputCBBorder, _tDCh3NegInputCBBorder, _tDCh3LPF2InputCBBorder, _tDCh3PB0UpperInputCBBorder,
                _tDCh3PB1UpperInputCBBorder, _fFTLowerBorder, _fFTUpperBorder;
        //Int and double for combobox selected items
        private int _selectedTDRate, _selectedTDCh0PosInput, _selectedTDCh0LPF1Input, _selectedTDCh0NegInput,
                _selectedTDCh0LPF2Input, _selectedTDCh1PosInput, _selectedTDCh1LPF1Input, _selectedTDCh1NegInput,
                _selectedTDCh1LPF2Input, _selectedTDCh2PosInput, _selectedTDCh2LPF1Input, _selectedFFTChannel, 
                _selectedTDCh2NegInput, _selectedTDCh2LPF2Input, _selectedFFTSize, _selectedMiscSampleRate, _selectedFFTWindowLoad,
                _selectedAccSampleRate, _selectedTDCh3PosInput, _selectedTDCh3LPF1Input, _selectedTDCh3NegInput,
                _selectedTDCh3LPF2Input;
        private double _selectedTDCh0PB0LowerInput, _selectedTDCh0PB1LowerInput, _selectedTDCh0PB0UpperInput, _selectedTDCh0PB1UpperInput,
                _selectedTDCh1PB0LowerInput, _selectedTDCh1PB1LowerInput, _selectedTDCh1PB0UpperInput, _selectedTDCh1PB1UpperInput,
                _selectedTDCh2PB0LowerInput, _selectedTDCh2PB1LowerInput, _selectedTDCh2PB0UpperInput, _selectedTDCh2PB1UpperInput,
                _selectedTDCh3PB0LowerInput, _selectedTDCh3PB1LowerInput, _selectedTDCh3PB0UpperInput, _selectedTDCh3PB1UpperInput,
                _selectedTDCh0HPF1Input, _selectedTDCh1HPF1Input, _selectedTDCh2HPF1Input, _selectedTDCh3HPF1Input,
                _selectedFFTLowerInput, _selectedFFTUpperInput;
        private uint _selectedFFTShift;
        //Combobox collections
        private BindableCollection<double> _tDCh0PB0LowerInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh0PB1LowerInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh0PB0UpperInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh0PB1UpperInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh1PB0LowerInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh1PB1LowerInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh1PB0UpperInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh1PB1UpperInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh2PB0LowerInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh2PB1LowerInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh2PB0UpperInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh2PB1UpperInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh3PB0LowerInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh3PB1LowerInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh3PB0UpperInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh3PB1UpperInputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh1HPF1InputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh02HPF1InputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh0HPF1InputCB = new BindableCollection<double>();
        private BindableCollection<double> _tDCh3HPF1InputCB = new BindableCollection<double>();
        private BindableCollection<double> _fFTUpperInputCB = new BindableCollection<double>();
        private BindableCollection<double> _fFTLowerInputCB = new BindableCollection<double>();
        private BindableCollection<int> _tDSampleRateCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh0PosInputCB = new BindableCollection<int>();       
        private BindableCollection<int> _tDCh0LPF1InputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh0NegInputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh0LPF2InputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh1PosInputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh1LPF1InputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh1NegInputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh1LPF2InputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh2PosInputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh2LPF1InputCB = new BindableCollection<int>();
        private BindableCollection<uint> _fFTShiftMultipliesCB = new BindableCollection<uint>();
        private BindableCollection<int> _fFTChannelCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh2NegInputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh2LPF2InputCB = new BindableCollection<int>();
        private BindableCollection<int> _fFTSizeCB = new BindableCollection<int>();
        private BindableCollection<int> _miscSampleRateCB = new BindableCollection<int>();
        private BindableCollection<int> _fFTWindowLoadCB = new BindableCollection<int>();
        private BindableCollection<int> _accSampleRateCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh3PosInputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh3LPF1InputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh3NegInputCB = new BindableCollection<int>();
        private BindableCollection<int> _tDCh3LPF2InputCB = new BindableCollection<int>();

        #region UI Bindings for Sense Settings (buttons)
        /// <summary>
        /// Enables/disables load/save/save+update sense buttons in sense settings
        /// </summary>
        public bool SenseButtonEnabled
        {
            get { return _senseButtonEnabled; }
            set
            {
                _senseButtonEnabled = value;
                NotifyOfPropertyChange(() => SenseButtonEnabled);
            }
        }
        /// <summary>
        /// Button Binding for TD Ch0 on or off
        /// </summary>
        public void TimeDomainCh0SenseButton()
        {
            if (TDCh0SenseButtonColor.Equals(Brushes.White))
            {
                TDCh0SenseButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.TimeDomains[0].IsEnabled = true;
            }
            else
            {
                TDCh0SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.TimeDomains[0].IsEnabled = false;
                TDCh0PB0SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[0].IsEnabled = false;
                TDCh0PB1SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[1].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[0].IsEnabled != senseConfig.Sense.TimeDomains[0].IsEnabled)
            {
                TimeDomainCh0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[0].IsEnabled != senseConfig.Sense.PowerBands[0].IsEnabled)
            {
                TDCh0PowerBand0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh0PowerBand0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[1].IsEnabled != senseConfig.Sense.PowerBands[1].IsEnabled)
            {
                TDCh0PowerBand1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh0PowerBand1SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for Ch0 PB0 on or off
        /// </summary>
        public void TDCh0PowerBand0SenseButton()
        {
            if (TDCh0PB0SenseButtonColor.Equals(Brushes.White))
            {
                TDCh0PB0SenseButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.PowerBands[0].IsEnabled = true;
                if (!senseConfigFromUI.Sense.TimeDomains[0].IsEnabled)
                {                   
                    TDCh0SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.Sense.TimeDomains[0].IsEnabled = true;
                }
            }
            else
            {
                TDCh0PB0SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[0].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[0].IsEnabled != senseConfig.Sense.TimeDomains[0].IsEnabled)
            {
                TimeDomainCh0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[0].IsEnabled != senseConfig.Sense.PowerBands[0].IsEnabled)
            {
                TDCh0PowerBand0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh0PowerBand0SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for Ch0 PB1 on or off
        /// </summary>
        public void TDCh0PowerBand1SenseButton()
        {
            if (TDCh0PB1SenseButtonColor.Equals(Brushes.White))
            {
                TDCh0PB1SenseButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.PowerBands[1].IsEnabled = true;
                if (!senseConfigFromUI.Sense.TimeDomains[0].IsEnabled)
                {
                    TDCh0SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.Sense.TimeDomains[0].IsEnabled = true;
                }
            }
            else
            {
                TDCh0PB1SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[1].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[0].IsEnabled != senseConfig.Sense.TimeDomains[0].IsEnabled)
            {
                TimeDomainCh0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[1].IsEnabled != senseConfig.Sense.PowerBands[1].IsEnabled)
            {
                TDCh0PowerBand1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh0PowerBand1SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for TimeDomainSenseButton
        /// </summary>
        public void TimeDomainSenseButton()
        {
            if (TDSenseButtonColor.Equals(Brushes.White))
            {
                TDSenseButtonColor = Brushes.Green;
                senseConfigFromUI.SenseOptions.TimeDomain = true;
            }
            else
            {
                TDSenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.TimeDomain = false;
                TDStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.TimeDomain = false;
                FFTStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.FFT = false;
                PowerStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.Power = false;
                PowerSenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.Power = false;
                FFTSenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.FFT = false;
            }
            if(senseConfigFromUI.SenseOptions.TimeDomain != senseConfig.SenseOptions.TimeDomain)
            {
                TimeDomainSenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.TimeDomain != senseConfig.StreamEnables.TimeDomain)
            {
                TimeDomainStreamBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainStreamBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.FFT != senseConfig.StreamEnables.FFT)
            {
                FFTStreamBorder = buttonChangedBrush;
            }
            else
            {
                FFTStreamBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.Power != senseConfig.StreamEnables.Power)
            {
                PowerStreamBorder = buttonChangedBrush;
            }
            else
            {
                PowerStreamBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.Power != senseConfig.SenseOptions.Power)
            {
                PowerSenseBorder = buttonChangedBrush;
            }
            else
            {
                PowerSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.FFT != senseConfig.SenseOptions.FFT)
            {
                FFTSenseBorder = buttonChangedBrush;
            }
            else
            {
                FFTSenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for TimeDomain stream Button
        /// </summary>
        public void TimeDomainStreamButton()
        {
            if (TDStreamButtonColor.Equals(Brushes.White))
            {
                TDStreamButtonColor = Brushes.Green;
                senseConfigFromUI.StreamEnables.TimeDomain = true;
                if (!senseConfigFromUI.SenseOptions.TimeDomain)
                {
                    TDSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.TimeDomain = true;
                }
            }
            else
            {
                TDStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.TimeDomain = false;
            }
            if (senseConfigFromUI.SenseOptions.TimeDomain != senseConfig.SenseOptions.TimeDomain)
            {
                TimeDomainSenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.TimeDomain != senseConfig.StreamEnables.TimeDomain)
            {
                TimeDomainStreamBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainStreamBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for fft sense Button
        /// </summary>
        public void FFTSenseButton()
        {
            if (FFTSenseButtonColor.Equals(Brushes.White))
            {
                FFTSenseButtonColor = Brushes.Green;
                senseConfigFromUI.SenseOptions.FFT = true;
                if (!senseConfigFromUI.SenseOptions.TimeDomain)
                {
                    TDSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.TimeDomain = true;
                }
            }
            else
            {
                FFTSenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.FFT = false;
                FFTStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.FFT = false;
                PowerStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.Power = false;
                PowerSenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.Power = false;
            }
            if (senseConfigFromUI.SenseOptions.TimeDomain != senseConfig.SenseOptions.TimeDomain)
            {
                TimeDomainSenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.FFT != senseConfig.StreamEnables.FFT)
            {
                FFTStreamBorder = buttonChangedBrush;
            }
            else
            {
                FFTStreamBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.Power != senseConfig.StreamEnables.Power)
            {
                PowerStreamBorder = buttonChangedBrush;
            }
            else
            {
                PowerStreamBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.Power != senseConfig.SenseOptions.Power)
            {
                PowerSenseBorder = buttonChangedBrush;
            }
            else
            {
                PowerSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.FFT != senseConfig.SenseOptions.FFT)
            {
                FFTSenseBorder = buttonChangedBrush;
            }
            else
            {
                FFTSenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for fft stream Button
        /// </summary>
        public void FFTStreamButton()
        {
            if (FFTStreamButtonColor.Equals(Brushes.White))
            {
                FFTStreamButtonColor = Brushes.Green;
                senseConfigFromUI.StreamEnables.FFT = true;
                if (!senseConfigFromUI.SenseOptions.FFT)
                {
                    FFTSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.FFT = true;
                }
                if (!senseConfigFromUI.SenseOptions.TimeDomain)
                {
                    TDSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.TimeDomain = true;
                }
            }
            else
            {
                FFTStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.FFT = false;
            }
            if (senseConfigFromUI.SenseOptions.FFT != senseConfig.SenseOptions.FFT)
            {
                FFTSenseBorder = buttonChangedBrush;
            }
            else
            {
                FFTSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.TimeDomain != senseConfig.SenseOptions.TimeDomain)
            {
                TimeDomainSenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.FFT != senseConfig.StreamEnables.FFT)
            {
                FFTStreamBorder = buttonChangedBrush;
            }
            else
            {
                FFTStreamBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for power sense Button
        /// </summary>
        public void PowerSenseButton()
        {
            if (PowerSenseButtonColor.Equals(Brushes.White))
            {
                PowerSenseButtonColor = Brushes.Green;
                senseConfigFromUI.SenseOptions.Power = true;
                if (!senseConfigFromUI.SenseOptions.FFT)
                {
                    FFTSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.FFT = true;
                }
                if (!senseConfigFromUI.SenseOptions.TimeDomain)
                {
                    TDSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.TimeDomain = true;
                }
            }
            else
            {
                PowerSenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.Power = false;
                PowerStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.Power = false;
            }
            if (senseConfigFromUI.SenseOptions.TimeDomain != senseConfig.SenseOptions.TimeDomain)
            {
                TimeDomainSenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.Power != senseConfig.StreamEnables.Power)
            {
                PowerStreamBorder = buttonChangedBrush;
            }
            else
            {
                PowerStreamBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.Power != senseConfig.SenseOptions.Power)
            {
                PowerSenseBorder = buttonChangedBrush;
            }
            else
            {
                PowerSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.FFT != senseConfig.SenseOptions.FFT)
            {
                FFTSenseBorder = buttonChangedBrush;
            }
            else
            {
                FFTSenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for power stream Button
        /// </summary>
        public void PowerStreamButton()
        {
            if (PowerStreamButtonColor.Equals(Brushes.White))
            {
                PowerStreamButtonColor = Brushes.Green;
                senseConfigFromUI.StreamEnables.Power = true;
                if (!senseConfigFromUI.SenseOptions.Power)
                {
                    PowerSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.Power = true;
                }
                if (!senseConfigFromUI.SenseOptions.FFT)
                {
                    FFTSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.FFT = true;
                }
                if (!senseConfigFromUI.SenseOptions.TimeDomain)
                {
                    TDSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.TimeDomain = true;
                }
            }
            else
            {
                PowerStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.Power = false;
            }
            if (senseConfigFromUI.SenseOptions.TimeDomain != senseConfig.SenseOptions.TimeDomain)
            {
                TimeDomainSenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.Power != senseConfig.StreamEnables.Power)
            {
                PowerStreamBorder = buttonChangedBrush;
            }
            else
            {
                PowerStreamBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.Power != senseConfig.SenseOptions.Power)
            {
                PowerSenseBorder = buttonChangedBrush;
            }
            else
            {
                PowerSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.FFT != senseConfig.SenseOptions.FFT)
            {
                FFTSenseBorder = buttonChangedBrush;
            }
            else
            {
                FFTSenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for TD Ch1 on or off
        /// </summary>
        public void TimeDomainCh1SenseButton()
        {
            if (TDCh1SenseButtonColor.Equals(Brushes.White))
            {
                TDCh1SenseButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.TimeDomains[1].IsEnabled = true;
            }
            else
            {
                TDCh1SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.TimeDomains[1].IsEnabled = false;
                Ch1PB0SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[2].IsEnabled = false;
                Ch1PB1SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[3].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[1].IsEnabled != senseConfig.Sense.TimeDomains[1].IsEnabled)
            {
                TimeDomainCh1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh1SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[2].IsEnabled != senseConfig.Sense.PowerBands[2].IsEnabled)
            {
                TDCh1PowerBand0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh1PowerBand0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[3].IsEnabled != senseConfig.Sense.PowerBands[3].IsEnabled)
            {
                TDCh1PowerBand1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh1PowerBand1SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for LD0 sense Button
        /// </summary>
        public void LD0SenseButton()
        {
            if (LD0SenseButtonColor.Equals(Brushes.White))
            {
                LD0SenseButtonColor = Brushes.Green;
                senseConfigFromUI.SenseOptions.LD0 = true;
            }
            else
            {
                LD0SenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.LD0 = false;
                LD1SenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.LD1 = false;
                AdaptherapyStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.AdaptiveTherapy = false;
                AdaptiveStateSenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.AdaptiveState = false;
                AdaptiveStateStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.AdaptiveState = false;
            }
            if (senseConfigFromUI.SenseOptions.LD0 != senseConfig.SenseOptions.LD0)
            {
                LD0SenseBorder = buttonChangedBrush;
            }
            else
            {
                LD0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.LD1 != senseConfig.SenseOptions.LD1)
            {
                LD1SenseBorder = buttonChangedBrush;
            }
            else
            {
                LD1SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.AdaptiveTherapy != senseConfig.StreamEnables.AdaptiveTherapy)
            {
                AdaptiveStreamBorder = buttonChangedBrush;
            }
            else
            {
                AdaptiveStreamBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.AdaptiveState != senseConfig.SenseOptions.AdaptiveState)
            {
                AdaptiveStateSenseBorder = buttonChangedBrush;
            }
            else
            {
                AdaptiveStateSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.AdaptiveState != senseConfig.StreamEnables.AdaptiveState)
            {
                AdaptiveStateStreamBorder = buttonChangedBrush;
            }
            else
            {
                AdaptiveStateStreamBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for acc stream Button
        /// </summary>
        public void AccStreamButton()
        {
            if (AccStreamButtonColor.Equals(Brushes.White))
            {
                AccStreamButtonColor = Brushes.Green;
                senseConfigFromUI.StreamEnables.Accelerometry = true;
            }
            else
            {
                AccStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.Accelerometry = false;
            }
            if (senseConfigFromUI.StreamEnables.Accelerometry != senseConfig.StreamEnables.Accelerometry)
            {
                AccStreamBorder = buttonChangedBrush;
            }
            else
            {
                AccStreamBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for Ch1 PB0 on or off
        /// </summary>
        public void TDCh1PowerBand0SenseButton()
        {
            if (Ch1PB0SenseButtonColor.Equals(Brushes.White))
            {
                Ch1PB0SenseButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.PowerBands[2].IsEnabled = true;
                if (!senseConfigFromUI.Sense.TimeDomains[1].IsEnabled)
                {
                    TDCh1SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.Sense.TimeDomains[1].IsEnabled = true;
                }
            }
            else
            {
                Ch1PB0SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[2].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[1].IsEnabled != senseConfig.Sense.TimeDomains[1].IsEnabled)
            {
                TimeDomainCh1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh1SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[2].IsEnabled != senseConfig.Sense.PowerBands[2].IsEnabled)
            {
                TDCh1PowerBand0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh1PowerBand0SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for Ch1 PB1 on or off
        /// </summary>
        public void TDCh1PowerBand1SenseButton()
        {
            if (Ch1PB1SenseButtonColor.Equals(Brushes.White))
            {
                Ch1PB1SenseButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.PowerBands[3].IsEnabled = true;
                if (!senseConfigFromUI.Sense.TimeDomains[1].IsEnabled)
                {
                    TDCh1SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.Sense.TimeDomains[1].IsEnabled = true;
                }
            }
            else
            {
                Ch1PB1SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[3].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[1].IsEnabled != senseConfig.Sense.TimeDomains[1].IsEnabled)
            {
                TimeDomainCh1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh1SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[3].IsEnabled != senseConfig.Sense.PowerBands[3].IsEnabled)
            {
                TDCh1PowerBand1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh1PowerBand1SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for LD1 sense Button
        /// </summary>
        public void LD1SenseButton()
        {
            if (LD1SenseButtonColor.Equals(Brushes.White))
            {
                LD1SenseButtonColor = Brushes.Green;
                senseConfigFromUI.SenseOptions.LD1 = true;
                if (!senseConfigFromUI.SenseOptions.LD0)
                {
                    LD0SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.LD0 = true;
                }
            }
            else
            {
                LD1SenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.LD1 = false;
            }
            if (senseConfigFromUI.SenseOptions.LD0 != senseConfig.SenseOptions.LD0)
            {
                LD0SenseBorder = buttonChangedBrush;
            }
            else
            {
                LD0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.LD1 != senseConfig.SenseOptions.LD1)
            {
                LD1SenseBorder = buttonChangedBrush;
            }
            else
            {
                LD1SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for adaptive therapy stream Button
        /// </summary>
        public void AdaptiveStreamButton()
        {
            if (AdaptherapyStreamButtonColor.Equals(Brushes.White))
            {
                AdaptherapyStreamButtonColor = Brushes.Green;
                senseConfigFromUI.StreamEnables.AdaptiveTherapy = true;
                if (!senseConfigFromUI.SenseOptions.LD0)
                {
                    LD0SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.LD0 = true;
                }
            }
            else
            {
                AdaptherapyStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.AdaptiveTherapy = false;
            }
            if (senseConfigFromUI.SenseOptions.LD0 != senseConfig.SenseOptions.LD0)
            {
                LD0SenseBorder = buttonChangedBrush;
            }
            else
            {
                LD0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.AdaptiveTherapy != senseConfig.StreamEnables.AdaptiveTherapy)
            {
                AdaptiveStreamBorder = buttonChangedBrush;
            }
            else
            {
                AdaptiveStreamBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for adaptive state sense Button
        /// </summary>
        public void AdaptiveStateSenseButton()
        {
            if (AdaptiveStateSenseButtonColor.Equals(Brushes.White))
            {
                AdaptiveStateSenseButtonColor = Brushes.Green;
                senseConfigFromUI.SenseOptions.AdaptiveState = true;
                if (!senseConfigFromUI.SenseOptions.LD0)
                {
                    LD0SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.LD0 = true;
                }
            }
            else
            {
                AdaptiveStateSenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.AdaptiveState = false;
                AdaptiveStateStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.AdaptiveState = false;
            }
            if (senseConfigFromUI.SenseOptions.LD0 != senseConfig.SenseOptions.LD0)
            {
                LD0SenseBorder = buttonChangedBrush;
            }
            else
            {
                LD0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.AdaptiveState != senseConfig.SenseOptions.AdaptiveState)
            {
                AdaptiveStateSenseBorder = buttonChangedBrush;
            }
            else
            {
                AdaptiveStateSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.AdaptiveState != senseConfig.StreamEnables.AdaptiveState)
            {
                AdaptiveStateStreamBorder = buttonChangedBrush;
            }
            else
            {
                AdaptiveStateStreamBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for adaptive state stream Button
        /// </summary>
        public void AdaptiveStateStreamButton()
        {
            if (AdaptiveStateStreamButtonColor.Equals(Brushes.White))
            {
                AdaptiveStateStreamButtonColor = Brushes.Green;
                senseConfigFromUI.StreamEnables.AdaptiveState = true;
                if (!senseConfigFromUI.SenseOptions.AdaptiveState)
                {
                    AdaptiveStateSenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.AdaptiveState = true;
                }
                if (!senseConfigFromUI.SenseOptions.LD0)
                {
                    LD0SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.SenseOptions.LD0 = true;
                }
            }
            else
            {
                AdaptiveStateStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.AdaptiveState = false;
            }
            if (senseConfigFromUI.SenseOptions.LD0 != senseConfig.SenseOptions.LD0)
            {
                LD0SenseBorder = buttonChangedBrush;
            }
            else
            {
                LD0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.SenseOptions.AdaptiveState != senseConfig.SenseOptions.AdaptiveState)
            {
                AdaptiveStateSenseBorder = buttonChangedBrush;
            }
            else
            {
                AdaptiveStateSenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.StreamEnables.AdaptiveState != senseConfig.StreamEnables.AdaptiveState)
            {
                AdaptiveStateStreamBorder = buttonChangedBrush;
            }
            else
            {
                AdaptiveStateStreamBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for loop recording sense Button
        /// </summary>
        public void LoopRecSenseButton()
        {
            if (LoopRecSenseButtonColor.Equals(Brushes.White))
            {
                LoopRecSenseButtonColor = Brushes.Green;
                senseConfigFromUI.SenseOptions.LoopRecording = true;
            }
            else
            {
                LoopRecSenseButtonColor = Brushes.White;
                senseConfigFromUI.SenseOptions.LoopRecording = false;
            }
            if (senseConfigFromUI.SenseOptions.LoopRecording != senseConfig.SenseOptions.LoopRecording)
            {
                LoopRecSenseBorder = buttonChangedBrush;
            }
            else
            {
                LoopRecSenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for event marker stream Button
        /// </summary>
        public void EventMarkerStreamButton()
        {
            if (EventStreamButtonColor.Equals(Brushes.White))
            {
                EventStreamButtonColor = Brushes.Green;
                senseConfigFromUI.StreamEnables.EventMarker = true;
            }
            else
            {
                EventStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.EventMarker = false;
            }
            if (senseConfigFromUI.StreamEnables.EventMarker != senseConfig.StreamEnables.EventMarker)
            {
                EventMarkerStreamBorder = buttonChangedBrush;
            }
            else
            {
                EventMarkerStreamBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for time stamp stream Button
        /// </summary>
        public void TimeStampStreamButton()
        {
            if (TimeStampStreamButtonColor.Equals(Brushes.White))
            {
                TimeStampStreamButtonColor = Brushes.Green;
                senseConfigFromUI.StreamEnables.TimeStamp = true;
            }
            else
            {
                TimeStampStreamButtonColor = Brushes.White;
                senseConfigFromUI.StreamEnables.TimeStamp = false;
            }
            if (senseConfigFromUI.StreamEnables.TimeStamp != senseConfig.StreamEnables.TimeStamp)
            {
                TimeStampStreamBorder = buttonChangedBrush;
            }
            else
            {
                TimeStampStreamBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for TD Ch2 on or off
        /// </summary>
        public void TimeDomainCh2SenseButton()
        {
            if (TDCh2SenseButtonColor.Equals(Brushes.White))
            {
                TDCh2SenseButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.TimeDomains[2].IsEnabled = true;
            }
            else
            {
                TDCh2SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.TimeDomains[2].IsEnabled = false;
                TDCh2PB0ButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[4].IsEnabled = false;
                TDCh2PB1ButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[5].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[2].IsEnabled != senseConfig.Sense.TimeDomains[2].IsEnabled)
            {
                TimeDomainCh2SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh2SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[4].IsEnabled != senseConfig.Sense.PowerBands[4].IsEnabled)
            {
                TDCh2PowerBand0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh2PowerBand0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[5].IsEnabled != senseConfig.Sense.PowerBands[5].IsEnabled)
            {
                TDCh2PowerBand1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh2PowerBand1SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for Ch2 PB0 on or off
        /// </summary>
        public void TDCh2PowerBand0SenseButton()
        {
            if (TDCh2PB0ButtonColor.Equals(Brushes.White))
            {
                TDCh2PB0ButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.PowerBands[4].IsEnabled = true;
                if (!senseConfigFromUI.Sense.TimeDomains[2].IsEnabled)
                {
                    TDCh2SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.Sense.TimeDomains[2].IsEnabled = true;
                }
            }
            else
            {
                TDCh2PB0ButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[4].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[2].IsEnabled != senseConfig.Sense.TimeDomains[2].IsEnabled)
            {
                TimeDomainCh2SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh2SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[4].IsEnabled != senseConfig.Sense.PowerBands[4].IsEnabled)
            {
                TDCh2PowerBand0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh2PowerBand0SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for Ch2 PB1 on or off
        /// </summary>
        public void TDCh2PowerBand1SenseButton()
        {
            if (TDCh2PB1ButtonColor.Equals(Brushes.White))
            {
                TDCh2PB1ButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.PowerBands[5].IsEnabled = true;
                if (!senseConfigFromUI.Sense.TimeDomains[2].IsEnabled)
                {
                    TDCh2SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.Sense.TimeDomains[2].IsEnabled = true;
                }
            }
            else
            {
                TDCh2PB1ButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[5].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[2].IsEnabled != senseConfig.Sense.TimeDomains[2].IsEnabled)
            {
                TimeDomainCh2SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh2SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[5].IsEnabled != senseConfig.Sense.PowerBands[5].IsEnabled)
            {
                TDCh2PowerBand1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh2PowerBand1SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for turning accelerometry on or off
        /// </summary>
        public void AccelOnOffButton()
        {
            if (AccelOnOffButtonColor.Equals(Brushes.White))
            {
                AccelOnOffButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.Accelerometer.SampleRateDisabled = false;
            }
            else
            {
                AccelOnOffButtonColor = Brushes.White;
                senseConfigFromUI.Sense.Accelerometer.SampleRateDisabled = true;
            }
            if (senseConfigFromUI.Sense.Accelerometer.SampleRateDisabled != senseConfig.Sense.Accelerometer.SampleRateDisabled)
            {
                AccelOnOffBorder = buttonChangedBrush;
            }
            else
            {
                AccelOnOffBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for TD Ch3 on or off
        /// </summary>
        public void TimeDomainCh3SenseButton()
        {
            if (TDCh3SenseButtonColor.Equals(Brushes.White))
            {
                TDCh3SenseButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.TimeDomains[3].IsEnabled = true;
            }
            else
            {
                TDCh3SenseButtonColor = Brushes.White;
                senseConfigFromUI.Sense.TimeDomains[3].IsEnabled = false;
                TDCh3PB0ButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[6].IsEnabled = false;
                TDCh3PB1ButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[7].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[3].IsEnabled != senseConfig.Sense.TimeDomains[3].IsEnabled)
            {
                TimeDomainCh3SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh3SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[6].IsEnabled != senseConfig.Sense.PowerBands[6].IsEnabled)
            {
                TDCh3PowerBand0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh3PowerBand0SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[7].IsEnabled != senseConfig.Sense.PowerBands[7].IsEnabled)
            {
                TDCh3PowerBand1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh3PowerBand1SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for Ch3 PB0 on or off
        /// </summary>
        public void TDCh3PowerBand0SenseButton()
        {
            if (TDCh3PB0ButtonColor.Equals(Brushes.White))
            {
                TDCh3PB0ButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.PowerBands[6].IsEnabled = true;
                if (!senseConfigFromUI.Sense.TimeDomains[3].IsEnabled)
                {
                    TDCh3SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.Sense.TimeDomains[3].IsEnabled = true;
                }
            }
            else
            {
                TDCh3PB0ButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[6].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[3].IsEnabled != senseConfig.Sense.TimeDomains[3].IsEnabled)
            {
                TimeDomainCh3SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh3SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[6].IsEnabled != senseConfig.Sense.PowerBands[6].IsEnabled)
            {
                TDCh3PowerBand0SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh3PowerBand0SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for Ch3 PB0 on or off
        /// </summary>
        public void TDCh3PowerBand1SenseButton()
        {
            if (TDCh3PB1ButtonColor.Equals(Brushes.White))
            {
                TDCh3PB1ButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.PowerBands[7].IsEnabled = true;
                if (!senseConfigFromUI.Sense.TimeDomains[3].IsEnabled)
                {
                    TDCh3SenseButtonColor = Brushes.Green;
                    senseConfigFromUI.Sense.TimeDomains[3].IsEnabled = true;
                }
            }
            else
            {
                TDCh3PB1ButtonColor = Brushes.White;
                senseConfigFromUI.Sense.PowerBands[7].IsEnabled = false;
            }
            if (senseConfigFromUI.Sense.TimeDomains[3].IsEnabled != senseConfig.Sense.TimeDomains[3].IsEnabled)
            {
                TimeDomainCh3SenseBorder = buttonChangedBrush;
            }
            else
            {
                TimeDomainCh3SenseBorder = buttonNotChangedBrush;
            }
            if (senseConfigFromUI.Sense.PowerBands[7].IsEnabled != senseConfig.Sense.PowerBands[7].IsEnabled)
            {
                TDCh3PowerBand1SenseBorder = buttonChangedBrush;
            }
            else
            {
                TDCh3PowerBand1SenseBorder = buttonNotChangedBrush;
            }
        }
        /// <summary>
        /// Button Binding for turning window enabled for fft on or off
        /// </summary>
        public void WindowEnabledFFTButton()
        {
            if (WindowEnabledButtonColor.Equals(Brushes.White))
            {
                WindowEnabledButtonColor = Brushes.Green;
                senseConfigFromUI.Sense.FFT.WindowEnabled = true;
            }
            else
            {
                WindowEnabledButtonColor = Brushes.White;
                senseConfigFromUI.Sense.FFT.WindowEnabled = false;
            }
            if (senseConfigFromUI.Sense.FFT.WindowEnabled != senseConfig.Sense.FFT.WindowEnabled)
            {
                WindowEnabledFFTBorder = buttonChangedBrush;
            }
            else
            {
                WindowEnabledFFTBorder = buttonNotChangedBrush;
            }
        }

        /// <summary>
        /// Text for the Time domain ch 0 button 
        /// </summary>
        public string TimeDomainCh0SenseText
        {
            get { return _timeDomainCh0SenseText; }
            set
            {
                _timeDomainCh0SenseText = value;
                NotifyOfPropertyChange(() => TimeDomainCh0SenseText);
            }
        }

        /// <summary>
        /// Text for the Time domain ch 1 button 
        /// </summary>
        public string TimeDomainCh1SenseText
        {
            get { return _timeDomainCh1SenseText; }
            set
            {
                _timeDomainCh1SenseText = value;
                NotifyOfPropertyChange(() => TimeDomainCh1SenseText);
            }
        }

        /// <summary>
        /// Text for the Time domain ch 2 button 
        /// </summary>
        public string TimeDomainCh2SenseText
        {
            get { return _timeDomainCh2SenseText; }
            set
            {
                _timeDomainCh2SenseText = value;
                NotifyOfPropertyChange(() => TimeDomainCh2SenseText);
            }
        }

        /// <summary>
        /// Text for the Time domain ch 3 button 
        /// </summary>
        public string TimeDomainCh3SenseText
        {
            get { return _timeDomainCh3SenseText; }
            set
            {
                _timeDomainCh3SenseText = value;
                NotifyOfPropertyChange(() => TimeDomainCh3SenseText);
            }
        }

        /// <summary>
        /// Binding used to change the color of the TDCh0SenseButtonColor
        /// </summary>
        public Brush TDCh0SenseButtonColor
        {
            get { return _tDCh0SenseButtonColor; }
            set
            {
                _tDCh0SenseButtonColor = value;
                NotifyOfPropertyChange(() => TDCh0SenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the TDCh0PB0SenseButtonColor
        /// </summary>
        public Brush TDCh0PB0SenseButtonColor
        {
            get { return _tDCh0PB0SenseButtonColor; }
            set
            {
                _tDCh0PB0SenseButtonColor = value;
                NotifyOfPropertyChange(() => TDCh0PB0SenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the TDCh0PB1SenseButtonColor
        /// </summary>
        public Brush TDCh0PB1SenseButtonColor
        {
            get { return _tDCh0PB1SenseButtonColor; }
            set
            {
                _tDCh0PB1SenseButtonColor = value;
                NotifyOfPropertyChange(() => TDCh0PB1SenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the TDSenseButtonColor
        /// </summary>
        public Brush TDSenseButtonColor
        {
            get { return _tDSenseButtonColor; }
            set
            {
                _tDSenseButtonColor = value;
                NotifyOfPropertyChange(() => TDSenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the TDStreamButtonColor
        /// </summary>
        public Brush TDStreamButtonColor
        {
            get { return _tDStreamButtonColor; }
            set
            {
                _tDStreamButtonColor = value;
                NotifyOfPropertyChange(() => TDStreamButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the FFTSenseButtonColor
        /// </summary>
        public Brush FFTSenseButtonColor
        {
            get { return _fFTSenseButtonColor; }
            set
            {
                _fFTSenseButtonColor = value;
                NotifyOfPropertyChange(() => FFTSenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the FFTStreamButtonColor
        /// </summary>
        public Brush FFTStreamButtonColor
        {
            get { return _fFTStreamButtonColor; }
            set
            {
                _fFTStreamButtonColor = value;
                NotifyOfPropertyChange(() => FFTStreamButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the PowerSenseButtonColor
        /// </summary>
        public Brush PowerSenseButtonColor
        {
            get { return _powerSenseButtonColor; }
            set
            {
                _powerSenseButtonColor = value;
                NotifyOfPropertyChange(() => PowerSenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the PowerStreamButtonColor
        /// </summary>
        public Brush PowerStreamButtonColor
        {
            get { return _powerStreamButtonColor; }
            set
            {
                _powerStreamButtonColor = value;
                NotifyOfPropertyChange(() => PowerStreamButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the TDCh1SenseButtonColor
        /// </summary>
        public Brush TDCh1SenseButtonColor
        {
            get { return _tDCh1SenseButtonColor; }
            set
            {
                _tDCh1SenseButtonColor = value;
                NotifyOfPropertyChange(() => TDCh1SenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the LD0SenseButtonColor
        /// </summary>
        public Brush LD0SenseButtonColor
        {
            get { return _lD0SenseButtonColor; }
            set
            {
                _lD0SenseButtonColor = value;
                NotifyOfPropertyChange(() => LD0SenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the AccStreamButtonColor
        /// </summary>
        public Brush AccStreamButtonColor
        {
            get { return _accStreamButtonColor; }
            set
            {
                _accStreamButtonColor = value;
                NotifyOfPropertyChange(() => AccStreamButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the Ch1PB0SenseButtonColor
        /// </summary>
        public Brush Ch1PB0SenseButtonColor
        {
            get { return _ch1PB0SenseButtonColor; }
            set
            {
                _ch1PB0SenseButtonColor = value;
                NotifyOfPropertyChange(() => Ch1PB0SenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the Ch1PB1SenseButtonColor
        /// </summary>
        public Brush Ch1PB1SenseButtonColor
        {
            get { return _ch1PB1SenseButtonColor; }
            set
            {
                _ch1PB1SenseButtonColor = value;
                NotifyOfPropertyChange(() => Ch1PB1SenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the LD1SenseButtonColor
        /// </summary>
        public Brush LD1SenseButtonColor
        {
            get { return _lD1SenseButtonColor; }
            set
            {
                _lD1SenseButtonColor = value;
                NotifyOfPropertyChange(() => LD1SenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the AdaptherapyStreamButtonColor
        /// </summary>
        public Brush AdaptherapyStreamButtonColor
        {
            get { return _adaptherapyStreamButtonColor; }
            set
            {
                _adaptherapyStreamButtonColor = value;
                NotifyOfPropertyChange(() => AdaptherapyStreamButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the AdaptiveStateSenseButtonColor
        /// </summary>
        public Brush AdaptiveStateSenseButtonColor
        {
            get { return _adaptiveStateSenseButtonColor; }
            set
            {
                _adaptiveStateSenseButtonColor = value;
                NotifyOfPropertyChange(() => AdaptiveStateSenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the AdaptiveStateStreamButtonColor
        /// </summary>
        public Brush AdaptiveStateStreamButtonColor
        {
            get { return _adaptiveStateStreamButtonColor; }
            set
            {
                _adaptiveStateStreamButtonColor = value;
                NotifyOfPropertyChange(() => AdaptiveStateStreamButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the LoopRecSenseButtonColor
        /// </summary>
        public Brush LoopRecSenseButtonColor
        {
            get { return _loopRecSenseButtonColor; }
            set
            {
                _loopRecSenseButtonColor = value;
                NotifyOfPropertyChange(() => LoopRecSenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the EventStreamButtonColor
        /// </summary>
        public Brush EventStreamButtonColor
        {
            get { return _eventStreamButtonColor; }
            set
            {
                _eventStreamButtonColor = value;
                NotifyOfPropertyChange(() => EventStreamButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the TimeStampStreamButtonColor
        /// </summary>
        public Brush TimeStampStreamButtonColor
        {
            get { return _timeStampStreamButtonColor; }
            set
            {
                _timeStampStreamButtonColor = value;
                NotifyOfPropertyChange(() => TimeStampStreamButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the TDCh2SenseButtonColor
        /// </summary>
        public Brush TDCh2SenseButtonColor
        {
            get { return _tDCh2SenseButtonColor; }
            set
            {
                _tDCh2SenseButtonColor = value;
                NotifyOfPropertyChange(() => TDCh2SenseButtonColor);
            }
        }

        /// <summary>
        /// Binding used to change the color of the TDCh2PB0ButtonColor
        /// </summary>
        public Brush TDCh2PB0ButtonColor
        {
            get { return _tDCh2PB0ButtonColor; }
            set
            {
                _tDCh2PB0ButtonColor = value;
                NotifyOfPropertyChange(() => TDCh2PB0ButtonColor);
            }
        }
        /// <summary>
        /// Binding used to change the color of the TDCh2PB1ButtonColor
        /// </summary>
        public Brush TDCh2PB1ButtonColor
        {
            get { return _tDCh2PB1ButtonColor; }
            set
            {
                _tDCh2PB1ButtonColor = value;
                NotifyOfPropertyChange(() => TDCh2PB1ButtonColor);
            }
        }
        /// <summary>
        /// Binding used to change the color of the AccelOnOffButtonColor
        /// </summary>
        public Brush AccelOnOffButtonColor
        {
            get { return _accelOnOffButtonColor; }
            set
            {
                _accelOnOffButtonColor = value;
                NotifyOfPropertyChange(() => AccelOnOffButtonColor);
            }
        }
        /// <summary>
        /// Binding used to change the color of the TDCh3SenseButtonColor
        /// </summary>
        public Brush TDCh3SenseButtonColor
        {
            get { return _tDCh3SenseButtonColor; }
            set
            {
                _tDCh3SenseButtonColor = value;
                NotifyOfPropertyChange(() => TDCh3SenseButtonColor);
            }
        }
        /// <summary>
        /// Binding used to change the color of the TDCh3PB0ButtonColor
        /// </summary>
        public Brush TDCh3PB0ButtonColor
        {
            get { return _tDCh3PB0ButtonColor; }
            set
            {
                _tDCh3PB0ButtonColor = value;
                NotifyOfPropertyChange(() => TDCh3PB0ButtonColor);
            }
        }
        /// <summary>
        /// Binding used to change the color of the TDCh3PB1ButtonColor
        /// </summary>
        public Brush TDCh3PB1ButtonColor
        {
            get { return _tDCh3PB1ButtonColor; }
            set
            {
                _tDCh3PB1ButtonColor = value;
                NotifyOfPropertyChange(() => TDCh3PB1ButtonColor);
            }
        }
        /// <summary>
        /// Binding used to change the color of the WindowEnabledButtonColor
        /// </summary>
        public Brush WindowEnabledButtonColor
        {
            get { return _windowEnabledButtonColor; }
            set
            {
                _windowEnabledButtonColor = value;
                NotifyOfPropertyChange(() => WindowEnabledButtonColor);
            }
        }

        #endregion

        #region UI Bindins for Sense Settings (button borders)
        /// <summary>
        /// Binding used to change the border color for TimeDomainCh0SenseBorder
        /// </summary>
        public Brush TimeDomainCh0SenseBorder
        {
            get { return _timeDomainCh0SenseBorder ?? (_timeDomainCh0SenseBorder = buttonNotChangedBrush); }
            set
            {
                _timeDomainCh0SenseBorder = value;
                NotifyOfPropertyChange(() => TimeDomainCh0SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TDCh0PowerBand0SenseBorder
        /// </summary>
        public Brush TDCh0PowerBand0SenseBorder
        {
            get { return _tDCh0PowerBand0SenseBorder ?? (_tDCh0PowerBand0SenseBorder = buttonNotChangedBrush); }
            set
            {
                _tDCh0PowerBand0SenseBorder = value;
                NotifyOfPropertyChange(() => TDCh0PowerBand0SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TDCh0PowerBand1SenseBorder
        /// </summary>
        public Brush TDCh0PowerBand1SenseBorder
        {
            get { return _tDCh0PowerBand1SenseBorder ?? (_tDCh0PowerBand1SenseBorder = buttonNotChangedBrush); }
            set
            {
                _tDCh0PowerBand1SenseBorder = value;
                NotifyOfPropertyChange(() => TDCh0PowerBand1SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TimeDomainSenseBorder
        /// </summary>
        public Brush TimeDomainSenseBorder
        {
            get { return _timeDomainSenseBorder ?? (_timeDomainSenseBorder = buttonNotChangedBrush); }
            set
            {
                _timeDomainSenseBorder = value;
                NotifyOfPropertyChange(() => TimeDomainSenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TimeDomainStreamBorder
        /// </summary>
        public Brush TimeDomainStreamBorder
        {
            get { return _timeDomainStreamBorder ?? (_timeDomainStreamBorder = buttonNotChangedBrush); }
            set
            {
                _timeDomainStreamBorder = value;
                NotifyOfPropertyChange(() => TimeDomainStreamBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for FFTSenseBorder
        /// </summary>
        public Brush FFTSenseBorder
        {
            get { return _fFTSenseBorder ?? (_fFTSenseBorder = buttonNotChangedBrush); }
            set
            {
                _fFTSenseBorder = value;
                NotifyOfPropertyChange(() => FFTSenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for FFTStreamBorder
        /// </summary>
        public Brush FFTStreamBorder
        {
            get { return _fFTStreamBorder ?? (_fFTStreamBorder = buttonNotChangedBrush); }
            set
            {
                _fFTStreamBorder = value;
                NotifyOfPropertyChange(() => FFTStreamBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for PowerSenseBorder
        /// </summary>
        public Brush PowerSenseBorder
        {
            get { return _powerSenseBorder ?? (_powerSenseBorder = buttonNotChangedBrush); }
            set
            {
                _powerSenseBorder = value;
                NotifyOfPropertyChange(() => PowerSenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for PowerStreamBorder
        /// </summary>
        public Brush PowerStreamBorder
        {
            get { return _powerStreamBorder ?? (_powerStreamBorder = buttonNotChangedBrush); }
            set
            {
                _powerStreamBorder = value;
                NotifyOfPropertyChange(() => PowerStreamBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TimeDomainCh1SenseBorder
        /// </summary>
        public Brush TimeDomainCh1SenseBorder
        {
            get { return _timeDomainCh1SenseBorder ?? (_timeDomainCh1SenseBorder = buttonNotChangedBrush); }
            set
            {
                _timeDomainCh1SenseBorder = value;
                NotifyOfPropertyChange(() => TimeDomainCh1SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for LD0SenseBorder
        /// </summary>
        public Brush LD0SenseBorder
        {
            get { return _lD0SenseBorder ?? (_lD0SenseBorder = buttonNotChangedBrush); }
            set
            {
                _lD0SenseBorder = value;
                NotifyOfPropertyChange(() => LD0SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for AccStreamBorder
        /// </summary>
        public Brush AccStreamBorder
        {
            get { return _accStreamBorder ?? (_accStreamBorder = buttonNotChangedBrush); }
            set
            {
                _accStreamBorder = value;
                NotifyOfPropertyChange(() => AccStreamBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TDCh1PowerBand0SenseBorder
        /// </summary>
        public Brush TDCh1PowerBand0SenseBorder
        {
            get { return _tDCh1PowerBand0SenseBorder ?? (_tDCh1PowerBand0SenseBorder = buttonNotChangedBrush); }
            set
            {
                _tDCh1PowerBand0SenseBorder = value;
                NotifyOfPropertyChange(() => TDCh1PowerBand0SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TDCh1PowerBand1SenseBorder
        /// </summary>
        public Brush TDCh1PowerBand1SenseBorder
        {
            get { return _tDCh1PowerBand1SenseBorder ?? (_tDCh1PowerBand1SenseBorder = buttonNotChangedBrush); }
            set
            {
                _tDCh1PowerBand1SenseBorder = value;
                NotifyOfPropertyChange(() => TDCh1PowerBand1SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for LD1SenseBorder
        /// </summary>
        public Brush LD1SenseBorder
        {
            get { return _lD1SenseBorder ?? (_lD1SenseBorder = buttonNotChangedBrush); }
            set
            {
                _lD1SenseBorder = value;
                NotifyOfPropertyChange(() => LD1SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for AdaptiveStreamBorder
        /// </summary>
        public Brush AdaptiveStreamBorder
        {
            get { return _adaptiveStreamBorder ?? (_adaptiveStreamBorder = buttonNotChangedBrush); }
            set
            {
                _adaptiveStreamBorder = value;
                NotifyOfPropertyChange(() => AdaptiveStreamBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for AdaptiveStateSenseBorder
        /// </summary>
        public Brush AdaptiveStateSenseBorder
        {
            get { return _adaptiveStateSenseBorder ?? (_adaptiveStateSenseBorder = buttonNotChangedBrush); }
            set
            {
                _adaptiveStateSenseBorder = value;
                NotifyOfPropertyChange(() => AdaptiveStateSenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for AdaptiveStateStreamBorder
        /// </summary>
        public Brush AdaptiveStateStreamBorder
        {
            get { return _adaptiveStateStreamBorder ?? (_adaptiveStateStreamBorder = buttonNotChangedBrush); }
            set
            {
                _adaptiveStateStreamBorder = value;
                NotifyOfPropertyChange(() => AdaptiveStateStreamBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for LoopRecSenseBorder
        /// </summary>
        public Brush LoopRecSenseBorder
        {
            get { return _loopRecSenseBorder ?? (_loopRecSenseBorder = buttonNotChangedBrush); }
            set
            {
                _loopRecSenseBorder = value;
                NotifyOfPropertyChange(() => LoopRecSenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for EventMarkerStreamBorder
        /// </summary>
        public Brush EventMarkerStreamBorder
        {
            get { return _eventMarkerStreamBorder ?? (_eventMarkerStreamBorder = buttonNotChangedBrush); }
            set
            {
                _eventMarkerStreamBorder = value;
                NotifyOfPropertyChange(() => EventMarkerStreamBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TimeStampStreamBorder
        /// </summary>
        public Brush TimeStampStreamBorder
        {
            get { return _timeStampStreamBorder ?? (_timeStampStreamBorder = buttonNotChangedBrush); }
            set
            {
                _timeStampStreamBorder = value;
                NotifyOfPropertyChange(() => TimeStampStreamBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TimeDomainCh2SenseBorder
        /// </summary>
        public Brush TimeDomainCh2SenseBorder
        {
            get { return _timeDomainCh2SenseBorder ?? (_timeDomainCh2SenseBorder = buttonNotChangedBrush); }
            set
            {
                _timeDomainCh2SenseBorder = value;
                NotifyOfPropertyChange(() => TimeDomainCh2SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TDCh2PowerBand0SenseBorder
        /// </summary>
        public Brush TDCh2PowerBand0SenseBorder
        {
            get { return _tDCh2PowerBand0SenseBorder ?? (_tDCh2PowerBand0SenseBorder = buttonNotChangedBrush); }
            set
            {
                _tDCh2PowerBand0SenseBorder = value;
                NotifyOfPropertyChange(() => TDCh2PowerBand0SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TDCh2PowerBand1SenseBorder
        /// </summary>
        public Brush TDCh2PowerBand1SenseBorder
        {
            get { return _tDCh2PowerBand1SenseBorder ?? (_tDCh2PowerBand1SenseBorder = buttonNotChangedBrush); }
            set
            {
                _tDCh2PowerBand1SenseBorder = value;
                NotifyOfPropertyChange(() => TDCh2PowerBand1SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for AccelOnOffBorder
        /// </summary>
        public Brush AccelOnOffBorder
        {
            get { return _accelOnOffBorder ?? (_accelOnOffBorder = buttonNotChangedBrush); }
            set
            {
                _accelOnOffBorder = value;
                NotifyOfPropertyChange(() => AccelOnOffBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TimeDomainCh3SenseBorder
        /// </summary>
        public Brush TimeDomainCh3SenseBorder
        {
            get { return _timeDomainCh3SenseBorder ?? (_timeDomainCh3SenseBorder = buttonNotChangedBrush); }
            set
            {
                _timeDomainCh3SenseBorder = value;
                NotifyOfPropertyChange(() => TimeDomainCh3SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TDCh3PowerBand0SenseBorder
        /// </summary>
        public Brush TDCh3PowerBand0SenseBorder
        {
            get { return _tDCh3PowerBand0SenseBorder ?? (_tDCh3PowerBand0SenseBorder = buttonNotChangedBrush); }
            set
            {
                _tDCh3PowerBand0SenseBorder = value;
                NotifyOfPropertyChange(() => TDCh3PowerBand0SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for TDCh3PowerBand1SenseBorder
        /// </summary>
        public Brush TDCh3PowerBand1SenseBorder
        {
            get { return _tDCh3PowerBand1SenseBorder ?? (_tDCh3PowerBand1SenseBorder = buttonNotChangedBrush); }
            set
            {
                _tDCh3PowerBand1SenseBorder = value;
                NotifyOfPropertyChange(() => TDCh3PowerBand1SenseBorder);
            }
        }
        /// <summary>
        /// Binding used to change the border color for WindowEnabledFFTBorder
        /// </summary>
        public Brush WindowEnabledFFTBorder
        {
            get { return _windowEnabledFFTBorder ?? (_windowEnabledFFTBorder = buttonNotChangedBrush); }
            set
            {
                _windowEnabledFFTBorder = value;
                NotifyOfPropertyChange(() => WindowEnabledFFTBorder);
            }
        }
        #endregion

        #region UI Bindings for Sense Settings (comboboxes)
        /// <summary>
        /// Combo box drop down list for sense settings for TDSampleRateCB
        /// </summary>
        public BindableCollection<int> TDSampleRateCB
        {
            get { return _tDSampleRateCB; }
            set
            {
                _tDSampleRateCB = value;
                NotifyOfPropertyChange(() => TDSampleRateCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDSampleRateCB
        /// </summary>
        public int SelectedTDRate
        {
            get { return _selectedTDRate; }
            set
            {
                _selectedTDRate = value;
                NotifyOfPropertyChange(() => SelectedTDRate);
                senseConfigFromUI.Sense.TDSampleRate = SelectedTDRate;
                ClearPowerCBValuesAndCalculateNewPowerBins(senseConfigFromUI);
                if (SelectedTDRate != senseConfig.Sense.TDSampleRate)
                {
                    TDSampleRateCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDSampleRateCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh0PosInputCB
        /// </summary>
        public BindableCollection<int> TDCh0PosInputCB
        {
            get { return _tDCh0PosInputCB; }
            set
            {
                _tDCh0PosInputCB = value;
                NotifyOfPropertyChange(() => TDCh0PosInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh0PosInputCB
        /// </summary>
        public int SelectedTDCh0PosInput
        {
            get { return _selectedTDCh0PosInput; }
            set
            {
                _selectedTDCh0PosInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh0PosInput);
                senseConfigFromUI.Sense.TimeDomains[0].Inputs[0] = SelectedTDCh0PosInput;
                if (SelectedTDCh0PosInput != senseConfig.Sense.TimeDomains[0].Inputs[0])
                {
                    TDCh0PosInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh0PosInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh0HPF1InputCB
        /// </summary>
        public BindableCollection<double> TDCh0HPF1InputCB
        {
            get { return _tDCh0HPF1InputCB; }
            set
            {
                _tDCh0HPF1InputCB = value;
                NotifyOfPropertyChange(() => TDCh0HPF1InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh0HPF1InputCB
        /// </summary>
        public double SelectedTDCh0HPF1Input
        {
            get { return _selectedTDCh0HPF1Input; }
            set
            {
                _selectedTDCh0HPF1Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh0HPF1Input);
                senseConfigFromUI.Sense.TimeDomains[0].Hpf = SelectedTDCh0HPF1Input;
                if (SelectedTDCh0HPF1Input != senseConfig.Sense.TimeDomains[0].Hpf)
                {
                    TDCh0HPF1InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh0HPF1InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh0LPF1InputCB
        /// </summary>
        public BindableCollection<int> TDCh0LPF1InputCB
        {
            get { return _tDCh0LPF1InputCB; }
            set
            {
                _tDCh0LPF1InputCB = value;
                NotifyOfPropertyChange(() => TDCh0LPF1InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh0LPF1InputCB
        /// </summary>
        public int SelectedTDCh0LPF1Input
        {
            get { return _selectedTDCh0LPF1Input; }
            set
            {
                _selectedTDCh0LPF1Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh0LPF1Input);
                senseConfigFromUI.Sense.TimeDomains[0].Lpf1 = SelectedTDCh0LPF1Input;
                if (SelectedTDCh0LPF1Input != senseConfig.Sense.TimeDomains[0].Lpf1)
                {
                    TDCh0LPF1InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh0LPF1InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh0PB0LowerInputCB
        /// </summary>
        public BindableCollection<double> TDCh0PB0LowerInputCB
        {
            get { return _tDCh0PB0LowerInputCB; }
            set
            {
                _tDCh0PB0LowerInputCB = value;
                NotifyOfPropertyChange(() => TDCh0PB0LowerInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh0PB0LowerInputCB
        /// </summary>
        public double SelectedTDCh0PB0LowerInput
        {
            get { return _selectedTDCh0PB0LowerInput; }
            set
            {
                _selectedTDCh0PB0LowerInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh0PB0LowerInput);
                senseConfigFromUI.Sense.PowerBands[0].ChannelPowerBand[0] = SelectedTDCh0PB0LowerInput;
                if (SelectedTDCh0PB0LowerInput != senseConfig.Sense.PowerBands[0].ChannelPowerBand[0])
                {
                    TDCh0PB0LowInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh0PB0LowInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh0PB1LowerInputCB
        /// </summary>
        public BindableCollection<double> TDCh0PB1LowerInputCB
        {
            get { return _tDCh0PB1LowerInputCB; }
            set
            {
                _tDCh0PB1LowerInputCB = value;
                NotifyOfPropertyChange(() => TDCh0PB1LowerInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh0PB1LowerInputCB
        /// </summary>
        public double SelectedTDCh0PB1LowerInput
        {
            get { return _selectedTDCh0PB1LowerInput; }
            set
            {
                _selectedTDCh0PB1LowerInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh0PB1LowerInput);
                senseConfigFromUI.Sense.PowerBands[1].ChannelPowerBand[0] = SelectedTDCh0PB1LowerInput;
                if (SelectedTDCh0PB1LowerInput != senseConfig.Sense.PowerBands[1].ChannelPowerBand[0])
                {
                    TDCh0PB1LowInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh0PB1LowInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh0NegInputCB
        /// </summary>
        public BindableCollection<int> TDCh0NegInputCB
        {
            get { return _tDCh0NegInputCB; }
            set
            {
                _tDCh0NegInputCB = value;
                NotifyOfPropertyChange(() => TDCh0NegInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh0NegInputCB
        /// </summary>
        public int SelectedTDCh0NegInput
        {
            get { return _selectedTDCh0NegInput; }
            set
            {
                _selectedTDCh0NegInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh0NegInput);
                senseConfigFromUI.Sense.TimeDomains[0].Inputs[1] = SelectedTDCh0NegInput;
                if (SelectedTDCh0NegInput != senseConfig.Sense.TimeDomains[0].Inputs[1])
                {
                    TDCh0NegInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh0NegInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh0LPF2InputCB
        /// </summary>
        public BindableCollection<int> TDCh0LPF2InputCB
        {
            get { return _tDCh0LPF2InputCB; }
            set
            {
                _tDCh0LPF2InputCB = value;
                NotifyOfPropertyChange(() => TDCh0LPF2InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh0LPF2InputCB
        /// </summary>
        public int SelectedTDCh0LPF2Input
        {
            get { return _selectedTDCh0LPF2Input; }
            set
            {
                _selectedTDCh0LPF2Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh0LPF2Input);
                senseConfigFromUI.Sense.TimeDomains[0].Lpf2 = SelectedTDCh0LPF2Input;
                if (SelectedTDCh0LPF2Input != senseConfig.Sense.TimeDomains[0].Lpf2)
                {
                    TDCh0LPF2InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh0LPF2InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh0PB0UpperInputCB
        /// </summary>
        public BindableCollection<double> TDCh0PB0UpperInputCB
        {
            get { return _tDCh0PB0UpperInputCB; }
            set
            {
                _tDCh0PB0UpperInputCB = value;
                NotifyOfPropertyChange(() => TDCh0PB0UpperInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh0PB0UpperInputCB
        /// </summary>
        public double SelectedTDCh0PB0UpperInput
        {
            get { return _selectedTDCh0PB0UpperInput; }
            set
            {
                _selectedTDCh0PB0UpperInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh0PB0UpperInput);
                senseConfigFromUI.Sense.PowerBands[0].ChannelPowerBand[1] = SelectedTDCh0PB0UpperInput;
                if (SelectedTDCh0PB0UpperInput != senseConfig.Sense.PowerBands[0].ChannelPowerBand[1])
                {
                    TDCh0PB0UpperInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh0PB0UpperInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh0PB1UpperInputCB
        /// </summary>
        public BindableCollection<double> TDCh0PB1UpperInputCB
        {
            get { return _tDCh0PB1UpperInputCB; }
            set
            {
                _tDCh0PB1UpperInputCB = value;
                NotifyOfPropertyChange(() => TDCh0PB1UpperInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh0PB1UpperInputCB
        /// </summary>
        public double SelectedTDCh0PB1UpperInput
        {
            get { return _selectedTDCh0PB1UpperInput; }
            set
            {
                _selectedTDCh0PB1UpperInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh0PB1UpperInput);
                senseConfigFromUI.Sense.PowerBands[1].ChannelPowerBand[1] = SelectedTDCh0PB1UpperInput;
                if (SelectedTDCh0PB1UpperInput != senseConfig.Sense.PowerBands[1].ChannelPowerBand[1])
                {
                    TDCh0PB1UpperInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh0PB1UpperInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh1PosInputCB
        /// </summary>
        public BindableCollection<int> TDCh1PosInputCB
        {
            get { return _tDCh1PosInputCB; }
            set
            {
                _tDCh1PosInputCB = value;
                NotifyOfPropertyChange(() => TDCh1PosInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh1PosInputCB
        /// </summary>
        public int SelectedTDCh1PosInput
        {
            get { return _selectedTDCh1PosInput; }
            set
            {
                _selectedTDCh1PosInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh1PosInput);
                senseConfigFromUI.Sense.TimeDomains[1].Inputs[0] = SelectedTDCh1PosInput;
                if (SelectedTDCh1PosInput != senseConfig.Sense.TimeDomains[1].Inputs[0])
                {
                    TDCh1PosInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh1PosInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh1HPF1InputCB
        /// </summary>
        public BindableCollection<double> TDCh1HPF1InputCB
        {
            get { return _tDCh1HPF1InputCB; }
            set
            {
                _tDCh1HPF1InputCB = value;
                NotifyOfPropertyChange(() => TDCh1HPF1InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh1HPF1InputCB
        /// </summary>
        public double SelectedTDCh1HPF1Input
        {
            get { return _selectedTDCh1HPF1Input; }
            set
            {
                _selectedTDCh1HPF1Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh1HPF1Input);
                senseConfigFromUI.Sense.TimeDomains[1].Hpf = SelectedTDCh1HPF1Input;
                if (SelectedTDCh1HPF1Input != senseConfig.Sense.TimeDomains[1].Hpf)
                {
                    TDCh1HPF1InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh1HPF1InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh1LPF1InputCB
        /// </summary>
        public BindableCollection<int> TDCh1LPF1InputCB
        {
            get { return _tDCh1LPF1InputCB; }
            set
            {
                _tDCh1LPF1InputCB = value;
                NotifyOfPropertyChange(() => TDCh1LPF1InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh1LPF1InputCB
        /// </summary>
        public int SelectedTDCh1LPF1Input
        {
            get { return _selectedTDCh1LPF1Input; }
            set
            {
                _selectedTDCh1LPF1Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh1LPF1Input);
                senseConfigFromUI.Sense.TimeDomains[1].Lpf1 = SelectedTDCh1LPF1Input;
                if (SelectedTDCh1LPF1Input != senseConfig.Sense.TimeDomains[1].Lpf1)
                {
                    TDCh1LPF1InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh1LPF1InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh1PB0LowerInputCB
        /// </summary>
        public BindableCollection<double> TDCh1PB0LowerInputCB
        {
            get { return _tDCh1PB0LowerInputCB; }
            set
            {
                _tDCh1PB0LowerInputCB = value;
                NotifyOfPropertyChange(() => TDCh1PB0LowerInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh1PB0LowerInputCB
        /// </summary>
        public double SelectedTDCh1PB0LowerInput
        {
            get { return _selectedTDCh1PB0LowerInput; }
            set
            {
                _selectedTDCh1PB0LowerInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh1PB0LowerInput);
                senseConfigFromUI.Sense.PowerBands[2].ChannelPowerBand[0] = SelectedTDCh1PB0LowerInput;
                if (SelectedTDCh1PB0LowerInput != senseConfig.Sense.PowerBands[2].ChannelPowerBand[0])
                {
                    TDCh1PB0LowerInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh1PB0LowerInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh1PB1LowerInputCB
        /// </summary>
        public BindableCollection<double> TDCh1PB1LowerInputCB
        {
            get { return _tDCh1PB1LowerInputCB; }
            set
            {
                _tDCh1PB1LowerInputCB = value;
                NotifyOfPropertyChange(() => TDCh1PB1LowerInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh1PB1LowerInputCB
        /// </summary>
        public double SelectedTDCh1PB1LowerInput
        {
            get { return _selectedTDCh1PB1LowerInput; }
            set
            {
                _selectedTDCh1PB1LowerInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh1PB1LowerInput);
                senseConfigFromUI.Sense.PowerBands[3].ChannelPowerBand[0] = SelectedTDCh1PB1LowerInput;
                if (SelectedTDCh1PB1LowerInput != senseConfig.Sense.PowerBands[3].ChannelPowerBand[0])
                {
                    TDCh1PB1LowerInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh1PB1LowerInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh1NegInputCB
        /// </summary>
        public BindableCollection<int> TDCh1NegInputCB
        {
            get { return _tDCh1NegInputCB; }
            set
            {
                _tDCh1NegInputCB = value;
                NotifyOfPropertyChange(() => TDCh1NegInputCB); 
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh1NegInputCB
        /// </summary>
        public int SelectedTDCh1NegInput
        {
            get { return _selectedTDCh1NegInput; }
            set
            {
                _selectedTDCh1NegInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh1NegInput);
                senseConfigFromUI.Sense.TimeDomains[1].Inputs[1] = SelectedTDCh1NegInput;
                if (SelectedTDCh1NegInput != senseConfig.Sense.TimeDomains[1].Inputs[1])
                {
                    TDCh1NegInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh1NegInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh1LPF2InputCB
        /// </summary>
        public BindableCollection<int> TDCh1LPF2InputCB
        {
            get { return _tDCh1LPF2InputCB; }
            set
            {
                _tDCh1LPF2InputCB = value;
                NotifyOfPropertyChange(() => TDCh1LPF2InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh1LPF2InputCB
        /// </summary>
        public int SelectedTDCh1LPF2Input
        {
            get { return _selectedTDCh1LPF2Input; }
            set
            {
                _selectedTDCh1LPF2Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh1LPF2Input);
                senseConfigFromUI.Sense.TimeDomains[1].Lpf2 = SelectedTDCh1LPF2Input;
                if (SelectedTDCh1LPF2Input != senseConfig.Sense.TimeDomains[1].Lpf2)
                {
                    TDCh1LPF2InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh1LPF2InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh1PB0UpperInputCB
        /// </summary>
        public BindableCollection<double> TDCh1PB0UpperInputCB
        {
            get { return _tDCh1PB0UpperInputCB; }
            set
            {
                _tDCh1PB0UpperInputCB = value;
                NotifyOfPropertyChange(() => TDCh1PB0UpperInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh1PB0UpperInputCB
        /// </summary>
        public double SelectedTDCh1PB0UpperInput
        {
            get { return _selectedTDCh1PB0UpperInput; }
            set
            {
                _selectedTDCh1PB0UpperInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh1PB0UpperInput);
                senseConfigFromUI.Sense.PowerBands[2].ChannelPowerBand[1] = SelectedTDCh1PB0UpperInput;
                if (SelectedTDCh1PB0UpperInput != senseConfig.Sense.PowerBands[2].ChannelPowerBand[1])
                {
                    TDCh1PB0UpperInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh1PB0UpperInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh1PB1UpperInputCB
        /// </summary>
        public BindableCollection<double> TDCh1PB1UpperInputCB
        {
            get { return _tDCh1PB1UpperInputCB; }
            set
            {
                _tDCh1PB1UpperInputCB = value;
                NotifyOfPropertyChange(() => TDCh1PB1UpperInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh1PB1UpperInputCB
        /// </summary>
        public double SelectedTDCh1PB1UpperInput
        {
            get { return _selectedTDCh1PB1UpperInput; }
            set
            {
                _selectedTDCh1PB1UpperInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh1PB1UpperInput);
                senseConfigFromUI.Sense.PowerBands[3].ChannelPowerBand[1] = SelectedTDCh1PB1UpperInput;
                if (SelectedTDCh1PB1UpperInput != senseConfig.Sense.PowerBands[3].ChannelPowerBand[1])
                {
                    TDCh1PB1UpperInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh1PB1UpperInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh2PosInputCB
        /// </summary>
        public BindableCollection<int> TDCh2PosInputCB
        {
            get { return _tDCh2PosInputCB; }
            set
            {
                _tDCh2PosInputCB = value;
                NotifyOfPropertyChange(() => TDCh2PosInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh2PosInputCB
        /// </summary>
        public int SelectedTDCh2PosInput
        {
            get { return _selectedTDCh2PosInput; }
            set
            {
                _selectedTDCh2PosInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh2PosInput);
                senseConfigFromUI.Sense.TimeDomains[2].Inputs[0] = SelectedTDCh2PosInput;
                if (SelectedTDCh2PosInput != senseConfig.Sense.TimeDomains[2].Inputs[0])
                {
                    TDCh2PosInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh2PosInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh02HPF1InputCB
        /// </summary>
        public BindableCollection<double> TDCh02HPF1InputCB
        {
            get { return _tDCh02HPF1InputCB; }
            set
            {
                _tDCh02HPF1InputCB = value;
                NotifyOfPropertyChange(() => TDCh02HPF1InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh02HPF1InputCB
        /// </summary>
        public double SelectedTDCh2HPF1Input
        {
            get { return _selectedTDCh2HPF1Input; }
            set
            {
                _selectedTDCh2HPF1Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh2HPF1Input);
                senseConfigFromUI.Sense.TimeDomains[2].Hpf = SelectedTDCh2HPF1Input;
                if (SelectedTDCh2HPF1Input != senseConfig.Sense.TimeDomains[2].Hpf)
                {
                    TDCh02HPF1InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh02HPF1InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh2LPF1InputCB
        /// </summary>
        public BindableCollection<int> TDCh2LPF1InputCB
        {
            get { return _tDCh2LPF1InputCB; }
            set
            {
                _tDCh2LPF1InputCB = value;
                NotifyOfPropertyChange(() => TDCh2LPF1InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh2LPF1InputCB
        /// </summary>
        public int SelectedTDCh2LPF1Input
        {
            get { return _selectedTDCh2LPF1Input; }
            set
            {
                _selectedTDCh2LPF1Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh2LPF1Input);
                senseConfigFromUI.Sense.TimeDomains[2].Lpf1 = SelectedTDCh2LPF1Input;
                if (SelectedTDCh2LPF1Input != senseConfig.Sense.TimeDomains[2].Lpf1)
                {
                    TDCh2LPF1InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh2LPF1InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh2PB0LowerInputCB
        /// </summary>
        public BindableCollection<double> TDCh2PB0LowerInputCB
        {
            get { return _tDCh2PB0LowerInputCB; }
            set
            {
                _tDCh2PB0LowerInputCB = value;
                NotifyOfPropertyChange(() => TDCh2PB0LowerInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh2PB0LowerInputCB
        /// </summary>
        public double SelectedTDCh2PB0LowerInput
        {
            get { return _selectedTDCh2PB0LowerInput; }
            set
            {
                _selectedTDCh2PB0LowerInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh2PB0LowerInput);
                senseConfigFromUI.Sense.PowerBands[4].ChannelPowerBand[0] = SelectedTDCh2PB0LowerInput;
                if (SelectedTDCh2PB0LowerInput != senseConfig.Sense.PowerBands[4].ChannelPowerBand[0])
                {
                    TDCh2PB0LowerInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh2PB0LowerInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh2PB1LowerInputCB
        /// </summary>
        public BindableCollection<double> TDCh2PB1LowerInputCB
        {
            get { return _tDCh2PB1LowerInputCB; }
            set
            {
                _tDCh2PB1LowerInputCB = value;
                NotifyOfPropertyChange(() => TDCh2PB1LowerInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh2PB1LowerInputCB
        /// </summary>
        public double SelectedTDCh2PB1LowerInput
        {
            get { return _selectedTDCh2PB1LowerInput; }
            set
            {
                _selectedTDCh2PB1LowerInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh2PB1LowerInput);
                senseConfigFromUI.Sense.PowerBands[5].ChannelPowerBand[0] = SelectedTDCh2PB1LowerInput;
                if (SelectedTDCh2PB1LowerInput != senseConfig.Sense.PowerBands[5].ChannelPowerBand[0])
                {
                    TDCh2PB1LowerInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh2PB1LowerInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for FFTShiftMultipliesCB
        /// </summary>
        public BindableCollection<uint> FFTShiftMultipliesCB
        {
            get { return _fFTShiftMultipliesCB; }
            set
            {
                _fFTShiftMultipliesCB = value;
                NotifyOfPropertyChange(() => FFTShiftMultipliesCB);
            }
        }
        /// <summary>
        /// Binding for the actual option selected in the drop down menu for FFTShiftMultipliesCB
        /// </summary>
        public uint SelectedFFTShift
        {
            get { return _selectedFFTShift; }
            set
            {
                _selectedFFTShift = value;
                NotifyOfPropertyChange(() => SelectedFFTShift);
                senseConfigFromUI.Sense.FFT.WeightMultiplies = SelectedFFTShift;
                if (SelectedFFTShift != senseConfig.Sense.FFT.WeightMultiplies)
                {
                    FFTShiftCBBorder = comboboxChangedBrush;
                }
                else
                {
                    FFTShiftCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for FFTChannelCB
        /// </summary>
        public BindableCollection<int> FFTChannelCB
        {
            get { return _fFTChannelCB; }
            set
            {
                _fFTChannelCB = value;
                NotifyOfPropertyChange(() => FFTChannelCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for FFTChannelCB
        /// </summary>
        public int SelectedFFTChannel
        {
            get { return _selectedFFTChannel; }
            set
            {
                _selectedFFTChannel = value;
                NotifyOfPropertyChange(() => SelectedFFTChannel);
                senseConfigFromUI.Sense.FFT.Channel = SelectedFFTChannel;
                if (SelectedFFTChannel != senseConfig.Sense.FFT.Channel)
                {
                    FFTChannelCBBorder = comboboxChangedBrush;
                }
                else
                {
                    FFTChannelCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh2NegInputCB
        /// </summary>
        public BindableCollection<int> TDCh2NegInputCB
        {
            get { return _tDCh2NegInputCB; }
            set
            {
                _tDCh2NegInputCB = value;
                NotifyOfPropertyChange(() => TDCh2NegInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh2NegInputCB
        /// </summary>
        public int SelectedTDCh2NegInput
        {
            get { return _selectedTDCh2NegInput; }
            set
            {
                _selectedTDCh2NegInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh2NegInput);
                senseConfigFromUI.Sense.TimeDomains[2].Inputs[1] = SelectedTDCh2NegInput;
                if (SelectedTDCh2NegInput != senseConfig.Sense.TimeDomains[2].Inputs[1])
                {
                    TDCh2NegInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh2NegInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh2LPF2InputCB
        /// </summary>
        public BindableCollection<int> TDCh2LPF2InputCB
        {
            get { return _tDCh2LPF2InputCB; }
            set
            {
                _tDCh2LPF2InputCB = value;
                NotifyOfPropertyChange(() => TDCh2LPF2InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh2LPF2InputCB
        /// </summary>
        public int SelectedTDCh2LPF2Input
        {
            get { return _selectedTDCh2LPF2Input; }
            set
            {
                _selectedTDCh2LPF2Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh2LPF2Input);
                senseConfigFromUI.Sense.TimeDomains[2].Lpf2 = SelectedTDCh2LPF2Input;
                if (SelectedTDCh2LPF2Input != senseConfig.Sense.TimeDomains[2].Lpf2)
                {
                    TDCh2LPF2InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh2LPF2InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh2PB0UpperInputCB
        /// </summary>
        public BindableCollection<double> TDCh2PB0UpperInputCB
        {
            get { return _tDCh2PB0UpperInputCB; }
            set
            {
                _tDCh2PB0UpperInputCB = value;
                NotifyOfPropertyChange(() => TDCh2PB0UpperInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh2PB0UpperInputCB
        /// </summary>
        public double SelectedTDCh2PB0UpperInput
        {
            get { return _selectedTDCh2PB0UpperInput; }
            set
            {
                _selectedTDCh2PB0UpperInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh2PB0UpperInput);
                senseConfigFromUI.Sense.PowerBands[4].ChannelPowerBand[1] = SelectedTDCh2PB0UpperInput;
                if (SelectedTDCh2PB0UpperInput != senseConfig.Sense.PowerBands[4].ChannelPowerBand[1])
                {
                    TDCh2PB0UpperInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh2PB0UpperInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh2PB1UpperInputCB
        /// </summary>
        public BindableCollection<double> TDCh2PB1UpperInputCB
        {
            get { return _tDCh2PB1UpperInputCB; }
            set
            {
                _tDCh2PB1UpperInputCB = value;
                NotifyOfPropertyChange(() => TDCh2PB1UpperInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh2PB1UpperInputCB
        /// </summary>
        public double SelectedTDCh2PB1UpperInput
        {
            get { return _selectedTDCh2PB1UpperInput; }
            set
            {
                _selectedTDCh2PB1UpperInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh2PB1UpperInput);
                senseConfigFromUI.Sense.PowerBands[5].ChannelPowerBand[1] = SelectedTDCh2PB1UpperInput;
                if (SelectedTDCh2PB1UpperInput != senseConfig.Sense.PowerBands[5].ChannelPowerBand[1])
                {
                    TDCh2PB1UpperInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh2PB1UpperInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for FFTSizeCB
        /// </summary>
        public BindableCollection<int> FFTSizeCB
        {
            get { return _fFTSizeCB; }
            set
            {
                _fFTSizeCB = value;
                NotifyOfPropertyChange(() => FFTSizeCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for FFTSizeCB
        /// </summary>
        public int SelectedFFTSize
        {
            get { return _selectedFFTSize; }
            set
            {
                _selectedFFTSize = value;
                NotifyOfPropertyChange(() => SelectedFFTSize);
                senseConfigFromUI.Sense.FFT.FftSize = SelectedFFTSize;
                ClearPowerCBValuesAndCalculateNewPowerBins(senseConfigFromUI);
                if (SelectedFFTSize != senseConfig.Sense.FFT.FftSize)
                {
                    FFTSizeCBBorder = comboboxChangedBrush;
                }
                else
                {
                    FFTSizeCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for MiscSampleRateCB
        /// </summary>
        public BindableCollection<int> MiscSampleRateCB
        {
            get { return _miscSampleRateCB; }
            set
            {
                _miscSampleRateCB = value;
                NotifyOfPropertyChange(() => MiscSampleRateCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for MiscSampleRateCB
        /// </summary>
        public int SelectedMiscSampleRate
        {
            get { return _selectedMiscSampleRate; }
            set
            {
                _selectedMiscSampleRate = value;
                NotifyOfPropertyChange(() => SelectedMiscSampleRate);
                senseConfigFromUI.Sense.Misc.StreamingRate = SelectedMiscSampleRate;
                if (SelectedMiscSampleRate != senseConfig.Sense.Misc.StreamingRate)
                {
                    MiscSampleRateCBBorder = comboboxChangedBrush;
                }
                else
                {
                    MiscSampleRateCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for FFTWindowLoadCB
        /// </summary>
        public BindableCollection<int> FFTWindowLoadCB
        {
            get { return _fFTWindowLoadCB; }
            set
            {
                _fFTWindowLoadCB = value;
                NotifyOfPropertyChange(() => FFTWindowLoadCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for FFTWindowLoadCB
        /// </summary>
        public int SelectedFFTWindowLoad
        {
            get { return _selectedFFTWindowLoad; }
            set
            {
                _selectedFFTWindowLoad = value;
                NotifyOfPropertyChange(() => SelectedFFTWindowLoad);
                senseConfigFromUI.Sense.FFT.WindowLoad = SelectedFFTWindowLoad;
                if (SelectedFFTWindowLoad != senseConfig.Sense.FFT.WindowLoad)
                {
                    FFTWindowLoadCBBorder = comboboxChangedBrush;
                }
                else
                {
                    FFTWindowLoadCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for AccSampleRateCB
        /// </summary>
        public BindableCollection<int> AccSampleRateCB
        {
            get { return _accSampleRateCB; }
            set
            {
                _accSampleRateCB = value;
                NotifyOfPropertyChange(() => AccSampleRateCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for AccSampleRateCB
        /// </summary>
        public int SelectedAccSampleRate
        {
            get { return _selectedAccSampleRate; }
            set
            {
                _selectedAccSampleRate = value;
                NotifyOfPropertyChange(() => SelectedAccSampleRate);
                senseConfigFromUI.Sense.Accelerometer.SampleRate = SelectedAccSampleRate;
                if (SelectedAccSampleRate != senseConfig.Sense.Accelerometer.SampleRate)
                {
                    AccSampleRateCBBorder = comboboxChangedBrush;
                }
                else
                {
                    AccSampleRateCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh3PosInputCB
        /// </summary>
        public BindableCollection<int> TDCh3PosInputCB
        {
            get { return _tDCh3PosInputCB; }
            set
            {
                _tDCh3PosInputCB = value;
                NotifyOfPropertyChange(() => TDCh3PosInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh3PosInputCB
        /// </summary>
        public int SelectedTDCh3PosInput
        {
            get { return _selectedTDCh3PosInput; }
            set
            {
                _selectedTDCh3PosInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh3PosInput);
                senseConfigFromUI.Sense.TimeDomains[3].Inputs[0] = SelectedTDCh3PosInput;
                if (SelectedTDCh3PosInput != senseConfig.Sense.TimeDomains[3].Inputs[0])
                {
                    TDCh3PosInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh3PosInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh3HPF1InputCB
        /// </summary>
        public BindableCollection<double> TDCh3HPF1InputCB
        {
            get { return _tDCh3HPF1InputCB; }
            set
            {
                _tDCh3HPF1InputCB = value;
                NotifyOfPropertyChange(() => TDCh3HPF1InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh3HPF1InputCB
        /// </summary>
        public double SelectedTDCh3HPF1Input
        {
            get { return _selectedTDCh3HPF1Input; }
            set
            {
                _selectedTDCh3HPF1Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh3HPF1Input);
                senseConfigFromUI.Sense.TimeDomains[3].Hpf = SelectedTDCh3HPF1Input;
                if (SelectedTDCh3HPF1Input != senseConfig.Sense.TimeDomains[3].Hpf)
                {
                    TDCh3HPF1InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh3HPF1InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh3LPF1InputCB
        /// </summary>
        public BindableCollection<int> TDCh3LPF1InputCB
        {
            get { return _tDCh3LPF1InputCB; }
            set
            {
                _tDCh3LPF1InputCB = value;
                NotifyOfPropertyChange(() => TDCh3LPF1InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh3LPF1InputCB
        /// </summary>
        public int SelectedTDCh3LPF1Input
        {
            get { return _selectedTDCh3LPF1Input; }
            set
            {
                _selectedTDCh3LPF1Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh3LPF1Input);
                senseConfigFromUI.Sense.TimeDomains[3].Lpf1 = SelectedTDCh3LPF1Input;
                if (SelectedTDCh3LPF1Input != senseConfig.Sense.TimeDomains[3].Lpf1)
                {
                    TDCh3LPF1InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh3LPF1InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh3PB0LowerInputCB
        /// </summary>
        public BindableCollection<double> TDCh3PB0LowerInputCB
        {
            get { return _tDCh3PB0LowerInputCB; }
            set
            {
                _tDCh3PB0LowerInputCB = value;
                NotifyOfPropertyChange(() => TDCh3PB0LowerInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh3PB0LowerInputCB
        /// </summary>
        public double SelectedTDCh3PB0LowerInput
        {
            get { return _selectedTDCh3PB0LowerInput; }
            set
            {
                _selectedTDCh3PB0LowerInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh3PB0LowerInput);
                senseConfigFromUI.Sense.PowerBands[6].ChannelPowerBand[0] = SelectedTDCh3PB0LowerInput;
                if (SelectedTDCh3PB0LowerInput != senseConfig.Sense.PowerBands[6].ChannelPowerBand[0])
                {
                    TDCh3PB0LowerInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh3PB0LowerInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh3PB1LowerInputCB
        /// </summary>
        public BindableCollection<double> TDCh3PB1LowerInputCB
        {
            get { return _tDCh3PB1LowerInputCB; }
            set
            {
                _tDCh3PB1LowerInputCB = value;
                NotifyOfPropertyChange(() => TDCh3PB1LowerInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh3PB1LowerInputCB
        /// </summary>
        public double SelectedTDCh3PB1LowerInput
        {
            get { return _selectedTDCh3PB1LowerInput; }
            set
            {
                _selectedTDCh3PB1LowerInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh3PB1LowerInput);
                senseConfigFromUI.Sense.PowerBands[7].ChannelPowerBand[0] = SelectedTDCh3PB1LowerInput;
                if (SelectedTDCh3PB1LowerInput != senseConfig.Sense.PowerBands[7].ChannelPowerBand[0])
                {
                    TDCh3PB1LowerInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh3PB1LowerInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh3NegInputCB
        /// </summary>
        public BindableCollection<int> TDCh3NegInputCB
        {
            get { return _tDCh3NegInputCB; }
            set
            {
                _tDCh3NegInputCB = value;
                NotifyOfPropertyChange(() => TDCh3NegInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh3NegInputCB
        /// </summary>
        public int SelectedTDCh3NegInput
        {
            get { return _selectedTDCh3NegInput; }
            set
            {
                _selectedTDCh3NegInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh3NegInput);
                senseConfigFromUI.Sense.TimeDomains[3].Inputs[1] = SelectedTDCh3NegInput;
                if (SelectedTDCh3NegInput != senseConfig.Sense.TimeDomains[3].Inputs[1])
                {
                    TDCh3NegInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh3NegInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh3LPF2InputCB
        /// </summary>
        public BindableCollection<int> TDCh3LPF2InputCB
        {
            get { return _tDCh3LPF2InputCB; }
            set
            {
                _tDCh3LPF2InputCB = value;
                NotifyOfPropertyChange(() => TDCh3LPF2InputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh3LPF2InputCB
        /// </summary>
        public int SelectedTDCh3LPF2Input
        {
            get { return _selectedTDCh3LPF2Input; }
            set
            {
                _selectedTDCh3LPF2Input = value;
                NotifyOfPropertyChange(() => SelectedTDCh3LPF2Input);
                senseConfigFromUI.Sense.TimeDomains[3].Lpf2 = SelectedTDCh3LPF2Input;
                if (SelectedTDCh3LPF2Input != senseConfig.Sense.TimeDomains[3].Lpf2)
                {
                    TDCh3LPF2InputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh3LPF2InputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh3PB0UpperInputCB
        /// </summary>
        public BindableCollection<double> TDCh3PB0UpperInputCB
        {
            get { return _tDCh3PB0UpperInputCB; }
            set
            {
                _tDCh3PB0UpperInputCB = value;
                NotifyOfPropertyChange(() => TDCh3PB0UpperInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh3PB0UpperInputCB
        /// </summary>
        public double SelectedTDCh3PB0UpperInput
        {
            get { return _selectedTDCh3PB0UpperInput; }
            set
            {
                _selectedTDCh3PB0UpperInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh3PB0UpperInput);
                senseConfigFromUI.Sense.PowerBands[6].ChannelPowerBand[1] = SelectedTDCh3PB0UpperInput;
                if (SelectedTDCh3PB0UpperInput != senseConfig.Sense.PowerBands[6].ChannelPowerBand[1])
                {
                    TDCh3PB0UpperInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh3PB0UpperInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for TDCh3PB1UpperInputCB
        /// </summary>
        public BindableCollection<double> TDCh3PB1UpperInputCB
        {
            get { return _tDCh3PB1UpperInputCB; }
            set
            {
                _tDCh3PB1UpperInputCB = value;
                NotifyOfPropertyChange(() => TDCh3PB1UpperInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for TDCh3PB1UpperInputCB
        /// </summary>
        public double SelectedTDCh3PB1UpperInput
        {
            get { return _selectedTDCh3PB1UpperInput; }
            set
            {
                _selectedTDCh3PB1UpperInput = value;
                NotifyOfPropertyChange(() => SelectedTDCh3PB1UpperInput);
                senseConfigFromUI.Sense.PowerBands[7].ChannelPowerBand[1] = SelectedTDCh3PB1UpperInput;
                if (SelectedTDCh3PB1UpperInput != senseConfig.Sense.PowerBands[7].ChannelPowerBand[1])
                {
                    TDCh3PB1UpperInputCBBorder = comboboxChangedBrush;
                }
                else
                {
                    TDCh3PB1UpperInputCBBorder = comboboxNotChangedBrush;
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for FFTLowerInputCB
        /// </summary>
        public BindableCollection<double> FFTLowerInputCB
        {
            get { return _fFTLowerInputCB; }
            set
            {
                _fFTLowerInputCB = value;
                NotifyOfPropertyChange(() => FFTLowerInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for FFTLowerInputCB
        /// </summary>
        public double SelectedFFTLowerInput
        {
            get { return _selectedFFTLowerInput; }
            set
            {
                _selectedFFTLowerInput = value;
                NotifyOfPropertyChange(() => SelectedFFTLowerInput);
                senseConfigFromUI.Sense.FFT.StreamOffsetBins = (ushort)FFTLowerInputCB.IndexOf(SelectedFFTLowerInput);
                if((FFTUpperInputCB.Count()-1) < (senseConfigFromUI.Sense.FFT.StreamOffsetBins + senseConfigFromUI.Sense.FFT.StreamSizeBins - 1))
                {
                    SelectedFFTUpperInput = FFTUpperInputCB[FFTUpperInputCB.Count() - 1];
                }
                else if(senseConfigFromUI.Sense.FFT.StreamSizeBins == 0)
                {
                    SelectedFFTUpperInput = FFTUpperInputCB[FFTUpperInputCB.Count() - 1];
                }
                else
                {
                    SelectedFFTUpperInput = FFTUpperInputCB[senseConfigFromUI.Sense.FFT.StreamOffsetBins + senseConfigFromUI.Sense.FFT.StreamSizeBins - 1];
                }
            }
        }
        /// <summary>
        /// Combo box drop down list for sense settings for FFTUpperInputCB
        /// </summary>
        public BindableCollection<double> FFTUpperInputCB
        {
            get { return _fFTUpperInputCB; }
            set
            {
                _fFTUpperInputCB = value;
                NotifyOfPropertyChange(() => FFTUpperInputCB);
            }
        }

        /// <summary>
        /// Binding for the actual option selected in the drop down menu for FFTUpperInputCB
        /// </summary>
        public double SelectedFFTUpperInput
        {
            get { return _selectedFFTUpperInput; }
            set
            {
                _selectedFFTUpperInput = value;
                NotifyOfPropertyChange(() => SelectedFFTUpperInput);
                if(SelectedFFTUpperInput != FFTUpperInputCB[FFTUpperInputCB.Count() - 1])
                {
                    senseConfigFromUI.Sense.FFT.StreamSizeBins = (ushort)(FFTUpperInputCB.IndexOf(SelectedFFTUpperInput) - FFTLowerInputCB.IndexOf(SelectedFFTLowerInput) + 1);
                }
                else
                {
                    senseConfigFromUI.Sense.FFT.StreamSizeBins = 0;
                }
            }
        }
        #endregion

        #region UI Bindings for Sense Settings (combobox border colors)
        /// <summary>
        /// Changes border color for TDSampleRateCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDSampleRateCBBorder
        {
            get { return _tDSampleRateCBBorder ?? (_tDSampleRateCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDSampleRateCBBorder = value;
                NotifyOfPropertyChange(() => TDSampleRateCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh0PosInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh0PosInputCBBorder
        {
            get { return _tDCh0PosInputCBBorder ?? (_tDCh0PosInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh0PosInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh0PosInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh0HPF1InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh0HPF1InputCBBorder
        {
            get { return _tDCh0HPF1InputCBBorder ?? (_tDCh0HPF1InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh0HPF1InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh0HPF1InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh0LPF1InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh0LPF1InputCBBorder
        {
            get { return _tDCh0LPF1InputCBBorder ?? (_tDCh0LPF1InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh0LPF1InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh0LPF1InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh0PB0LowInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh0PB0LowInputCBBorder
        {
            get { return _tDCh0PB0LowInputCBBorder ?? (_tDCh0PB0LowInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh0PB0LowInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh0PB0LowInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh0PB1LowInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh0PB1LowInputCBBorder
        {
            get { return _tDCh0PB1LowInputCBBorder ?? (_tDCh0PB1LowInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh0PB1LowInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh0PB1LowInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh0NegInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh0NegInputCBBorder
        {
            get { return _tDCh0NegInputCBBorder ?? (_tDCh0NegInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh0NegInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh0NegInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh0LPF2InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh0LPF2InputCBBorder
        {
            get { return _tDCh0LPF2InputCBBorder ?? (_tDCh0LPF2InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh0LPF2InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh0LPF2InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh0PB0UpperInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh0PB0UpperInputCBBorder
        {
            get { return _tDCh0PB0UpperInputCBBorder ?? (_tDCh0PB0UpperInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh0PB0UpperInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh0PB0UpperInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh0PB1UpperInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh0PB1UpperInputCBBorder
        {
            get { return _tDCh0PB1UpperInputCBBorder ?? (_tDCh0PB1UpperInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh0PB1UpperInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh0PB1UpperInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh1PosInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh1PosInputCBBorder
        {
            get { return _tDCh1PosInputCBBorder ?? (_tDCh1PosInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh1PosInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh1PosInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh1HPF1InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh1HPF1InputCBBorder
        {
            get { return _tDCh1HPF1InputCBBorder ?? (_tDCh1HPF1InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh1HPF1InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh1HPF1InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh1LPF1InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh1LPF1InputCBBorder
        {
            get { return _tDCh1LPF1InputCBBorder ?? (_tDCh1LPF1InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh1LPF1InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh1LPF1InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh1PB0LowerInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh1PB0LowerInputCBBorder
        {
            get { return _tDCh1PB0LowerInputCBBorder ?? (_tDCh1PB0LowerInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh1PB0LowerInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh1PB0LowerInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh1PB1LowerInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh1PB1LowerInputCBBorder
        {
            get { return _tDCh1PB1LowerInputCBBorder ?? (_tDCh1PB1LowerInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh1PB1LowerInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh1PB1LowerInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh1NegInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh1NegInputCBBorder
        {
            get { return _tDCh1NegInputCBBorder ?? (_tDCh1NegInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh1NegInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh1NegInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh1LPF2InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh1LPF2InputCBBorder
        {
            get { return _tDCh1LPF2InputCBBorder ?? (_tDCh1LPF2InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh1LPF2InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh1LPF2InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh1PB0UpperInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh1PB0UpperInputCBBorder
        {
            get { return _tDCh1PB0UpperInputCBBorder ?? (_tDCh1PB0UpperInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh1PB0UpperInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh1PB0UpperInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh1PB1UpperInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh1PB1UpperInputCBBorder
        {
            get { return _tDCh1PB1UpperInputCBBorder ?? (_tDCh1PB1UpperInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh1PB1UpperInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh1PB1UpperInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh2PosInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh2PosInputCBBorder
        {
            get { return _tDCh2PosInputCBBorder ?? (_tDCh2PosInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh2PosInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh2PosInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh02HPF1InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh02HPF1InputCBBorder
        {
            get { return _tDCh02HPF1InputCBBorder ?? (_tDCh02HPF1InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh02HPF1InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh02HPF1InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh2LPF1InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh2LPF1InputCBBorder
        {
            get { return _tDCh2LPF1InputCBBorder ?? (_tDCh2LPF1InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh2LPF1InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh2LPF1InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh2PB0LowerInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh2PB0LowerInputCBBorder
        {
            get { return _tDCh2PB0LowerInputCBBorder ?? (_tDCh2PB0LowerInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh2PB0LowerInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh2PB0LowerInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh2PB1LowerInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh2PB1LowerInputCBBorder
        {
            get { return _tDCh2PB1LowerInputCBBorder ?? (_tDCh2PB1LowerInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh2PB1LowerInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh2PB1LowerInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for FFTShiftCBBorder so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush FFTShiftCBBorder
        {
            get { return _fFTShiftCBBorder ?? (_fFTShiftCBBorder = comboboxNotChangedBrush); }
            set
            {
                _fFTShiftCBBorder = value;
                NotifyOfPropertyChange(() => FFTShiftCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for FFTChannelCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush FFTChannelCBBorder
        {
            get { return _fFTChannelCBBorder ?? (_fFTChannelCBBorder = comboboxNotChangedBrush); }
            set
            {
                _fFTChannelCBBorder = value;
                NotifyOfPropertyChange(() => FFTChannelCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh2NegInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh2NegInputCBBorder
        {
            get { return _tDCh2NegInputCBBorder ?? (_tDCh2NegInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh2NegInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh2NegInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh2LPF2InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh2LPF2InputCBBorder
        {
            get { return _tDCh2LPF2InputCBBorder ?? (_tDCh2LPF2InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh2LPF2InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh2LPF2InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh2PB0UpperInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh2PB0UpperInputCBBorder
        {
            get { return _tDCh2PB0UpperInputCBBorder ?? (_tDCh2PB0UpperInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh2PB0UpperInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh2PB0UpperInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh2PB1UpperInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh2PB1UpperInputCBBorder
        {
            get { return _tDCh2PB1UpperInputCBBorder ?? (_tDCh2PB1UpperInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh2PB1UpperInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh2PB1UpperInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for FFTSizeCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush FFTSizeCBBorder
        {
            get { return _fFTSizeCBBorder ?? (_fFTSizeCBBorder = comboboxNotChangedBrush); }
            set
            {
                _fFTSizeCBBorder = value;
                NotifyOfPropertyChange(() => FFTSizeCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for MiscSampleRateCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush MiscSampleRateCBBorder
        {
            get { return _miscSampleRateCBBorder ?? (_miscSampleRateCBBorder = comboboxNotChangedBrush); }
            set
            {
                _miscSampleRateCBBorder = value;
                NotifyOfPropertyChange(() => MiscSampleRateCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for FFTWindowLoadCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush FFTWindowLoadCBBorder
        {
            get { return _fFTWindowLoadCBBorder ?? (_fFTWindowLoadCBBorder = comboboxNotChangedBrush); }
            set
            {
                _fFTWindowLoadCBBorder = value;
                NotifyOfPropertyChange(() => FFTWindowLoadCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for AccSampleRateCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush AccSampleRateCBBorder
        {
            get { return _accSampleRateCBBorder ?? (_accSampleRateCBBorder = comboboxNotChangedBrush); }
            set
            {
                _accSampleRateCBBorder = value;
                NotifyOfPropertyChange(() => AccSampleRateCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh3PosInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh3PosInputCBBorder
        {
            get { return _tDCh3PosInputCBBorder ?? (_tDCh3PosInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh3PosInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh3PosInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh3HPF1InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh3HPF1InputCBBorder
        {
            get { return _tDCh3HPF1InputCBBorder ?? (_tDCh3HPF1InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh3HPF1InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh3HPF1InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh3LPF1InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh3LPF1InputCBBorder
        {
            get { return _tDCh3LPF1InputCBBorder ?? (_tDCh3LPF1InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh3LPF1InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh3LPF1InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh3PB0LowerInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh3PB0LowerInputCBBorder
        {
            get { return _tDCh3PB0LowerInputCBBorder ?? (_tDCh3PB0LowerInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh3PB0LowerInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh3PB0LowerInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh3PB1LowerInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh3PB1LowerInputCBBorder
        {
            get { return _tDCh3PB1LowerInputCBBorder ?? (_tDCh3PB1LowerInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh3PB1LowerInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh3PB1LowerInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh3NegInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh3NegInputCBBorder
        {
            get { return _tDCh3NegInputCBBorder ?? (_tDCh3NegInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh3NegInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh3NegInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh3LPF2InputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh3LPF2InputCBBorder
        {
            get { return _tDCh3LPF2InputCBBorder ?? (_tDCh3LPF2InputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh3LPF2InputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh3LPF2InputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh3PB0UpperInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh3PB0UpperInputCBBorder
        {
            get { return _tDCh3PB0UpperInputCBBorder ?? (_tDCh3PB0UpperInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh3PB0UpperInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh3PB0UpperInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for TDCh3PB1UpperInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush TDCh3PB1UpperInputCBBorder
        {
            get { return _tDCh3PB1UpperInputCBBorder ?? (_tDCh3PB1UpperInputCBBorder = comboboxNotChangedBrush); }
            set
            {
                _tDCh3PB1UpperInputCBBorder = value;
                NotifyOfPropertyChange(() => TDCh3PB1UpperInputCBBorder);
            }
        }
        /// <summary>
        /// Changes border color for FFTLowerInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush FFTLowerBorder
        {
            get { return _fFTLowerBorder ?? (_fFTLowerBorder = comboboxNotChangedBrush); }
            set
            {
                _fFTLowerBorder = value;
                NotifyOfPropertyChange(() => FFTLowerBorder);
            }
        }
        /// <summary>
        /// Changes border color for FFTUpperInputCB so that user knows when a change from normal settings has occurred. 
        /// </summary>
        public Brush FFTUpperBorder
        {
            get { return _fFTUpperBorder ?? (_fFTUpperBorder = comboboxNotChangedBrush); }
            set
            {
                _fFTUpperBorder = value;
                NotifyOfPropertyChange(() => FFTUpperBorder);
            }
        }
        #endregion

        #region UI Bindings for Sense Settings (misc)
        /// <summary>
        /// Binding for fft interval text box
        /// </summary>
        public string FFTIntervalTB
        {
            get { return _fFTIntervalTB; }
            set
            {
                _fFTIntervalTB = value;
                NotifyOfPropertyChange(() => FFTIntervalTB);
                if (String.IsNullOrWhiteSpace(FFTIntervalTB) || !Int32.TryParse(FFTIntervalTB, out int nothing))
                {
                    MessageBox.Show(Application.Current.MainWindow, "FFT Interval is missing or incorrect format. Please fix and try again", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
                else if (!String.IsNullOrWhiteSpace(FFTIntervalTB) && Int32.TryParse(FFTIntervalTB, out int fftIntervalResult))
                {
                    senseConfigFromUI.Sense.FFT.FftInterval = (ushort)fftIntervalResult;
                    if ((ushort)fftIntervalResult != senseConfig.Sense.FFT.FftInterval)
                    {
                        FFTIntervalTBBorder = buttonChangedBrush;
                    }
                    else
                    {
                        FFTIntervalTBBorder = buttonNotChangedBrush;
                    }
                }
            }
        }
        /// <summary>
        /// Binding used to change the border color for FFTIntervalTBBorder
        /// </summary>
        public Brush FFTIntervalTBBorder
        {
            get { return _fFTIntervalTBBorder ?? (_fFTIntervalTBBorder = buttonNotChangedBrush); }
            set
            {
                _fFTIntervalTBBorder = value;
                NotifyOfPropertyChange(() => FFTIntervalTBBorder);
            }
        }
        ///// <summary>
        ///// Binding for fft size bins text box
        ///// </summary>
        //public string FFTSizeBinsTB
        //{
        //    get { return _fFTSizeBinsTB; }
        //    set
        //    {
        //        _fFTSizeBinsTB = value;
        //        NotifyOfPropertyChange(() => FFTSizeBinsTB);
        //        //Get FFT Size Bins and set
        //        if (String.IsNullOrWhiteSpace(FFTSizeBinsTB) || !Int32.TryParse(FFTSizeBinsTB, out int nothing))
        //        {
        //            MessageBox.Show(Application.Current.MainWindow, "FFT Size Bins is missing or incorrect format. Please fix and try again", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
        //        }
        //        else if (!String.IsNullOrWhiteSpace(FFTSizeBinsTB) && Int32.TryParse(FFTSizeBinsTB, out int fftSizeBinsresult))
        //        {
        //            senseConfigFromUI.Sense.FFT.StreamSizeBins = (ushort)fftSizeBinsresult;
        //            if ((ushort)fftSizeBinsresult != senseConfig.Sense.FFT.StreamSizeBins)
        //            {
        //                FFTSizeBinsTBBorder = buttonChangedBrush;
        //            }
        //            else
        //            {
        //                FFTSizeBinsTBBorder = buttonNotChangedBrush;
        //            }
        //        }
        //    }
        //}
        ///// <summary>
        ///// Binding used to change the border color for FFTSizeBinsTBBorder
        ///// </summary>
        //public Brush FFTSizeBinsTBBorder
        //{
        //    get { return _fFTSizeBinsTBBorder ?? (_fFTSizeBinsTBBorder = buttonNotChangedBrush); }
        //    set
        //    {
        //        _fFTSizeBinsTBBorder = value;
        //        NotifyOfPropertyChange(() => FFTSizeBinsTBBorder);
        //    }
        //}
        ///// <summary>
        ///// Binding for fft offset text box
        ///// </summary>
        //public string FFTOffsetTB
        //{
        //    get { return _fFTOffsetTB; }
        //    set
        //    {
        //        _fFTOffsetTB = value;
        //        NotifyOfPropertyChange(() => FFTOffsetTB);
        //        //Get FFT Offset and set
        //        if (String.IsNullOrWhiteSpace(FFTOffsetTB) || !Int32.TryParse(FFTOffsetTB, out int nothing))
        //        {
        //            MessageBox.Show(Application.Current.MainWindow, "FFT Offset is missing or incorrect format. Please fix and try again", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
        //        }
        //        else if (!String.IsNullOrWhiteSpace(FFTOffsetTB) && Int32.TryParse(FFTOffsetTB, out int fftOffsetresult))
        //        {
        //            senseConfigFromUI.Sense.FFT.StreamOffsetBins = (ushort)fftOffsetresult;
        //            if ((ushort)fftOffsetresult != senseConfig.Sense.FFT.StreamOffsetBins)
        //            {
        //                FFTOffsetTBBorder = buttonChangedBrush;
        //            }
        //            else
        //            {
        //                FFTOffsetTBBorder = buttonNotChangedBrush;
        //            }
        //        }
        //    }
        //}
        ///// <summary>
        ///// Binding used to change the border color for FFTOffsetTBBorder
        ///// </summary>
        //public Brush FFTOffsetTBBorder
        //{
        //    get { return _fFTOffsetTBBorder ?? (_fFTOffsetTBBorder = buttonNotChangedBrush); }
        //    set
        //    {
        //        _fFTOffsetTBBorder = value;
        //        NotifyOfPropertyChange(() => FFTOffsetTBBorder);
        //    }
        //}
        /// <summary>
        /// Message to user when successful write of config file
        /// </summary>
        public string SuccessMessageInSenseSettings
        {
            get { return _successMessageInSenseSettings; }
            set
            {
                _successMessageInSenseSettings = value;
                NotifyOfPropertyChange(() => SuccessMessageInSenseSettings);
            }
        }
        #endregion

        #region Populate Combo Boxes and buttons

        private void PopulateComboBoxes(SenseModel localSense)
        {
            //Time domain sample rates
            TDSampleRateCB.Add(250);
            TDSampleRateCB.Add(500);
            TDSampleRateCB.Add(1000);
            //time domain anode cathode inputs
            for(int i = 0; i < 8; i++)
            {
                TDCh0PosInputCB.Add(i);
                TDCh0NegInputCB.Add(i);
                TDCh1PosInputCB.Add(i);
                TDCh1NegInputCB.Add(i);
            }
            for (int i = 8; i < 16; i++)
            {
                TDCh2PosInputCB.Add(i);
                TDCh2NegInputCB.Add(i);
                TDCh3PosInputCB.Add(i);
                TDCh3NegInputCB.Add(i);
            }
            //HPF
            TDCh0HPF1InputCB.Add(0.85);
            TDCh1HPF1InputCB.Add(0.85);
            TDCh02HPF1InputCB.Add(0.85);
            TDCh3HPF1InputCB.Add(0.85);
            TDCh0HPF1InputCB.Add(1.2);
            TDCh1HPF1InputCB.Add(1.2);
            TDCh02HPF1InputCB.Add(1.2);
            TDCh3HPF1InputCB.Add(1.2);
            TDCh0HPF1InputCB.Add(3.3);
            TDCh1HPF1InputCB.Add(3.3);
            TDCh02HPF1InputCB.Add(3.3);
            TDCh3HPF1InputCB.Add(3.3);
            TDCh0HPF1InputCB.Add(8.6);
            TDCh1HPF1InputCB.Add(8.6);
            TDCh02HPF1InputCB.Add(8.6);
            TDCh3HPF1InputCB.Add(8.6);
            //LPF1
            TDCh0LPF1InputCB.Add(50);
            TDCh1LPF1InputCB.Add(50);
            TDCh2LPF1InputCB.Add(50);
            TDCh3LPF1InputCB.Add(50);
            TDCh0LPF1InputCB.Add(100);
            TDCh1LPF1InputCB.Add(100);
            TDCh2LPF1InputCB.Add(100);
            TDCh3LPF1InputCB.Add(100);
            TDCh0LPF1InputCB.Add(450);
            TDCh1LPF1InputCB.Add(450);
            TDCh2LPF1InputCB.Add(450);
            TDCh3LPF1InputCB.Add(450);
            //LPF2
            TDCh0LPF2InputCB.Add(100);
            TDCh1LPF2InputCB.Add(100);
            TDCh2LPF2InputCB.Add(100);
            TDCh3LPF2InputCB.Add(100);
            TDCh0LPF2InputCB.Add(160);
            TDCh1LPF2InputCB.Add(160);
            TDCh2LPF2InputCB.Add(160);
            TDCh3LPF2InputCB.Add(160);
            TDCh0LPF2InputCB.Add(350);
            TDCh1LPF2InputCB.Add(350);
            TDCh2LPF2InputCB.Add(350);
            TDCh3LPF2InputCB.Add(350);
            TDCh0LPF2InputCB.Add(1700);
            TDCh1LPF2InputCB.Add(1700);
            TDCh2LPF2InputCB.Add(1700);
            TDCh3LPF2InputCB.Add(1700);
            //fft channel
            FFTShiftMultipliesCB.Add(0);
            FFTShiftMultipliesCB.Add(1);
            FFTShiftMultipliesCB.Add(2);
            FFTShiftMultipliesCB.Add(3);
            FFTShiftMultipliesCB.Add(4);
            FFTShiftMultipliesCB.Add(5);
            FFTShiftMultipliesCB.Add(6);
            FFTShiftMultipliesCB.Add(7);
            //fft channel
            FFTChannelCB.Add(0);
            FFTChannelCB.Add(1);
            FFTChannelCB.Add(2);
            FFTChannelCB.Add(3);
            //fft size
            FFTSizeCB.Add(64);
            FFTSizeCB.Add(256);
            FFTSizeCB.Add(1024);
            //fft window load
            FFTWindowLoadCB.Add(25);
            FFTWindowLoadCB.Add(50);
            FFTWindowLoadCB.Add(100);
            //accelerometry
            AccSampleRateCB.Add(4);
            AccSampleRateCB.Add(8);
            AccSampleRateCB.Add(16);
            AccSampleRateCB.Add(32);
            AccSampleRateCB.Add(64);
            //misc sample rate
            for(int i = 30; i <= 100; i += 10)
            {
                MiscSampleRateCB.Add(i);
            }
            PopulatePowerBins(localSense);

            //Populate text in buttons
            TimeDomainCh0SenseText = "TD CH0 ";
            TimeDomainCh1SenseText = "TD CH1 ";
            TimeDomainCh2SenseText = "TD CH2 ";
            TimeDomainCh3SenseText = "TD CH3 ";
        }

        private void PopulatePowerBins(SenseModel localSense)
        {
            //Power Bins
            CalculatePowerBins powerBins = new CalculatePowerBins(_log);
            //lower bins
            List<double> lowerBins = powerBins.GetLowerBinsInHz(localSense);
            if (lowerBins == null)
            {
                return;
            }
            List<double> upperBins = powerBins.GetUpperBinsInHz(localSense);
            if(upperBins == null)
            {
                return;
            }
            TDCh0PB0LowerInputCB.AddRange(lowerBins);
            TDCh0PB1LowerInputCB.AddRange(lowerBins);
            TDCh1PB0LowerInputCB.AddRange(lowerBins);
            TDCh1PB1LowerInputCB.AddRange(lowerBins);
            TDCh2PB0LowerInputCB.AddRange(lowerBins);
            TDCh2PB1LowerInputCB.AddRange(lowerBins);
            TDCh3PB0LowerInputCB.AddRange(lowerBins);
            TDCh3PB1LowerInputCB.AddRange(lowerBins);
            //upper bins
            TDCh0PB0UpperInputCB.AddRange(upperBins);
            TDCh0PB1UpperInputCB.AddRange(upperBins);
            TDCh1PB0UpperInputCB.AddRange(upperBins);
            TDCh1PB1UpperInputCB.AddRange(upperBins);
            TDCh2PB0UpperInputCB.AddRange(upperBins);
            TDCh2PB1UpperInputCB.AddRange(upperBins);
            TDCh3PB0UpperInputCB.AddRange(upperBins);
            TDCh3PB1UpperInputCB.AddRange(upperBins);
            lowerPowerBinActualValues = powerBins.GetLowerPowerBinActualValues(localSense);
            upperPowerBinActualValues = powerBins.GetUpperPowerBinActualValues(localSense);

            //Add upper and lower fft
            List<double> fftValues = powerBins.CalculateFFTBins(ConfigConversions.FftSizesConvert(localSense.Sense.FFT.FftSize), ConfigConversions.TDSampleRateConvert(localSense.Sense.TDSampleRate));
            if(fftValues == null)
            {
                return;
            }

            for(int indexForList = 0; indexForList < fftValues.Count(); indexForList++)
            {
                fftValues[indexForList] = Math.Round(fftValues[indexForList], 2);
            }
            FFTUpperInputCB.AddRange(fftValues);
            FFTLowerInputCB.AddRange(fftValues);
            if((FFTLowerInputCB.Count() -1) < localSense.Sense.FFT.StreamOffsetBins)
            {
                SelectedFFTLowerInput = FFTLowerInputCB[0];
            }
            else
            {
                SelectedFFTLowerInput = FFTLowerInputCB[localSense.Sense.FFT.StreamOffsetBins];
            }
            if ((localSense.Sense.FFT.StreamSizeBins != 0 || localSense.Sense.FFT.StreamOffsetBins != 0))
            {
                SelectedFFTUpperInput = FFTUpperInputCB[FFTLowerInputCB.IndexOf(SelectedFFTLowerInput) + localSense.Sense.FFT.StreamSizeBins - 1];
            }
            else
            {
                SelectedFFTUpperInput = FFTUpperInputCB[FFTUpperInputCB.Count() - 1];
            }
        }

        private void ClearPowerCBValuesAndCalculateNewPowerBins(SenseModel localSense)
        {
            //Clear lower CB
            TDCh0PB0LowerInputCB.Clear();
            TDCh0PB1LowerInputCB.Clear();
            TDCh1PB0LowerInputCB.Clear();
            TDCh1PB1LowerInputCB.Clear();
            TDCh2PB0LowerInputCB.Clear();
            TDCh2PB1LowerInputCB.Clear();
            TDCh3PB0LowerInputCB.Clear();
            TDCh3PB1LowerInputCB.Clear();
            //Clear upper CB
            TDCh0PB0UpperInputCB.Clear();
            TDCh0PB1UpperInputCB.Clear();
            TDCh1PB0UpperInputCB.Clear();
            TDCh1PB1UpperInputCB.Clear();
            TDCh2PB0UpperInputCB.Clear();
            TDCh2PB1UpperInputCB.Clear();
            TDCh3PB0UpperInputCB.Clear();
            TDCh3PB1UpperInputCB.Clear();
            //Clear upper and lower fft
            FFTUpperInputCB.Clear();
            FFTLowerInputCB.Clear();
            //Calculate new CB values
            PopulatePowerBins(localSense);
            //Select new power bands from old ones
            //lower
            SelectedTDCh0PB0LowerInput = lowerPowerBinActualValues[0];
            SelectedTDCh0PB1LowerInput = lowerPowerBinActualValues[1];
            SelectedTDCh1PB0LowerInput = lowerPowerBinActualValues[2];
            SelectedTDCh1PB1LowerInput = lowerPowerBinActualValues[3];
            SelectedTDCh2PB0LowerInput = lowerPowerBinActualValues[4];
            SelectedTDCh2PB1LowerInput = lowerPowerBinActualValues[5];
            SelectedTDCh3PB0LowerInput = lowerPowerBinActualValues[6];
            SelectedTDCh3PB1LowerInput = lowerPowerBinActualValues[7];
            //upper
            SelectedTDCh0PB0UpperInput = upperPowerBinActualValues[0];
            SelectedTDCh0PB1UpperInput = upperPowerBinActualValues[1];
            SelectedTDCh1PB0UpperInput = upperPowerBinActualValues[2];
            SelectedTDCh1PB1UpperInput = upperPowerBinActualValues[3];
            SelectedTDCh2PB0UpperInput = upperPowerBinActualValues[4];
            SelectedTDCh2PB1UpperInput = upperPowerBinActualValues[5];
            SelectedTDCh3PB0UpperInput = upperPowerBinActualValues[6];
            SelectedTDCh3PB1UpperInput = upperPowerBinActualValues[7];
        }

        private void LoadValuesFromSenseCongifToUI(SenseModel localSense)
        {
            SelectedTDRate = localSense.Sense.TDSampleRate;
            //Inputs
            SelectedTDCh0PosInput = localSense.Sense.TimeDomains[0].Inputs[0];
            SelectedTDCh0NegInput = localSense.Sense.TimeDomains[0].Inputs[1];
            SelectedTDCh1PosInput = localSense.Sense.TimeDomains[1].Inputs[0];
            SelectedTDCh1NegInput = localSense.Sense.TimeDomains[1].Inputs[1];
            SelectedTDCh2PosInput = localSense.Sense.TimeDomains[2].Inputs[0];
            SelectedTDCh2NegInput = localSense.Sense.TimeDomains[2].Inputs[1];
            SelectedTDCh3PosInput = localSense.Sense.TimeDomains[3].Inputs[0];
            SelectedTDCh3NegInput = localSense.Sense.TimeDomains[3].Inputs[1];
            //LPF1
            SelectedTDCh0LPF1Input = localSense.Sense.TimeDomains[0].Lpf1;
            SelectedTDCh1LPF1Input = localSense.Sense.TimeDomains[1].Lpf1;
            SelectedTDCh2LPF1Input = localSense.Sense.TimeDomains[2].Lpf1;
            SelectedTDCh3LPF1Input = localSense.Sense.TimeDomains[3].Lpf1;
            //LPF2
            SelectedTDCh0LPF2Input = localSense.Sense.TimeDomains[0].Lpf2;
            SelectedTDCh1LPF2Input = localSense.Sense.TimeDomains[1].Lpf2;
            SelectedTDCh2LPF2Input = localSense.Sense.TimeDomains[2].Lpf2;
            SelectedTDCh3LPF2Input = localSense.Sense.TimeDomains[3].Lpf2;
            //HPF
            SelectedTDCh0HPF1Input = localSense.Sense.TimeDomains[0].Hpf;
            SelectedTDCh1HPF1Input = localSense.Sense.TimeDomains[1].Hpf;
            SelectedTDCh2HPF1Input = localSense.Sense.TimeDomains[2].Hpf;
            SelectedTDCh3HPF1Input = localSense.Sense.TimeDomains[3].Hpf;
            //FFT
            SelectedFFTShift = localSense.Sense.FFT.WeightMultiplies;
            SelectedFFTChannel = localSense.Sense.FFT.Channel;
            SelectedFFTSize = localSense.Sense.FFT.FftSize;
            SelectedFFTWindowLoad = localSense.Sense.FFT.WindowLoad;

            //Accelerometry
            SelectedAccSampleRate = localSense.Sense.Accelerometer.SampleRate;

            //Misc
            SelectedMiscSampleRate = localSense.Sense.Misc.StreamingRate;

            //Power Bands
            //lower
            SelectedTDCh0PB0LowerInput = lowerPowerBinActualValues[0];
            SelectedTDCh0PB1LowerInput = lowerPowerBinActualValues[1];
            SelectedTDCh1PB0LowerInput = lowerPowerBinActualValues[2];
            SelectedTDCh1PB1LowerInput = lowerPowerBinActualValues[3];
            SelectedTDCh2PB0LowerInput = lowerPowerBinActualValues[4];
            SelectedTDCh2PB1LowerInput = lowerPowerBinActualValues[5];
            SelectedTDCh3PB0LowerInput = lowerPowerBinActualValues[6];
            SelectedTDCh3PB1LowerInput = lowerPowerBinActualValues[7];
            //upper
            SelectedTDCh0PB0UpperInput = upperPowerBinActualValues[0];
            SelectedTDCh0PB1UpperInput = upperPowerBinActualValues[1];
            SelectedTDCh1PB0UpperInput = upperPowerBinActualValues[2];
            SelectedTDCh1PB1UpperInput = upperPowerBinActualValues[3];
            SelectedTDCh2PB0UpperInput = upperPowerBinActualValues[4];
            SelectedTDCh2PB1UpperInput = upperPowerBinActualValues[5];
            SelectedTDCh3PB0UpperInput = upperPowerBinActualValues[6];
            SelectedTDCh3PB1UpperInput = upperPowerBinActualValues[7];

            //Textboxes
            FFTIntervalTB = localSense.Sense.FFT.FftInterval.ToString();
            //FFTSizeBinsTB = localSense.Sense.FFT.StreamSizeBins.ToString();
            //FFTOffsetTB = localSense.Sense.FFT.StreamOffsetBins.ToString();

            //Sense Buttons
            if (localSense.SenseOptions.TimeDomain)
            {
                TDSenseButtonColor = Brushes.Green;
            }
            else
            {
                TDSenseButtonColor = Brushes.White;
            }
            if (localSense.SenseOptions.FFT)
            {
                FFTSenseButtonColor = Brushes.Green;
            }
            else
            {
                FFTSenseButtonColor = Brushes.White;
            }
            if (localSense.SenseOptions.Power)
            {
                PowerSenseButtonColor = Brushes.Green;
            }
            else
            {
                PowerSenseButtonColor = Brushes.White;
            }
            if (localSense.SenseOptions.LD0)
            {
                LD0SenseButtonColor = Brushes.Green;
            }
            else
            {
                LD0SenseButtonColor = Brushes.White;
            }
            if (localSense.SenseOptions.LD1)
            {
                LD1SenseButtonColor = Brushes.Green;
            }
            else
            {
                LD1SenseButtonColor = Brushes.White;
            }
            if (localSense.SenseOptions.AdaptiveState)
            {
                AdaptiveStateSenseButtonColor = Brushes.Green;
            }
            else
            {
                AdaptiveStateSenseButtonColor = Brushes.White;
            }
            if (localSense.SenseOptions.LoopRecording)
            {
                LoopRecSenseButtonColor = Brushes.Green;
            }
            else
            {
                LoopRecSenseButtonColor = Brushes.White;
            }
            //Stream Buttons
            if (localSense.StreamEnables.TimeDomain)
            {
                TDStreamButtonColor = Brushes.Green;
            }
            else
            {
                TDStreamButtonColor = Brushes.White;
            }
            if (localSense.StreamEnables.FFT)
            {
                FFTStreamButtonColor = Brushes.Green;
            }
            else
            {
                FFTStreamButtonColor = Brushes.White;
            }
            if (localSense.StreamEnables.Power)
            {
                PowerStreamButtonColor = Brushes.Green;
            }
            else
            {
                PowerStreamButtonColor = Brushes.White;
            }
            if (localSense.StreamEnables.Accelerometry)
            {
                AccStreamButtonColor = Brushes.Green;
            }
            else
            {
                AccStreamButtonColor = Brushes.White;
            }
            if (localSense.StreamEnables.AdaptiveTherapy)
            {
                AdaptherapyStreamButtonColor = Brushes.Green;
            }
            else
            {
                AdaptherapyStreamButtonColor = Brushes.White;
            }
            if (localSense.StreamEnables.AdaptiveState)
            {
                AdaptiveStateStreamButtonColor = Brushes.Green;
            }
            else
            {
                AdaptiveStateStreamButtonColor = Brushes.White;
            }
            if (localSense.StreamEnables.EventMarker)
            {
                EventStreamButtonColor = Brushes.Green;
            }
            else
            {
                EventStreamButtonColor = Brushes.White;
            }
            if (localSense.StreamEnables.TimeStamp)
            {
                TimeStampStreamButtonColor = Brushes.Green;
            }
            else
            {
                TimeStampStreamButtonColor = Brushes.White;
            }

            //Power Band buttons
            if (localSense.Sense.PowerBands[0].IsEnabled)
            {
                TDCh0PB0SenseButtonColor = Brushes.Green;
            }
            else
            {
                TDCh0PB0SenseButtonColor = Brushes.White;
            }
            if (localSense.Sense.PowerBands[1].IsEnabled)
            {
                TDCh0PB1SenseButtonColor = Brushes.Green;
            }
            else
            {
                TDCh0PB1SenseButtonColor = Brushes.White;
            }
            if (localSense.Sense.PowerBands[2].IsEnabled)
            {
                Ch1PB0SenseButtonColor = Brushes.Green;
            }
            else
            {
                Ch1PB0SenseButtonColor = Brushes.White;
            }

            if (localSense.Sense.PowerBands[3].IsEnabled)
            {
                Ch1PB1SenseButtonColor = Brushes.Green;
            }
            else
            {
                Ch1PB1SenseButtonColor = Brushes.White;
            }
            if (localSense.Sense.PowerBands[4].IsEnabled)
            {
                TDCh2PB0ButtonColor = Brushes.Green;
            }
            else
            {
                TDCh2PB0ButtonColor = Brushes.White;
            }
            if (localSense.Sense.PowerBands[5].IsEnabled)
            {
                TDCh2PB1ButtonColor = Brushes.Green;
            }
            else
            {
                TDCh2PB1ButtonColor = Brushes.White;
            }
            if (localSense.Sense.PowerBands[6].IsEnabled)
            {
                TDCh3PB0ButtonColor = Brushes.Green;
            }
            else
            {
                TDCh3PB0ButtonColor = Brushes.White;
            }
            if (localSense.Sense.PowerBands[7].IsEnabled)
            {
                TDCh3PB1ButtonColor = Brushes.Green;
            }
            else
            {
                TDCh3PB1ButtonColor = Brushes.White;
            }
            //TD Channel Buttons
            if (localSense.Sense.TimeDomains[0].IsEnabled)
            {
                TDCh0SenseButtonColor = Brushes.Green;
            }
            else
            {
                TDCh0SenseButtonColor = Brushes.White;
            }
            if (localSense.Sense.TimeDomains[1].IsEnabled)
            {
                TDCh1SenseButtonColor = Brushes.Green;
            }
            else
            {
                TDCh1SenseButtonColor = Brushes.White;
            }
            if (localSense.Sense.TimeDomains[2].IsEnabled)
            {
                TDCh2SenseButtonColor = Brushes.Green;
            }
            else
            {
                TDCh2SenseButtonColor = Brushes.White;
            }
            if (localSense.Sense.TimeDomains[3].IsEnabled)
            {
                TDCh3SenseButtonColor = Brushes.Green;
            }
            else
            {
                TDCh3SenseButtonColor = Brushes.White;
            }
            //Other buttons
            if (!localSense.Sense.Accelerometer.SampleRateDisabled)
            {
                AccelOnOffButtonColor = Brushes.Green;
            }
            else
            {
                AccelOnOffButtonColor = Brushes.White;
            }
            if (localSense.Sense.FFT.WindowEnabled)
            {
                WindowEnabledButtonColor = Brushes.Green;
            }
            else
            {
                WindowEnabledButtonColor = Brushes.White;
            }
        }

        /// <summary>
        /// Make deep copy of object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">object to make deep copy of</param>
        /// <returns>Deep copied object returned</returns>
        public static T Clone<T> (T obj)
        {
            using(var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
        #endregion

        #region Reset Buttons, Combobox and textbox border colors
        private void ResetButtonBorderColorsToDefault()
        {
            TimeDomainCh0SenseBorder = buttonNotChangedBrush;
            TDCh0PowerBand0SenseBorder = buttonNotChangedBrush;
            TDCh0PowerBand1SenseBorder = buttonNotChangedBrush;
            TimeDomainSenseBorder = buttonNotChangedBrush;
            TimeDomainStreamBorder = buttonNotChangedBrush;
            FFTSenseBorder = buttonNotChangedBrush;
            FFTStreamBorder = buttonNotChangedBrush;
            PowerSenseBorder = buttonNotChangedBrush;
            PowerStreamBorder = buttonNotChangedBrush;
            TimeDomainCh1SenseBorder = buttonNotChangedBrush;
            LD0SenseBorder = buttonNotChangedBrush;
            AccStreamBorder = buttonNotChangedBrush;
            TDCh1PowerBand0SenseBorder = buttonNotChangedBrush;
            TDCh1PowerBand1SenseBorder = buttonNotChangedBrush;
            LD1SenseBorder = buttonNotChangedBrush;
            AdaptiveStreamBorder = buttonNotChangedBrush;
            AdaptiveStateSenseBorder = buttonNotChangedBrush;
            AdaptiveStateStreamBorder = buttonNotChangedBrush;
            LoopRecSenseBorder = buttonNotChangedBrush;
            EventMarkerStreamBorder = buttonNotChangedBrush;
            TimeStampStreamBorder = buttonNotChangedBrush;
            TimeDomainCh2SenseBorder = buttonNotChangedBrush;
            TDCh2PowerBand0SenseBorder = buttonNotChangedBrush;
            TDCh2PowerBand1SenseBorder = buttonNotChangedBrush;
            AccelOnOffBorder = buttonNotChangedBrush;
            TimeDomainCh3SenseBorder = buttonNotChangedBrush;
            TDCh3PowerBand0SenseBorder = buttonNotChangedBrush;
            TDCh3PowerBand1SenseBorder = buttonNotChangedBrush;
            WindowEnabledFFTBorder = buttonNotChangedBrush;
        }

        private void ResetComboboxBorderColorsToDefault()
        {
            TDSampleRateCBBorder = buttonNotChangedBrush;
            TDCh0PosInputCBBorder = buttonNotChangedBrush;
            TDCh0HPF1InputCBBorder = buttonNotChangedBrush;
            TDCh0LPF1InputCBBorder = buttonNotChangedBrush;
            TDCh0PB0LowInputCBBorder = buttonNotChangedBrush;
            TDCh0PB1LowInputCBBorder = buttonNotChangedBrush;
            TDCh0NegInputCBBorder = buttonNotChangedBrush;
            TDCh0LPF2InputCBBorder = buttonNotChangedBrush;
            TDCh0PB0UpperInputCBBorder = buttonNotChangedBrush;
            TDCh0PB1UpperInputCBBorder = buttonNotChangedBrush;
            TDCh1PosInputCBBorder = buttonNotChangedBrush;
            TDCh1HPF1InputCBBorder = buttonNotChangedBrush;
            TDCh1LPF1InputCBBorder = buttonNotChangedBrush;
            TDCh1PB0LowerInputCBBorder = buttonNotChangedBrush;
            TDCh1PB1LowerInputCBBorder = buttonNotChangedBrush;
            TDCh1NegInputCBBorder = buttonNotChangedBrush;
            TDCh1LPF2InputCBBorder = buttonNotChangedBrush;
            TDCh1PB0UpperInputCBBorder = buttonNotChangedBrush;
            TDCh1PB1UpperInputCBBorder = buttonNotChangedBrush;
            TDCh2PosInputCBBorder = buttonNotChangedBrush;
            TDCh02HPF1InputCBBorder = buttonNotChangedBrush;
            TDCh2LPF1InputCBBorder = buttonNotChangedBrush;
            TDCh2PB0LowerInputCBBorder = buttonNotChangedBrush;
            TDCh2PB1LowerInputCBBorder = buttonNotChangedBrush;
            FFTChannelCBBorder = buttonNotChangedBrush;
            FFTShiftCBBorder = buttonNotChangedBrush;
            TDCh2NegInputCBBorder = buttonNotChangedBrush;
            TDCh2LPF2InputCBBorder = buttonNotChangedBrush;
            TDCh2PB0UpperInputCBBorder = buttonNotChangedBrush;
            TDCh2PB1UpperInputCBBorder = buttonNotChangedBrush;
            FFTSizeCBBorder = buttonNotChangedBrush;
            MiscSampleRateCBBorder = buttonNotChangedBrush;
            FFTWindowLoadCBBorder = buttonNotChangedBrush;
            AccSampleRateCBBorder = buttonNotChangedBrush;
            TDCh3PosInputCBBorder = buttonNotChangedBrush;
            TDCh3HPF1InputCBBorder = buttonNotChangedBrush;
            TDCh3LPF1InputCBBorder = buttonNotChangedBrush;
            TDCh3PB0LowerInputCBBorder = buttonNotChangedBrush;
            TDCh3PB1LowerInputCBBorder = buttonNotChangedBrush;
            TDCh3NegInputCBBorder = buttonNotChangedBrush;
            TDCh3LPF2InputCBBorder = buttonNotChangedBrush;
            TDCh3PB0UpperInputCBBorder = buttonNotChangedBrush;
            TDCh3PB1UpperInputCBBorder = buttonNotChangedBrush;
            FFTUpperBorder = buttonNotChangedBrush;
            FFTLowerBorder = buttonNotChangedBrush;
        }
        private void ResetTextBoxBorderColorsToDefault()
        {
            FFTIntervalTBBorder = buttonNotChangedBrush;
            //FFTSizeBinsTBBorder = buttonNotChangedBrush;
            //FFTOffsetTBBorder = buttonNotChangedBrush;
        }
        #endregion

        #region Buttons for setting and loading data
        /// <summary>
        /// Reloads the data from the config file to the UI
        /// </summary>
        public void ReloadConfigButton()
        {
            SenseButtonEnabled = false;
            IsSpinnerVisible = true;
            senseConfigFromUI = Clone<SenseModel>(senseConfig);
            if (!CheckPacketLoss(senseConfigFromUI))
            {
                MessageBox.Show(Application.Current.MainWindow, "Packet Loss over maximum. Please check config file settings and adjust to lower bandwidth to avoid major packet loss.", "Warning", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            LoadValuesFromSenseCongifToUI(senseConfigFromUI);
            ClearPowerCBValuesAndCalculateNewPowerBins(senseConfigFromUI);
            IsSpinnerVisible = false;
            SenseButtonEnabled = true;
        }

        /// <summary>
        /// Saves the UI data to the config file
        /// </summary>
        public void SaveConfigButton()
        {
            SenseButtonEnabled = false;
            //Check that lower FFT is lower than upper FFT
            if(SelectedFFTLowerInput > SelectedFFTUpperInput || (SelectedFFTLowerInput != 0 && SelectedFFTLowerInput == SelectedFFTUpperInput))
            {
                ErrorMessageToUser("Per Medtronic, FFT upper needs to be higher than FFT lower. Please adjust Upper or Lower FFT and try again.");
                return;
            }
            //Check that there is no more than 64 between upper and lower indices
            if (!CheckThatUpperPowerBandLessLessThanSixtyFourFromLower(TDCh0PB0LowerInputCB.IndexOf(SelectedTDCh0PB0LowerInput), TDCh0PB0UpperInputCB.IndexOf(SelectedTDCh0PB0UpperInput)))
            {
                ErrorMessageToUser("Per Medtronic, powerbands must be closer together. Please adjust your (ch0,powerband0) upper or lower power band to create less seperation.");
                return;
            }
            if (!CheckThatUpperPowerBandLessLessThanSixtyFourFromLower(TDCh0PB1LowerInputCB.IndexOf(SelectedTDCh0PB1LowerInput), TDCh0PB1UpperInputCB.IndexOf(SelectedTDCh0PB1UpperInput)))
            {
                ErrorMessageToUser("Per Medtronic, powerbands must be closer together. Please adjust your (ch0,powerband1) upper or lower power band to create less seperation.");
                return;
            }
            if (!CheckThatUpperPowerBandLessLessThanSixtyFourFromLower(TDCh1PB0LowerInputCB.IndexOf(SelectedTDCh1PB0LowerInput), TDCh1PB0UpperInputCB.IndexOf(SelectedTDCh1PB0UpperInput)))
            {
                ErrorMessageToUser("Per Medtronic, powerbands must be closer together. Please adjust your (ch1,powerband0) upper or lower power band to create less seperation.");
                return;
            }
            if (!CheckThatUpperPowerBandLessLessThanSixtyFourFromLower(TDCh1PB1LowerInputCB.IndexOf(SelectedTDCh1PB1LowerInput), TDCh1PB1UpperInputCB.IndexOf(SelectedTDCh1PB1UpperInput)))
            {
                ErrorMessageToUser("Per Medtronic, powerbands must be closer together. Please adjust your (ch1,powerband1) upper or lower power band to create less seperation.");
                return;
            }
            if (!CheckThatUpperPowerBandLessLessThanSixtyFourFromLower(TDCh2PB0LowerInputCB.IndexOf(SelectedTDCh2PB0LowerInput), TDCh2PB0UpperInputCB.IndexOf(SelectedTDCh2PB0UpperInput)))
            {
                ErrorMessageToUser("Per Medtronic, powerbands must be closer together. Please adjust your (ch2,powerband0) upper or lower power band to create less seperation.");
                return;
            }
            if (!CheckThatUpperPowerBandLessLessThanSixtyFourFromLower(TDCh2PB1LowerInputCB.IndexOf(SelectedTDCh2PB1LowerInput), TDCh2PB1UpperInputCB.IndexOf(SelectedTDCh2PB1UpperInput)))
            {
                ErrorMessageToUser("Per Medtronic, powerbands must be closer together. Please adjust your (ch2,powerband1) upper or lower power band to create less seperation.");
                return;
            }
            if (!CheckThatUpperPowerBandLessLessThanSixtyFourFromLower(TDCh3PB0LowerInputCB.IndexOf(SelectedTDCh3PB0LowerInput), TDCh3PB0UpperInputCB.IndexOf(SelectedTDCh3PB0UpperInput)))
            {
                ErrorMessageToUser("Per Medtronic, powerbands must be closer together. Please adjust your (ch3,powerband0) upper or lower power band to create less seperation.");
                return;
            }
            if (!CheckThatUpperPowerBandLessLessThanSixtyFourFromLower(TDCh3PB1LowerInputCB.IndexOf(SelectedTDCh3PB1LowerInput), TDCh3PB1UpperInputCB.IndexOf(SelectedTDCh3PB1UpperInput)))
            {
                ErrorMessageToUser("Per Medtronic, powerbands must be closer together. Please adjust your (ch3,powerband1) upper or lower power band to create less seperation.");
                return;
            }

            //Check that fft size and upper power band in range per medtronic
            if (!CheckThatUpperPowerBandInRangePerFFT(TDCh0PB0UpperInputCB.IndexOf(SelectedTDCh0PB0UpperInput), SelectedFFTSize))
            {
                ErrorMessageToUser("Per Medtronic, upper powerband must be under certain number according to FFT size. Please lower (ch0,powerband0) upper power band or change FFT Size");
                return;
            }
            if (!CheckThatUpperPowerBandInRangePerFFT(TDCh0PB1UpperInputCB.IndexOf(SelectedTDCh0PB1UpperInput), SelectedFFTSize))
            {
                ErrorMessageToUser("Per Medtronic, upper powerband must be under certain number according to FFT size. Please lower (ch0,powerband1) upper power band or change FFT Size");
                return;
            }
            if (!CheckThatUpperPowerBandInRangePerFFT(TDCh1PB0UpperInputCB.IndexOf(SelectedTDCh1PB0UpperInput), SelectedFFTSize))
            {
                ErrorMessageToUser("Per Medtronic, upper powerband must be under certain number according to FFT size. Please lower (ch1,powerband0) upper power band or change FFT Size");
                return;
            }
            if (!CheckThatUpperPowerBandInRangePerFFT(TDCh1PB1UpperInputCB.IndexOf(SelectedTDCh1PB1UpperInput), SelectedFFTSize))
            {
                ErrorMessageToUser("Per Medtronic, upper powerband must be under certain number according to FFT size. Please lower (ch1,powerband1) upper power band or change FFT Size");
                return;
            }
            if (!CheckThatUpperPowerBandInRangePerFFT(TDCh2PB0UpperInputCB.IndexOf(SelectedTDCh2PB0UpperInput), SelectedFFTSize))
            {
                ErrorMessageToUser("Per Medtronic, upper powerband must be under certain number according to FFT size. Please lower (ch2,powerband0) upper power band or change FFT Size");
                return;
            }
            if (!CheckThatUpperPowerBandInRangePerFFT(TDCh2PB1UpperInputCB.IndexOf(SelectedTDCh2PB1UpperInput), SelectedFFTSize))
            {
                ErrorMessageToUser("Per Medtronic, upper powerband must be under certain number according to FFT size. Please lower (ch2,powerband1) upper power band or change FFT Size");
                return;
            }
            if (!CheckThatUpperPowerBandInRangePerFFT(TDCh3PB0UpperInputCB.IndexOf(SelectedTDCh3PB0UpperInput), SelectedFFTSize))
            {
                ErrorMessageToUser("Per Medtronic, upper powerband must be under certain number according to FFT size. Please lower (ch3,powerband0) upper power band or change FFT Size");
                return;
            }
            if (!CheckThatUpperPowerBandInRangePerFFT(TDCh3PB1UpperInputCB.IndexOf(SelectedTDCh3PB1UpperInput), SelectedFFTSize))
            {
                ErrorMessageToUser("Per Medtronic, upper powerband must be under certain number according to FFT size. Please lower (ch3,powerband1) upper power band or change FFT Size");
                return;
            }

            //check power value lower inputs less than upper inputs
            if (SelectedTDCh0PB0LowerInput >= SelectedTDCh0PB0UpperInput)
            {
                ErrorMessageToUser("TD Channel 0, Power Band 0 Lower input must be less than upper input");
                return;
            }
            if(SelectedTDCh0PB1LowerInput >= SelectedTDCh0PB1UpperInput)
            {
                ErrorMessageToUser("TD Channel 0, Power Band 1 Lower input must be less than upper input");
                return;
            }
            if(SelectedTDCh1PB0LowerInput >= SelectedTDCh1PB0UpperInput)
            {
                ErrorMessageToUser("TD Channel 1, Power Band 0 Lower input must be less than upper input");
                return;
            }
            if(SelectedTDCh1PB1LowerInput >= SelectedTDCh1PB1UpperInput)
            {
                ErrorMessageToUser("TD Channel 1, Power Band 1 Lower input must be less than upper input");
                return;
            }
            if(SelectedTDCh2PB0LowerInput >= SelectedTDCh2PB0UpperInput)
            {
                ErrorMessageToUser("TD Channel 2, Power Band 0 Lower input must be less than upper input");
                return;
            }
            if(SelectedTDCh2PB1LowerInput >= SelectedTDCh2PB1UpperInput)
            {
                ErrorMessageToUser("TD Channel 2, Power Band 1 Lower input must be less than upper input");
                return;
            }
            if(SelectedTDCh3PB0LowerInput >= SelectedTDCh3PB0UpperInput)
            {
                ErrorMessageToUser("TD Channel 3, Power Band 0 Lower input must be less than upper input");
                return;
            }
            if(SelectedTDCh3PB1LowerInput >= SelectedTDCh3PB1UpperInput)
            {
                ErrorMessageToUser("TD Channel 3, Power Band 1 Lower input must be less than upper input");
                return;
            }

            if (!CheckPacketLoss(senseConfigFromUI))
            {
                ErrorMessageToUser("Packet Loss over maximum. Please check config file settings and adjust to lower bandwidth to avoid major packet loss.");
                return;
            }
            //save to file
            JSONService jSONService = new JSONService(_log);
            if (jSONService.WriteSenseConfigToFile(senseConfigFromUI, senseFileLocation))
            {
                Messages.Insert(0, DateTime.Now + ":: Success writing Sense Config to file");
            }
            else
            {
                ErrorMessageToUser("Error writing config to file. Please try again");
                return;
            }
            senseConfig = jSONService.GetSenseModelFromFile(senseFileLocation);
            if (senseConfig == null)
            {
                ErrorMessageToUser("Sense Config could not be loaded. Please check that it exists or has the correct format");
                return;
            }
            AutoClosingMessageBox.Show("Save was successful", "Success!", 1500);
            SuccessMessageInSenseSettings = "Success! " + DateTime.Now;
            ResetButtonBorderColorsToDefault();
            ResetComboboxBorderColorsToDefault();
            ResetTextBoxBorderColorsToDefault();
            SenseButtonEnabled = true;
            IsSpinnerVisible = false;
        }

        /// <summary>
        /// Saves the config to file, turns stream off if already on and then starts stream with saved config file
        /// </summary>
        /// <returns></returns>
        public async Task SaveConfigStartStreamButton()
        {
            SenseButtonEnabled = false;
            IsSpinnerVisible = true;
            await Task.Run(() => SaveConfigButton());
            await Task.Run(() => SenseStreamOffButton());
            await Task.Run(() => SenseStreamOnButton());
            IsSpinnerVisible = false;
            SenseButtonEnabled = true;
        }

        private void ErrorMessageToUser(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            SenseButtonEnabled = true;
            IsSpinnerVisible = false;
        }
        #endregion

        #region Error checks
        /// <summary>
        /// Messagebox that closes automatically after a certain number of seconds in milliseconds
        /// </summary>
        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                using (_timeoutTimer)
                    MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }
        /// <summary>
        /// Checks to make sure that the upper value minus the lower value is under 64 as per medtronic rules.
        /// </summary>
        /// <param name="lower">Lower power band value</param>
        /// <param name="upper">Upper power band value</param>
        /// <returns>true if within range or false if not</returns>
        private bool CheckThatUpperPowerBandLessLessThanSixtyFourFromLower(int lower, int upper)
        {
            bool isWithinRange = false;
            //if within range, return true.
            if ((upper - lower) < 64)
            {
                isWithinRange = true;
            }
            return isWithinRange;
        }
        /// <summary>
        /// Makes sure that upper power band is under certain number according to FFT size.
        /// </summary>
        /// <param name="upper">Upper Power band index</param>
        /// <param name="FFT">FFT currently being used</param>
        /// <returns>True if powerband within range or false if not within range</returns>
        private bool CheckThatUpperPowerBandInRangePerFFT(int upper, int FFT)
        {
            bool isTrue = false;
            switch (FFT)
            {
                case 64:
                    if (upper < 32) { isTrue = true; }
                    break;
                case 256:
                    if (upper < 128) { isTrue = true; }
                    break;
                case 1024:
                    if (upper < 512) { isTrue = true; }
                    break;
                default:
                    isTrue = false;
                    break;
            }
            return isTrue;
        }
        #endregion
    }
}
