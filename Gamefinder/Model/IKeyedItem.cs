namespace Fumbbl.Gamefinder.Model
{
    public interface IKeyedItem<K>
    {
        public K Key { get; }
    }
}