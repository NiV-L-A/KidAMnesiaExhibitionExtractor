using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.AssetRegistry;
using CUE4Parse.UE4.Assets.Exports.Sound;
using CUE4Parse.UE4.Assets.Exports.Wwise;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using System.Reflection;

namespace KidAMnesiaExhibitionExtractor
{
    internal class Program
    {
        private static string _defaultConsoleTitle = "";
        private static string _assetRegistryPath = "Paperbag/AssetRegistry.bin";
        private static string _eventsPath = "Paperbag/Content/WwiseAudio/Events/Default_Work_Unit/";
        private static string _mediaPath = "Paperbag/Content/WwiseAudio/Media";

        public static void Main()
        {
            var version = Assembly.GetExecutingAssembly()
                          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                          ?.InformationalVersion;
            if (version!.Contains('+'))
            {
                version = version!.Split('+')[0];
            }

            _defaultConsoleTitle = $"Kid A Mnesia Exhibition Extractor (v{version})";
            Console.Title = _defaultConsoleTitle;
            Console.WriteLine(_defaultConsoleTitle);
            Console.WriteLine();
            var pakDirectory = GetPaksDirectory();
            pakDirectory = pakDirectory.Trim('"');
            if (string.IsNullOrWhiteSpace(pakDirectory) || !Directory.Exists(pakDirectory))
            {
                Console.WriteLine($"Directory not found: \"{pakDirectory}\"");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
                return;
            }

            DefaultFileProvider provider = new(pakDirectory, SearchOption.TopDirectoryOnly, VersionContainer.DEFAULT_VERSION_CONTAINER, StringComparer.InvariantCultureIgnoreCase);
            provider.Initialize();
            if (provider.UnloadedVfs.Count == 0)
            {
                Console.WriteLine("No pak files found in the specified directory.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
                return;
            }

            Console.Write("Enter the AES key (format: 0xN where N is a 64 digit hexadecimal number): ");
            var aesKey = Console.ReadLine();
            if (!aesKey.StartsWith("0x") || aesKey.Length != 66)
            {
                Console.WriteLine("Invalid AES key format. The key must start with \"0x\" and followed by a 64 digit hexadecimal number.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
                return;
            }

            if (provider.SubmitKey(new FGuid(), new FAesKey(aesKey)) == 0)
            {
                Console.WriteLine("Failed to submit AES key. Wrong key provided.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
                return;
            }

            Console.Write("Enter the output directory: ");
            var outputDirectory = Console.ReadLine();
            outputDirectory = outputDirectory.Trim('"');
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                Console.WriteLine("Output directory not specified.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"Asset count: {provider.Files.Count}");
            Console.WriteLine("Press any key to start extracting audio files...");
            Console.ReadKey(true);
            Directory.CreateDirectory(outputDirectory);
            Console.WriteLine("Linking asset ids with filenames from asset registry and event data...");
            FAssetRegistryState? assetRegistry = GetAssetRegistry(provider);
            Dictionary<string, string> fileNamesFromAssetRegistry = GetFilenamesFromAssetRegistry(assetRegistry);
            Dictionary<string, string> assetIdFilenameDict = GetAssetIdFilenameMap(provider, fileNamesFromAssetRegistry);
            Console.WriteLine();
            Console.WriteLine("Extracting audio files...");
            int failCount = 0;
            List<string> extracted = new();
            foreach (GameFile file in provider.Files.Values)
            {
                if (!file.Directory.StartsWith(_mediaPath))
                {
                    continue;
                }

                if (file.Extension != "uasset")
                {
                    continue;
                }

                if (extracted.Contains(file.Name))
                {
                    // 486 uasset files appear multiple times with the same asset id, but they are 1:1 byte identical
                    continue;
                }

                try
                {
                    var package = provider.LoadPackage(file.Path);
                    var exports = package.GetExports();
                    foreach (var export in exports)
                    {
                        if (export is not UAkMediaAsset mediaAsset)
                        {
                            continue;
                        }

                        byte[] data = ExtractData(exports);
                        if (data.Length == 0)
                        {
                            // The following asset ids have no media data
                            // 625141396
                            // 602284699
                            // 602284700
                            // 685264471
                            // 685264475
                            // 768857212
                            continue;
                        }

                        string assetId = mediaAsset.ID.ToString();
                        string fileName = assetId;
                        string directory = "";
                        if (assetIdFilenameDict.TryGetValue(assetId, out var filePath))
                        {
                            directory = Path.GetDirectoryName(filePath);
                            fileName = $"{Path.GetFileName(filePath)}_{assetId}";
                        }
                        else if (fileNamesFromAssetRegistry.TryGetValue(assetId, out var registryName))
                        {
                            // file in asset registry but not linked to any event
                            fileName = $"{registryName}_{assetId}";
                        }

                        fileName = $"{fileName}.wem";
                        if (fileName.Length > 255)
                        {
                            // Windows caps file name length at 255 characters
                            // Final_Amb\Studio\AMBRoom_Immersive Room Tone Dense And Low Designed Tone For Dark Office Ambience Ambix_SSLAB_SSL38-Play_rtAMB_StudioHallway-Play_rtAMB_PixelateJellyHall-Play_rtAMB_LiminalStairwell_A_Hallway-Play_rtAMB_LiminalPyrHall_A-Play_rtAMB_JellyStudioHall-Play_rtAMB_Landscape-Play_rtAMB_Kaleidoscope_Hallway-Play_rtAMB_PyramidApproach_330095037.wem
                            // Final_Amb\Kaleidoscope\Thesholds\AMBRoom_Immersive Room Tone Dense And Low Designed Tone For Dark Office Ambience Ambix_SSLAB_SSL38-Play_thAMB_Kaleidoscope_Hallway-Play_thAMB_Landscape-Play_thAMB_JellyStudioHall-Play_thAMB_LiminalPyrHall_A-Play_thAMB_LiminalStairwell_A_Hallway-Play_thAMB_PixelateJellyHall-Play_thAMB_StudioHallway_292832817.wem
                            fileName = fileName.Substring(0, 251) + ".wem";
                        }

                        // Should be 1210 .wem files
                        string outputFile = Path.Combine(outputDirectory, "Default_Work_Unit", directory, fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                        File.WriteAllBytes(outputFile, data);
                        extracted.Add(file.Name);
                        Console.Write($"\r{extracted.Count:d4}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Failed to extract \"{file.Path}\"");
                    Console.WriteLine($"\t{ex.Message}");
                    failCount++;
                }
            }

            // Should be 16 .ogg files
            // We can get them from the asset registry
            foreach (var assetData in assetRegistry.PreallocatedAssetDataBuffers)
            {
                if (assetData == null
                 || assetData.AssetClass.Text != "SoundWave")
                {
                    continue;
                }

                // from          "/Game/Developers/sean/AUDIO/ArtStudio_INLIMBO_SFX_v1_"
                // to "Paperbag/Content/Developers/sean/AUDIO/ArtStudio_INLIMBO_SFX_v1_.uasset"
                var file = provider.Files[$"{assetData.PackageName.Text.Replace("/Game", "Paperbag/Content")}.uasset"];
                var package = provider.LoadPackage(file.Path);
                var exports = package.GetExports();
                foreach (var export in exports)
                {
                    if (export is not USoundWave soundWave)
                    {
                        continue;
                    }

                    var finalPath = soundWave.Owner.Name; // "Paperbag/Content/Developers/sean/AUDIO/ArtStudio_INLIMBO_SFX_v1_"
                    finalPath = finalPath.Replace('/', Path.DirectorySeparatorChar);
                    string outputFile = $"{Path.Combine(outputDirectory, finalPath)}.ogg";
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
                    File.WriteAllBytes(outputFile, soundWave.CompressedFormatData.Formats.FirstOrDefault().Value.Data);
                    extracted.Add(file.Name);
                    Console.Write($"\r{extracted.Count:d4}");
                    break;
                }
            }

            Console.WriteLine();
            if (failCount > 0)
            {
                Console.WriteLine($"Failed to extract {failCount} files");
            }

            Console.WriteLine();
            Console.WriteLine("Done.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        private static string? GetPaksDirectory()
        {
            var pakDirectory = "C:\\Program Files\\Epic Games\\KidAMnesiaExhibition\\Paperbag\\Content\\Paks";
            if (Directory.Exists(pakDirectory))
            {
                Console.WriteLine($"Detected paks directory: \"{pakDirectory}\"");
                Console.Write($"Use this directory? (Y/N): ");
                var tmp = Console.ReadKey();
                Console.WriteLine();
                if (tmp.Key == ConsoleKey.Y)
                {
                    return pakDirectory;
                }
            }

            Console.Write("Enter the KID A MNESIA EXHIBITION paks directory: ");
            pakDirectory = Console.ReadLine();
            return pakDirectory;
        }

        private static FAssetRegistryState? GetAssetRegistry(DefaultFileProvider provider)
        {
            byte[] registryData = provider.SaveAsset(_assetRegistryPath);
            using FByteArchive archive = new(_assetRegistryPath, registryData);
            FAssetRegistryState? assetRegistry = new(archive);
            return assetRegistry;
        }

        private static Dictionary<string, string> GetFilenamesFromAssetRegistry(FAssetRegistryState assetRegistry)
        {
            Dictionary<string, string> result = [];
            foreach (var assetData in assetRegistry.PreallocatedAssetDataBuffers)
            {
                if (assetData == null
                 || assetData.AssetClass.Text != "AkMediaAsset")
                {
                    continue;
                }

                foreach (var tag in assetData.TagsAndValues)
                {
                    if (tag.Key.Text != "MediaName")
                    {
                        continue;
                    }

                    result[assetData.AssetName.Text] = tag.Value;
                    break;
                }
            }

            return result;
        }

        private static Dictionary<string, string> GetAssetIdFilenameMap(DefaultFileProvider provider, Dictionary<string, string> fileNamesFromAssetRegistry)
        {
            Dictionary<string, string> result = [];
            int i = 0;
            foreach (var file in provider.Files.Values)
            {
                i++;
                Console.Write($"\r{i:d5}/{provider.Files.Count:d5}");
                if (!file.Directory.StartsWith(_eventsPath))
                {
                    continue;
                }

                if (file.Extension != "uasset")
                {
                    continue;
                }

                var package = provider.LoadPackage(file.Path);
                var exports = package.GetExports();
                foreach (var export in exports)
                {
                    if (export is not UAkAudioEventData audioEventData)
                    {
                        continue;
                    }

                    string pathWithoutExtension = export.Owner.Name.Replace(_eventsPath, "");
                    string nameWithoutExtension = Path.GetFileName(pathWithoutExtension);
                    foreach (var media in audioEventData.MediaList)
                    {
                        var assetId = media.Name.Text;
                        if (result.TryGetValue(assetId, out var currentPath))
                        {
                            // Asset id with multiple filenames
                            result[assetId] = $"{currentPath}-{nameWithoutExtension}";
                        }
                        else if (fileNamesFromAssetRegistry.TryGetValue(assetId, out string? registryName))
                        {
                            // From asset registry
                            string directoryName = Path.GetDirectoryName(pathWithoutExtension);
                            result[assetId] = Path.Combine(directoryName, $"{registryName}-{nameWithoutExtension}");
                        }
                    }
                }
            }

            return result;
        }

        private static byte[] ExtractData(IEnumerable<CUE4Parse.UE4.Assets.Exports.UObject> exports)
        {
            foreach (var export in exports)
            {
                if (export is not UAkMediaAssetData mediaAssetData)
                {
                    continue;
                }

                foreach (var chunk in mediaAssetData.DataChunks)
                {
                    if (chunk.IsPrefetch)
                    {
                        continue;
                    }

                    return chunk.Data.Data;
                }
            }

            return [];
        }
    }
}