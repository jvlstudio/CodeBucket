using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeBucket.Core.ViewModels;
using CodeBucket.Core.ViewModels.App;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Linq;

namespace CodeBucket.iOS.Views.App
{
	public class MenuView : MenuBaseViewController
    {
        private MenuElement _notifications;
		private Section _favoriteRepoSection;

	    public new MenuViewModel ViewModel
	    {
	        get { return (MenuViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
	    }

	    protected override void CreateMenuRoot()
		{
            var username = ViewModel.Account.Username;
			Title = username;
            var root = new RootElement(username);

            root.Add(new Section
            {
                new MenuElement("Profile", () => ViewModel.GoToProfileCommand.Execute(null), Images.Person),
            });

            var eventsSection = new Section { HeaderView = new MenuSectionView("Events") };
            eventsSection.Add(new MenuElement(username, () => ViewModel.GoToMyEvents.Execute(null), Images.Event));
//			if (ViewModel.Account.Teams != null && !ViewModel.Account.DontShowTeamEvents)
//				ViewModel.Account.Teams.ForEach(team => eventsSection.Add(new MenuElement(team, () => ViewModel.GoToOrganizationEventsCommand.Execute(x)), Images.Buttons.Event));
            root.Add(eventsSection);

            var repoSection = new Section() { HeaderView = new MenuSectionView("Repositories") };
			repoSection.Add(new MenuElement("Owned", () => ViewModel.GoToOwnedRepositoriesCommand.Execute(null), Images.Repo));
			//repoSection.Add(new MenuElement("Watching", () => NavPush(new WatchedRepositoryController(Application.Accounts.ActiveAccount.Username)), Images.RepoFollow));
            repoSection.Add(new MenuElement("Starred", () => ViewModel.GoToStarredRepositoriesCommand.Execute(null), Images.Star));
			repoSection.Add(new MenuElement("Explore", () => ViewModel.GoToExploreRepositoriesCommand.Execute(null), Images.Explore));
            root.Add(repoSection);
            
			if (ViewModel.PinnedRepositories.Count() > 0)
			{
				_favoriteRepoSection = new Section() { HeaderView = new MenuSectionView("Favorite Repositories".t()) };
				foreach (var pinnedRepository in ViewModel.PinnedRepositories)
					_favoriteRepoSection.Add(new PinnedRepoElement(pinnedRepository, ViewModel.GoToRepositoryCommand));
				root.Add(_favoriteRepoSection);
			}
			else
			{
				_favoriteRepoSection = null;
			}

//            var groupsTeamsSection = new Section() { HeaderView = new MenuSectionView("Collaborations".t()) };
//			if (ViewModel.Account.DontExpandTeamsAndGroups)
//            {
//                groupsTeamsSection.Add(new MenuElement("Groups".t(), () => NavPush(new GroupViewController(Application.Account.Username)), Images.Buttons.Group));
//                groupsTeamsSection.Add(new MenuElement("Teams".t(), () => NavPush(new TeamViewController()), Images.Team));
//            }
//            else
//            {
//				if (ViewModel.Account.Groups != null)
//					ViewModel.Account.Groups.ForEach(x => groupsTeamsSection.Add(new MenuElement(x.Name, () => NavPush(new GroupMembersViewController(Application.Account.Username, x.Slug, x.Members) { Title = x.Name }), Images.Buttons.Group)));
//				if (ViewModel.Account.Teams != null)
//					ViewModel.Account.Teams.ForEach(x => groupsTeamsSection.Add(new MenuElement(x, () => NavPush(new ProfileViewController(x)), Images.Team)));
//            }
//

            //There should be atleast 1 thing...
//            if (orgSection.Elements.Count > 0)
//                root.Add(orgSection);
//
            var infoSection = new Section() { HeaderView = new MenuSectionView("Info & Preferences".t()) };
            root.Add(infoSection);
			infoSection.Add(new MenuElement("Settings".t(), () => ViewModel.GoToSettingsCommand.Execute(null), Images.Cog));
            infoSection.Add(new MenuElement("About".t(), () => NavPush(new AboutViewController()), Images.Info));
            infoSection.Add(new MenuElement("Feedback & Support".t(), PresentUserVoice, Images.Flag));
            infoSection.Add(new MenuElement("Accounts".t(), () => ProfileButtonClicked(this, System.EventArgs.Empty), Images.User));
            Root = root;
		}

        private void PresentUserVoice()
        {
			var config = UserVoice.UVConfig.Create("http://codebucket.uservoice.com", "pnuDmPENErDiDpXrms1DTg", "iDboMdCIwe2E5hJFa8hy9K9I5wZqnjKCE0RPHLhZIk");
			UserVoice.UserVoice.PresentUserVoiceInterface(this, config);
        }

        protected override void ProfileButtonClicked(object sender, System.EventArgs e)
        {
            ViewModel.GoToAccountsCommand.Execute(null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			TableView.SeparatorInset = UIEdgeInsets.Zero;
			TableView.SeparatorColor = UIColor.FromRGB(50, 50, 50);

			ProfileButton.Uri = new System.Uri(ViewModel.Account.AvatarUrl);

            ViewModel.Bind(x => x.Notifications, x =>
            {
                _notifications.NotificationNumber = x;
                Root.Reload(_notifications, UITableViewRowAnimation.None);
            });

            ViewModel.LoadCommand.Execute(null);
        }

		private class PinnedRepoElement : MenuElement
		{
			public CodeFramework.Core.Data.PinnedRepository PinnedRepo
			{
				get;
				private set; 
			}

			public PinnedRepoElement(CodeFramework.Core.Data.PinnedRepository pinnedRepo, System.Windows.Input.ICommand command)
				: base(pinnedRepo.Name, () => command.Execute(new CodeBucket.Core.Utils.RepositoryIdentifier { Owner = pinnedRepo.Owner, Name = pinnedRepo.Name }), Images.Repo)
			{
				PinnedRepo = pinnedRepo;
				ImageUri = new System.Uri(PinnedRepo.ImageUri);
			}
		}

		private void DeletePinnedRepo(PinnedRepoElement el)
		{
			ViewModel.DeletePinnedRepositoryCommand.Execute(el.PinnedRepo);

			if (_favoriteRepoSection.Elements.Count == 1)
			{
				Root.Remove(_favoriteRepoSection);
				_favoriteRepoSection = null;
			}
			else
			{
				_favoriteRepoSection.Remove(el);
			}
		}

		public override DialogViewController.Source CreateSizingSource(bool unevenRows)
		{
			return new EditSource(this);
		}

		private class EditSource : SizingSource
		{
			private readonly MenuView _parent;
			public EditSource(MenuView dvc) 
				: base (dvc)
			{
				_parent = dvc;
			}

			public override bool CanEditRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection == null)
					return false;
				if (_parent.Root[indexPath.Section] == _parent._favoriteRepoSection)
					return true;
				return false;
			}

			public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				if (_parent._favoriteRepoSection != null && _parent.Root[indexPath.Section] == _parent._favoriteRepoSection)
					return UITableViewCellEditingStyle.Delete;
				return UITableViewCellEditingStyle.None;
			}

			public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				switch (editingStyle)
				{
					case UITableViewCellEditingStyle.Delete:
						var section = _parent.Root[indexPath.Section];
						var element = section[indexPath.Row];
						_parent.DeletePinnedRepo(element as PinnedRepoElement);
						break;
				}
			}
		}
    }
}
