using System;
using System.Collections.Generic;

namespace Data {
    public class Token
    {
        public Guid Id { get; set; }
        public Guid APIToken { get; set; }
        public Guid User { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}