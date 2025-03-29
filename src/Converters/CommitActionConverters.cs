using Avalonia.Data.Converters;

namespace SourceGit.Converters
{
    public static class CommitActionConverters
    {
        public static readonly FuncValueConverter<ViewModels.CommitAction, bool> IsCommitOnly = 
            new(value => value == ViewModels.CommitAction.Commit);
        
        public static readonly FuncValueConverter<ViewModels.CommitAction, bool> IsCommitWithPush = 
            new(value => value == ViewModels.CommitAction.CommitWithPush);
        
        public static readonly FuncValueConverter<ViewModels.CommitAction, bool> IsCommitWithPullPush = 
            new(value => value == ViewModels.CommitAction.CommitWithPullPush);
    }
}
