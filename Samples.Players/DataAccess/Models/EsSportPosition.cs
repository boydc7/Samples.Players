using Nest;

namespace Samples.Players.DataAccess.Models;

[ElasticsearchType(IdProperty = nameof(SportPositionId))]
public class EsSportPosition
{
    [Text(Analyzer = "english", SearchAnalyzer = "english", Norms = true)]
    public string SearchValue { get; set; }

    [Keyword(Index = true, EagerGlobalOrdinals = false, IgnoreAbove = 50, Normalizer = "samplekeyword")]
    public string SportPositionId { get; set; }
    
    [Keyword(Index = true, EagerGlobalOrdinals = false, IgnoreAbove = 30, Normalizer = "samplekeyword")]
    public string Sport { get; set; }

    [Keyword(Index = true, EagerGlobalOrdinals = false, IgnoreAbove = 10, Normalizer = "samplekeyword")]
    public string Position { get; set; }
    
    [Number(NumberType.Double, Coerce = true)]
    public double AverageAge { get; set; }
    
    [Number(NumberType.Long, Coerce = true)]
    public long PlayerCount { get; set; }

    public static string GetId(string sport, string position)
        => string.Concat(sport, "|", position);
    
    public static DocumentPath<EsSportPosition> GetDocumentPath(string sport, string position)
        => new DocumentPath<EsSportPosition>(new Id(GetId(sport, position))).Index(EsIndexes.SportPositionsIndex);
}
