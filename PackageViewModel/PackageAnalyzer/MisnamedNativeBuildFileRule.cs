using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using NuGetPackageExplorer.Types;

using NuGetPe;

namespace PackageExplorerViewModel.Rules
{
    [Export(typeof(IPackageRule))]
    internal sealed class MisnamedNativeBuildFileRule : IPackageRule
    {
        #region IPackageRule Members

        public IEnumerable<PackageIssue> Validate(IPackage package, string packagePath)
        {
            var files =
                package.GetFiles().Where(static x =>
                    x.Path.EndsWith(".props", StringComparison.OrdinalIgnoreCase) ||
                    x.Path.EndsWith(".targets", StringComparison.OrdinalIgnoreCase));

            foreach (var file in files)
            {
                var path = file.Path;
                var segments = path.Split('\\');

                var frameworkFolder = segments[^2];
                var filename = segments.Last();
                var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

                if (string.Equals(frameworkFolder, "native", StringComparison.OrdinalIgnoreCase) &&
                    filenameWithoutExtension != package.Id)
                {
                    yield return CreatePackageIssueForMisnamedNativeBuildFile(filename, package.Id);
                }
            }
        }

        #endregion


        private static PackageIssue CreatePackageIssueForMisnamedNativeBuildFile(string filename, string packageName)
        {
            return new PackageIssue(
                PackageIssueLevel.Warning,
                "本机构建文件命名错误",
                $"构建文件 '{filename}' 与 NuGet 包名称不匹配。对于本机包，这会导致被引用时行为异常。",
                $"将构建文件 '{filename}' 重命名为与 NuGet 包名称 '{packageName}' 一致。"
            );
        }
    }
}
