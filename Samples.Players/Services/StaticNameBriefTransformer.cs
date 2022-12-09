namespace Samples.Players.Services;

public interface INameBriefTransformer
{
    string ToNameBrief<T>(T source)
        where T : INameBriefSource;
}

public class StaticNameBriefTransformer : INameBriefTransformer
{
    private static readonly IReadOnlyDictionary<string, Func<INameBriefSource, string>> _sportNameMap =
        new Dictionary<string, Func<INameBriefSource, string>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "baseball", s =>
                            {
                                var nameInfo = new NameInfo(s);

                                return string.Concat(nameInfo.FirstInitial == null
                                                         ? ""
                                                         : $"{nameInfo.FirstInitial}. ",
                                                     nameInfo.LastInitial == null
                                                         ? ""
                                                         : $"{nameInfo.LastInitial}.")
                                             .Trim();
                            }
            },
            {
                "football", s =>
                            {
                                var nameInfo = new NameInfo(s);

                                return string.Concat(nameInfo.FirstInitial == null
                                                         ? ""
                                                         : $"{nameInfo.FirstInitial}. ",
                                                     nameInfo.LastName)
                                             .Trim();
                            }
            },
            {
                "basketball", s =>
                            {
                                var nameInfo = new NameInfo(s);

                                return string.Concat(nameInfo.FirstName,
                                                     nameInfo.LastInitial == null
                                                         ? ""
                                                         : $" {nameInfo.LastInitial}.")
                                             .Trim();
                            }
            },
        };

    public string ToNameBrief<T>(T source)
        where T : INameBriefSource
    {
        if (source.Sport.IsNullOrEmpty() || !_sportNameMap.ContainsKey(source.Sport))
        {
            return null;
        }

        var nameBrief = _sportNameMap[source.Sport](source);

        return nameBrief;
    }

    private readonly struct NameInfo
    {
        public NameInfo(INameBriefSource source)
        {
            FirstName = source.FirstName;
            LastName = source.LastName;

            FirstInitial = FirstName is { Length: > 0 }
                               ? FirstName[..1]
                               : null;

            LastInitial = LastName is { Length: > 0 }
                              ? LastInitial = LastName[..1]
                              : null;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public string FirstInitial { get; }
        public string LastInitial { get; }
    }
}
