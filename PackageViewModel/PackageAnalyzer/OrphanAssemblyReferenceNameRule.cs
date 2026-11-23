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
    internal sealed class OrphanAssemblyReferenceNameRule : IPackageRule
    {
        #region IPackageRule Members

        public IEnumerable<PackageIssue> Validate(IPackage package, string packagePath)
        {
            if (package.PackageAssemblyReferences.Any())
            {
                var allLibFiles = package.GetFilesInFolder("lib").Select(Path.GetFileName);
                var libFilesSet = new HashSet<string>(allLibFiles!, StringComparer.OrdinalIgnoreCase);

                return from reference in package.PackageAssemblyReferences.SelectMany(set => set.References)
                       where !libFilesSet.Contains(reference)
                       select CreateIssue(reference);
            }
            return Array.Empty<PackageIssue>();
        }

        #endregion

        private static PackageIssue CreateIssue(string reference)
        {
            return new PackageIssue(
                PackageIssueLevel.Error,
                "未找到程序集引用名称。",
                "已筛选的程序集引用中的名称 '" + reference + "' 在 'lib' 文件夹下不存在。",
                "请移除该程序集引用名称，或在 'lib' 文件夹下添加同名文件。");
        }
    }
}
