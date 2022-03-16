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
        public MP4BOXConfig MP4BOX { get; }

        public AudiobookCreatorConfig(IConfigurationRoot config)
        {
            DataDirectory = config["DataDirectory"];
            TempDirectory = config["TempDirectory"];
            AudioFileExtensions = config.GetSection("AudioFileExtensions")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            TimecodesFile = config["TimecodesFile"];

            FFMPEG = new FFMPEGConfig()
            {
                Parameters = config["FFMPEG:Parameters"],
            };

            MP4BOX = new MP4BOXConfig()
            {
                Parameters = config["MP4BOX:Parameters"]
            };            
        }        
    }

    public class FFMPEGConfig
    {
        public string Parameters { get; set; }
    }

    public class MP4BOXConfig
    {
        public string Parameters { get; set; }
    }
}