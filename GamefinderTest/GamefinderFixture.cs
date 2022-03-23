﻿using Fumbbl.Gamefinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamefinderTest
{
    public class GamefinderFixture : IDisposable
    {
        public readonly GamefinderModel GamefinderModel;
        public readonly MatchGraph MatchGraph;
        public readonly List<Coach> Coaches;
        public readonly List<Team> Teams;
        public GamefinderFixture()
        {
            MatchGraph = new();
            GamefinderModel = new(MatchGraph);
            Coaches = new();
            Teams = new();

            int teamId = 0;
            for (var i=0; i<20; i++)
            {
                Coaches.Add(CreateCoach(i));
                for (var t=0; t<3; t++)
                {
                    teamId++;
                    Teams.Add(new Team(Coaches[i]) { Id = teamId, Name=$"Team {teamId}" } );
                }
            }

            MatchGraph.Start();
        }

        public Coach CreateCoach(int id)
        {
            return new Coach() { Id = id, Name = $"Coach {id}" };
        }

        public Team SimpleTeam(int teamId, int coachId = 0)
        {
            if (coachId == 0)
            {
                coachId = teamId;
            }
            return new Team(CreateCoach(coachId)) { Id = teamId, Name = $"Team {teamId}" };
        }

        public void Dispose()
        {
            MatchGraph.Stop();
        }
    }
}