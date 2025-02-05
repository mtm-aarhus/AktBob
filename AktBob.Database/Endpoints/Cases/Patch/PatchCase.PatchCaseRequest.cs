namespace AktBob.Database.Endpoints.Cases.Patch;
internal record PatchCaseRequest
{
    public int Id { get; set; }
    public long? PodioItemId { get; set; }
    public string? CaseNumber { get; set; }
    public Guid? FilArkivCaseId { get; set; }
    public string? SharepointFolderName { get; set; }
}
