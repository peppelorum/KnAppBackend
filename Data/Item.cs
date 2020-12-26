using System;
using System.Collections.Generic;
using Piranha.Data.EF;

using Newtonsoft.Json;
using NetTopologySuite.Geometries;
// 
namespace Data {

    // [JsonObject(IsReference = true)]
    public class Item
    {
        public Guid Id { get; set; }
        public DateTime? Created { get; set; }

        public double? Long { 
            get {
                return Location.Coordinate.X;
            }
        }
        
        public double? Lat { 
            get {
                return Location.Coordinate.Y;
            }
        }

        [JsonIgnore]
        public Point Location { get; set; }

        public String Image { get; set; }

    }

}