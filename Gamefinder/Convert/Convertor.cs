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

using ApiCoach = Fumbbl.Api.DTO.Coach;
using ApiRaceLogo = Fumbbl.Api.DTO.RaceLogo;
using ApiTeam = Fumbbl.Api.DTO.Team;

namespace Fumbbl.Gamefinder.Convert
{
    public static class Convertor
    {
        private static readonly int UNKNOWN_LOGO = 486370;
        #region Coach
        public static ModelCoach ToModel(this ApiCoach apiCoach)
        {
            return new ModelCoach
            {
                Id = apiCoach.Id,
                Name = apiCoach.Name,
                Rating = apiCoach.Rating,
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
            var uiCoach = new UiCoach();
            uiCoach.Id = apiCoach.Id;
            uiCoach.Name = apiCoach.Name;
            uiCoach.Rating = apiCoach.Rating;

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
            var modelCoach = new ModelCoach();
            modelCoach.Id = dtoCoach.Id;
            modelCoach.Name = dtoCoach.Name;

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
                RosterLogo32 = apiTeam.RaceLogos.FirstOrDefault(l => l.Size == 32)?.Logo ?? UNKNOWN_LOGO,
                Season = apiTeam.Season?.Number ?? 0,
                SeasonGames = apiTeam.Season?.Games ?? 0,
                LeagueName = apiTeam.League ?? string.Empty,
                LeagueId = apiTeam.LeagueId ?? 0,
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
                Roster = new UiRoster(modelTeam.Roster, modelTeam.RosterLogo32),
                SeasonInfo = new UiSeasonInfo { CurrentSeason = modelTeam.Season, GamesPlayedInCurrentSeason = modelTeam.SeasonGames }
            };
        }

        public static UiTeam ToUi(this ApiTeam apiTeam, ApiCoach apiCoach)
        {
            UiLeague? uiLeague = null;

            if (apiTeam.LeagueId != null)
            {
                uiLeague = new UiLeague();
                uiLeague.Id = apiTeam.LeagueId ?? 0;
                uiLeague.Name = apiTeam.League ?? string.Empty;
            }

            var smallLogoId = apiTeam.RaceLogos.FirstOrDefault(l => l.Size == 32)?.Logo ?? UNKNOWN_LOGO;

            var uiRoster = new UiRoster(apiTeam.Race, smallLogoId);

            var uiSeasonInfo = new UiSeasonInfo();
            uiSeasonInfo.CurrentSeason = apiTeam.Season?.Number ?? 0;
            uiSeasonInfo.GamesPlayedInCurrentSeason = apiTeam.Season?.Games ?? 0;

            var uiTeam = new UiTeam();
            uiTeam.Id = apiTeam.Id;
            uiTeam.Name = apiTeam.Name;
            uiTeam.TeamValue = apiTeam.TeamValue;
            uiTeam.Division = apiTeam.Division;
            uiTeam.IsLfg = apiTeam.IsLfg == "Yes";
            uiTeam.League = uiLeague;
            uiTeam.Roster = uiRoster;
            uiTeam.Coach = apiCoach.ToUi();
            uiTeam.SeasonInfo = uiSeasonInfo;

            return uiTeam;
        }

        public static ModelTeam ToModel(this UiTeam dtoTeam, ModelCoach modelCoach)
        {
            var modelTeam = new ModelTeam(modelCoach);
            modelTeam.Id = dtoTeam.Id;
            modelTeam.Name = dtoTeam.Name;

            return modelTeam;
        }
        #endregion

        #region Match
        public static UiOffer ToUiOffer(this ModelBasicMatch modelBasicMatch)
        {
            var match = modelBasicMatch as ModelMatch;

            var c1 = modelBasicMatch.Team1.Coach.ToUi();
            var c2 = modelBasicMatch.Team2.Coach.ToUi();
            Console.WriteLine($"The coach {c1.Name}, {c2.Name}");

            return new UiOffer()
            {
                Team1Coach = modelBasicMatch.Team1.Coach.ToUi(),
                Team2Coach = modelBasicMatch.Team2.Coach.ToUi(),
                Team1 = modelBasicMatch.Team1.ToUi(),
                Team2 = modelBasicMatch.Team2.ToUi(),
                Id = $"{modelBasicMatch.Team1.Id} {modelBasicMatch.Team2.Id}",
                Lifetime = ModelMatch.DEFAULT_TIMEOUT * 1000,
                TimeRemaining = match?.TimeUntilReset ?? 0
            };
        }
        #endregion
    }
}