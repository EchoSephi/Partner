using System;

namespace Bill.Model
{
    public class dtoBills
    {
        public string Account { get; set; }
        public string Password { get; set; }
        public string UrlAddress { get; set; }
        public Guid Cases_Guid { get; set; }
        public string Cases_Name { get; set; }
    }
}