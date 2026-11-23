using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using NuGetPackageExplorer.Types;

using NuGetPe;

namespace PackageExplorerViewModel.Rules
{
    [Export(typeof(IPackageRule))]
    internal sealed class MissingContentOrDependencyRule : IPackageRule
    {
        #region IPackageRule Members

        public IEnumerable<PackageIssue> Validate(IPackage package, string packagePath)
        {
            if (!HasContentOrDependency(package))
            {
                yield return new PackageIssue(
                    PackageIssueLevel.Error,
                    "包不包含任何内容或依赖项。",
                    "该包不包含任何文件、依赖项或框架程序集引用，因此不是有效的包。",
                    "请添加文件、包依赖项或框架程序集引用。"
                    );
            }
        }

        #endregion

        private static bool HasContentOrDependency(IPackage package)
        {
            return package.GetFiles().Any() ||
                   package.DependencyGroups.SelectMany(static p => p.Packages).Any() ||
                   package.FrameworkReferences.Any();
        }
    }
}
