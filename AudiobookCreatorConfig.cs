using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BasharTools.AudiobookCreator
{
    public class AudiobookCreatorConfig
    {
        public string DataDirectory { get; internal set; }
        public string TempDirectory { get; internal set; }
        public string[] AudioFileExtensions { get; }
        public string TimecodesFile { get; }
        public FFMPEGConfig FFMPEG { get; }
        public MetadataConfig Metadata { get; }

        public AudiobookCreatorConfig(IConfigurationRoot config)
        {
            DataDirectory = config["DataDirectory"];
            TempDirectory = config["TempDirectory"];
            AudioFileExtensions = config.GetSection("AudioFileExtensions")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            TimecodesFile = config["TimecodesFile"];

            FFMPEG = new FFMPEGConfig()
            {
                Conversion = config["FFMPEG:Conversion"],
                Compilation = config["FFMPEG:Compilation"]
            };

            Metadata = new MetadataConfig()
            {
                Cover = config["Metadata:Cover"],
                Title = config["Metadata:Title"],
                Artist = config["Metadata:Artist"],
                Album = config["Metadata:Album"],
                Year = config["Metadata:Year"],
                Genre = config["Metadata:Genre"],
                Description = config["Metadata:Description"],
                Language = config["Metadata:Language"]
            };            
        }        
    }

    public class FFMPEGConfig
    {
        public string Conversion { get; set; }
        public string Compilation { get; set; }
    }

    public class MetadataConfig
    {
        public string Cover { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
    }
}