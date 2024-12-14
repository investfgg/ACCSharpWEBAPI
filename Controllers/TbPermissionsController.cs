using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using netwebapi_access_control.Data;
using netwebapi_access_control.Models;

namespace netwebapi_access_control.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TbPermissionsController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;

        public TbPermissionsController( AccessControlContext context )
        {
            _context = context;
        }

        // GET: api/TbPermissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbPermission>>> GetTbPermissions()
        {
            // Retorna contendo uma lista de todas as permissões que não estão deletadas (O campo 'deleted_at' cujo valor padrão sempre é nulo).
            return await _context.TbPermissions.Where( x => x.DeletedAt == null ).ToListAsync();
        }

        // GET: api/TbPermissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbPermission>> GetTbPermission( long id )
        {
            var tbPermission = await _context.TbPermissions.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponsePermission( tbPermission!, id, "GET" ) ) || !tbPermission!.DeletedAt.Equals( null ))
            {
                return NotFound( strResponse );
            }

            return tbPermission;
        }

        // POST: api/TbPermissions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbPermission>> PostTbPermission( TbPermission tbPermission )
        {
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the Permissions table!",
                                            " Broken rules: one of the fields (" );

            ValidMsg = ValidatorFieldsPermission( tbPermission, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsPermission( tbPermission, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbPermission.CreatedAt = DateTime.Now;

            _context.TbPermissions.Add( tbPermission );
            await _context.SaveChangesAsync();

            return CreatedAtAction( "GetTbPermission", new { id = tbPermission.Id }, tbPermission );
        }

        // PUT: api/TbPermissions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<TbPermission?>> PutTbPermission( long id, TbPermission tbPermission )
        {
            strResponse = "";
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to update the actual ID (", id,
                                            ") in the Permissions table! Broken rules: one of the fields (" );

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de Objetos no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            var findInTbPermission = FindPermissionByID( id );

            if (!string.IsNullOrEmpty( MsgResponsePermission( tbPermission, id, "PUT" ) ))
            {
                return NotFound( strResponse );
            }

            ValidMsg = ValidatorFieldsPermission( tbPermission, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsPermission( tbPermission, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            _context.Entry( tbPermission ).State = EntityState.Modified;

            try
            {
                // Para evitar erros bobos, no API, como a não inclusão de campo 'CreatedAt' no JSON sem valor preenchido
                // o que grava no banco o valor "01-01-0001 00:00:00" e não na data e hora atual do sistema.
                tbPermission.CreatedAt = findInTbPermission!.CreatedAt;
                tbPermission.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {

                if (!TbPermissionExists( id ))
                {
                    return NotFound( strResponse );
                }

                else
                {
                    throw;
                }
            }

            return await _context.TbPermissions.FindAsync( id );
        }

        // DELETE: api/TbPermissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbPermission( long id )
        {
            var tbPermission = await _context.TbPermissions.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponsePermission( tbPermission!, id, "DEL" ) ))
            {
                return NotFound( strResponse );
            }

            // Remoção desta linha pois não faz parte de minha estratégia (exclusão física na tabela)
            // [_context TbPermissions Remove( tbPermission );]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente. 

            tbPermission!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the ID (", id, ") of the Permissions table is deleted." ) );
        }

        private bool TbPermissionExists( long id )
        {
            return _context.TbPermissions.Any( e => e.Id == id && e.DeletedAt == null );
        }

        private string MsgResponsePermission( TbPermission tbPermission, long idPermission, string operation )
        {
            strResponse = "";

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de Permissões no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "PUT")
            {

                if (tbPermission.Id == 0)
                {
                    strResponse = "** REASON: Unfortunately, the field 'ID' in the JSON format is not present in Permission set. Put the field and try it.";
                }

                if (tbPermission.DeletedAt != null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", tbPermission.Id, ") is not in the Permissions table. Try other ID." );
                }

                if (idPermission != tbPermission.Id)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idPermission,
                                       ") whose parameter with described value is different than the ID filled (", tbPermission.Id,
                                       ") in set of Permission by the JSON format. Fill the same correct ID in both." );
                }

                if (FindPermissionByID( idPermission ) == null)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idPermission,
                                       ") whose parameter with described value is not in the Permissions table. Try other ID." );
                }
            }

            if (operation == "DEL")
            {

                if (FindPermissionByID( idPermission ) == null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", idPermission,
                                                 ") whose parameter with described value is not in the Permissions table. Try other ID.");
                }

                // Encontrar ID de Permissão na(s) tabela(s) relacionada(s) - IDPermExistInRelationships.
                if (tbPermission != null)
                {
                    strResponse = (IDPermExistInRelationships( tbPermission.Id ).Length > 0) ?
                                    String.Concat("** REASON: Unfortunately, the actual ID (", idPermission,
                                                   ") in the Permissions table cannot be deleted because it has a relationship with the some tables (",
                                                   IDPermExistInRelationships( tbPermission.Id ),
                                                   ").", " You will need to delete it before continuing with the operation." ) : "";
                }
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar a aplicação que não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).
                strResponse = (tbPermission == null || tbPermission.DeletedAt != null) ?
                    String.Concat( "** REASON: Unfortunately, the actual ID (", idPermission, ") is not in the Permissions table. Try other ID." ) : "";
            }

            return strResponse;
        }

        private TbPermission? FindPermissionByID( long idPermission )
        {
            return _context.TbPermissions.AsNoTracking().FirstOrDefault( x => x.Id == idPermission );
        }

        private string IDPermExistInRelationships( long idPermission )
        {
            var tbUstypeperm = _context.TbUstypeperms.AsNoTracking().FirstOrDefault( utp => utp.IdPermissions == idPermission && utp.DeletedAt == null );

            return (tbUstypeperm != null) ? "TBUsTypePerms" : "";
        }

        private static string ValidatorFieldsPermission( TbPermission? tbPermission, int option )
        {
            String strTentativas = "";

            if (option.Equals( 1 ))
            {
                strTentativas +=
                    CharactLimitPermission( "Title", tbPermission!.Title.Trim(), 100, option ) +
                    CharactLimitPermission( "Description", tbPermission!.Description.Trim(), 250, option );
            }

            if (option.Equals( 2 ))
            {
                strTentativas +=
                    CharactLimitPermission( "Title", tbPermission!.Title.Trim(), 3, option ) +
                    CharactLimitPermission( "Description", tbPermission!.Description.Trim(), 3, option );
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private static string CharactLimitPermission( String strField, String strValueField, int lengthField, int option )
        {
            var strReturn = "";

            if (option.Equals( 1 ))
            {
                strReturn = (strValueField.Length > lengthField) ?
                            strField + "(" + strValueField.Length + " of " + lengthField + "), " : "";
            }

            if (option.Equals( 2 ))
            {
                strReturn = (strValueField.Trim().Equals("") || strValueField.Length <= lengthField) ? strField + ", " : "";
            }

            return strReturn;
        }
    }
}