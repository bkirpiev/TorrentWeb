namespace SitefinityWebApp.Mvc.Controllers
{
    using SitefinityWebApp.Mvc.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Mvc;

    using Telerik.Sitefinity;
    using Telerik.Sitefinity.Data;
    using Telerik.Sitefinity.DynamicModules;
    using Telerik.Sitefinity.DynamicModules.Model;
    using Telerik.Sitefinity.Libraries.Model;
    using Telerik.Sitefinity.Lifecycle;
    using Telerik.Sitefinity.Model;
    using Telerik.Sitefinity.Modules.Libraries;
    using Telerik.Sitefinity.Mvc;
    using Telerik.Sitefinity.RelatedData;
    using Telerik.Sitefinity.Security;
    using Telerik.Sitefinity.Utilities.TypeConverters;
    using Telerik.Sitefinity.Versioning;
    using Telerik.Sitefinity.Workflow;

    [ControllerToolboxItem(Name = "AddTorrent", Title = "Add Torrent", SectionName = "MVCTorrents")]
    public class AddTorrentController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            TorrentModel torrentModel = new TorrentModel();
            PrepereViewBagModel();
            return View("Upsert", torrentModel);
        }

        [HttpPost]
        public ActionResult Upsert(TorrentModel model)
        {
            if (ModelState.IsValid)
            {
                var providerName = string.Empty;

                // Set a transaction name and get the version manager
                var transactionName = "someTransactionName";
                var versionManager = VersionManager.GetManager(null, transactionName);

                DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName, transactionName);
                Type torrentType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Torrents.Torrent");
                DynamicContent torrentItem = dynamicModuleManager.CreateDataItem(torrentType);

                // This is how values for the properties are set
                torrentItem.SetValue("Title", model.Title);
                torrentItem.SetValue("Description", model.Description);
                torrentItem.SetValue("AdditionalInfo", model.AdditionalInfo);
                // Set the selected value 
                torrentItem.SetValue("Genre", model.Genre);
                torrentItem.SetValue("CreatedOn", DateTime.Now);
                torrentItem.SetValue("Owner", SecurityManager.GetCurrentUserId());
                torrentItem.SetValue("PublicationDate", DateTime.UtcNow);

                torrentItem.SetString("UrlName", torrentItem.Id.ToString());

                var image = GetSavedImage(model);
                torrentItem.CreateRelation(image, "Image");

                var file = GetSavedDocument(model);
                torrentItem.CreateRelation(file, "File");

                torrentItem.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Draft");

                // Create a version and commit the transaction in order changes to be persisted to data store
                versionManager.CreateVersion(torrentItem, false);

                // We can now call the following to publish the item
                ILifecycleDataItem publishedTorrentItem = dynamicModuleManager.Lifecycle.Publish(torrentItem);

                // You need to set appropriate workflow status
                torrentItem.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Published");

                // Create a version and commit the transaction in order changes to be persisted to data store
                versionManager.CreateVersion(torrentItem, true);

                TransactionManager.CommitTransaction(transactionName);

                return RedirectToAction("Index", "Torrents");
            }

            PrepereViewBagModel();

            return View(model);
        }

        private void PrepereViewBagModel()
        {
            List<SelectListItem> choices = new List<SelectListItem>();

            choices.Add(new SelectListItem
            {
                Value = "1",
                Text = "Action"
            });

            choices.Add(new SelectListItem
            {
                Value = "2",
                Text = "Adventure"
            });

            choices.Add(new SelectListItem
            {
                Value = "3",
                Text = "Comedy"
            });

            choices.Add(new SelectListItem
            {
                Value = "4",
                Text = "Fantasy"
            });

            choices.Add(new SelectListItem
            {
                Value = "5",
                Text = "Historical"
            });

            choices.Add(new SelectListItem
            {
                Value = "6",
                Text = "Biography"
            });

            choices.Add(new SelectListItem
            {
                Value = "7",
                Text = "None"
            });

            ViewBag.Choices = choices;
        }

        private Image GetSavedImage(TorrentModel model)
        {
            LibrariesManager imageManager = LibrariesManager.GetManager();
            Album album = imageManager.GetAlbums().FirstOrDefault();

            var image = imageManager.CreateImage();

            image.Parent = album;
            image.Title = model.Image.FileName;
            image.DateCreated = DateTime.UtcNow;
            image.PublicationDate = DateTime.UtcNow;
            image.LastModified = DateTime.UtcNow;
            image.UrlName = Regex.Replace(string.Format("{0}-{1}", model.Image.FileName.ToLower(), image.Id.ToString()), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
            image.MediaFileUrlName = Regex.Replace(string.Format("{0}-{1}", model.Image.FileName.ToLower(), image.Id.ToString()), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");

            imageManager.Upload(image, model.Image.InputStream, Path.GetExtension(model.Image.FileName));
            imageManager.SaveChanges();

            //Publish the Albums item. The live version acquires new ID.
            var bag = new Dictionary<string, string>();
            bag.Add("ContentType", typeof(Image).FullName);
            WorkflowManager.MessageWorkflow(image.Id, typeof(Image), null, "Publish", false, bag);

            return image;
        }

        private Document GetSavedDocument(TorrentModel model)
        {
            LibrariesManager documentManager = LibrariesManager.GetManager();
            DocumentLibrary documentLibrary = documentManager.GetDocumentLibraries().FirstOrDefault();

            var document = documentManager.CreateDocument();

            //Set the properties of the document.
            document.Parent = documentLibrary;
            document.Title = model.File.FileName;
            document.DateCreated = DateTime.UtcNow;
            document.PublicationDate = DateTime.UtcNow;
            document.LastModified = DateTime.UtcNow;
            document.UrlName = Regex.Replace(string.Format("{0}-{1}", model.File.FileName.ToLower(), document.Id.ToString()), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
            document.MediaFileUrlName = Regex.Replace(string.Format("{0}-{1}", model.File.FileName.ToLower(), document.Id.ToString()), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");

            //Upload the document file.
            documentManager.Upload(document, model.File.InputStream, Path.GetExtension(model.File.FileName));
            documentManager.SaveChanges();

            //Publish the Albums item. The live version acquires new ID.
            var bag = new Dictionary<string, string>();
            bag.Add("ContentType", typeof(Document).FullName);
            WorkflowManager.MessageWorkflow(document.Id, typeof(Document), null, "Publish", false, bag);

            return document;
        }
    }
}