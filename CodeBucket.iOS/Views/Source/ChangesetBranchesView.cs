using CodeFramework.ViewControllers;
using CodeBucket.Core.ViewModels.Source;
using MonoTouch.Dialog;

namespace CodeBucket.iOS.Views.Source
{
    public class ChangesetBranchesView : ViewModelCollectionDrivenDialogViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Changeset Branch".t();
            NoItemsText = "No Branches".t();

            base.ViewDidLoad();

            var vm = (ChangesetBranchesViewModel) ViewModel;
            BindCollection(vm.Branches, x => new StyledStringElement(x.Name, () => vm.GoToBranchCommand.Execute(x)));
        }
    }
}

