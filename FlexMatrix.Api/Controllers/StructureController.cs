using FlexMatrix.Api.Data.Models;
using FlexMatrix.Api.Data.Services.StructureService;
using Microsoft.AspNetCore.Mvc;
using UnitOfWork = FlexMatrix.Api.Attributes.UnitOfWorkAttribute;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        [HttpPost(Name = "CreateNewTableStructure")]
        public async Task<IActionResult> CreateNewTableStructure(TableStructureDto tableStructure)
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
    }
}
