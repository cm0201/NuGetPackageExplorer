using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using NuGetPackageExplorer.Types;

using NuGetPe;

namespace PackageExplorerViewModel.Rules
{
    [Export(typeof(IPackageRule))]
    internal sealed class MisplacedTransformFileRule : IPackageRule
    {
        private const string ContentFolder = "content";
        private const string CodeTransformExtension = ".pp";
        private const string ConfigTransformExtension = ".transform";

        #region IPackageRule Members

        public IEnumerable<PackageIssue> Validate(IPackage package, string packagePath)
        {
            foreach (var file in package.GetFiles())
            {
                var path = file.Path;

                // if not a .transform file, ignore 
                if (!path.EndsWith(CodeTransformExtension, StringComparison.OrdinalIgnoreCase) &&
                    !path.EndsWith(ConfigTransformExtension, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // if not inside 'content' folder, warn
                if (!path.StartsWith(ContentFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    yield return CreatePackageIssueForMisplacedContent(path);
                }
            }
        }

        #endregion

        private static PackageIssue CreatePackageIssueForMisplacedContent(string path)
        {
            return new PackageIssue(
                PackageIssueLevel.Warning,
                "转换文件位于 content 文件夹之外",
                "转换文件 '" + path +
                "' 位于 'content' 文件夹之外，因此在安装此包时不会被转换。",
                "将其移动到 'content' 文件夹内。");
        }
    }
}
