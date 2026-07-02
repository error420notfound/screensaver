namespace MujiScreenSaver.ViewModels;

public sealed class WeekDayViewModel : ViewModelBase
{
    private string _dayLabel = string.Empty;
    private string _dateLabel = string.Empty;
    private bool _isToday;

    public string DayLabel
    {
        get => _dayLabel;
        set => SetProperty(ref _dayLabel, value);
    }

    public string DateLabel
    {
        get => _dateLabel;
        set => SetProperty(ref _dateLabel, value);
    }

    public bool IsToday
    {
        get => _isToday;
        set => SetProperty(ref _isToday, value);
    }
}
