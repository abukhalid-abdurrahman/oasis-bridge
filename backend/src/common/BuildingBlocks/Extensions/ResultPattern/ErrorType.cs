namespace BuildingBlocks.Extensions.ResultPattern;

public enum ErrorType
{
    None,
    BadRequest,
    NotFound,
    AlreadyExist,
    Conflict,
    InternalServerError
}