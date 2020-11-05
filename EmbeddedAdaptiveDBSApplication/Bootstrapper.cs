/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using Caliburn.Micro;
using EmbeddedAdaptiveDBSApplication.Models;
using EmbeddedAdaptiveDBSApplication.ViewModels;
using SciChart.Charting.Visuals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoUpdaterDotNET;

/// <summary>
/// Caliburn Micro MVVM class for starting MVVM program
/// </summary>
namespace EmbeddedAdaptiveDBSApplication
{
    class Bootstrapper : BootstrapperBase
    {
        private string sciChartLicenseFileLocation = @"C:\AdaptiveDBS\sciChartLicense.txt";
        public Bootstrapper()
        {
            LogManager.GetLog = type => new Log4netLogger(type);
            Initialize();
        }

        /// <summary>
        /// Decides what happens on startup of program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            //Get the scichart license from file
            string sciChartLicense = null;
            try
            {
                using (System.IO.StreamReader sr = new StreamReader(sciChartLicenseFileLocation))
                {
                    sciChartLicense = sr.ReadToEnd();
                }
                SciChartSurface.SetRuntimeLicenseKey(sciChartLicense);
            }
            catch
            {
               MessageBox.Show(@"Error Importing SciChart License. Charts will not work without it. Please be sure it is located in the directory C:\AdaptiveDBS\sciChartLicense.txt. Proceed if you don't need to use the charts.", "Warning", MessageBoxButton.OK, MessageBoxImage.Hand);
            }

            //Get the file containing the url where the xml file is stored. 
            //Check xml file to see if the version has increased.  If so, download update and update application.
            string urlForAutoUpdateContainingXML = null;
            try
            {
                using (StreamReader fileContainingAutoUpdateURL = new StreamReader(@"C:\AdaptiveDBS\url.txt"))
                {
                    urlForAutoUpdateContainingXML = fileContainingAutoUpdateURL.ReadToEnd();
                }
            }
            catch
            {
            }
            //make sure url is not null, not empty and in correct format. If it isn't, then skip the auto-update code and log.
            //otherwise start update download
            if (!string.IsNullOrEmpty(urlForAutoUpdateContainingXML))
            {
                if (Uri.IsWellFormedUriString(urlForAutoUpdateContainingXML, UriKind.Absolute))
                {
                    AutoUpdater.Mandatory = true;
                    AutoUpdater.UpdateMode = Mode.Forced;
                    try
                    {
                        AutoUpdater.Start(urlForAutoUpdateContainingXML);
                    }
                    catch
                    {
                    }
                }
            }

            DisplayRootViewFor<MainViewModel>();
        }
        /// <summary>
        /// Decides what happens on window closing of program
        /// Calls the WindowClosing method in MainViewModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnExit(object sender, EventArgs e)
        {
            MainViewModel.WindowClosing();
        }
    }
}
