using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Drawing;
using BitbucketSharp.Models;
using System.Threading;
using BitbucketSharp;
using MonoTouch.Foundation;
using MonoTouch.Dialog.Utilities;

namespace BitbucketBrowser.UI
{
	public class ProfileController : Controller<UsersModel>, IImageUpdated
	{
        private HeaderView _header;

        public string Username { get; private set; }

		public ProfileController(string username, bool push = true) 
            : base(push)
		{
            Title = username;
			Username = username;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _header = new HeaderView(View.Bounds.Width) { Title = Username };
            Root.Add(new Section(_header));

            var followers = new StyledElement("Followers", () => NavigationController.PushViewController(new UserFollowersController(Username), true), Images.Heart);
            var events = new StyledElement("Events", () => NavigationController.PushViewController(new EventsController(Username) { ReportUser = false, ReportRepository = true }, true), Images.Event);
            var groups = new StyledElement("Groups", () => NavigationController.PushViewController(new GroupController(Username), true), Images.Group);
            var repos = new StyledElement("Repositories", () => NavigationController.PushViewController(new RepositoryController(Username) { Model = Model.Repositories }, true), Images.Repo);
            Root.Add(new [] { new Section { followers, events, groups }, new Section { repos } });
        }

        protected override void OnRefresh()
        {
            _header.Subtitle = Model.User.FirstName ?? "" + " " + Model.User.LastName ?? "";
            _header.Image = ImageLoader.DefaultRequestImage(new System.Uri(Model.User.Avatar), this);
            BeginInvokeOnMainThread(delegate { _header.SetNeedsDisplay(); });
        }

        protected override UsersModel OnUpdate()
        {
            return Application.Client.Users[Username].GetInfo();
        }

        public void UpdatedImage (System.Uri uri)
        {
            _header.Image = ImageLoader.DefaultRequestImage(uri, this);
            if (_header.Image != null)
                _header.SetNeedsDisplay();
        }
	}
}
