﻿using ConcurrentCollections;
using System.Collections.Concurrent;

namespace Fumbbl.Gamefinder.Model.Store
{
    internal class CoachStore
    {
        private readonly ILogger<MatchGraph> _logger;
        private readonly ConcurrentHashSet<Coach> _coaches;
        private readonly ConcurrentDictionary<Coach, DateTime> _lastEvents;

        public CoachStore(ILogger<MatchGraph> logger)
        {
            _logger = logger;
            _coaches = new();
            _lastEvents = new();
        }

        internal void Clear()
        {
            _coaches.Clear();
        }

        internal IEnumerable<Coach> GetCoaches()
        {
            return _coaches;
        }

        internal bool Contains(Coach coach)
        {
            return _coaches.Contains(coach);
        }

        internal bool IsTimedOut(Coach coach)
        {
            if (_lastEvents.TryGetValue(coach, out var lastEvent)) {
                return (DateTime.Now - lastEvent).TotalSeconds > Coach.COACH_TIMEOUT;
            }
            return true;
        }

        internal void Ping(Coach coach, int offsetSeconds)
        {
            if (_lastEvents.TryGetValue(coach, out var lastEvent))
            {
                DateTime newValue = DateTime.Now;

                if (newValue > lastEvent)
                {
                    newValue = newValue.AddSeconds(Math.Max(0, offsetSeconds - Coach.COACH_TIMEOUT)); // Subtract timeout seconds to get expiry time correct
                    _lastEvents.TryUpdate(coach, newValue, lastEvent);
                    _logger.LogTrace($"Set lastEvent for {coach} to {newValue}.");
                }
            }
            else
            {
                _logger.LogTrace($"Unable to set lastEvent for {coach}.");
            }
        }

        internal bool Add(Coach coach)
        {
            _lastEvents.TryAdd(coach, DateTime.Now);
            return _coaches.Add(coach);
        }

        internal bool Remove(Coach coach)
        {
            return _coaches.TryRemove(coach);
        }
    }
}