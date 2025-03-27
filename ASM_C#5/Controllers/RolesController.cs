using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASM_C_5.Data;
using ASM_C_5.Models;
using ASM_C_5.DTOS.Responses;
using Microsoft.AspNetCore.Authorization;

namespace ASM_C_5.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ASM_C_5Context _context;

        public RolesController(ASM_C_5Context context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<BaseResponse<IEnumerable<ApplicationRole>>>> GetApplicationRole()
        {
            var roles = await _context.ApplicationRole.ToListAsync();
            return new BaseResponse<IEnumerable<ApplicationRole>>
            {
                ErrorCode = 0,
                Message = "Success",
                Data = roles
            };
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<ApplicationRole>>> GetApplicationRole(string id)
        {
            var applicationRole = await _context.ApplicationRole.FindAsync(id);

            if (applicationRole == null)
            {
                return NotFound(new BaseResponse<ApplicationRole>
                {
                    ErrorCode = 404,
                    Message = "Role not found",
                    Data = null
                });
            }

            return new BaseResponse<ApplicationRole>
            {
                ErrorCode = 0,
                Message = "Success",
                Data = applicationRole
            };
        }

        // PUT: api/Roles/5
        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> PutApplicationRole(string id, ApplicationRole applicationRole)
        {
            if (id != applicationRole.Id)
            {
                return BadRequest(new BaseResponse<bool>
                {
                    ErrorCode = 400,
                    Message = "ID mismatch",
                    Data = false
                });
            }

            _context.Entry(applicationRole).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return new BaseResponse<bool>
                {
                    ErrorCode = 0,
                    Message = "Role updated successfully",
                    Data = true
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationRoleExists(id))
                {
                    return NotFound(new BaseResponse<bool>
                    {
                        ErrorCode = 404,
                        Message = "Role not found",
                        Data = false
                    });
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Roles
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<ApplicationRole>>> PostApplicationRole(ApplicationRole applicationRole)
        {
            _context.ApplicationRole.Add(applicationRole);
            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetApplicationRole), new { id = applicationRole.Id }, new BaseResponse<ApplicationRole>
                {
                    ErrorCode = 0,
                    Message = "Role created successfully",
                    Data = applicationRole
                });
            }
            catch (DbUpdateException)
            {
                if (ApplicationRoleExists(applicationRole.Id))
                {
                    return Conflict(new BaseResponse<ApplicationRole>
                    {
                        ErrorCode = 409,
                        Message = "Role already exists",
                        Data = null
                    });
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/Roles/5
        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteApplicationRole(string id)
        {
            var applicationRole = await _context.ApplicationRole.FindAsync(id);
            if (applicationRole == null)
            {
                return NotFound(new BaseResponse<bool>
                {
                    ErrorCode = 404,
                    Message = "Role not found",
                    Data = false
                });
            }

            _context.ApplicationRole.Remove(applicationRole);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                ErrorCode = 0,
                Message = "Role deleted successfully",
                Data = true
            };
        }

        private bool ApplicationRoleExists(string id)
        {
            return _context.ApplicationRole.Any(e => e.Id == id);
        }
    }
}
