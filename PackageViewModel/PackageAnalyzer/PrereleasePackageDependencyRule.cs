using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;

using NuGet.Packaging.Core;

using NuGetPackageExplorer.Types;

using NuGetPe;

namespace PackageExplorerViewModel.Rules
{
    [Export(typeof(IPackageRule))]
    internal sealed class PrereleasePackageDependencyRule : IPackageRule
    {
        #region IPackageRule Members

        public IEnumerable<PackageIssue> Validate(IPackage package, string packagePath)
        {
            if (package.Version.IsPrerelease)
            {
                return Array.Empty<PackageIssue>();
            }

            return package.DependencyGroups.SelectMany(static p => p.Packages)
                                         .Where(IsPrereleaseDependency)
                                         .Select(CreatePackageIssue);
        }

        #endregion

        private static bool IsPrereleaseDependency(PackageDependency pd)
        {
            if (pd.VersionRange == null)
            {
                return false;
            }

            return pd.VersionRange.MinVersion?.IsPrerelease == true || pd.VersionRange.MaxVersion?.IsPrerelease == true;
        }

        private static PackageIssue CreatePackageIssue(PackageDependency target)
        {
            return new PackageIssue(
                PackageIssueLevel.Warning,
                "无效的预发布依赖项",
                string.Format(CultureInfo.CurrentCulture,
                              "包的稳定版本不得依赖预发布包 '{0}'。",
                              target),
                string.Format(CultureInfo.CurrentCulture,
                              "请修改依赖项 '{0}' 的版本规范，或更新版本字段。", target)
                );
        }
    }
}
