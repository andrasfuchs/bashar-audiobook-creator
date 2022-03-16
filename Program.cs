using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace BasharTools.AudiobookCreator
{
    class Program
    {
        static string versionString = "v0.1.1 (2022-03-15)";

        static ILogger logger;

        static Mutex mutex = new Mutex(true, "{8962cf9b-47bf-4a5c-b1fb-5b004cf31c5d}");

        static string[] audioFrameworks = { "dshow", "alsa", "openal", "oss" };
        static List<string> ffmpegAudioNames = new List<string>();

        static string ffmpegAudioRecordingExtension = "wav";
        static string ffmpegAudioRecordingParameters = "-c:a pcm_s16le -ac 1 -ar 44100";
        static string ffmpegAudioEncodingExtension = "mp3";
        static string ffmpegAudioEncodingParameters = "-c:a mp3 -ac 1 -ar 44100 -q:a 9 -filter:a loudnorm";

        static DriveInfo spaceCheckDrive;

        static IConfigurationRoot configuration;
        static AudiobookCreatorConfig config;

        static void Main(string[] args)
        {
            // Build configuration
            configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false)
            .Build();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", Enum.Parse<LogLevel>(configuration["Logging:LogLevel:Default"]))
                    .AddFilter("System", Enum.Parse<LogLevel>(configuration["Logging:LogLevel:Default"]))
                    .AddFilter("Bashar.AudiobookCreator.Program", Enum.Parse<LogLevel>(configuration["Logging:LogLevel:Bashar.AudiobookCreator.Program"]))
                    .AddConfiguration(configuration)
                    .AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = false;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss.ffff ";
                        options.UseUtcTimestamp = false;
                    });
            });
            logger = loggerFactory.CreateLogger<Program>();

            Console.WriteLine($"Bashar Audiobook Creator {versionString}");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("bashar-audiobook-creator <options> <output M4B file>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("--data <data directory>                Set the date directory where all the audio files are");
            Console.WriteLine("--temp <temp directory>                Set the temp directory where the temporarily generated files are stored");
            Console.WriteLine();

            logger.LogInformation($"Bashar Audiobook Creator {versionString}");
            logger.LogInformation($"(The current time is {DateTime.Now:yyyy-MM-dd HH:mm:ss})");
            try
            {
                configuration.Reload();
                config = new AudiobookCreatorConfig(configuration);
            }
            catch (Exception ex)
            {
                logger.LogError($"There was a problem with the configuration file. {ex.Message}");
                return;
            }

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                logger.LogError($"An instance of this app is already running on this machine.");
                return;
            }

            Console.CancelKeyPress += Console_CancelKeyPress;

            if (args.Length == 0)
            {
                logger.LogError($"You must define an output file name.");
                return;
            }

            if (args.Length > 1)
            {
                if (args[0] == "--data")
                {
                    config.DataDirectory = args[1];
                }

                if (args[0] == "--temp")
                {
                    config.TempDirectory = args[1];
                }
            }

            if (!Directory.Exists(config.DataDirectory))
            {
                logger.LogError($"Data folder '{config.DataDirectory}' does not exist.");
                return;
            }

            try
            {
                if (!Directory.Exists(config.TempDirectory))
                {
                    Directory.CreateDirectory(config.TempDirectory);
                }
            }
            catch
            {
                logger.LogError($"Couldn't create temp folder '{config.TempDirectory}'.");
                return;
            }

            if (!Directory.Exists(config.TempDirectory))
            {
                logger.LogError($"Temp folder '{config.TempDirectory}' does not exist.");
                return;
            }

            string outputFilename = args[^1];

            AudiobookChapterEntry[] audioTimecodes = ReadTimeCodeEntries(config.TimecodesFile);
            GenerateAudiobook(audioTimecodes, config.DataDirectory, outputFilename);
        }

        private static AudiobookChapterEntry[] ReadTimeCodeEntries(string timecodeFilename)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };

            using (var csvReader = new StreamReader(timecodeFilename))
            using (var csv = new CsvReader(csvReader, csvConfig))
            {
                csv.Context.RegisterClassMap<AudioTimecodeMap>();
                return csv.GetRecords<AudiobookChapterEntry>().ToArray();
            }
        }

        /*
        private static void DataAcquisition_SamplesReceived(object sender, SamplesReceivedEventArgs e)
        {
            if (config.AudioRecording.Enabled)
            {
                Task.Run(() =>
                {
                    TimeSpan tp = captureTime.Add(new TimeSpan(0, 0, (int)config.MP4BOX.IntervalSeconds * 2)) - DateTime.Now;

                    string pathToAudioFile = GeneratePathToFile(DateTime.Now);

                    string recFilename = AppendDataDir($"{pathToAudioFile}.{ffmpegAudioRecordingExtension}");

                    string ffmpegAudioFramework = config.AudioRecording.PreferredDevice.Split("/")[0];
                    string ffmpegAudioDevice = config.AudioRecording.PreferredDevice.Split("/")[1];
                    string audioRecordingCommandLine = $"-f {ffmpegAudioFramework} -ac 1 -i audio=\"{ffmpegAudioDevice}\" {ffmpegAudioRecordingParameters} -t {tp:mm':'ss'.'fff} \"{recFilename}\"";
                    logger.LogDebug($"ffmpeg {audioRecordingCommandLine}");

                    try
                    {
                        FFmpeg.Conversions.New().Start(audioRecordingCommandLine)
                            .ContinueWith((Task<IConversionResult> cr) =>
                            {
                                try
                                {
                                    Stopwatch sw = Stopwatch.StartNew();
                                    sw.Restart();

                                    float waveRms = 0;
                                    float wavePeak = 0;
                                    using (var waveStream = new FileStream(recFilename, FileMode.Open))
                                    {
                                        DiscreteSignal recordedAudio = new WaveFile(waveStream).Signals[0];
                                        waveRms = recordedAudio.Rms();
                                        wavePeak = Math.Max(-recordedAudio.Samples.Min(), recordedAudio.Samples.Max()) * 100;
                                    }

                                    //if (waveRms >= config.AudioRecording.SilenceThreshold)
                                    if (wavePeak >= config.AudioRecording.SilenceThreshold)
                                    {
                                        string finalFilename = AppendDataDir($"{pathToAudioFile}_{wavePeak.ToString("00.0")}%.{ffmpegAudioEncodingExtension}");
                                        string audioEncodingCommandLine = $"-i {recFilename} {ffmpegAudioEncodingParameters} \"{finalFilename}\"";

                                        logger.LogDebug($"ffmpeg {audioEncodingCommandLine}");
                                        FFmpeg.Conversions.New().Start(audioEncodingCommandLine).Wait();
                                    }

                                    File.Delete(recFilename);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogWarning($"There was an error while encoding audio.");
                                    logger.LogDebug($"{ex.Message.Split(Environment.NewLine)[^1]}");
                                }
                            });
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"There was an error while recording audio.");
                        logger.LogDebug($"{ex.Message.Split(Environment.NewLine)[^1]}");
                    }
                });
            }
        }
        */

        private static void GenerateAudiobook(AudiobookChapterEntry[] audiobookChapters, string dataDirectory, string outputFilename)
        {
            StringBuilder sb = new StringBuilder();
            List<AudioInputFile> audioFileCache = new List<AudioInputFile>();
            DateTimeOffset nextConsoleFeedback = DateTime.UtcNow;

            dataDirectory = AppendDataDir(dataDirectory);

            if (Directory.Exists(dataDirectory))
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                logger.LogInformation($"Scanning data directory for audio files...");

                int audioFileCount = 0;
                foreach (AudioInputFile inputAudioFile in EnumerateAudioFilesInFolder(dataDirectory))
                {
                    audioFileCache.Add(inputAudioFile);
                    audioFileCount++;
                }
                sw.Stop();
                logger.LogInformation($"{audioFileCount} files were found in the data directory in {sw.ElapsedMilliseconds:N0} ms.");

                // Ignore invalid chapters
                audiobookChapters = audiobookChapters.Where(ac => ac.IsIncluded && !String.IsNullOrEmpty(ac.StartTime) && !String.IsNullOrEmpty(ac.StopTime)).OrderBy(ac => ac.Index).ToArray();
                for (int acIndex = 0; acIndex < audiobookChapters.Length; acIndex++)
                {
                    audiobookChapters[acIndex].AudioInputFile = audioFileCache.FirstOrDefault(af => af.Filename == audiobookChapters[acIndex].Filename);
                    audiobookChapters[acIndex].OutputFilename = $"{audiobookChapters[acIndex].Index.ToString("0000")}.m4a";
                }
               

                // Convert the audio files
                foreach (AudiobookChapterEntry audiobookChapterEntry in audiobookChapters)
                {
                    logger.LogInformation($"Converting '{audiobookChapterEntry.Filename}', please wait.");

                    string ffmpegConversionCommandLine = $"{config.FFMPEG.Conversion}";
                        ffmpegConversionCommandLine = ffmpegConversionCommandLine.Replace("%inputfile%", audiobookChapterEntry.AudioInputFile.FullPath);
                        ffmpegConversionCommandLine = ffmpegConversionCommandLine.Replace("%outputfile%", Path.Combine(config.TempDirectory, audiobookChapterEntry.OutputFilename));
                        ffmpegConversionCommandLine = ffmpegConversionCommandLine.Replace("%starttime%", audiobookChapterEntry.StartTime);
                        ffmpegConversionCommandLine = ffmpegConversionCommandLine.Replace("%stoptime%", audiobookChapterEntry.StopTime);

                        logger.LogDebug($"Running 'ffmpeg {ffmpegConversionCommandLine}'...");

                        try
                        {
                            var ffmpegConversion = FFmpeg.Conversions.New();
                            ffmpegConversion.Start(ffmpegConversionCommandLine).Wait();
                        }
                        catch (Exception ex)
                        {
                            audiobookChapterEntry.ExceptionMessage = ex.Message;

                            if (ex.Message.Contains(" already exists. Exiting."))
                            {
                                logger.LogWarning($"File '{audiobookChapterEntry.OutputFilename}' already exists, skipping.");
                            } else
                            {
                                logger.LogError(ex, "Exception occured during the FFMPEG conversion.");
                            }
                        }
                }


                // Generate audio file list file
                sb.Clear();
                sb.AppendLine("# generated list of audio files to compile");
                foreach (AudiobookChapterEntry audiobookChapterEntry in audiobookChapters)
                {
                    sb.AppendLine($"file '{audiobookChapterEntry.OutputFilename}'");
                }
                File.WriteAllText(Path.Combine(config.TempDirectory, "chapterfilelist.txt"), sb.ToString());
                
                
                // Generate metadata file
                double currentPositionInSeconds = 0.0;

                sb.Clear();
                sb.AppendLine(";FFMETADATA1");
                sb.AppendLine($"author={config.Metadata.Artist}");
                sb.AppendLine($"artist={config.Metadata.Artist}");
                sb.AppendLine($"album_artist={config.Metadata.Artist}");
                sb.AppendLine($"title={config.Metadata.Title}");
                sb.AppendLine($"album={config.Metadata.Album}");
                sb.AppendLine($"year={config.Metadata.Year}");
                sb.AppendLine($"date={config.Metadata.Year}");
                sb.AppendLine($"genre={config.Metadata.Genre}");
                sb.AppendLine($"description={config.Metadata.Description}");
                sb.AppendLine($"comment={config.Metadata.Description}");
                sb.AppendLine($"language={config.Metadata.Language}");
                foreach (AudiobookChapterEntry audiobookChapterEntry in audiobookChapters)
                {
                    sb.AppendLine("");
                    sb.AppendLine("[CHAPTER]");
                    sb.AppendLine("TIMEBASE=1/1000");
                    sb.AppendLine($"START={(int)(currentPositionInSeconds * 1000)}");
                    sb.AppendLine($"END={(int)((currentPositionInSeconds + audiobookChapterEntry.DurationInSeconds) * 1000)}");
                    sb.AppendLine($"title={audiobookChapterEntry.Chapter}");

                    currentPositionInSeconds += audiobookChapterEntry.DurationInSeconds;
                }
                File.WriteAllText(Path.Combine(config.TempDirectory, "FFMETADATA.txt"), sb.ToString());


                logger.LogInformation($"Compiling {audiobookChapters.Length} audio files into '{outputFilename}'...");

                if (File.Exists(outputFilename))
                {
                    File.Delete(outputFilename);
                }

                string ffmpegCompilationCommandLine = $"{config.FFMPEG.Compilation}";
                ffmpegCompilationCommandLine = ffmpegCompilationCommandLine.Replace("%inputfiles%", Path.Combine(config.TempDirectory, "chapterfilelist.txt"));
                ffmpegCompilationCommandLine = ffmpegCompilationCommandLine.Replace("%metadatafile%", Path.Combine(config.TempDirectory, "FFMETADATA.txt"));
                ffmpegCompilationCommandLine = ffmpegCompilationCommandLine.Replace("%coverfile%", Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), config.Metadata.Cover));
                ffmpegCompilationCommandLine = ffmpegCompilationCommandLine.Replace("%outputfile%", outputFilename);

                logger.LogDebug($"Running 'ffmpeg {ffmpegCompilationCommandLine}'...");

                try
                {
                    var ffmpegConversion = FFmpeg.Conversions.New();
                    ffmpegConversion.Start(ffmpegCompilationCommandLine).Wait();
                }
                catch (Exception ex)
                {                    
                    logger.LogError(ex, "Exception occured during the FFMPEG compilation.");
                }

                logger.LogInformation($"M4B audiobook creation is completed.");
            }
        }

        static private IEnumerable<AudioInputFile> EnumerateAudioFilesInFolder(string foldername)
        {
            if (Directory.Exists(AppendDataDir(foldername)))
            {
                foreach (string filename in Directory.GetFiles(AppendDataDir(foldername)).OrderBy(n => n))
                {
                    string pathToFile = Path.GetFullPath(filename);

                    if (!config.AudioFileExtensions.Contains(Path.GetExtension(filename)))
                    {
                        continue;
                    }

                    AudioInputFile audioInputFile = null;
                    try
                    {
                        var fi = new FileInfo(pathToFile);

                        audioInputFile = new AudioInputFile();
                        audioInputFile.Filename = fi.Name;
                        audioInputFile.DirectoryName = fi.DirectoryName;
                        audioInputFile.FullPath = fi.FullName;
                        audioInputFile.FileSize = fi.Length;
                        audioInputFile.FileModificationTime = fi.LastWriteTime;
                        audioInputFile.Name = Path.GetFileNameWithoutExtension(pathToFile);
                    }
                    catch
                    {
                    }

                    if (audioInputFile != null)
                    {
                        yield return audioInputFile;
                    }
                }

                foreach (string directoryName in Directory.GetDirectories(AppendDataDir(foldername)).OrderBy(n => n))
                {
                    foreach (var audioInputFile in EnumerateAudioFilesInFolder(directoryName))
                    {
                        yield return audioInputFile;
                    }
                }
            }
        }

        private static string AppendDataDir(string filename)
        {
            if (!String.IsNullOrWhiteSpace(config.DataDirectory))
            {
                return Path.Combine(config.DataDirectory, filename);
            }
            else
            {
                return Path.Combine(Directory.GetCurrentDirectory(), filename);
            }
        }


        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;

            mutex.ReleaseMutex();
        }

    }
}
