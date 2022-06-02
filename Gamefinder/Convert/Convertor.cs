using UiCoach = Fumbbl.Gamefinder.DTO.Coach;
using UiOpponent = Fumbbl.Gamefinder.DTO.Opponent;
using UiLeague = Fumbbl.Gamefinder.DTO.League;
using UiRoster = Fumbbl.Gamefinder.DTO.Roster;
using UiSeasonInfo = Fumbbl.Gamefinder.DTO.SeasonInfo;
using UiTeam = Fumbbl.Gamefinder.DTO.Team;
using UiOffer = Fumbbl.Gamefinder.DTO.Offer;

using ModelCoach = Fumbbl.Gamefinder.Model.Coach;
using ModelTeam = Fumbbl.Gamefinder.Model.Team;
using ModelBasicMatch = Fumbbl.Gamefinder.Model.BasicMatch;
using ModelMatch = Fumbbl.Gamefinder.Model.Match;
using ModelTournament= Fumbbl.Gamefinder.Model.Tournament;

using ApiCoach = Fumbbl.Api.DTO.Coach;
using ApiRaceLogo = Fumbbl.Api.DTO.RaceLogo;
using ApiTeam = Fumbbl.Api.DTO.Team;
using ApiTournament = Fumbbl.Api.DTO.Tournament;

namespace Fumbbl.Gamefinder.Convert
{
    public static class Convertor
    {
        private static readonly int UNKNOWN_LOGO_32 = 486370;
        private static readonly int UNKNOWN_LOGO_64 = 486371;
        #region Coach
        public static ModelCoach ToModel(this ApiCoach apiCoach)
        {
            return new ModelCoach
            {
                Id = apiCoach.Id,
                Name = apiCoach.Name,
                Rating = apiCoach.Rating,
                CanLfg = apiCoach.CanLfg,
            };
        }

        public static UiCoach ToUi(this ModelCoach modelCoach)
        {
            return new UiCoach
            {
                Id = modelCoach.Id,
                Name = modelCoach.Name,
                Rating = modelCoach.Rating
            };
        }

        public static UiCoach ToUi(this ApiCoach apiCoach)
        {
            var uiCoach = new UiCoach
            {
                Id = apiCoach.Id,
                Name = apiCoach.Name,
                Rating = apiCoach.Rating
            };

            return uiCoach;
        }

        public static UiOpponent ToOpponent(this ModelCoach modelCoach)
        {
            return new UiOpponent
            {
                Id = modelCoach.Id,
                Name = modelCoach.Name,
                Ranking = modelCoach.Rating
            };
        }

        public static ModelCoach ToModel(this UiCoach dtoCoach)
        {
            var modelCoach = new ModelCoach
            {
                Id = dtoCoach.Id,
                Name = dtoCoach.Name
            };

            return modelCoach;
        }
        #endregion

        #region Team
        public static ModelTeam ToModel(this ApiTeam apiTeam, ModelCoach modelCoach)
        {
            return new ModelTeam(modelCoach)
            {
                Id = apiTeam.Id,
                Name = apiTeam.Name,
                TeamValue = apiTeam.TeamValue,
                Division = apiTeam.Division,
                Roster = apiTeam.Race,
                RosterLogo32 = apiTeam.RaceLogos.FirstOrDefault(l => l.Size == 32)?.Logo ?? UNKNOWN_LOGO_32,
                RosterLogo64 = apiTeam.RaceLogos.FirstOrDefault(l => l.Size == 64)?.Logo ?? UNKNOWN_LOGO_64,
                Season = apiTeam.Season?.Number ?? 0,
                SeasonGames = apiTeam.Season?.Games ?? 0,
                LeagueName = apiTeam.League ?? string.Empty,
                LeagueId = apiTeam.LeagueId ?? 0,
                Status = apiTeam.Status,
                Tournament = apiTeam?.Tournament?.ToModel(),
                Ruleset = apiTeam?.Ruleset ?? 0,
                AllowCrossLeagueMatches = apiTeam?.Options.CrossLeagueMatches ?? false
            };
        }

