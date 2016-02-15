 namespace Assets.GameAssets.Scripts.UI.Controls
{
    public class DropdownOption<TKey,TVal>
    {
        public DropdownOption(TKey key, TVal val)
        {
            Key = key;
            Value = val;
        }

        public TKey Key { get; private set; }
        public TVal Value { get; private  set; }
    }
}