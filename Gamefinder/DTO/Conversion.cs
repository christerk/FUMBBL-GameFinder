using DtoCoach = Fumbbl.Gamefinder.DTO.Coach;
using ModelCoach = Fumbbl.Gamefinder.Model.Coach;

namespace Fumbbl.Gamefinder.DTO
{
    public static class Conversion
    {
        #region Coach
        public static ModelCoach ToModel(this DtoCoach coach)
        {
            return new ModelCoach()
            {
                Id = coach.Id
            };
        }

        public static DtoCoach ToDto(this ModelCoach coach)
        {
            return new DtoCoach()
            {
                Id = coach.Id
            };
        }
        #endregion
    }
}
