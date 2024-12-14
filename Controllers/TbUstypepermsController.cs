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
    public class TbUstypepermsController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;

        public TbUstypepermsController( AccessControlContext context )
        {
            _context = context;
        }

        // GET: api/TbUstypeperms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbUstypeperm>>> GetTbUstypeperms()
        {
            // Retorna contendo uma lista de todas as usertypes relacionadas com permissões que não estão deletadas (O campo 'deleted_at' cujo valor padrão sempre é nulo).

            return await _context.TbUstypeperms.Where( x => x.DeletedAt == null ).ToListAsync();
        }

        // GET: api/TbUstypeperms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbUstypeperm>> GetTbUstypeperm( long id )
        {
            var tbUstypeperm = await _context.TbUstypeperms.FindAsync( id );

            if (!string.IsNullOrEmpty(MsgResponseUstypeperm( tbUstypeperm!, id, "GET" ) ) || !tbUstypeperm!.DeletedAt.Equals( null ))
            {
                return NotFound( strResponse );
            }

            return tbUstypeperm;
        }

        // POST: api/TbUstypeperms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbUstypeperm>> PostTbUstypeperm( TbUstypeperm tbUstypeperm )
        {
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the UstypePerms table!",
                                            " Broken rules: one of the fields (" );

            string ValidMsg = ValidatorFieldsUstyeperm( "Pro", tbUstypeperm.Id ).Trim() + 
                              ValidatorFieldsUstyeperm( "UsT", tbUstypeperm.IdUsertypes!.Value ).Trim() +
                              ValidatorFieldsUstyeperm( "Per", tbUstypeperm.IdPermissions!.Value ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg.Replace( ")id_", "), id_" ), ") contain some problems. Please, check it again." ) );
            }

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbUstypeperm.CreatedAt = DateTime.Now;

            _context.TbUstypeperms.Add( tbUstypeperm );
            await _context.SaveChangesAsync();

            return CreatedAtAction( "GetTbUstypeperm", new { id = tbUstypeperm.Id }, tbUstypeperm );
        }

        // DELETE: api/TbUstypeperms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbUstypeperm( long id )
        {
            var tbUstypeperm = await _context.TbUstypeperms.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseUstypeperm( tbUstypeperm!, id, "DEL" ) ))
            {

                if (strResponse.StartsWith( "BD" ))
                {
                    return BadRequest( strResponse.Replace( "BD", "" ) );
                }

                else
                {
                    return NotFound( strResponse );
                }
            }

            // Remoção desta linha pois não faz parte de minha estratégia (exclusão física na tabela)
            // [_context TbUstypeperm Remove( tbUstypeperm );]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente. 

            tbUstypeperm!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the ID (", id, ") of the Ustypeperms table is deleted." ) );
        }

        private string TbUstypepermExists( long idUsTypePerm )
        {
            var boolProfiles   = _context.TbProfiles.Any( x => x.IdUstypeperms == idUsTypePerm && x.DeletedAt == null )!;
            var boolUsTypePerm = _context.TbUstypeperms.Any( e => e.Id == idUsTypePerm && e.DeletedAt == null );

            if (!boolProfiles)
            {
                return String.Concat( "BD", "** REASON: Unfortunately, the actual ID (", idUsTypePerm,
                                      ") in the AppsObjs table cannot be deleted because it has a relationship with the some tables (Profiles). ",
                                      "You will need to delete it before continuing with the operation." );
            }

            if (!boolUsTypePerm)
            {
                return String.Concat( "** REASON: Unfortunately, the actual ID (", idUsTypePerm,
                                      ") whose parameter with described value is not in the AppsObjs table. Try other ID." );
            }

            return "";
        }

        private string MsgResponseUstypeperm( TbUstypeperm tbUstypeperm, long idUstypeperm, string operation )
        {
            strResponse = "";

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de AppsObjs no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "DEL")
            {
                strResponse = TbUstypepermExists( idUstypeperm );
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar se a informação da ustypeperm não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).

                strResponse = (tbUstypeperm == null || tbUstypeperm.DeletedAt != null) ?
                    String.Concat( "** REASON: Unfortunately, the actual ID (", idUstypeperm, ") is not in the Ustypeperms table. Try other ID." ) : "";
            }

            return strResponse;
        }

        private string ValidatorFieldsUstyeperm( string option, long id )
        {
            String strTentativas = "";

            if (!id.Equals( -1 ))
            {
                strTentativas += (option.Equals( "Pro" ) && id == 0) ? "id_ustypeperms (no code)" : "";
                strTentativas += (option.Equals( "UsT" ) && id == 0) ? "id_usertypes (no code)" : "";
                strTentativas += (option.Equals( "Per" ) && id == 0) ? "id_permissions (no code)" : "";

                strTentativas += (option.Equals( "Pro" ) && IdExistInTBProfile( id ) == null) ? "id_ustypeperms (not exists), " : "";
                strTentativas += (option.Equals( "UsT" ) && IdExistInTBUserType( id ) == null) ? "id_usertypes (not exists), " : "";
                strTentativas += (option.Equals( "Per" ) && IdExistInTBPermission( id ) == null) ? "id_permissions (not exists), " : "";
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private TbProfile? IdExistInTBProfile( long idUstypeperms )
        {
            return (idUstypeperms.Equals( 0 )) ?
                null : _context.TbProfiles.AsNoTracking().FirstOrDefault( x => x.IdUstypeperms == idUstypeperms && x.DeletedAt == null )!;
        }

        private TbUsertype? IdExistInTBUserType( long idUserType )
        {
            return (idUserType.Equals( 0 )) ?
                null : _context.TbUsertypes.AsNoTracking().FirstOrDefault( x => x.Id == idUserType && x.DeletedAt == null )!;
        }

        private TbPermission? IdExistInTBPermission( long idPermission )
        {
            return (idPermission.Equals( 0 )) ?
                null : _context.TbPermissions.AsNoTracking().FirstOrDefault( x => x.Id == idPermission && x.DeletedAt == null )!;
        }
    }
}