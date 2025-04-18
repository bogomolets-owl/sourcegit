using System.Threading.Tasks;

namespace SourceGit.ViewModels
{
    public class Sync : Popup
    {
        private readonly Repository _repo;
        private readonly Models.Branch _selectedLocalBranch;
        private readonly Models.Remote _selectedRemote;
        private readonly Models.Branch _selectedRemoteBranch;
        private readonly int _retryCount;

        public Sync(Repository repo, Models.Branch localBranch, Models.Remote remote, Models.Branch remoteBranch, int retryCount = 3)
        {
            _repo = repo;
            _selectedLocalBranch = localBranch;
            _selectedRemote = remote;
            _selectedRemoteBranch = remoteBranch;
            _retryCount = retryCount;
        }

        public override async Task<bool> Sure()
        {
            _repo.SetWatcherEnabled(false);

            var changes = new Commands.CountLocalChangesWithoutUntracked(_repo.FullPath).Result();
            var needPopStash = false;
            if (changes > 0)
            {
                SetProgressDescription("Stash local changes...");
                var succ = new Commands.Stash(_repo.FullPath).Push("SYNC_AUTO_STASH");
                if (!succ)
                {
                    CallUIThread(() => _repo.SetWatcherEnabled(true));
                    return false;
                }
                needPopStash = true;
            }

            for (int i = 0; i < _retryCount; i++)
            {
                var pullSuccess = await PerformPull();
                if (!pullSuccess)
                {
                    CallUIThread(() => _repo.SetWatcherEnabled(true));
                    return false;
                }

                var pushSuccess = await PerformPush();
                if (pushSuccess)
                {
                    if (needPopStash)
                    {
                        SetProgressDescription("Re-apply local changes...");
                        var popSuccess = new Commands.Stash(_repo.FullPath).Pop("stash@{0}");
                        if (!popSuccess)
                        {
                            CallUIThread(() => _repo.SetWatcherEnabled(true));
                            return false;
                        }
                    }

                    CallUIThread(() => _repo.SetWatcherEnabled(true));
                    return true;
                }
            }

            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return false;
        }

        private Task<bool> PerformPull()
        {
            return Task.Run(() =>
            {
                SetProgressDescription($"Pull {_selectedRemote.Name}/{_selectedRemoteBranch.Name}...");
                var rs = new Commands.Pull(
                    _repo.FullPath,
                    _selectedRemote.Name,
                    _selectedRemoteBranch.Name,
                    true, // Use rebase
                    _repo.Settings.FetchWithoutTagsOnPull,
                    SetProgressDescription).Exec();

                CallUIThread(() => _repo.SetWatcherEnabled(true));
                return rs;
            });
        }

        private Task<bool> PerformPush()
        {
            return Task.Run(() =>
            {
                var remoteBranchName = _selectedRemoteBranch.Name;
                SetProgressDescription($"Push {_selectedLocalBranch.Name} -> {_selectedRemote.Name}/{remoteBranchName} ...");

                var succ = new Commands.Push(
                    _repo.FullPath,
                    _selectedLocalBranch.Name,
                    _selectedRemote.Name,
                    remoteBranchName,
                    _repo.Settings.PushAllTags,
                    false, // No submodules check
                    false, // No tracking
                    false, // No force push
                    SetProgressDescription).Exec();
                CallUIThread(() => _repo.SetWatcherEnabled(true));
                return succ;
            });
        }
    }
}
