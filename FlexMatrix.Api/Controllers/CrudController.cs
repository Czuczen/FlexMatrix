using FlexMatrix.Api.Attributes;
using FlexMatrix.Api.Data.Models;
using FlexMatrix.Api.Data.Services.CrudService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlexMatrix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrudController : ControllerBase
    {
        private readonly ICrudService _crudService;


        public CrudController(ICrudService crudService)
        {
            _crudService = crudService;
        }


        [HttpGet("GetById/{id}", Name = "GetById")]
        [UnitOfWork]
        public async Task<IActionResult> Get(string tableName, int id)
        {
            var result = await _crudService.GetById(tableName, id);

            if (result != null)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
            }
        }

        [HttpGet("GetAll", Name = "GetAll")]
        [UnitOfWork]
        public async Task<IActionResult> GetAll(string tableName)
        {
            var result = await _crudService.GetAll(tableName);

            if (result != null)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
            }
        }

        [HttpPost("Create")]
        [UnitOfWork]
        public async Task<IActionResult> Create(string tableName, Dictionary<string, object> creationObject)
        {
            var result = await _crudService.Create(tableName, creationObject);

            if (result)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
            }
        }

        [HttpPost("Update")]
        [UnitOfWork]
        public async Task<IActionResult> Update(string tableName, Dictionary<string, object> updatingObject)
        {
            var result = await _crudService.Update(tableName, updatingObject);

            if (result)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
            }
        }

        [HttpPost("Delete/{id}")]
        [UnitOfWork]
        public async Task<IActionResult> Delete(string tableName, int id)
        {
            var result = await _crudService.Delete(tableName, id);

            if (result)
            {
                return Ok();
            }
            else
            {
                return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
            }
        }
    }
}
