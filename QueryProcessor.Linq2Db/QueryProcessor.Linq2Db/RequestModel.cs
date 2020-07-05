using System;
using System.Collections.Generic;

namespace QueryProcessor.Linq2Db {
    public class RequestModel {
        public Sort Sort { get; set; }
        public Paging Paging { get; set; }
        public List<Filter> Filters { get; set; }
    }

    public class Sort {
        public string Field { get; set; }
        public Direction Direction { get; set; }
    }

    public class Paging {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class Filter {
        public string Field { get; set; }
        public FilterType FilterType { get; set; }
        public string Value { get; set; }
    }
    
    
    public enum Direction {
      Asc = 1,
      Desc = 2
    }

    public enum FilterType {
        Contains = 1,
        From = 2,
        To = 3,
        Equals = 4
    }
}