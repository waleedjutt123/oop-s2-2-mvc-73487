using FoodSafetyTracker.Models;
using FoodSafetyTracker.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace FoodSafetyTracker.Tests;

public class FollowUpClosedValidationTests
{
    [Fact]
    public void FollowUp_ClosedWithoutClosedDate_ValidationFails()
    {
        var followUp = new FollowUp
        {
            Id = 1,
            InspectionId = 1,
            DueDate = DateTime.Today,
            Status = FollowUpStatus.Closed,
            ClosedDate = null
        };
        var modelState = new ModelStateDictionary();

        FollowUpEditValidation.ValidateClosedDateRequired(followUp, modelState);

        Assert.False(modelState.IsValid);
        Assert.True(modelState.ContainsKey(nameof(FollowUp.ClosedDate)));
    }

    [Fact]
    public void FollowUp_ClosedWithClosedDate_ValidationPasses()
    {
        var followUp = new FollowUp
        {
            Id = 1,
            InspectionId = 1,
            DueDate = DateTime.Today,
            Status = FollowUpStatus.Closed,
            ClosedDate = DateTime.Today
        };
        var modelState = new ModelStateDictionary();

        FollowUpEditValidation.ValidateClosedDateRequired(followUp, modelState);

        Assert.True(modelState.IsValid);
    }
}
