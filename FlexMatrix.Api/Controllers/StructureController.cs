using FlexMatrix.Api.Data.Models;
using FlexMatrix.Api.Data.Services.StructureService;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork = FlexMatrix.Api.Attributes.UnitOfWorkAttribute;

namespace FlexMatrix.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StructureController : ControllerBase
    {
        private readonly IStructureService _structureService;


        public StructureController(IStructureService structureService)
        {
            _structureService = structureService;
        }


        [UnitOfWork]
        [HttpPost("CreateTable")]
        public async Task<IActionResult> CreateTableStructure(TableStructureDto tableStructure)
        {
            if (ModelState.IsValid)
            {
                var result = await _structureService.CreateTableStructure(tableStructure);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [UnitOfWork]
        [HttpPost("AddColumn")]
        public async Task<IActionResult> AddColumnStructure(ColumnStructureDto column, string tableName)
        {
            if (ModelState.IsValid)
            {
                var result = await _structureService.AddColumnStructure(column, tableName);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [UnitOfWork]
        [HttpPost("RemoveColumn")]
        public async Task<IActionResult> RemoveColumn(string tableName, string columnName)
        {
            if (ModelState.IsValid)
            {
                var result = await _structureService.RemoveColumn(tableName, columnName);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [UnitOfWork]
        [HttpPost("DeleteTable")]
        public async Task<IActionResult> DeleteTable(string tableName)
        {
            if (ModelState.IsValid)
            {
                var result = await _structureService.DeleteTable(tableName);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [UnitOfWork]
        [HttpPost("RemoveRelations")]
        public async Task<IActionResult> RemoveRelations(string tableName)
        {
            if (ModelState.IsValid)
            {
                var result = await _structureService.RemoveRelations(tableName);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(500, "Internal server error: " + "eeeerrrrroooorrr");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
