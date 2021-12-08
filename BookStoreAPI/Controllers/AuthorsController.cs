using AutoMapper;
using BookStoreAPI.Contracts;
using BookStoreAPI.Data;
using BookStoreAPI.Data.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreAPI.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository, 
            ILoggerService loggerService, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _loggerService = loggerService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _loggerService.LogInfo("Attempted get all authors");
                var authorsList = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authorsList);
                _loggerService.LogInfo("Successfully got all Authors");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get single Author by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Author</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _loggerService.LogInfo($"Attempted get author with id:${id}");
                var author = await _authorRepository.FindById(id);
                if (author is null)
                {
                    _loggerService.LogWarn($"Author with id:{id} was not found");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _loggerService.LogInfo($"Successfully got author with id:{id}");
                return Ok(response);

            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Create an author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns>Entity type author</returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _loggerService.LogInfo("Author Submission Attempted");

                if (!ModelState.IsValid || authorDTO is null)
                {
                    _loggerService.LogWarn($"Empty Request was submitted");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSucces = await _authorRepository.Create(author);

                if (isSucces is false)
                {
                    _loggerService.LogWarn($"Author creation failed");
                    return InternalError($"Author creation failed.");
                }

                _loggerService.LogInfo("Author Created");
                return StatusCode(201, author);
            }   
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }


        /// <summary>
        /// Updates an author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _loggerService.LogInfo($"Author update initialized id: {id}.");
                if (id < 1 || authorDTO is null || id != authorDTO.Id)
                {
                    _loggerService.LogWarn($"Author update failed with bad data.");
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"Author data was incomplete.");
                    return BadRequest(ModelState);
                }

                var isExsists = await _authorRepository.isExsists(id);

                if (!isExsists)
                {
                    _loggerService.LogWarn($"Author with id {id} was not found.");
                    return NotFound();
                }

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);

                if (!isSuccess)
                {
                    return InternalError($"Something went wrong.");
                }

                return StatusCode(204);
            }
            catch (Exception ex)
            {

                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Deletes an author from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _loggerService.LogInfo($"Author delete with id: {id} attempted");

                if (id < 1)
                {
                    _loggerService.LogWarn($"Author delete failed with bad data.");
                    return BadRequest();
                }

                var isExsists = await _authorRepository.isExsists(id);

                if (!isExsists)
                {
                    _loggerService.LogWarn($"Author with id {id} was not found.");
                    return NotFound();
                }

                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"Author delete failed.");
                }

                return StatusCode(204);
            }
            catch (Exception)
            {
                return InternalError($"Something went wrong.");
            }
        }
        // PRIVATES
        private ObjectResult InternalError(string message)
        {
            _loggerService.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }
    }
}
