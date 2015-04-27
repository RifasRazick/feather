using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MbUnit.Framework;
using Telerik.Sitefinity.Frontend.TestUtilities;
using Telerik.Sitefinity.Frontend.TestUtilities.CommonOperations;
using Telerik.Sitefinity.Frontend.TestUtilities.Mvc.Controllers;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.TestUtilities.CommonOperations;

namespace Telerik.Sitefinity.Frontend.TestIntegration.ResourcePackages
{
    [TestFixture]
    public class ResourcePackagesTests
    {
        [SetUp]
        public void Setup()
        {
            string packageResource = "Telerik.Sitefinity.Frontend.TestUtilities.Data.Package1.zip";

            FeatherServerOperations.ResourcePackages().AddNewResourcePackage(packageResource);
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        [Test]
        [Category(TestCategories.Packages)]
        [Author(FeatherTeams.Team2)]
        [Description("Adds new package without layout file and adds new widget view to the package, renames the template, keeping the package in the title and verifies the template content")]
        public void ResourcePackage_RenameTemplateAndKeepThePackageInTheTitle_VerifyTemplateAndPage()
        {
            string pageName = "FeatherPage";
            string pageName2 = "FeatherPage2";
            string widgetCaption = "TestMvcWidget";
            string placeHolderId = "Body";
            string packageName = "Package1";
            string viewFileName = "Default.cshtml";
            string widgetName = "MvcTest";
            string fileResource = "Telerik.Sitefinity.Frontend.TestUtilities.Data.Default.cshtml";
            string templateTitle = "Package1.test-layout";
            string viewText = "This is a view from package.";
            string layoutText = "Package1 - test layout";
            string templateRenamed = "Package1.TemplateRenamed";

            try
            {
                // Verify template is generated successfully
                var template = this.PageManager.GetTemplates().Where(t => t.Title == templateTitle).FirstOrDefault();
                Assert.IsNotNull(template, "Template was not found");

                // Add new view to the package
                string filePath = FeatherServerOperations.ResourcePackages().GetResourcePackageMvcViewDestinationFilePath(packageName, widgetName, viewFileName);
                FeatherServerOperations.ResourcePackages().AddNewResource(fileResource, filePath);

                // Create page based on the new template from the package
                Guid pageId = ServerOperations.Pages().CreatePage(pageName, template.Id);
                this.PageManager.SaveChanges();
                pageId = ServerOperations.Pages().GetPageNodeId(pageId);

                // Verify the page content contains the text from the layout file
                var content = FeatherServerOperations.Pages().GetPageContent(pageId);
                Assert.IsTrue(content.Contains(layoutText), "Template is not based on the layout file");

                // Rename the template
                template.Title = templateRenamed;
                template.Name = templateRenamed;
                this.PageManager.SaveChanges();

                // Create page based on the renamed template
                Guid pageId2 = ServerOperations.Pages().CreatePage(pageName2, template.Id);
                pageId2 = ServerOperations.Pages().GetPageNodeId(pageId2);

                FeatherServerOperations.Pages().AddMvcWidgetToPage(pageId2, typeof(MvcTestController).FullName, widgetCaption, placeHolderId);

                // Verify the page content contains the text from the view added in the package
                var content2 = FeatherServerOperations.Pages().GetPageContent(pageId2);
                Assert.IsFalse(content2.Contains(layoutText), "Template is based on the layout file, but it shouldn't be");
                Assert.IsTrue(content2.Contains(viewText), "Template is not based on the package");
            }
            finally
            {
                string path = FeatherServerOperations.ResourcePackages().GetResourcePackagesDestination(packageName);

                ServerOperations.Pages().DeleteAllPages();
                ServerOperations.Templates().DeletePageTemplate(templateRenamed);
                ServerOperations.Templates().DeletePageTemplate(templateTitle);
                FeatherServerOperations.ResourcePackages().DeleteDirectory(path);
            }
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
