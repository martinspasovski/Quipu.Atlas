using Quipu.Atlas.Domain.Applications;

namespace Quipu.Atlas.Infrastructure.Providers.WorldCheck
{
    internal static class WorldCheckHttpClientHelpers
    {

        public static bool TryParseListType(string? value, out WorldCheckListType listType)
        {
            listType = value?.ToUpperInvariant() switch
            {
                "SANCTIONS" => WorldCheckListType.Sanctions,
                "PEP" => WorldCheckListType.Pep,
                _ => (WorldCheckListType)(-1)
            };

            return Enum.IsDefined(listType);
        }

        public static bool TryParseStatus(string? value, out WorldCheckScreeningStatus status)
        {
            status = value?.ToUpperInvariant() switch
            {
                "CLEAR" => WorldCheckScreeningStatus.Clear,
                "POSSIBLE_MATCH" => WorldCheckScreeningStatus.PossibleMatch,
                _ => (WorldCheckScreeningStatus)(-1)
            };

            return Enum.IsDefined(status);
        }
    }
}