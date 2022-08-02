using Fumbbl.Api;
using Fumbbl.Gamefinder.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        public readonly BlackboxModel BlackboxModel;
        public readonly List<Coach> Coaches;
        public readonly List<Team> Teams;
        public ILoggerFactory? LoggerFactory { get; }

        public GamefinderFixture()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
                .AddConsole()
                .AddFilter(level => level >= LogLevel.Information)
                );
            LoggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            EventQueue queue = new EventQueue(LoggerFactory?.CreateLogger<EventQueue>());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8604 // Possible null reference argument.
            GamefinderModel = new(queue, LoggerFactory, null);
            BlackboxModel = new(LoggerFactory, null);
            BlackboxModel.DisableScheduler();
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            GamefinderModel.DisableEventHandling();
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
        }

        public Coach CreateCoach(int id)
        {
            return new Coach() { Id = id, Name = $"Coach {id}", CanLfg = true };
        }

        public Team SimpleTeam(int teamId, Coach? coach = null, int teamValue = 1000000)
        {
            if (coach is null)
            {
                coach = CreateCoach(teamId);
            }
            var team = new Team(coach) { Id = teamId, Name = $"Team {teamId}", SchedulingTeamValue = teamValue, Status="Active", Division="Competitive" };

            return team;
        }

        public void Dispose()
        {
            GamefinderModel.Stop();
        }
    }
}
