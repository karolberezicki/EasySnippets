namespace EasySnippets.ViewModels;

public class Snippet : ViewModelBase
{
    public string Name
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Value
    {
        get;
        set => SetProperty(ref field, value);
    }
}