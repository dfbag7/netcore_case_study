using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp.Models
{
    internal class Config
    {
        public string? ConnectionString { get; set; }
        public int Parallelism { get; set; }
    }
}
