﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SourceGit.Commands
{
    /// <summary>
    /// Query stash changes. Requires git >= 2.32.0
    /// </summary>
    public partial class QueryStashChanges : Command
    {
        [GeneratedRegex(@"^([MADRC])\s+(.+)$")]
        private static partial Regex REG_FORMAT();

        public QueryStashChanges(string repo, string stash) 
        {
            WorkingDirectory = repo;
            Context = repo;
            Args = $"stash show -u --name-status \"{stash}\"";
        }

        public List<Models.Change> Result()
        {
            var rs = ReadToEnd();
            if (!rs.IsSuccess)
                return [];

            var lines = rs.StdOut.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            var outs = new List<Models.Change>();
            foreach (var line in lines)
            {
                var match = REG_FORMAT().Match(line);
                if (!match.Success)
                    continue;

                var change = new Models.Change(WorkingDirectory, match.Groups[2].Value);
                var status = match.Groups[1].Value;

                switch (status[0])
                {
                    case 'M':
                        change.Set(Models.ChangeState.Modified);
                        outs.Add(change);
                        break;
                    case 'A':
                        change.Set(Models.ChangeState.Added);
                        outs.Add(change);
                        break;
                    case 'D':
                        change.Set(Models.ChangeState.Deleted);
                        outs.Add(change);
                        break;
                    case 'R':
                        change.Set(Models.ChangeState.Renamed);
                        outs.Add(change);
                        break;
                    case 'C':
                        change.Set(Models.ChangeState.Copied);
                        outs.Add(change);
                        break;
                }
            }

            outs.Sort((l, r) => string.Compare(l.Path, r.Path, StringComparison.Ordinal));
            return outs;
        }
    }
}
