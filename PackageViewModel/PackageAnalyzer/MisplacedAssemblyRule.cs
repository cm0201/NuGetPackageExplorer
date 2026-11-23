using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using NuGetPackageExplorer.Types;

using NuGetPe;

namespace PackageExplorerViewModel.Rules
{
    [Export(typeof(IPackageRule))]
    internal sealed class MisplacedAssemblyRule : IPackageRule
    {
        private static readonly HashSet<string> AssemblyFolders = new(["lib", "analyzers", "build", "ref", "tools"], StringComparer.OrdinalIgnoreCase);

        #region IPackageRule Members

        public IEnumerable<PackageIssue> Validate(IPackage package, string packagePath)
        {
            foreach (var file in package.GetFiles())
            {
                var path = file.Path;
                var segments = path.Split('\\');
                var directory = segments.First();

                // if under 'folder' directly
                if (AssemblyFolders.Contains(directory))
                {
                    // file under the directory. Tools can do anything
                    if (segments.Length == 2 && FileHelper.IsAssembly(path) && !"tools".Equals(directory, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return CreatePackageIssueForAssembliesUnderLib(path, directory);
                    }
                }
                else
                {
                    // when checking for assemblies outside known folders, only check .dll files.
                    // .exe files are often legitimate outside 'lib'.
                    if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                        path.EndsWith(".winmd", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return CreatePackageIssueForAssembliesOutsideLib(path, directory);
                    }
                }

            }
        }

        #endregion

        private static PackageIssue CreatePackageIssueForAssembliesUnderLib(string target, string folder)
        {
            return new PackageIssue(
                PackageIssueLevel.Warning,
                "程序集不在框架文件夹内",
                "程序集 '" + target +
                $"' 直接位于 '{folder}' 文件夹下。建议将程序集放在特定框架的子文件夹内。",
                "将其移动到特定框架的子文件夹内。如果该程序集面向多个框架，可忽略此警告。"
                );
        }

        private static PackageIssue CreatePackageIssueForAssembliesOutsideLib(string target, string folder)
        {
            return new PackageIssue(
                PackageIssueLevel.Warning,
                "程序集位于已知文件夹之外",
                "程序集 '" + target +
                $"' 位于 '{folder}' 文件夹内，因此在包安装到项目时不会被使用。",
                "将其移动到已知文件夹内。"
                );
        }
    }
}
