using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Telerik.Sitefinity.Frontend.TestUtilities;
using Telerik.Sitefinity.Frontend.TestUtilities.CommonOperations;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.TestUtilities.CommonOperations;

namespace Telerik.Sitefinity.Frontend.TestIntegration.ResourcePackages
{
    [TestFixture]
    public class ResourcePackagesReplacePackageTest
    {
        [FixtureSetUp]
        public void FixtureSetUp()
        {
            string packageResource = "Telerik.Sitefinity.Frontend.TestUtilities.Data.Package1.zip";

            FeatherServerOperations.ResourcePackages().AddNewResourcePackage(packageResource);
        }

        [Test]
        [Category(TestCategories.Packages)]
        [Author(FeatherTeams.Team2)]
        [Description("Adds new package without layout file, replaces it with the same package with different layouts and verifies new templates are generated")]
        public void ResourcePackage_ReplaceExistingPackage_VerifyNewTemplatesGenerated()
        {
            string packageName = "Package1";
            string templateTitle = "Package1.test-layout";
            string newPackageResource = "Telerik.Sitefinity.Frontend.TestUtilities.Data.ReplaceTest.Package1.zip";
            string newTemplateTitle = "Package1.replace-test";
            string tempFolder = "Temp";
            string packagesFolder = "ResourcePackages";
            string sitefinityPath = FeatherServerOperations.ResourcePackages().SfPath;

            var packagePath = Path.Combine(sitefinityPath, packagesFolder, packageName);

            string tempFolderPath = Path.Combine(sitefinityPath, tempFolder);

            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }

            try
            {             
                int templatesCount = this.PageManager.GetTemplates().Count();

                var template = this.PageManager.GetTemplates().Where(t => t.Title == templateTitle).FirstOrDefault();
                Assert.IsNotNull(template, "Template was not found");
                FeatherServerOperations.ResourcePackages().AddNewResourcePackage(newPackageResource, tempFolder);

                var tempPath = Path.Combine(sitefinityPath, tempFolder, packageName);

                DirectoryInfo originalPackage = new DirectoryInfo(packagePath);
                DirectoryInfo newPackage = new DirectoryInfo(tempPath);

                MergeFolders(newPackage, originalPackage);

                FeatherServerOperations.ResourcePackages().WaitForTemplatesCountToIncrease(templatesCount, 1);
                Assert.IsTrue(this.PageManager.GetTemplates().Count().Equals(templatesCount + 1), "templates count is not correct");

                var newTemplate = this.PageManager.GetTemplates().Where(t => t.Title == newTemplateTitle).FirstOrDefault();
                Assert.IsNotNull(newTemplate, "New template was not found");
            }
            finally
            {
                ServerOperations.Templates().DeletePageTemplate(templateTitle);
                ServerOperations.Templates().DeletePageTemplate(newTemplateTitle);
                FeatherServerOperations.ResourcePackages().DeleteDirectory(tempFolderPath);
                FeatherServerOperations.ResourcePackages().DeleteDirectory(packagePath);
            }
        }

        private PageManager pageManager;

        private PageManager PageManager
        {
            get
            {
                if (this.pageManager == null)
                {
                    this.pageManager = PageManager.GetManager();
                }

                return this.pageManager;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
        private static void MergeFolders(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo dirSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(dirSourceSubDir.Name);
                MergeFolders(dirSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
