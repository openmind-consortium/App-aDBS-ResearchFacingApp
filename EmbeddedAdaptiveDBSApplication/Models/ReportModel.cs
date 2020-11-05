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
    /// Model for Report Config JSON File
    /// This class is used to convert the json file into class data
    /// </summary>
    public class ReportModel
    {
        /// <summary>
        /// Medications
        /// </summary>
        public List<string> Medications { get; set; }
        /// <summary>
        /// Symptoms
        /// </summary>
        public List<string> Symptoms { get; set; }
    }
}
