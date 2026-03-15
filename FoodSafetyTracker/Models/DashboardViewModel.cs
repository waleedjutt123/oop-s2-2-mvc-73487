namespace FoodSafetyTracker.Models;

public class DashboardViewModel
{
    public int InspectionsThisMonth { get; set; }
    public int FailedInspectionsThisMonth { get; set; }
    public int OverdueOpenFollowUps { get; set; }

    public string? SelectedTown { get; set; }
    public RiskRating? SelectedRiskRating { get; set; }
    public List<string> Towns { get; set; } = new();
    public List<RiskRating> RiskRatings { get; set; } = new() { RiskRating.Low, RiskRating.Medium, RiskRating.High };
}
