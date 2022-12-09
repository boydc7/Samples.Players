using Microsoft.AspNetCore.Mvc;

namespace Samples.Players.Controllers;

[ApiController]
[Route("[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public abstract class SamplePlayerControllerBase : ControllerBase { }
