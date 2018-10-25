using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using RazorEngine;
using RazorEngine.Templating;
using System.Web.Hosting;
using System.Reflection;

namespace MyHomeSecureWeb.Utilities
{
    public class HtmlActionResult : IHttpActionResult
    {
        private readonly string _view;
        private readonly dynamic _model;

        private readonly string _contentFile;
        private readonly string _contentType;

        private static string ViewPath = "~/Views";
        private static string ContentPath = "~/Content";
        private static string AzurePath = @"D:\home\site\wwwroot\bin\";

        private static bool _initialised = false;
        private static object _initialiseLock = new object();
        private static void InitialiseTemplates()
        {
            lock(_initialiseLock)
            {
                if (!_initialised)
                {
                    var partials = GetTemplates("_*");
                    foreach (var partial in partials)
                    {
                        Razor.Compile(LoadView(partial), partial + ".cshtml");
                    }
                    _initialised = true;
                }
            }
        }

        public HtmlActionResult(string viewName, dynamic model)
        {
            InitialiseTemplates();

            _view = LoadView(viewName);
            _model = model;
        }

        public HtmlActionResult(string contentFile, string contentType)
        {
            _contentFile = contentFile;
            _contentType = contentType;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_contentFile))
            {
                return ExecuteContent();
            }
            else
            {
                return ExecuteView();
            }
        }

        private Task<HttpResponseMessage> ExecuteView()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var parsedView = Razor.Parse(_view, _model);

            response.Content = new StringContent(parsedView);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return Task.FromResult(response);
        }

        private Task<HttpResponseMessage> ExecuteContent()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var viewPath = Path.Combine(GetFolderPath(ContentPath), _contentFile);
            var viewBytes = File.ReadAllBytes(viewPath);

            response.Content = new ByteArrayContent(viewBytes);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(_contentType);
            return Task.FromResult(response);
        }

        private static string LoadView(string name)
        {
            var viewPath = Path.Combine(GetFolderPath(ViewPath), string.Format("{0}.cshtml", name));
            var view = File.ReadAllText(viewPath);
            return view;
        }

        private static string[] GetTemplates(string query)
        {
            var files = Directory.GetFiles(GetFolderPath(ViewPath), query + ".cshtml");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
        }

        private static string GetFolderPath(string folderName)
        {
            var path = HostingEnvironment.MapPath(folderName);
            if (!Directory.Exists(path))
            {
                folderName = folderName.Substring(folderName.IndexOf("~/") + 2);
                path = Path.Combine(AzurePath, folderName);
            }
            return path;
        }
    }
}
