using System.Runtime.InteropServices;

using RailworksForge.Core.Models;

namespace RailworksForge.Core;

public static class Paths
{
    private enum Platform
    {
        Linux = 0,
        FreeBsd = 1,
        Osx = 2,
        Windows = 3,
    }

    private const string PrimaryDirName = nameof(RailworksForge);

    private static Platform GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Platform.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Platform.Osx;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return Platform.FreeBsd;
        }

        return Platform.Linux;
    }

    public static string? GetHomeDirectory() => GetPlatform() switch
    {
        Platform.Linux => Environment.GetEnvironmentVariable("HOME"),
        Platform.Osx => Environment.GetEnvironmentVariable("HOME"),
        Platform.FreeBsd => Environment.GetEnvironmentVariable("HOME"),
        Platform.Windows => GetHomeDirectoryWindows(),
        _ => throw new PlatformNotSupportedException(),
    };

    private static string GetHomeDirectoryWindows()
    {
        var homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
        var homePath = Environment.GetEnvironmentVariable("HOMEPATH");

        return Path.Join(homeDrive, homePath);
    }

    public static string GetConfigurationFolder()
    {
        var home = GetHomeDirectory();
        const string configFolder = ".config";

        return Path.Join(home, configFolder, PrimaryDirName);
    }

    public static string GetCacheFolder()
    {
        var home = GetHomeDirectory();
        const string configFolder = ".cache";

        return Path.Join(home, configFolder, PrimaryDirName);
    }

    public static string GetGameDirectory()
    {
        return ApplicationConfig.Get().GameDirectoryPath;
    }

    public static string GetAssetsDirectory()
    {
        return Path.Join(GetGameDirectory(), "Assets");
    }

    public static string GetScenariosDirectory()
    {
        return Path.Join(GetGameDirectory(), "Content", "Routes");
    }

    public static string ToWindowsPath(this string path)
    {
        return path.Replace("/", @"\").Replace(@"\cache\SteamLibrary\steamapps\", @"s:\");
    }

    public static List<BrowserDirectory> GetTopLevelRailVehicleDirectories()
    {
        var assetsPath = GetAssetsDirectory();
        var assetDirectories = Directory.GetDirectories(assetsPath);

        var directories = assetDirectories.Where(dir => EnumerateRailVehicles(dir, 2).Any());

        var sorted = directories
            .Select(dir => new BrowserDirectory(dir))
            .OrderBy(dir => dir.Name);

        return [..sorted];
    }

    public static IEnumerable<string> EnumerateRailVehicles(string dir, int depth)
    {
        return Directory.EnumerateFileSystemEntries(dir, "RailVehicles", new EnumerationOptions
        {
            MaxRecursionDepth = depth,
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
        });
    }
}
