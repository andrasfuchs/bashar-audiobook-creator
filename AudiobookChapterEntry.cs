using CsvHelper.Configuration;
using System;
using System.Globalization;

namespace BasharTools.AudiobookCreator
{
    internal class AudiobookChapterEntry
    {
        public float Index { get; set; }
        public bool IsIncluded { get; set; }
        public string Filename { get; set; }
        public string StartTime { get; set; }
        public string StopTime { get; set; }
        public string Chapter { get; set; }
        
        public double StartTimeInSeconds
        {
            get
            {
                return (TimeSpan.Parse(StartTime)).TotalSeconds;
            }
        }
        public double DurationInSeconds
        {
            get
            {
                return (TimeSpan.Parse(StopTime) - TimeSpan.Parse(StartTime)).TotalSeconds;
            }
        }

        public AudioInputFile AudioInputFile { get; set; }
        public string OutputFilename { get; internal set; }
        public string ExceptionMessage { get; internal set; }

        public override string ToString()
        {
            return $"{this.Filename} ({this.StartTime}-{this.StopTime})";
        }
    }

    internal sealed class AudioTimecodeMap : ClassMap<AudiobookChapterEntry>
    {
        public AudioTimecodeMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.StartTimeInSeconds).Ignore();
            Map(m => m.DurationInSeconds).Ignore();
            Map(m => m.AudioInputFile).Ignore();
            Map(m => m.OutputFilename).Ignore();
            Map(m => m.ExceptionMessage).Ignore();
        }
    }
}