namespace AktBob.FilArkiv.Contracts.DTOs;

public record FileProcessStatusDto(bool IsInQueue, bool IsBeingProcessed);