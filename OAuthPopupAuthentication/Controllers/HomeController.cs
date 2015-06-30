using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using OAuthPopupAuthentication.Models;
using Octokit;

namespace OAuthPopupAuthentication.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var userProfile = await GetUserProfile();

            return View(userProfile);
        }

        public async Task<ActionResult> UserProfilePartial()
        {
            var userProfile = await GetUserProfile();

            return PartialView("_UserProfilePartial", userProfile);
        }

        public ActionResult AuthorizeGitHub(string connectionId)
        {
            return new ChallengeResult("GitHub", Url.Action("AuthorizeGitHubCallback", new { connectionId = connectionId }));
        }

        public ActionResult AuthorizeGitHubCallback(string connectionId)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<AuthenticationHub>();
            hubContext.Clients.Clients(new[] { connectionId }).refreshPage();

            // Render output script to close the popup window
            return Content(@"
                <script>
                    window.close();
                </script>
            ");
        }

        public async Task<UserDisplayModel> GetUserProfile()
        {
            var userDisplayModel = new UserDisplayModel();

            var authenticateResult = await HttpContext.GetOwinContext().Authentication.AuthenticateAsync("ExternalCookie");
            if (authenticateResult != null)
            {
                var tokenClaim = authenticateResult.Identity.Claims.FirstOrDefault(claim => claim.Type == "urn:token:github");
                if (tokenClaim != null)
                {
                    var accessToken = tokenClaim.Value;

                    var gitHubClient = new GitHubClient(new ProductHeaderValue("OAuthTestClient"));
                    gitHubClient.Credentials = new Credentials(accessToken);

                    var user = await gitHubClient.User.Current();

                    userDisplayModel.AccessToken = accessToken;
                    userDisplayModel.Name = user.Name;
                    userDisplayModel.AvatarUrl = user.AvatarUrl;
                }
            }
            return userDisplayModel;
        }

    }
}