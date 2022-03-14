namespace Fumbbl.Gamefinder.Model
{
    public class GamefinderModel
    {
        private int _counter = 0;
        public int Counter
        {
            get
            {
                _counter++;
                return _counter;
            }
        }
    }
}
