using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_5.Models
{
    public class PartitionProblemModel
    {
        public string FileName { get; set; }
        public PartitionSet Partitions { get; set; }
        public double TotalDifference { get; set; }
        public double MillisecondsToRun { get; set; }
    }
}