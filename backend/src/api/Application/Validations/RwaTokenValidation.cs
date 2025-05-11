namespace Application.Validations;

public static class CreateRwaTokenValidation
{
    public static Result<CreateRwaTokenResponse> CreateValidateNftFields(this CreateRwaTokenRequest request)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 32)
            errors.Add(Messages.TitleInvalid);

        if (string.IsNullOrWhiteSpace(request.Image) || !Uri.IsWellFormedUriString(request.Image, UriKind.Absolute))
            errors.Add(Messages.ImageInvalid);

        if (string.IsNullOrWhiteSpace(request.AssetDescription) || request.AssetDescription.Length > 500)
            errors.Add(Messages.AssetDescriptionInvalid);

        if (string.IsNullOrWhiteSpace(request.UniqueIdentifier) || request.UniqueIdentifier.Length > 64)
            errors.Add(Messages.UniqueIdentifierInvalid);

        if (errors.Any())
            return Result<CreateRwaTokenResponse>.Failure(ResultPatternError.BadRequest(string.Join(" | ", errors)));

        return Result<CreateRwaTokenResponse>.Success();
    }

    public static Result<UpdateRwaTokenResponse> UpdateValidateNftFields(this UpdateRwaTokenRequest request)
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 32)
            errors.Add(Messages.TitleInvalid);
        
        if (string.IsNullOrWhiteSpace(request.AssetDescription) || request.AssetDescription.Length > 500)
            errors.Add(Messages.AssetDescriptionInvalid);


        if (errors.Any())
            return Result<UpdateRwaTokenResponse>.Failure(ResultPatternError.BadRequest(string.Join(" | ", errors)));

        return Result<UpdateRwaTokenResponse>.Success();
    }
}