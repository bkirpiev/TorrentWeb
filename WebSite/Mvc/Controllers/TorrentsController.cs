namespace SitefinityWebApp.Mvc.Controllers
{
    using System;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Web.Mvc;
    using Telerik.Sitefinity.DynamicModules;
    using Telerik.Sitefinity.DynamicModules.Model;
    using Telerik.Sitefinity.GenericContent.Model;
    using Telerik.Sitefinity.Libraries.Model;
    using Telerik.Sitefinity.Modules.Libraries;
    using Telerik.Sitefinity.Mvc;
    using Telerik.Sitefinity.Utilities.TypeConverters;

    [ControllerToolboxItem(Name = "MVCTorrents", Title = "Mvc Torrents", SectionName = "MVCTorrents")]
    public class TorrentsController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var model = GetLiveTorrents();

            return View(model);
        }

        [HttpPost]
        public PartialViewResult SearchTorrents(string title)
        {
            var torrents = RetrieveTorrentsThroughFiltering(title);

            return PartialView(torrents);
        }

        public ActionResult Details(Guid id)
        {
            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(string.Empty);
            Type torrentType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Torrents.Torrent");

            DynamicContent torrentModel = dynamicModuleManager.GetDataItem(torrentType, id);

            return View(torrentModel);
        }

        public FileResult DownloadTorrent(Guid fileId)
        {
            LibrariesManager librariesManager = LibrariesManager.GetManager();
            var file = librariesManager.GetDocument(fileId);
            byte[] content = null;
            string fileName = string.Empty;


            if (file != null)
            {
                using (var stream = librariesManager.Download(file))
                {
                    content = new byte[stream.Length];
                    stream.Read(content, 0, content.Length);
                }
            }

            return File(content, "application/octet-stream", string.Format("{0}{1}", file.Title, file.Extension));
        }

        private IQueryable<DynamicContent> GetLiveTorrents()
        {
            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(string.Empty);
            Type torrentType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Torrents.Torrent");
            var torrents = dynamicModuleManager.GetDataItems(torrentType).Where(p => p.Status == ContentLifecycleStatus.Live && p.Visible == true);

            return torrents;
        }

        public IQueryable<DynamicContent> RetrieveTorrentsThroughFiltering(string title)
        {
            var providerName = string.Empty;

            // Set a transaction name
            var transactionName = "someTransactionName";

            DynamicModuleManager dynamicModuleManager = DynamicModuleManager.GetManager(providerName, transactionName);
            Type torrentType = TypeResolutionService.ResolveType("Telerik.Sitefinity.DynamicTypes.Model.Torrents.Torrent");

            // This is how we get the torrent items through filtering
            var torrents = dynamicModuleManager.GetDataItems(torrentType).Where(p => p.Status == ContentLifecycleStatus.Live && p.Visible == true);

            return torrents.Where(string.Format("Title.Contains(\"{0}\")", title));
        }
    }
}