using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;
using AutoMapper;
using Newtonsoft.Json;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly LinkGenerator _linkGenerator;

    public UsersController(IUserRepository userRepository, IMapper mapper,  LinkGenerator linkGenerator)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _linkGenerator = linkGenerator;
    }

    [HttpGet("{userId}")]
    [Produces("application/json", "application/xml")]
    public ActionResult GetUserById([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user == null)
        {
            return NotFound();
        }

        var dto = _mapper.Map<UserDto>(user);

        return Ok(dto);
    }
    
    [HttpGet(Name = "GetUsers")]
    [Produces("application/json", "application/xml")]
    public IActionResult GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        // Ограничиваем параметры
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 20) pageSize = 20;

        // Получаем пользователей из репозитория
        var pageList = _userRepository.GetPage(pageNumber, pageSize);

        var users = _mapper.Map<IEnumerable<UserDto>>(pageList);

        // Считаем инфу для пагинации
        var totalCount = pageList.TotalCount;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Генерируем ссылки на предыдущую и следующую страницы
        var previousPageLink = pageNumber > 1
            ? _linkGenerator.GetUriByRouteValues(
                HttpContext,
                "GetUsers",
                new { pageNumber = pageNumber - 1, pageSize })
            : null;

        var nextPageLink = pageNumber < totalPages
            ? _linkGenerator.GetUriByRouteValues(
                HttpContext,
                "GetUsers",
                new { pageNumber = pageNumber + 1, pageSize })
            : null;

        var paginationHeader = new
        {
            previousPageLink,
            nextPageLink,
            totalCount,
            pageSize,
            currentPage = pageNumber,
            totalPages
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

        return Ok(users);
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] object user)
    {
        throw new NotImplementedException();
    }
}