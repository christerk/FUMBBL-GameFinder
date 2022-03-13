﻿
using ConcurrentCollections;
using Fumbbl.Gamefinder.Model.Event;
using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model
{
    public class MatchGraph
    {
        private readonly GamefinderModel _gameFinder;
        private readonly BlockingCollection<Action> _eventQueue;
        private readonly ConcurrentHashSet<Team> _teams;
        private readonly ConcurrentHashSet<Coach> _coaches;
        private readonly ConcurrentHashSet<Match> _matches;

        public event EventHandler? CoachAdded;
        public event EventHandler? CoachRemoved;
        public event EventHandler? TeamAdded;
        public event EventHandler? TeamRemoved;
        public event EventHandler? MatchAdded;
        public event EventHandler? MatchRemoved;
        public event EventHandler? UpdateComplete;
        public MatchGraph(GamefinderModel gameFinder)
        {
            _gameFinder = gameFinder;
            _teams = new();
            _coaches = new();
            _matches = new();
            _eventQueue = new();
        }

        public void Start()
        {
            Task.Run(() =>
            {
                while (!_eventQueue.IsAddingCompleted)
                {
                    if (_eventQueue.TryTake(out Action? action, TimeSpan.FromSeconds(1)))
                    {
                        action.Invoke();
                    }
                    else
                    {
                        Tick();
                        UpdateComplete?.Invoke(this, EventArgs.Empty);
                    }
                }
                Console.WriteLine($"MatchGraph Task Ending");
            });
        }

        public void Stop()
        {
            Console.WriteLine($"Halting MatchGraph Task");
            _eventQueue.CompleteAdding();
        }

        public void Tick()
        {
            foreach (var match in _matches)
            {
                match.Tick();
            }

            foreach (var coach in _coaches)
            {
                if (coach.IsTimedOut)
                {
                    Console.WriteLine($"{coach} timed out");
                    RemoveCoach(coach);
                }
            }
        }

        internal void TriggerLaunchGame(Match match)
        {
            var coach1 = match.Team1.Coach;
            var coach2 = match.Team2.Coach;

            coach1.Lock();
            coach2.Lock();

            foreach (var team in coach1.GetTeams().Concat(coach2.GetTeams()))
            {
                foreach (var m in team.GetMatches())
                {
                    if (!m.Equals(match))
                    {
                        m.Act(TeamAction.Cancel);
                    }
                }
            }
        }

        public void AddTeam(Team team) => _eventQueue.Add(() => InternalAddTeam(team));
        public void RemoveTeam(Team team) => _eventQueue.Add(() => InternalRemoveTeam(team));
        public void AddCoach(Coach coach) => _eventQueue.Add(() => InternalAddCoach(coach));
        public void RemoveCoach(Coach coach) => _eventQueue.Add(() => InternalRemoveCoach(coach));
        public void AddTeamToCoach(Team team, Coach coach) => _eventQueue.Add(() => InternalAddTeamToCoach(team, coach));

        public IEnumerable<Team> GetTeams()
        {
            return _teams;
        }

        public IEnumerable<Match> GetMatches()
        {
            return _matches;
        }

        public Match? GetMatch(Team team1, Team team2)
        {
            return team1.GetMatches().Where(m => m.Includes(team2)).FirstOrDefault();
        }

        public IEnumerable<Match> GetMatches(Coach coach)
        {
            return coach.GetTeams().SelectMany(team => team.GetMatches());
        }

        private void InternalAddTeam(Team team)
        {
            if (team is null || _teams.Contains(team))
            {
                return;
            }

            if (!_coaches.Contains(team.Coach))
            {
                Console.WriteLine($"Adding {team.Coach}");
                _coaches.Add(team.Coach);
                CoachAdded?.Invoke(this, new CoachUpdatedArgs { Coach = team.Coach });
            }

            Console.WriteLine($"Adding {team}");
            _teams.Add(team);
            TeamAdded?.Invoke(this, new TeamUpdatedArgs { Team = team });
            foreach (var opponent in _teams)
            {
                if (team is not null && team.IsOpponentAllowed(opponent) && !opponent.Coach.Locked)
                {
                    var m = new Match(this, opponent, team);
                    Console.WriteLine($"Adding {m}");
                    _matches.Add(m);
                    opponent.Add(m);
                    team.Add(m);
                    MatchAdded?.Invoke(this, new MatchUpdatedArgs { Match = m });
                }
            }
        }

        private void InternalRemoveTeam(Team team)
        {
            if (team is null || !_teams.Contains(team))
            {
                return;
            }

            foreach (var match in team.GetMatches())
            {
                var t = match.GetOpponent(team);
                if (t is not null)
                {
                    Console.WriteLine($"Removing {match}");
                    t.Remove(match);
                    _matches.TryRemove(match);
                    MatchRemoved?.Invoke(this, new MatchUpdatedArgs { Match = match });

                }
            }
            _teams.TryRemove(team);
            TeamRemoved?.Invoke(this, new TeamUpdatedArgs { Team = team });
        }

        private void InternalAddCoach(Coach coach)
        {
            Console.WriteLine($"Adding {coach}");
            if (!_coaches.Contains(coach))
            {
                _coaches.Add(coach);
                CoachAdded?.Invoke(this, new CoachUpdatedArgs { Coach = coach });
            }

            foreach (var team in coach.GetTeams())
            {
                InternalAddTeam(team);
            }
        }

        private void InternalRemoveCoach(Coach coach)
        {
            Console.WriteLine($"Removing {coach}");

            foreach (var team in coach.GetTeams())
            {
                InternalRemoveTeam(team);
            }
            _coaches.TryRemove(coach);
            CoachRemoved?.Invoke(this, new CoachUpdatedArgs { Coach = coach });
        }

        private void InternalAddTeamToCoach(Team team, Coach coach)
        {
            coach.Add(team);
        }


    }
}
