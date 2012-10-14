using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Text;
using System.Net;
using RedPlum;
using System.Threading;
using BitbucketBrowser.UI;
using CodeFramework.UI.Views;
using MonoTouch;

namespace BitbucketBrowser
{
	public partial class LoginViewController : UIViewController
	{

        public Action LoginComplete;

		public LoginViewController() : base ("LoginViewController", null)
		{
		}
		
		public override void DidReceiveMemoryWarning()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning();
		}
		
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();


            View.BackgroundColor = UIColor.FromPatternImage(Images.LogoBehind);

            Logo.Image = Images.Logo;
            Title = "Add Account";
			
			User.ShouldReturn = delegate {
				Password.BecomeFirstResponder();
				return true;
			};
			Password.ShouldReturn = delegate {
				Password.ResignFirstResponder();

                //Run this in another thread
                ThreadPool.QueueUserWorkItem(delegate { BeginLogin(); });
				return true;
			};
		}
		
		public override void ViewDidUnload()
		{
			base.ViewDidUnload();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets();
		}
		
		private void BeginLogin()
        {
            MBProgressHUD hud;
            bool successful = false;
            string username = null, password = null;

            //The nice hud
            InvokeOnMainThread(delegate {
                username = User.Text;
                password = Password.Text;
                hud = new MBProgressHUD(this.View); 
                hud.Mode = MBProgressHUDMode.Indeterminate;
                hud.TitleText = "Logging In...";
                this.View.AddSubview(hud);
                hud.Show(true);
            });

            try
            {
                var client = new BitbucketSharp.Client(username, password);
                client.Account.SSHKeys.GetKeys();
                successful = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error = " + e.Message);
            }


            InvokeOnMainThread(delegate {
                //Dismiss the hud
                hud.Hide(true);
                hud.RemoveFromSuperview();

                if (!successful)
                {
                    Utilities.ShowAlert("Unable to Authenticate", "Unable to login as user " + username + ". Please check your credentials and try again. Remember, credentials are case sensitive!");
                    return;
                }

                var newAccount = new Account() { Username = User.Text, Password = Password.Text };

                if (Application.Accounts.Exists(newAccount))
                {
                    Utilities.ShowAlert("Unable to Add User", "That user already exists!");
                    return;
                }

                //Logged in correctly!
                //Go back to the other view and add the username
                Application.Accounts.Insert(newAccount);

                if (NavigationController != null)
                    NavigationController.PopViewControllerAnimated(true);

                if (LoginComplete != null)
                    LoginComplete();
            });
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                if (toInterfaceOrientation == UIInterfaceOrientation.Portrait || toInterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown)
                    return true;
            }
            else
            {
    			// Return true for supported orientations
                return true;
            }

            return false;
		}
	}
}
