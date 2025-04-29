using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using AktBob.Database.Validators;
using AktBob.Shared.DataAccess;
using FluentValidation;

namespace AktBob.Database.Repositories;
internal class OS2FormsSubmissionRepository : IOS2FormsSubmissionRepository
{
    private readonly ISqlDataAccess<IDatabaseSqlConnectionFactory> _sqlDataAccess;

    public OS2FormsSubmissionRepository(ISqlDataAccess<IDatabaseSqlConnectionFactory> sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    public async Task<bool> Add(OS2FormsSubmission submission)
    {
        var validator = new OS2FormsSubmissionValidator();
        validator.ValidateAndThrow(submission);

        var parameters = new DynamicParameters();
        parameters.Add("DeskproTicketId", submission.DeskproTicketId);
        parameters.Add("SubmissionId", submission.SubmissionId);
        parameters.Add("DescriptionFieldValue", submission.DescriptionFieldValue);
        parameters.Add("Id", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        var id = await _sqlDataAccess.ExecuteProcedure("spOS2FormsSubmission_Create", parameters);
        return id > 0;
    }

    public async Task<OS2FormsSubmission?> GetByDeskproTicketId(int deskproTicketId) => await _sqlDataAccess.QuerySingle<OS2FormsSubmission>("SELECT * FROM v_OS2FormsSubmissions WHERE DeskproTicketId = @DeskproTicketId", new { DeskproTicketId = deskproTicketId });

    public async Task<OS2FormsSubmission?> GetBySubmissionId(Guid submissionId) => await _sqlDataAccess.QuerySingle<OS2FormsSubmission>("SELECT * FROM v_OS2FormsSubmissions WHERE SubmissionId = @SubmissionId", new { SubmissionId = submissionId });

}
