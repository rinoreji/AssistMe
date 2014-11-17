using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Util.Store;
using System;
using System.Web;
using System.Web.Mvc;

namespace AssistMe.Helpers
{
    public class AppFlowMetadata : FlowMetadata
    {
        private static readonly IAuthorizationCodeFlow flow =
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "clientid",
                    ClientSecret = "secret"
                },
                Scopes = new[] { DriveService.Scope.Drive, Oauth2Service.Scope.UserinfoEmail },
                DataStore = GetFileDataStore()
            });

        static FileDataStore GetFileDataStore()
        {
            var path = HttpContext.Current.Server.MapPath("~/App_Data/Drive.Api.Auth.Store");
            var store = new FileDataStore(path, fullPath: true);
            return store;
        }

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should have a logic to identify
            // a user. You might want to use "OpenID Connect".
            // You can read more about the protocol in the following link:
            // https://developers.google.com/accounts/docs/OAuth2Login.
            var user = controller.Session["user"];
            if (user == null)
            {
                user = Guid.NewGuid();
                controller.Session["user"] = user;
            }
            return user.ToString();

        }

        public override IAuthorizationCodeFlow Flow
        {
            get { return flow; }
        }

        public override string AuthCallback
        {
            get
            {
                return GetApplicationPath() + base.AuthCallback;
            }
        }

        string GetApplicationPath()
        {
            var path = HttpContext.Current.Request.ApplicationPath;
            if (path == "/")
                return string.Empty;

            return path;
        }
    }
}