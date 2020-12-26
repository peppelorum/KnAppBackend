using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Piranha.Data.EF;
 
namespace Data {

    public class UploadItem
    {
        public double Long { 
            get;
            set;
        }
        
        public double Lat { 
            get;
            set;
        }
        public IFormFile Image { get; set; }
        
    }
}