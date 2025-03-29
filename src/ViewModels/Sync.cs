using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SourceGit.ViewModels
{
    public class Sync : Popup
    {
        private readonly Repository _repo;
        private Models.Branch _selectedLocalBranch;
        private Models.Remote _selectedRemote;
        private Models.Branch _selectedRemoteBranch;
        private bool _isSetTrackOptionVisible;
        private bool _tracking = true;
        private bool _checkSubmodules = true;
        private bool _forcePush;

        public Sync(Repository repo, Models.Branch localBranch)
        {
            _repo = repo;
            // Initialize Pull and Push components
            PullComponent = new Pull(repo, null);
            PushComponent = new Push(repo, localBranch);
        }

        public Pull PullComponent { get; }
        public Push PushComponent { get; }

        public override async Task<bool> Sure()
        {
            _repo.SetWatcherEnabled(false);

            // Stash local changes
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

            // Perform Pull
            var pullSuccess = await PullComponent.Sure();
            if (!pullSuccess)
            {
                CallUIThread(() => _repo.SetWatcherEnabled(true));
                return false;
            }

            // Perform Push
            var pushSuccess = await PushComponent.Sure();
            if (!pushSuccess)
            {
                CallUIThread(() => _repo.SetWatcherEnabled(true));
                return false;
            }

            // Re-apply stashed changes if any
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

        public override bool CanStartDirectly()
        {
            return PullComponent.CanStartDirectly() && PushComponent.CanStartDirectly();
        }
    }
}
