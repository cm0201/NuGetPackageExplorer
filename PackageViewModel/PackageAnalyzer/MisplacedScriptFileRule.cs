using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using NuGetPackageExplorer.Types;

using NuGetPe;

namespace PackageExplorerViewModel.Rules
{
    [Export(typeof(IPackageRule))]
    internal sealed class MisplacedScriptFileRule : IPackageRule
    {
        private const string ToolsFolder = "tools";
        private const string ScriptExtension = ".ps1";

        #region IPackageRule Members

        public IEnumerable<PackageIssue> Validate(IPackage package, string packagePath)
        {
            foreach (var file in package.GetFiles())
            {
                var path = file.Path;
                if (!path.EndsWith(ScriptExtension, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!path.StartsWith(ToolsFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    yield return CreatePackageIssueForMisplacedScript(path);
                }
                else
                {
                    var directory = Path.GetDirectoryName(path);
                    var name = Path.GetFileNameWithoutExtension(path);
                    if (!ToolsFolder.Equals(directory, StringComparison.OrdinalIgnoreCase) ||
                        !"install".Equals(name, StringComparison.OrdinalIgnoreCase) &&
                        !"uninstall".Equals(name, StringComparison.OrdinalIgnoreCase) &&
                        !"init".Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return CreatePackageIssueForUnrecognizedScripts(path);
                    }
                }
            }
        }

        #endregion

        private static PackageIssue CreatePackageIssueForMisplacedScript(string path)
        {
            return new PackageIssue(
                PackageIssueLevel.Warning,
                "脚本文件位于 tools 文件夹之外",
                "脚本文件 '" + path +
                "' 位于 'tools' 文件夹之外，因此在安装此包时不会执行。",
                "将其移动到 'tools' 文件夹内。");
        }

        private static PackageIssue CreatePackageIssueForUnrecognizedScripts(string path)
        {
            return new PackageIssue(
                PackageIssueLevel.Warning,
                "无法识别的脚本文件",
                "脚本文件 '" + path +
                "' 未被 NuGet 识别，因此在安装此包时不会执行。",
                "将其重命名为 install.ps1、uninstall.ps1 或 init.ps1，并直接放在 'tools' 下。");
        }
    }
}
