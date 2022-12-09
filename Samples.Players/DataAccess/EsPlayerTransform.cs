using Samples.Players.DataAccess.Models;
using Samples.Players.Models;
using Samples.Players.Services;

namespace Samples.Players.DataAccess;

public class EsPlayerTransform : ITransform<Player, EsPlayer>
{
    private readonly INameBriefTransformer _nameBriefTransformer;
    
    public EsPlayerTransform(INameBriefTransformer nameBriefTransformer)
    {
        _nameBriefTransformer = nameBriefTransformer;
    }

    public EsPlayer Transform(Player source)
        => new EsPlayer
           {
               SearchValue = $"{source.Id} {source.FirstName} {source.LastName} s:{source.Sport} p:{source.Position}",
               PlayerId = source.Id,
               Sport = source.Sport.ToLowerInvariant(),
               LastNameInitial = source.LastName.IsNullOrEmpty()
                                     ? null
                                     : source.LastName[..1],
               FirstName = source.FirstName,
               LastName = source.LastName,
               Age = source.Age,
               Position = source.Position.ToUpperInvariant(),
               NameBrief = _nameBriefTransformer.ToNameBrief(source)
           };

    public Player Transform(EsPlayer source)
        => new Player
           {
               Position = source.Position,
               Sport = source.Sport,
               LastName = source.LastName,
               FirstName = source.FirstName,
               Age = source.Age,
               NameBrief = source.NameBrief,
               Id = source.PlayerId
           };
}
