using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController
{

}
