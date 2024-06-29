#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;

namespace F00F.Init
{
    internal static partial class SyncSharp
    {
        [GeneratedRegex(@"^Project.+ = ""(?<PrjName>.+?)"", ""\1\.csproj""", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
        private static partial Regex SlnRegex();

        [GeneratedRegex(@"^export_path="".export/(?<Platform>.+?)/(?<PrjName>.+)\.(?<Ext>.+?)""", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
        private static partial Regex ExportPathRegex();

        [GeneratedRegex(@"^application/bundle_identifier=""(?<App>.+?)\.(?<PrjName>.+)\.(?<Org>.+?)""", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
        private static partial Regex ExportBundleRegex();

        private static string curName = GetName();
        private static string curDesc = GetDesc();

        #region Config

        public static void Activate()
            => ProjectSettings.SettingsChanged += OnSettingsChanged;

        public static void Deactivate()
            => ProjectSettings.SettingsChanged -= OnSettingsChanged;

        private static void OnSettingsChanged()
        {
            if (Changed())
                Execute();

            static bool Changed()
            {
                return
                    curName != GetName() ||
                    curDesc != GetDesc();
            }
        }

        #endregion

        public static void Execute()
        {
            var newName = GetName();
            var newDesc = GetDesc();

            UpdateReadme();
            UpdateProject();
            UpdateDataDir();
            RemoveOldFiles();
            UpdateWorkflows();
            UpdateExportConfig();

            curName = newName;
            curDesc = newDesc;

            void UpdateReadme()
            {
                const string source = "res://README.md";
                var lines = FS.ReadLines(source).ToArray();
                if (!UpdateLines()) return;
                FS.WriteLines(source, lines);

                bool UpdateLines()
                {
                    var changed = false;
                    UpdateLine(0, $"# {newName}");
                    UpdateLine(1, newDesc);
                    return changed;

                    void UpdateLine(int i, string newLine)
                    {
                        if (i < lines.Length)
                        {
                            var oldLine = lines[i];
                            if (oldLine == newLine) return;

                            ReportProgress($"Readme: {oldLine} => {newLine}");

                            lines[i] = newLine;
                            changed = true;
                        }
                    }
                }
            }

            void UpdateProject()
            {
                RenameProjectFiles();
                UpdateProjectFiles();

                void RenameProjectFiles()
                {
                    Rename(Get(".sln"), $"{newName}.sln");
                    Rename(Get(".csproj"), $"{newName}.csproj");

                    string Get(string ext)
                        => FS.GetFiles("res://").Where(x => x.EndsWith(ext)).SingleOrDefault();

                    void Rename(string oldFile, string newFile)
                    {
                        if (oldFile == newFile) return;
                        ReportProgress($"Rename: {oldFile} => {newFile}");
                        FS.Rename(oldFile, newFile);
                    }
                }

                void UpdateProjectFiles()
                {
                    UpdateSln();
                    UpdateCfg();

                    void UpdateSln()
                    {
                        var sln = $"{newName}.sln";
                        var lines = FS.ReadLines(sln).ToArray();
                        if (!UpdateLines()) return;
                        FS.WriteLines(sln, lines);

                        bool UpdateLines()
                        {
                            var changed = false;
                            UpdateLines();
                            return changed;

                            void UpdateLines()
                            {
                                for (var i = 0; i < lines.Length; ++i)
                                {
                                    var line = lines[i];
                                    if (!line.StartsWith("Project")) continue;

                                    var match = SlnRegex().Match(line);
                                    if (!match.Success) continue;

                                    var oldName = match.Groups["PrjName"].Value;
                                    if (oldName == newName) return;

                                    var newLine = line.Replace(oldName, newName);
                                    ReportProgress($"{sln}: {line} => {newLine}");
                                    lines[i] = newLine;
                                    changed = true;
                                    return;
                                }
                            }
                        }
                    }

                    void UpdateCfg()
                    {
                        var oldAssemblyName = GetProj();
                        if (oldAssemblyName == newName) return;

                        ReportProgress($"project.godot: {oldAssemblyName} => {newName}");

                        SetProj(newName);
                        SaveSettings();
                    }
                }
            }

            void UpdateDataDir()
            {
                // TODO: Update shortcut
                //const string source = "res://.appdata.lnk";
            }

            void RemoveOldFiles()
            {
                Remove(Get(".csproj.old"));

                IEnumerable<string> Get(string ext)
                    => FS.GetFiles("res://").Where(x => x.EndsWith(ext));

                void Remove(IEnumerable<string> files)
                {
                    foreach (var file in files.Select(ProjectSettings.GlobalizePath))
                    {
                        ReportProgress($"Trash: {file}");
                        OS.MoveToTrash(file);
                    }
                }
            }

            void UpdateWorkflows()
            {
                const string source = "res://.github/workflows";
                if (!FS.DirExists(source)) return;

                var (version, status) = GetVersion();
                foreach (var yaml in Get(".yml").Concat(Get(".yaml")))
                {
                    var lines = FS.ReadLines(yaml).ToArray();
                    if (!UpdateLines(yaml, lines)) return;
                    FS.WriteLines(yaml, lines);
                }

                IEnumerable<string> Get(string ext)
                {
                    return FS.GetFiles(source)
                        .Where(x => x.EndsWith(ext))
                        .Select(x => $"{source}/{x}");
                }

                bool UpdateLines(string source, string[] lines)
                {
                    if (lines.Length < 4) return false;
                    if (lines[0] != "env:") return false;
                    if (!lines[1].StartsWith("  GODOT_ROOT:")) return false;

                    var changed = false;
                    UpdateLine(2, "  GODOT_PATH:", status is "stable" ? version : $"{version}/{status}");
                    UpdateLine(3, "  GODOT_NAME:", $"{version}-{status}");
                    return changed;

                    void UpdateLine(int i, string token, string value)
                    {
                        var oldLine = lines[i];
                        if (!oldLine.StartsWith(token)) return;

                        var newLine = $"{token} {value}";
                        if (oldLine == newLine) return;

                        ReportProgress($"{source.GetFile()}: {oldLine.Trim()} => {newLine.Trim()}");

                        lines[i] = newLine;
                        changed = true;
                    }
                }
            }

            void UpdateExportConfig()
            {
                const string source = "res://export_presets.cfg";
                var lines = FS.ReadLines(source).ToArray();
                if (!UpdateLines()) return;
                FS.WriteLines(source, lines);

                bool UpdateLines()
                {
                    var changed = false;
                    UpdateLines("export_path", ExportPathRegex);
                    UpdateLines("application/bundle_identifier", ExportBundleRegex);
                    return changed;

                    void UpdateLines(string token, Func<Regex> Regex)
                    {
                        for (var i = 0; i < lines.Length; ++i)
                        {
                            var line = lines[i];
                            if (line.StartsWith(token))
                            {
                                var match = Regex().Match(line);
                                if (!match.Success) continue;

                                var oldName = match.Groups["PrjName"].Value;
                                if (oldName == newName) continue;

                                var newLine = line.Replace(oldName, newName);
                                ReportProgress($"{source}: {line} => {newLine} ({oldName} => {newName})");
                                lines[i] = newLine;
                                changed = true;
                            }
                        }
                    }
                }
            }

        }

        #region Utilities

        private static string GetName()
            => ProjectSettings.GetSetting("application/config/name").AsString();

        private static string GetDesc()
            => ProjectSettings.GetSetting("application/config/description").AsString();

        private static string GetProj()
            => ProjectSettings.GetSetting("dotnet/project/assembly_name").AsString();

        private static void SetProj(string name)
            => ProjectSettings.SetSetting("dotnet/project/assembly_name", name);

        private static void SaveSettings()
            => ProjectSettings.Save();

        private static (string Version, string Status) GetVersion()
        {
            var info = Engine.GetVersionInfo();
            var major = (int)info["major"];
            var minor = (int)info["minor"];
            var patch = (int)info["patch"];
            var status = (string)info["status"];
            var version = patch is 0
                ? $"{major}.{minor}"
                : $"{major}.{minor}.{patch}";
            return (version, status);
        }

        private static void ReportProgress(string msg)
            => GD.Print($"[F00F.{nameof(SyncSharp)}] {msg}");

        #region FileSystem

        private static class FS
        {
            public static bool DirExists(string path) => DirAccess.DirExistsAbsolute(path);
            public static bool FileExists(string path) => FileAccess.FileExists(path);
            public static string[] GetFiles(string path) => DirAccess.GetFilesAt(path);
            public static Error Rename(string path, string to) => DirAccess.RenameAbsolute(path, to);

            public static IEnumerable<string> ReadLines(string path)
            {
                if (FileExists(path))
                {
                    using (var file = FileAccess.Open(path, FileAccess.ModeFlags.Read))
                    {
                        while (file.GetPosition() < file.GetLength())
                            yield return file.GetLine();
                    }
                }
            }

            public static void WriteLines(string path, IEnumerable<string> lines)
            {
                using (var file = FileAccess.Open(path, FileAccess.ModeFlags.WriteRead))
                {
                    foreach (var line in lines)
                        file.StoreLine(line);
                }
            }
        }

        #endregion

        #endregion
    }
}
#endif
