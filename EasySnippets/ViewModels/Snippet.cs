namespace EasySnippets.ViewModels
{
    public class Snippet : ViewModelBase
    {
        private string _name;
        private string _value;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}