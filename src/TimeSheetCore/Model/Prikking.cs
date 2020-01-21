using System;

namespace TimeSheetCore.Model
{
    public class Prikking
    {
        public DateTime Datum { get; internal set; }
        public string Prikklok { get; internal set; }
        public string Zone { get; internal set; }
    }
}
