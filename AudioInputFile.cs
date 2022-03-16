using System;

namespace BasharTools.AudiobookCreator
{
    internal class AudioInputFile
    {
        public string DirectoryName { get; internal set; }
        public string Filename { get; internal set; }
        public string FullPath { get; internal set; }
        public string FilenameWithPath { get; internal set; }
        public long FileSize { get; internal set; }
        public DateTime FileModificationTime { get; internal set; }
        public string Name { get; internal set; }
    }
}