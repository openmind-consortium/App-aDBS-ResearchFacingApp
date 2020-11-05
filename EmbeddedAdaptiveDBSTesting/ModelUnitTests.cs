using EmbeddedAdaptiveDBSApplication.Models;
using Medtronic.NeuroStim.Olympus.DataTypes.Sensing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedAdaptiveDBSTesting
{
    [TestFixture]
    public class ModelUnitTests
    {
        #region Json String variables
        public static readonly string senseJsonToTestThatIsCorrect = @"{
          'eventType': {
            'comment': 'event name to use to log to .json files',
            'type': 'Home streaming'
          },
          'Mode': 3,
          'Ratio': 32,
          'SenseOptions': {
            'comment': 'lets you set what to sense',
            'TimeDomain': true,
            'FFT': true,
            'Power': true,
            'LD0': true,
            'LD1': false,
            'AdaptiveState': false,
            'LoopRecording': false,
            'Unused': false
          },
          'StreamEnables': {
            'comment': 'lets you set what to stream',
            'TimeDomain': true,
            'FFT': false,
            'Power': true,
            'Accelerometry': true,
            'AdaptiveTherapy': true,
            'AdaptiveState': true,
            'EventMarker': false,
            'TimeStamp': true
          },
          'Sense': {
            'commentTDChannelDefinitions': 'No more than two channels can be on a single bore. When configuring, channels on first bore will always be first. Can only have sampling rates of: 250, 500, and 1000 (Hz) or disable it by setting IsDisabled to true',
            'commentFilters': 'Stage one low pass(Lpf1) can only be: 450, 100, or 50 (Hz). Stage two low pass(Lpf2) can only be: 1700, 350, 160, or 100 (Hz). High pass(Hpf) can only be: 0.85, 1.2, 3.3, or 8.6 (Hz), Inputs[ anode(positive), cathode(negative) ]',
            'TDSampleRate': 250,
	        'TimeDomains': [
	          {
		        'ch0': 'STN',
		        'IsEnabled': true,
		        'Hpf': 0.85,
		        'Lpf1': 100,
		        'Lpf2': 100,
		        'Inputs': [ 0, 2 ]
	          },
	          {
		        'ch1': 'STN',
		        'IsEnabled': true,
		        'Hpf': 0.85,
		        'Lpf1': 100,
		        'Lpf2': 100,
		        'Inputs': [ 1, 3 ]
	          },
	          {
		        'ch2': 'M1',
		        'IsEnabled': true,
		        'Hpf': 0.85,
		        'Lpf1': 450,
		        'Lpf2': 1700,
		        'Inputs': [ 8, 10 ]
	          },
	          {
		        'ch3': 'M1',
		        'IsEnabled': true,
		        'Hpf': 0.85,
		        'Lpf1': 450,
		        'Lpf2': 1700,
		        'Inputs': [ 9, 11 ]
	          }
	        ],
            'FFT': {
              'commentFFTParameters': 'FFT Size can be: 64, 256, or 1024 samples, Hanning window load can be: 25, 50, or 100 (%)',
              'Channel': 1,
              'FftSize': 1024,
              'FftInterval': 100,
              'WindowLoad': 100,
              'StreamSizeBins': 0,
              'StreamOffsetBins': 0,
	          'WindowEnabled': true
            },
	        'commentPower': 'each power band can be set from 0-250hz, 2 pos bands per channel. Ex: ChNPowerBandN:[lower, upper]',
            'PowerBands': [
		        {
			        'comment': 'Channel: 0 PowerBand: 0',
			        'ChannelPowerBand': [ 18, 22 ],
			        'IsEnabled': true
		        },
		        {
			        'comment': 'Channel: 0 PowerBand: 1',
			        'ChannelPowerBand': [ 10, 12 ],
			        'IsEnabled': true
		        },
		        {	
			        'comment': 'Channel: 1 PowerBand: 0',
			        'ChannelPowerBand': [ 6, 7 ],
			        'IsEnabled': false
		        },
		        {
			        'comment': 'Channel: 1 PowerBand: 1',
			        'ChannelPowerBand': [ 6, 7 ],
			        'IsEnabled': false
		        },
		        {
			        'comment': 'Channel: 2 PowerBand: 0',
			        'ChannelPowerBand': [ 6, 7 ],
			        'IsEnabled': true
		        },
		        {
			        'comment': 'Channel: 2 PowerBand: 1',
			        'ChannelPowerBand': [ 6, 7 ],
			        'IsEnabled': false
		        },
		        {
			        'comment': 'Channel: 3 PowerBand: 0',
			        'ChannelPowerBand': [ 6, 7 ],
			        'IsEnabled': false
		        },
		        {
			        'comment': 'Channel: 3 PowerBand: 1',
			        'ChannelPowerBand': [ 6, 7 ],
			        'IsEnabled': false
		        }
            ],
            'Accelerometer': {
              'commentAcc': 'Can be 4,8,16,32,64Hz or set SampleRateDisabled to true for disabled',
              'SampleRateDisabled': false,
              'SampleRate': 64
            }
          }
        }";

        public static readonly string adaptiveJsonToTestThatIsCorrect = @"{
	            'comment': 'config file for the adaptive DBS configurations',
	            'Detection':{
		            'LD0': {
			            'comment': 'Detection settings for LD0',
			            'B0': 1000,
			            'B1': 3000,
			            'UpdateRate': 5,
			            'OnsetDuration': 0,
			            'TerminationDuration': 0,
			            'HoldOffOnStartupTime': 1,
			            'StateChangeBlankingUponStateChange': 2,
			            'FractionalFixedPointValue': 1,
			            'DualThreshold': true,
			            'Inputs': {
				            'Ch0Band0': true,
				            'Ch0Band1': false,
				            'Ch1Band0': false,
				            'Ch1Band1': false,
				            'Ch2Band0': false,
				            'Ch2Band1': false,
				            'Ch3Band0': false,
				            'Ch3Band1': false

                        }
            }
	            },
	            'Adaptive': {
		            'Program0': {
			            'RiseTimes': 6500,
			            'FallTimes': 6500,
			            'RateTargetInHz': 130,
			            'State0AmpInMilliamps': 0.5,
			            'State1AmpInMilliamps': 2.5,
			            'State2AmpInMilliamps': 4.5
		            }
	            }	
            }";
        public static string reportJsonToTestThatIsCorrect = @"{
	        'comment': 'List of medications and symptoms for Report Window to user',
	        'Medications': [
		        'medicine1',
		        'medicine2'
	        ],
	        'Symptoms': [
		        'Feeling something',
		        'Balance and posture',
		        'Slowness of Movement',
		        'Dyskinesia',
		        'Dystonia',
		        'Rigidity (Muscle Stiffness)',
		        'Speech',
		        'Tremor',
		        'Mania',
		        'Inappropriate Sleepiness'
	        ]
    }";
        #endregion
        #region CalculatePowerBins Unit Tests  
        [Test]
        public void GetLowerIndex_TestCorrectBins_ReturnOne()
        {
            FftSizes fftSize = FftSizes.Size0064;
            TdSampleRates timeRate = TdSampleRates.Sample0250Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetLowerIndex(2), Is.EqualTo(1));
        }

        [Test]
        public void GetLowerIndex_TestDifferentInputs_ReturnTen()
        {
            FftSizes fftSize = FftSizes.Size0256;
            TdSampleRates timeRate = TdSampleRates.Sample0500Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetLowerIndex(18), Is.EqualTo(10));
        }

        [Test]
        public void GetLowerIndex_TestDifferentInputsReturnSeventeen()
        {
            FftSizes fftSize = FftSizes.Size1024;
            TdSampleRates timeRate = TdSampleRates.Sample1000Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetLowerIndex(16), Is.EqualTo(17));
        }

        [Test]
        public void GetLowerIndex_TestInputTooLow_ReturnZero()
        {
            FftSizes fftSize = FftSizes.Size1024;
            TdSampleRates timeRate = TdSampleRates.Sample1000Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetLowerIndex(-2), Is.EqualTo(0));
        }

        [Test]
        public void GetLowerIndex_TestInputTooHigh_ReturnFiveHundredEleven()
        {
            FftSizes fftSize = FftSizes.Size1024;
            TdSampleRates timeRate = TdSampleRates.Sample1000Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetLowerIndex(100000), Is.EqualTo(511));
        }

        [Test]
        public void GetUpperIndex_TestCorrectBins_ReturnThree()
        {
            FftSizes fftSize = FftSizes.Size0064;
            TdSampleRates timeRate = TdSampleRates.Sample0250Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetUpperIndex(14), Is.EqualTo(3));
        }

        [Test]
        public void GetUpperIndex_TestDifferentInputs_ReturnNine()
        {
            FftSizes fftSize = FftSizes.Size0256;
            TdSampleRates timeRate = TdSampleRates.Sample0500Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetUpperIndex(18), Is.EqualTo(9));
        }

        [Test]
        public void GetUpperIndex_TestDifferentInputs_ReturnOneHunThirtyOne()
        {
            FftSizes fftSize = FftSizes.Size1024;
            TdSampleRates timeRate = TdSampleRates.Sample1000Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetUpperIndex(128), Is.EqualTo(131));
        }

        [Test]
        public void GetUpperIndex_TestInputTooLow_ReturnZero()
        {
            FftSizes fftSize = FftSizes.Size1024;
            TdSampleRates timeRate = TdSampleRates.Sample1000Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetUpperIndex(-2), Is.EqualTo(0));
        }

        [Test]
        public void GetUpperIndex_TestInputTooHigh_ReturnFiveHundredEleven()
        {
            FftSizes fftSize = FftSizes.Size1024;
            TdSampleRates timeRate = TdSampleRates.Sample1000Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            Assert.That(bins.GetUpperIndex(100000), Is.EqualTo(511));
        }

        [Test]
        public void ActualUpperPowerValue_TestEstimatedValue_ReturnSixtySeven()
        {
            FftSizes fftSize = FftSizes.Size1024;
            TdSampleRates timeRate = TdSampleRates.Sample1000Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            bins.GetUpperIndex(68);
            Assert.That(bins.ActualUpperPowerValue, Is.EqualTo(67.87109375));
        }

        [Test]
        public void ActualUpperPowerValue_TestDifferentValue_ReturnThirtyEight()
        {
            FftSizes fftSize = FftSizes.Size0256;
            TdSampleRates timeRate = TdSampleRates.Sample0500Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            bins.GetUpperIndex(38);
            Assert.That(bins.ActualUpperPowerValue, Is.EqualTo(38.0859375));
        }

        [Test]
        public void ActualUpperPowerValue_TestDifferentValue_ReturnOneTwentyThree()
        {
            FftSizes fftSize = FftSizes.Size0064;
            TdSampleRates timeRate = TdSampleRates.Sample0250Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            bins.GetUpperIndex(150);
            Assert.That(bins.ActualUpperPowerValue, Is.EqualTo(123.046875));
        }

        [Test]
        public void ActualLowerPowerValue_TestEstimatedValue_ReturnNinetyTwo()
        {
            FftSizes fftSize = FftSizes.Size1024;
            TdSampleRates timeRate = TdSampleRates.Sample1000Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            bins.GetLowerIndex(92);
            Assert.That(bins.ActualLowerPowerValue, Is.EqualTo(92.28515625));
        }

        [Test]
        public void ActualLowerPowerValue_TestDifferentValue_ReturnTwenty()
        {
            FftSizes fftSize = FftSizes.Size0256;
            TdSampleRates timeRate = TdSampleRates.Sample0500Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            bins.GetLowerIndex(20);
            Assert.That(bins.ActualLowerPowerValue, Is.EqualTo(20.5078125));
        }

        [Test]
        public void ActualLowerPowerValue_TestDifferentValue_ReturnOneNineteen()
        {
            FftSizes fftSize = FftSizes.Size0064;
            TdSampleRates timeRate = TdSampleRates.Sample0250Hz;
            EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins bins = new EmbeddedAdaptiveDBSApplication.Models.CalculatePowerBins(fftSize, timeRate);
            bins.GetLowerIndex(130);
            Assert.That(bins.ActualLowerPowerValue, Is.EqualTo(119.140625));
        }
        #endregion

        #region JSONService Unit Tests 
        [Test]
        public void ValidateJSON_TestValidation_ReturnTrue()
        {
            EmbeddedAdaptiveDBSApplication.Models.JSONService jsonService = new EmbeddedAdaptiveDBSApplication.Models.JSONService();
            EmbeddedAdaptiveDBSApplication.Models.SchemaModel model = new EmbeddedAdaptiveDBSApplication.Models.SchemaModel();
            bool result = jsonService.ValidateJSON(senseJsonToTestThatIsCorrect, model.GetSenseSchema());
            Assert.That(result, Is.EqualTo(true));
        }
        [Test]
        public void ValidateJSON_TestIncorrectJsonFormat_ReturnFalse()
        {
            EmbeddedAdaptiveDBSApplication.Models.JSONService jsonService = new EmbeddedAdaptiveDBSApplication.Models.JSONService();
            EmbeddedAdaptiveDBSApplication.Models.SchemaModel model = new EmbeddedAdaptiveDBSApplication.Models.SchemaModel();
            bool result = jsonService.ValidateJSON("incorrect json format", model.GetSenseSchema());
            Assert.That(result, Is.EqualTo(false));
        }
        [Test]
        public void ValidateJSON_TestIncorrectSchema_ReturnFalse()
        {
            EmbeddedAdaptiveDBSApplication.Models.JSONService jsonService = new EmbeddedAdaptiveDBSApplication.Models.JSONService();
            EmbeddedAdaptiveDBSApplication.Models.SchemaModel model = new EmbeddedAdaptiveDBSApplication.Models.SchemaModel();
            bool result = jsonService.ValidateJSON(senseJsonToTestThatIsCorrect, "some incorrect schema");
            Assert.That(result, Is.EqualTo(false));
        }
        [Test]
        public void GetSenseConfig_TestCorrectConversion_ReturnSameModel()
        {
            SenseModel testModel = new SenseModel();
            testModel.Mode = 3;
            testModel.Ratio = 32;
            testModel.SenseOptions = new SenseOptions();
            testModel.SenseOptions.TimeDomain = true;
            testModel.SenseOptions.FFT = true;
            testModel.SenseOptions.Power = true;
            testModel.SenseOptions.LD0 = true;
            testModel.SenseOptions.LD1 = false;
            testModel.SenseOptions.AdaptiveState = false;
            testModel.SenseOptions.LoopRecording = false;
            testModel.SenseOptions.Unused = false;
            testModel.StreamEnables = new StreamEnables();
            testModel.StreamEnables.TimeDomain = true;
            testModel.StreamEnables.FFT = false;
            testModel.StreamEnables.Power = true;
            testModel.StreamEnables.Accelerometry = true;
            testModel.StreamEnables.AdaptiveTherapy = true;
            testModel.StreamEnables.AdaptiveState = true;
            testModel.StreamEnables.EventMarker = false;
            testModel.StreamEnables.TimeStamp = true;
            testModel.Sense = new Sense();
            testModel.Sense.TDSampleRate = 250;
            testModel.Sense.TimeDomains = new List<TimeDomain>
            {
                new TimeDomain { IsEnabled = true, Hpf = 0.85, Lpf1 = 100, Lpf2 = 100, Inputs = new List<int>{ 0, 2 } },
                new TimeDomain{IsEnabled = true, Hpf=0.85, Lpf1=100, Lpf2=100, Inputs=new List<int>{1,3 } },
                new TimeDomain{IsEnabled = true, Hpf=0.85, Lpf1=450, Lpf2=1700, Inputs=new List<int>{8,10 } },
                new TimeDomain{IsEnabled = true, Hpf=0.85, Lpf1=450, Lpf2=1700, Inputs=new List<int>{9,11 } }
            };
            testModel.Sense.FFT = new FFT();
            testModel.Sense.FFT.Channel = 1;
            testModel.Sense.FFT.FftSize = 1024;
            testModel.Sense.FFT.FftInterval = 100;
            testModel.Sense.FFT.WindowLoad = 100;
            testModel.Sense.FFT.StreamOffsetBins = 0;
            testModel.Sense.FFT.StreamSizeBins = 0;
            testModel.Sense.FFT.WindowEnabled = true;
            testModel.Sense.PowerBands = new List<Power>
            {
                new Power{ChannelPowerBand=new List<ushort>{18,22 }, IsEnabled=true },
                new Power{ChannelPowerBand = new List<ushort> { 10,12 }, IsEnabled=true },
                new Power{ChannelPowerBand=new List<ushort>{6,7 }, IsEnabled=false },
                new Power{ChannelPowerBand=new List<ushort>{6,7 }, IsEnabled=false },
                new Power{ChannelPowerBand=new List<ushort>{6,7 }, IsEnabled=true },
                new Power{ChannelPowerBand=new List<ushort>{6,7 }, IsEnabled=false },
                new Power{ChannelPowerBand=new List<ushort>{6,7 }, IsEnabled=false },
                new Power{ChannelPowerBand=new List<ushort>{6,7 }, IsEnabled=false },
            };
            testModel.Sense.Accelerometer = new Accelerometer();
            testModel.Sense.Accelerometer.SampleRate = 64;
            testModel.Sense.Accelerometer.SampleRateDisabled = false;
            EmbeddedAdaptiveDBSApplication.Models.JSONService jsonService = new EmbeddedAdaptiveDBSApplication.Models.JSONService();
            SenseModel model = jsonService.GetSenseConfig(senseJsonToTestThatIsCorrect);
            Assert.AreEqual(model.Mode, testModel.Mode);
            Assert.AreEqual(model.Ratio, testModel.Ratio);
            Assert.AreEqual(model.SenseOptions.LD0, testModel.SenseOptions.LD0);
            Assert.AreEqual(model.StreamEnables.FFT, testModel.StreamEnables.FFT);
            Assert.AreEqual(model.Sense.TDSampleRate, testModel.Sense.TDSampleRate);
            Assert.AreEqual(model.Sense.TimeDomains[0].Hpf, testModel.Sense.TimeDomains[0].Hpf);
            Assert.AreEqual(model.Sense.FFT.FftSize, testModel.Sense.FFT.FftSize);
            Assert.AreEqual(model.Sense.PowerBands[0].ChannelPowerBand[0], testModel.Sense.PowerBands[0].ChannelPowerBand[0]);
        }
        [Test]
        public void GetAdaptiveConfig_TestCorrectConversion_ReturnSameModel()
        {
            AdaptiveModel testModel = new AdaptiveModel();
            testModel.Detection = new Detection();
            testModel.Detection.LD0 = new LD0();
            testModel.Detection.LD0.B0 = 1000;
            testModel.Detection.LD0.B1 = 3000;
            testModel.Detection.LD0.TerminationDuration = 0;
            testModel.Detection.LD0.Inputs = new Inputs();
            testModel.Detection.LD0.Inputs.Ch0Band0 = true;
            testModel.Adaptive = new Adaptive();
            testModel.Adaptive.Program0 = new Program0();
            testModel.Adaptive.Program0.RiseTimes = 6500;
            testModel.Adaptive.Program0.FallTimes = 6500;
            testModel.Adaptive.Program0.State0AmpInMilliamps = 0.5;
            testModel.Adaptive.Program0.State2AmpInMilliamps = 4.5;
            EmbeddedAdaptiveDBSApplication.Models.JSONService jsonService = new EmbeddedAdaptiveDBSApplication.Models.JSONService();
            AdaptiveModel model = jsonService.GetAdaptiveConfig(adaptiveJsonToTestThatIsCorrect);
            Assert.AreEqual(model.Detection.LD0.B0, testModel.Detection.LD0.B0);
            Assert.AreEqual(model.Detection.LD0.B1, testModel.Detection.LD0.B1);
            Assert.AreEqual(model.Detection.LD0.TerminationDuration, testModel.Detection.LD0.TerminationDuration);
            Assert.AreEqual(model.Detection.LD0.Inputs.Ch0Band0, testModel.Detection.LD0.Inputs.Ch0Band0);
            Assert.AreEqual(model.Adaptive.Program0.RiseTimes, testModel.Adaptive.Program0.RiseTimes);
            Assert.AreEqual(model.Adaptive.Program0.FallTimes, testModel.Adaptive.Program0.FallTimes);
            Assert.AreEqual(model.Adaptive.Program0.State0AmpInMilliamps, testModel.Adaptive.Program0.State0AmpInMilliamps);
            Assert.AreEqual(model.Adaptive.Program0.State2AmpInMilliamps, testModel.Adaptive.Program0.State2AmpInMilliamps);
        }
        [Test]
        public void GetReportConfig_TestCorrectConversion_ReturnSameModel()
        {
            ReportModel testModel = new ReportModel();
            testModel.Medications = new List<string>{ "medicine1", "medicine2" };
            testModel.Symptoms = new List<string>(new string[] { "Feeling something",
                                                                "Balance and posture",
                                                                "Slowness of Movement",
                                                                "Dyskinesia",});
            EmbeddedAdaptiveDBSApplication.Models.JSONService jsonService = new EmbeddedAdaptiveDBSApplication.Models.JSONService();
            ReportModel model = jsonService.GetReportConfig(reportJsonToTestThatIsCorrect);
            Assert.AreEqual(model.Medications[0], testModel.Medications[0]);
            Assert.AreEqual(model.Medications[1], testModel.Medications[1]);
            Assert.AreEqual(model.Symptoms[0], testModel.Symptoms[0]);
            Assert.AreEqual(model.Symptoms[1], testModel.Symptoms[1]);
        }
        #endregion

        #region ConfigConversions Unit Tests
        [Test]
        public void PowerBandEnablesConvert_TestCorrectReturn_ReturnCorrectPowerBandCombo()
        {
            EmbeddedAdaptiveDBSApplication.Models.JSONService jsonService = new EmbeddedAdaptiveDBSApplication.Models.JSONService();
            SenseModel model = jsonService.GetSenseConfig(senseJsonToTestThatIsCorrect);
            BandEnables result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.PowerBandEnablesConvert(model);
            Assert.AreEqual(result, BandEnables.Ch0Band0Enabled | BandEnables.Ch0Band1Enabled | BandEnables.Ch2Band0Enabled);
        }
        #region Detection Inputs unit tests
        [Test]
        public void DetectionInputsConvert_TestCorrectReturn_ReturnAllTrue()
        {
            DetectionInputs result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.DetectionInputsConvert(true, true, true, true, true, true, true, true);
            Assert.AreEqual(result, DetectionInputs.Ch0Band0 | DetectionInputs.Ch0Band1 | DetectionInputs.Ch1Band0 | DetectionInputs.Ch1Band1 | DetectionInputs.Ch2Band0 | DetectionInputs.Ch2Band1 | DetectionInputs.Ch3Band0 | DetectionInputs.Ch3Band1);
        }
        [Test]
        public void DetectionInputsConvert_TestCorrectReturn_ReturnAllFalse()
        {
            DetectionInputs result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.DetectionInputsConvert(false, false, false, false, false, false, false, false);
            Assert.AreEqual(result, DetectionInputs.None);
        }
        [Test]
        public void DetectionInputsConvert_TestCorrectReturn_ReturnVariation()
        {
            DetectionInputs result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.DetectionInputsConvert(true, true, false, false, true, true, false, true);
            Assert.AreEqual(result, DetectionInputs.Ch0Band0 | DetectionInputs.Ch0Band1 | DetectionInputs.Ch2Band0 | DetectionInputs.Ch2Band1 | DetectionInputs.Ch3Band1);
        }
        #endregion

        #region TD sample rates Unit test
        [Test]
        public void TDSampleRateConvert_Test250_ReturnsTdSampleRatesSample0250Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TDSampleRateConvert(250);

            Assert.That(result.Equals(TdSampleRates.Sample0250Hz));
        }
        [Test]
        public void TDSampleRateConvert_Test500_ReturnsTdSampleRatesSample0500Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TDSampleRateConvert(500);

            Assert.That(result.Equals(TdSampleRates.Sample0500Hz));
        }
        [Test]
        public void TDSampleRateConvert_Test1000_ReturnsTdSampleRatesSample1000Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TDSampleRateConvert(1000);

            Assert.That(result.Equals(TdSampleRates.Sample1000Hz));
        }
        #endregion

        #region TD hpfs Unit test
        [Test]
        public void TdHpfsConvert_Test85_ReturnsTdHpfsHpf0_85Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdHpfsConvert(0.85);

            Assert.That(result.Equals(TdHpfs.Hpf0_85Hz));
        }
        [Test]
        public void TdHpfsConvert_Test85_ReturnsTdHpfsHpf1_2Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdHpfsConvert(1.2);

            Assert.That(result.Equals(TdHpfs.Hpf1_2Hz));
        }
        [Test]
        public void TdHpfsConvert_Test85_ReturnsTdHpfsHp3_3Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdHpfsConvert(3.3);

            Assert.That(result.Equals(TdHpfs.Hpf3_3Hz));
        }
        [Test]
        public void TdHpfsConvert_Test85_ReturnsTdHpfsHpf8_6Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdHpfsConvert(8.6);

            Assert.That(result.Equals(TdHpfs.Hpf8_6Hz));
        }
        #endregion

        #region Td lpf1 unit test
        [Test]
        public void TdLpfStage1Convert_Test50_ReturnsTdLpfStage1_Lpf50Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdLpfStage1Convert(50);

            Assert.That(result.Equals(TdLpfStage1.Lpf50Hz));
        }
        [Test]
        public void TdLpfStage1Convert_Test100_ReturnsTdLpfStage1_Lpf100Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdLpfStage1Convert(100);

            Assert.That(result.Equals(TdLpfStage1.Lpf100Hz));
        }
        [Test]
        public void TdLpfStage1Convert_Test450_ReturnsTdLpfStage1_Lpf450Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdLpfStage1Convert(450);

            Assert.That(result.Equals(TdLpfStage1.Lpf450Hz));
        }
        #endregion

        #region Td lpf2 unit test
        [Test]
        public void TdLpfStage2Convert_Test100_ReturnsTdLpfStage2_Lpf100Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdLpfStage2Convert(100);

            Assert.That(result.Equals(TdLpfStage2.Lpf100Hz));
        }
        [Test]
        public void TdLpfStage2Convert_Test160_ReturnsTdLpfStage2_Lpf160Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdLpfStage2Convert(160);

            Assert.That(result.Equals(TdLpfStage2.Lpf160Hz));
        }
        [Test]
        public void TdLpfStage2Convert_Test350_ReturnsTdLpfStage2_Lpf350Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdLpfStage2Convert(350);

            Assert.That(result.Equals(TdLpfStage2.Lpf350Hz));
        }
        [Test]
        public void TdLpfStage2Convert_Test1700_ReturnsTdLpfStage2_Lpf1700Hz()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdLpfStage2Convert(1700);

            Assert.That(result.Equals(TdLpfStage2.Lpf1700Hz));
        }
        #endregion

        #region Td Mux Inputs unit test
        [Test]
        public void TdMuxInputsConvert_Test0_ReturnsTdMuxInputs_Mux0()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(0);

            Assert.That(result.Equals(TdMuxInputs.Mux0));
        }
        [Test]
        public void TdMuxInputsConvert_Test8_ReturnsTdMuxInputs_Mux0()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(8);

            Assert.That(result.Equals(TdMuxInputs.Mux0));
        }
        [Test]
        public void TdMuxInputsConvert_Test1_ReturnsTdMuxInputs_Mux1()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(1);

            Assert.That(result.Equals(TdMuxInputs.Mux1));
        }
        [Test]
        public void TdMuxInputsConvert_Test9_ReturnsTdMuxInputs_Mux1()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(9);

            Assert.That(result.Equals(TdMuxInputs.Mux1));
        }
        [Test]
        public void TdMuxInputsConvert_Test2_ReturnsTdMuxInputs_Mux2()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(2);

            Assert.That(result.Equals(TdMuxInputs.Mux2));
        }
        [Test]
        public void TdMuxInputsConvert_Test10_ReturnsTdMuxInputs_Mux2()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(10);

            Assert.That(result.Equals(TdMuxInputs.Mux2));
        }
        [Test]
        public void TdMuxInputsConvert_Test3_ReturnsTdMuxInputs_Mux3()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(3);

            Assert.That(result.Equals(TdMuxInputs.Mux3));
        }
        [Test]
        public void TdMuxInputsConvert_Test11_ReturnsTdMuxInputs_Mux3()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(11);

            Assert.That(result.Equals(TdMuxInputs.Mux3));
        }
        [Test]
        public void TdMuxInputsConvert_Test4_ReturnsTdMuxInputs_Mux4()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(4);

            Assert.That(result.Equals(TdMuxInputs.Mux4));
        }
        [Test]
        public void TdMuxInputsConvert_Test12_ReturnsTdMuxInputs_Mux4()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(12);

            Assert.That(result.Equals(TdMuxInputs.Mux4));
        }
        [Test]
        public void TdMuxInputsConvert_Test5_ReturnsTdMuxInputs_Mux5()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(5);

            Assert.That(result.Equals(TdMuxInputs.Mux5));
        }
        [Test]
        public void TdMuxInputsConvert_Test13_ReturnsTdMuxInputs_Mux5()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(13);

            Assert.That(result.Equals(TdMuxInputs.Mux5));
        }
        [Test]
        public void TdMuxInputsConvert_Test6_ReturnsTdMuxInputs_Mux6()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(6);

            Assert.That(result.Equals(TdMuxInputs.Mux6));
        }
        [Test]
        public void TdMuxInputsConvert_Test14_ReturnsTdMuxInputs_Mux6()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(14);

            Assert.That(result.Equals(TdMuxInputs.Mux6));
        }
        [Test]
        public void TdMuxInputsConvert_Test7_ReturnsTdMuxInputs_Mux7()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(7);

            Assert.That(result.Equals(TdMuxInputs.Mux7));
        }
        [Test]
        public void TdMuxInputsConvert_Test15_ReturnsTdMuxInputs_Mux7()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TdMuxInputsConvert(15);

            Assert.That(result.Equals(TdMuxInputs.Mux7));
        }
        #endregion

        #region FFT sizes unit test
        [Test]
        public void FftSizesConvert_Test64_ReturnsFftSizes_Size0064()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.FftSizesConvert(64);

            Assert.That(result.Equals(FftSizes.Size0064));
        }
        [Test]
        public void FftSizesConvert_Test256_ReturnsFftSizes_Size0256()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.FftSizesConvert(256);

            Assert.That(result.Equals(FftSizes.Size0256));
        }
        [Test]
        public void FftSizesConvert_Test1024_ReturnsFftSizes_Size1024()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.FftSizesConvert(1024);

            Assert.That(result.Equals(FftSizes.Size1024));
        }
        #endregion

        #region FFT window load unit test
        [Test]
        public void FftWindowAutoLoadsConvert_Test25_ReturnsFftWindowAutoLoads_Hann25()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.FftWindowAutoLoadsConvert(25);

            Assert.That(result.Equals(FftWindowAutoLoads.Hann25));
        }
        [Test]
        public void FftWindowAutoLoadsConvert_Test50_ReturnsFftWindowAutoLoads_Hann50()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.FftWindowAutoLoadsConvert(50);

            Assert.That(result.Equals(FftWindowAutoLoads.Hann50));
        }
        [Test]
        public void FftWindowAutoLoadsConvert_Test100_ReturnsFftWindowAutoLoads_Hann100()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.FftWindowAutoLoadsConvert(100);

            Assert.That(result.Equals(FftWindowAutoLoads.Hann100));
        }
        #endregion

        #region Acceleration sample rate unit test
        [Test]
        public void AccelSampleRateConvert_Test4_ReturnsAccelSampleRate_Sample04()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.AccelSampleRateConvert(4, false);

            Assert.That(result.Equals(AccelSampleRate.Sample04));
        }
        [Test]
        public void AccelSampleRateConvert_Test8_ReturnsAccelSampleRate_Sample08()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.AccelSampleRateConvert(8, false);

            Assert.That(result.Equals(AccelSampleRate.Sample08));
        }
        [Test]
        public void AccelSampleRateConvert_Test16_ReturnsAccelSampleRate_Sample16()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.AccelSampleRateConvert(16, false);

            Assert.That(result.Equals(AccelSampleRate.Sample16));
        }
        [Test]
        public void AccelSampleRateConvert_Test32_ReturnsAccelSampleRate_Sample32()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.AccelSampleRateConvert(32, false);

            Assert.That(result.Equals(AccelSampleRate.Sample32));
        }
        [Test]
        public void AccelSampleRateConvert_Test64_ReturnsAccelSampleRate_Sample64()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.AccelSampleRateConvert(64, false);

            Assert.That(result.Equals(AccelSampleRate.Sample64));
        }
        [Test]
        public void AccelSampleRateConvert_TestTrue_ReturnsAccelSampleRate_Sample04()
        {
            var result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.AccelSampleRateConvert(0, true);

            Assert.That(result.Equals(AccelSampleRate.Disabled));
        }
        #endregion

        #region TD sensestatesconvert unit test
        [Test]
        public void TDSenseStatesConvert_TestProperConversion_ReturnAllTrue()
        {
            SenseStates result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TDSenseStatesConvert(true, true, true, true, true, true, true, true);
            Assert.AreEqual(result, SenseStates.LfpSense | SenseStates.Fft | SenseStates.Power | SenseStates.DetectionLd0 | SenseStates.DetectionLd1 | SenseStates.AdaptiveStim | SenseStates.LoopRecording | SenseStates.Unused08);
        }
        [Test]
        public void TDSenseStatesConvert_TestProperConversion_ReturnAllFalse()
        {
            SenseStates result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TDSenseStatesConvert(false, false, false, false, false, false, false, false);
            Assert.AreEqual(result, SenseStates.None);
        }
        [Test]
        public void TDSenseStatesConvert_TestProperConversion_ReturnVariation()
        {
            SenseStates result = EmbeddedAdaptiveDBSApplication.Models.ConfigConversions.TDSenseStatesConvert(false, true, false, true, false, true, false, true);
            Assert.AreEqual(result, SenseStates.Fft | SenseStates.DetectionLd0 | SenseStates.AdaptiveStim | SenseStates.Unused08);
        }
        #endregion
        #endregion

    }

}
