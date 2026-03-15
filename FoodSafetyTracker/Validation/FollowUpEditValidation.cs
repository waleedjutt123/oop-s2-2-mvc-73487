using FoodSafetyTracker.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FoodSafetyTracker.Validation;

public static class FollowUpEditValidation
{
    /// <summary>
   /// </summary>
    public static void ValidateClosedDateRequired(FollowUp followUp, ModelStateDictionary modelState)
    {
        if (followUp.Status == FollowUpStatus.Closed && !followUp.ClosedDate.HasValue)
            modelState.AddModelError(nameof(FollowUp.ClosedDate), "Closed date is required when status is Closed.");
    }
}
