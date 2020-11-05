/********************
Copyright Notice
Copyright © 2019, The Regents of the University of California
All rights reserved.

Please see the file LICENSE in this distribution for license terms.
**********************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedAdaptiveDBSApplication.Models
{
    /// <summary>
    /// Model for montage config file data
    /// </summary>
    public class MontageModel
    {
        /// <summary>
        /// List of montage file names and times to run
        /// </summary>
        public List<MontageFile> MontageFiles { get; set; }
    }

    /// <summary>
    /// Contains the file name and time to run for montage config files
    /// </summary>
    public class MontageFile
    {
        /// <summary>
        /// File name for config file
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// Number value for the amount of seconds to run this config file
        /// </summary>
        public int TimeToRunInSeconds { get; set; }
    }
}
