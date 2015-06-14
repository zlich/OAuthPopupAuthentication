using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using OAuthPopupAuthentication.Models;
using Octokit;

namespace OAuthPopupAuthentication.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
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

            return View(userDisplayModel);
        }

        public ActionResult AuthorizeGitHub()
        {
            return new ChallengeResult("GitHub", Url.Action("Index"));
        }
    }
}