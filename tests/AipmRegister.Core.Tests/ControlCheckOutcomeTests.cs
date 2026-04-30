using System.Net;
using AipmRegister.Core.Api;
using AipmRegister.Core.Models;

namespace AipmRegister.Core.Tests;

public sealed class ControlCheckOutcomeTests
{
    [Theory]
    [InlineData("{\"e\":[{\"n\":\"/100/0/31\",\"sv\":\"true\"}]}",  ControlCheckOutcome.Success)]
    [InlineData("{\"e\":[{\"n\":\"/100/0/31\",\"sv\":\"false\"}]}", ControlCheckOutcome.Success)]
    [InlineData("{\"e\":[{\"n\":\"/100/0/31\",\"sv\":true}]}",      ControlCheckOutcome.Success)]
    [InlineData("{\"e\":[{\"n\":\"/100/0/31\",\"sv\":false}]}",     ControlCheckOutcome.Success)]
    public void Returns_Success_When_Sv_Is_TrueOrFalse(string body, ControlCheckOutcome expected)
    {
        var outcome = RegisterApiClient.ClassifyOutcome(HttpStatusCode.OK, body);
        Assert.Equal(expected, outcome);
    }

    [Theory]
    [InlineData("{\"status\":\"STATUSERROR\"}",       ControlCheckOutcome.AlreadyRegistered)]
    [InlineData("STATUS ERROR",                       ControlCheckOutcome.AlreadyRegistered)]
    [InlineData("{\"status\":\"NOTREGISTERED\"}",     ControlCheckOutcome.NotRegistered)]
    [InlineData("NOT REGISTERED",                     ControlCheckOutcome.NotRegistered)]
    [InlineData("{\"status\":\"TIMEFAILED\"}",        ControlCheckOutcome.AuthCodeExpired)]
    public void Recognises_String_Status_Markers(string body, ControlCheckOutcome expected)
    {
        var outcome = RegisterApiClient.ClassifyOutcome(HttpStatusCode.OK, body);
        Assert.Equal(expected, outcome);
    }

    [Fact]
    public void Returns_Pending_When_Sv_Field_Missing()
    {
        var outcome = RegisterApiClient.ClassifyOutcome(HttpStatusCode.OK, "{\"some\":\"other\"}");
        Assert.Equal(ControlCheckOutcome.Pending, outcome);
    }

    [Fact]
    public void Returns_UnknownError_On_Non_200_Without_Markers()
    {
        var outcome = RegisterApiClient.ClassifyOutcome(HttpStatusCode.InternalServerError, "{}");
        Assert.Equal(ControlCheckOutcome.UnknownError, outcome);
    }

    [Theory]
    [InlineData("{\"e\":[{\"n\":\"/100/0/31\",\"sv\":\"true\"}], \"status\":\"STATUSERROR\"}",
                ControlCheckOutcome.AlreadyRegistered)]
    public void Status_Markers_Win_Over_Sv_Heuristic(string body, ControlCheckOutcome expected)
    {
        // STATUSERROR is checked first; protects against the sv heuristic
        // accidentally treating an error envelope as success.
        Assert.Equal(expected, RegisterApiClient.ClassifyOutcome(HttpStatusCode.OK, body));
    }
}