        public static UiTeam ToUi(this ModelTeam modelTeam)
        {
            return new UiTeam
            {
                Id = modelTeam.Id,
                Name = modelTeam.Name,
                IsLfg = true,
                TeamValue = modelTeam.TeamValue,
                Division = modelTeam.Division,
                Coach = modelTeam.Coach.ToUi(),
                League = new UiLeague { Id = modelTeam.LeagueId, Name = modelTeam.LeagueName },
                Roster = new UiRoster(modelTeam.Roster, modelTeam.RosterLogo32, modelTeam.RosterLogo64),
                SeasonInfo = new UiSeasonInfo { CurrentSeason = modelTeam.Season, GamesPlayedInCurrentSeason = modelTeam.SeasonGames },
                IsInTournament = modelTeam.Tournament?.Opponents.Any() != null
            };
        }

        public static UiTeam ToUi(this ApiTeam apiTeam, ApiCoach apiCoach)
        {
            UiLeague? uiLeague = null;

            if (apiTeam.LeagueId != null && apiTeam.LeagueId != 0)
            {
                uiLeague = new UiLeague
                {
                    Id = apiTeam.LeagueId ?? 0,
                    Name = apiTeam.League ?? string.Empty
                };
            }

            var logoId32 = apiTeam.RaceLogos.FirstOrDefault(l => l.Size == 32)?.Logo ?? UNKNOWN_LOGO_32;
            var logoId64 = apiTeam.RaceLogos.FirstOrDefault(l => l.Size == 64)?.Logo ?? UNKNOWN_LOGO_64;

            var uiRoster = new UiRoster(apiTeam.Race, logoId32, logoId64);

            var uiSeasonInfo = new UiSeasonInfo
            {
                CurrentSeason = apiTeam.Season?.Number ?? 0,
                GamesPlayedInCurrentSeason = apiTeam.Season?.Games ?? 0
            };

            var uiTeam = new UiTeam
            {
                Id = apiTeam.Id,
                Name = apiTeam.Name,
                TeamValue = apiTeam.TeamValue,
                Division = apiTeam.Division,
                IsLfg = apiTeam.IsLfg == "Yes",
                League = uiLeague,
                Roster = uiRoster,
                Coach = apiCoach.ToUi(),
                SeasonInfo = uiSeasonInfo
            };

            return uiTeam;
        }

        public static ModelTeam ToModel(this UiTeam dtoTeam, ModelCoach modelCoach)
        {
            var modelTeam = new ModelTeam(modelCoach)
            {
                Id = dtoTeam.Id,
                Name = dtoTeam.Name
            };

            return modelTeam;
        }
        #endregion

        #region Match
        public static UiOffer ToUiOffer(this ModelBasicMatch modelBasicMatch)
        {
            var match = modelBasicMatch as ModelMatch;

            return new UiOffer()
            {
                Team1 = modelBasicMatch.Team1.ToUi(),
                Team2 = modelBasicMatch.Team2.ToUi(),
                Id = $"{modelBasicMatch.Team1.Id} {modelBasicMatch.Team2.Id}",
                Lifetime = ModelMatch.DEFAULT_TIMEOUT * 1000,
                TimeRemaining = match?.TimeUntilReset ?? 0,
                Visible = !modelBasicMatch.MatchState.IsHidden,
                LaunchGame = modelBasicMatch.MatchState.TriggerLaunchGame,
                ClientId = modelBasicMatch.ClientId,
                SchedulingError = modelBasicMatch.SchedulingError,
            };
        }
        #endregion

        #region Tournament
        public static ModelTournament ToModel(this ApiTournament apiTournament)
        {
            return new ModelTournament()
            {
                Id = apiTournament.Id,
                Opponents = apiTournament.Opponents.ToList()
            };
        }
        #endregion
}
}