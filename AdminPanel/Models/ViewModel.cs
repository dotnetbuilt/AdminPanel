using AdminPanel.Enums;

namespace AdminPanel.Models;

public class ViewModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTime LastLoginTime { get; set; }
    public DateTime RegistrationTime { get; set; }
    public Status Status { get; set; }
    public bool IsChecked { get; set; }
}
