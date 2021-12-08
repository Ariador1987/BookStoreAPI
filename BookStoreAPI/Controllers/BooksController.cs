using AutoMapper;
using BookStoreAPI.Contracts;
using BookStoreAPI.Data;
using BookStoreAPI.Data.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BookStoreAPI.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Books in book stores database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository,
            ILoggerService loggerService,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _loggerService = loggerService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get the list of books
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll()
        {
            var location = GetControllerActionNames();

            try
            {
                _loggerService.LogInfo($"{location}: Attempted call.");
                var books = await _bookRepository.FindAll();       
                var response = _mapper.Map<IList<BookDTO>>(books); 
                _loggerService.LogInfo($"{location}: Sucessful call.");

                return StatusCode(200, response);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get a single book with param id
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                _loggerService.LogInfo($"{location}: Attempted call for id: {id}.");
                var book = await _bookRepository.FindById(id);

                if (book is null)
                {
                    _loggerService.LogWarn($"{location} failed to retrieve record for {id}.");
                    return NotFound();
                }

                var response = _mapper.Map<BookDTO>(book);
                _loggerService.LogInfo($"{location}: Sucessful call for id: {id}.");

                return StatusCode(200, response);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }


        /// <summary>
        /// Creates a single book
        /// </summary>
        /// <param name="bookDTO"></param>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                _loggerService.LogInfo("Create attempted");
                if (bookDTO is null)
                {
                    _loggerService.LogWarn($"{location}: Empty request was submitted.");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"{location}: Data was incomplete.");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);
                if (!isSuccess)
                {
                    _loggerService.LogWarn($"{location}: Something went wrong with creating a book.");
                    return InternalError($"{location}: Creation failed.");
                }

                _loggerService.LogInfo("Create sucessful");
                return StatusCode(201);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Updates a single book
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                _loggerService.LogInfo($"{location}: Update attempted on record {id}.");
                if (id < 1 || bookDTO is null || bookDTO.Id != id)
                {
                    _loggerService.LogWarn($"{location}: Update failed with bad data - id: {id}.");
                    return BadRequest();
                }

                var isExsists = await _bookRepository.isExsists(id);
                if (!isExsists)
                {
                    _loggerService.LogWarn($"{location}: failed to retrieve record with id: {id}");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"{location}: Data was incomplete");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Update failed");
                }

                _loggerService.LogInfo($"{location}: Record with id: {id} succesfully updated.");
                return StatusCode(204);
            }
            catch (Exception ex)
            {
                _loggerService.LogWarn($"{location}: Something went wrong with creating a book.");
                return InternalError($"{location}: Creation failed. {ex.Message} - {ex.InnerException}.");
            }
        }

        /// <summary>
        /// Deletes a book object from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                _loggerService.LogInfo($"{location}: Delete operation initialized for id: {id}.");
                if (id < 1)
                {
                    _loggerService.LogWarn($"{location}: Delete failed with bad data - id: {id}.");
                    return BadRequest();
                }

                var ifExsists = await _bookRepository.isExsists(id);
                if (!ifExsists)
                {
                    _loggerService.LogWarn($"{location}: failed to retrieve record with id: {id}.");
                    return NotFound();
                }

                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);

                if (!isSuccess)
                {
                    _loggerService.LogError($"{location}: Something went wrong when attempting to delete - id: {id}.");
                    return InternalError($"{location}: Delete failed for id: {id}.");
                }

                _loggerService.LogInfo($"{location}: Delete successful for id: {id}.");
                return StatusCode(204);
            }
            catch (Exception ex)
            {
                _loggerService.LogWarn($"{location}: Something went wrong with deleting a book.");
                return InternalError($"{location}: Creation failed. {ex.Message} - {ex.InnerException}.");
            }
        }

        // PRIVATES
        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _loggerService.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }
    }
}
