using Nest;

namespace Samples.Players.DataAccess.Models;

[ElasticsearchType(IdProperty = nameof(PlayerId))]
public class EsPlayer
{
    [Text(Analyzer = "english", SearchAnalyzer = "english", Norms = true)]
    public string SearchValue { get; set; }

    [Keyword(Index = true, EagerGlobalOrdinals = false, IgnoreAbove = 30, Normalizer = "samplekeyword")]
    public string PlayerId { get; set; }
    
    [Keyword(Index = true, EagerGlobalOrdinals = false, IgnoreAbove = 30, Normalizer = "samplekeyword")]
    public string Sport { get; set; }

    [Keyword(Index = true, EagerGlobalOrdinals = false, IgnoreAbove = 5, Normalizer = "samplekeyword")]
    public string LastNameInitial { get; set; }

    [Number(NumberType.Integer, Coerce = true)]
    public int Age { get; set; }
    
    [Keyword(Index = true, EagerGlobalOrdinals = false, IgnoreAbove = 10, Normalizer = "samplekeyword")]
    public string Position { get; set; }
    
    [Keyword(Index = false, EagerGlobalOrdinals = false, IgnoreAbove = 100, Normalizer = "samplekeyword")]
    public string FirstName { get; set; }
    
    [Keyword(Index = false, EagerGlobalOrdinals = false, IgnoreAbove = 100, Normalizer = "samplekeyword")]
    public string LastName { get; set; }
    
    [Keyword(Index = false, EagerGlobalOrdinals = false, IgnoreAbove = 10, Normalizer = "samplekeyword")]
    public string NameBrief { get; set; }

    public static DocumentPath<EsPlayer> GetDocumentPath(string forId)
        => new DocumentPath<EsPlayer>(new Id(forId)).Index(EsIndexes.PlayersIndex);
}
