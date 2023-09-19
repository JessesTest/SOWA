namespace Common.Services.AddressValidation;

public interface IAddressValidationService
{
    Task<IList<ValidAddress>> GetCandidates(
        string address,
        string city = null,
        string zip5 = null,
        int maxLocations = 1,
        CancellationToken cancellationToken = default);
}
