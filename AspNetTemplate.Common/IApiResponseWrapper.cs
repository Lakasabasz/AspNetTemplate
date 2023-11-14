namespace AspNetTemplate.Common;

interface IApiResponseWrapper
{
    int StatusCode { get; }
    IApiResponse? Response { get; }
}