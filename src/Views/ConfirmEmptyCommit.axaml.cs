using Avalonia.Interactivity;

namespace SourceGit.Views
{
    public partial class ConfirmEmptyCommit : ChromelessWindow
    {
        public ConfirmEmptyCommit()
        {
            InitializeComponent();
        }

        private void StageAllThenCommit(object _1, RoutedEventArgs _2)
        {
            (DataContext as ViewModels.ConfirmEmptyCommit)?.StageAllThenCommit();
            Close();
        }

        private void Continue(object _1, RoutedEventArgs _2)
        {
            (DataContext as ViewModels.ConfirmEmptyCommit)?.Continue();
            Close();
        }

        private void CloseWindow(object _1, RoutedEventArgs _2)
        {
            Close();
        }
    }
}
