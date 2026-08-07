using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Vention.API.Authorization;
using Vention.API.Extensions;
using Vention.API.Filters;
using Vention.Application.Abstractions;
using Vention.Application.Files;
using Vention.Application.Files.Commands.DeleteFile;
using Vention.Application.Files.Commands.UploadFile;
using Vention.Application.Files.Contracts;
using Vention.Application.Files.Queries.GetFiles;
using Vention.Application.Messaging;
using Vention.Domain.Membership;

namespace Vention.API.Controllers
{
    [ApiController]
    [Route("files")]
    public sealed class FilesController : ControllerBase
    {
        private const int DefaultLimit = 100;

        private const long MaxUploadRequestBytes = FileUploadRules.MaxFileSizeBytes + 1024 * 1024;

        private readonly ICurrentUserService _currentUser;
        private readonly IDispatcher _dispatcher;
        public FilesController(IDispatcher dispatcher, ICurrentUserService currentUser)
        {
            _dispatcher = dispatcher;
            _currentUser = currentUser;
        }

        [HttpGet]
        [RequireActiveOrganizationRole]
        public async Task<ActionResult<IReadOnlyList<FileResponse>>> GetAll(
            [FromQuery] int limit = DefaultLimit,
            CancellationToken ct = default)
        {
            var organizationId = Request.GetRequiredOrganizationId();
            var result = await _dispatcher.Send(new GetFilesQuery(organizationId, limit), ct);

            return Ok(result);
        }

        [HttpPost("upload")]
        [RequireActiveOrganizationRole(
            MembershipRole.Owner,
            MembershipRole.Admin,
            MembershipRole.Editor,
            MembershipRole.Member)]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting(RateLimitingExtensions.UploadPolicy)]
        [RequestSizeLimit(MaxUploadRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadRequestBytes)]
        public async Task<ActionResult<FileResponse>> Upload(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("A non-empty 'file' form field is required.", nameof(file));

            var organizationId = Request.GetRequiredOrganizationId();

            await using var content = file.OpenReadStream();
            var command = new UploadFileCommand(
                content,
                file.FileName,
                file.ContentType,
                file.Length,
                organizationId,
                _currentUser.UserId);

            var result = await _dispatcher.Send(command, ct);

            return Created($"/files/{result.Id}", result);
        }

        [HttpPost("upload/stream")]
        [RequireActiveOrganizationRole(
            MembershipRole.Owner,
            MembershipRole.Admin,
            MembershipRole.Editor,
            MembershipRole.Member)]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting(RateLimitingExtensions.UploadPolicy)]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MaxUploadRequestBytes)]
        public async Task<ActionResult<FileResponse>> UploadStream(CancellationToken ct)
        {
            if (!MediaTypeHeaderValue.TryParse(Request.ContentType, out var mediaType) ||
                !mediaType.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    "The request must be a 'multipart/form-data' upload.",
                    nameof(Request.ContentType));

            var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
                throw new ArgumentException(
                    "The multipart boundary is missing.",
                    nameof(Request.ContentType));

            var organizationId = Request.GetRequiredOrganizationId();
            var actingUserId = _currentUser.UserId;

            var reader = new MultipartReader(boundary, Request.Body);
            MultipartSection? section;

            while ((section = await reader.ReadNextSectionAsync(ct)) is not null)
            {
                if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                    continue;

                var isFileSection = contentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(contentDisposition.FileName.Value ?? contentDisposition.FileNameStar.Value);

                if (!isFileSection)
                    continue;

                var filename = HeaderUtilities.RemoveQuotes(contentDisposition.FileName).Value
                    ?? HeaderUtilities.RemoveQuotes(contentDisposition.FileNameStar).Value
                    ?? string.Empty;

                var command = new UploadFileCommand(
                    section.Body,
                    filename,
                    section.ContentType ?? string.Empty,
                    null,
                    organizationId,
                    actingUserId);

                var result = await _dispatcher.Send(command, ct);

                return Created($"/files/{result.Id}", result);
            }

            throw new ArgumentException(
                "A 'file' section was not found in the multipart request.",
                nameof(Request.Body));
        }

        [HttpDelete("{id:guid}")]
        [RequireActiveOrganizationRole(
            MembershipRole.Owner,
            MembershipRole.Admin,
            MembershipRole.Editor,
            MembershipRole.Member)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var organizationId = Request.GetRequiredOrganizationId();
            await _dispatcher.Send(new DeleteFileCommand(id, organizationId, _currentUser.UserId), ct);

            return NoContent();
        }
    }
}