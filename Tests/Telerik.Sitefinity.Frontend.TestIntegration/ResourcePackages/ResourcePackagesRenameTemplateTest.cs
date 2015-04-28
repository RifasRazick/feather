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
    public class ResourcePackagesRenameTemplateTest
    {
        [FixtureSetUp]
        public void FixtureSetUp()
        {
            string packageResource = "Telerik.Sitefinity.Frontend.TestUtilities.Data.SomePackage.zip";
            FeatherServerOperations.ResourcePackages().AddNewResourcePackage(packageResource);
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
            string packageName = "SomePackage";
            string viewFileName = "Default.cshtml";
            string widgetName = "MvcTest";
            string fileResource = "Telerik.Sitefinity.Frontend.TestUtilities.Data.Default.cshtml";
            string templateTitle = "SomePackage.test-layout";
            string viewText = "This is a view from package.";
            string layoutText = "SomePackage - test layout";
            string templateRenamed = "SomePackage.TemplateRenamed";

            try
            {
                // Verify template is generated successfully
                FeatherServerOperations.ResourcePackages().WaitForTemplatesCountToIncrease();
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
    }
}
