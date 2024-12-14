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
    public class TbProfilesController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;

        public TbProfilesController( AccessControlContext context )
        {
            _context = context;
        }

        // GET: api/TbProfiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbProfile>>> GetTbProfiles()
        {
            // Retorna contendo uma lista de todos os profiles que não estão deletados (O campo 'deleted_at' cujo valor padrão sempre é nulo).

            return await _context.TbProfiles.Where( x => x.DeletedAt == null ).ToListAsync();
        }

        // GET: api/TbProfiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbProfile>> GetTbProfile( long id )
        {
            var tbProfile = await _context.TbProfiles.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseProfile( tbProfile!, id, "GET" )) || !tbProfile!.DeletedAt.Equals( null ))
            {
                return NotFound( strResponse );
            }

            return tbProfile;
        }


        // POST: api/TbProfiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbProfile>> PostTbProfile( TbProfile tbProfile )
        {
            string MainMsg  = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the Profiles table!",
                                            " Broken rules: one of the fields (" );

            string ValidMsg = ValidatorFieldsProfile( "id_appsobjs", tbProfile.IdAppsobjs!.Value ).Trim() +
                              ValidatorFieldsProfile( "id_usersapps", tbProfile.IdUsersapps!.Value ).Trim() +
                              ValidatorFieldsProfile( "id_ustypeperms", tbProfile.IdUstypeperms!.Value ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg.Replace( ")id_", "), id_" ), ") contain some problems. Please, check it again." ) );
            }

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbProfile.CreatedAt = DateTime.Now;

            _context.TbProfiles.Add( tbProfile );
            await _context.SaveChangesAsync();

            return CreatedAtAction( "GetTbProfile", new { id = tbProfile.Id }, tbProfile );
        }

        // DELETE: api/TbProfiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbProfile( long id )
        {
            var tbProfile = await _context.TbProfiles.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseProfile( tbProfile!, id, "DEL" )))
            {
                return NotFound( strResponse );
            }

            // Remoção desta linha pois não faz parte de minha estratégia (exclusão física na tabela)
            // [_context TbProfiles Remove(tbProfile!);]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente. 

            tbProfile!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the ID (", id, ") of the Profiles table is deleted." ) );
        }

        private bool TbProfileExists( long id )
        {
            return _context.TbProfiles.Any( e => e.Id == id && e.DeletedAt == null );
        }

        private string MsgResponseProfile( TbProfile? tbProfile, long idProfile, string operation )
        {
            strResponse = "";

            // Inclusão do método privado TbProfileExists para localizar ID na tabela de Profiles no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbUser do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "DEL")
            {

                if (!TbProfileExists( idProfile ))
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", idProfile,
                                                 ") whose parameter with described value is not in the Profiles table. Try other ID." );
                }
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar se a informação da profile não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).

                strResponse = (tbProfile == null || tbProfile.DeletedAt != null) ?
                    String.Concat( "** REASON: Unfortunately, the actual ID (", idProfile, ") is not in the Profiles table. Try other ID." ) : "";
            }

            return strResponse;
        }

        private string ValidatorFieldsProfile( string option, long id )
        {
            String strTentativas = "";

            if (!id.Equals( -1 ))
            {
                strTentativas += (id == 0) ? String.Concat( option, " (no code)" ) : "";

                strTentativas += (option.Equals( "id_appsobjs" ) && IdExistInTBAppsObjs( id ) == null) ? "id_appsobjs (not exists), " : "";
                strTentativas += (option.Equals( "id_usersapps" ) && IdExistInTBUsersApps( id ) == null) ? "id_usersapps (not exists), " : "";
                strTentativas += (option.Equals( "id_ustypeperms" ) && IdExistInTBUsTypePerm( id ) == null) ? "id_ustypeperms (not exists), " : "";
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private TbAppsobj? IdExistInTBAppsObjs( long idAppsobjs )
        {
            return (idAppsobjs.Equals( 0 )) ?
                null : _context.TbAppsobjs.AsNoTracking().FirstOrDefault( x => x.Id == idAppsobjs && x.DeletedAt == null )!;
        }

        private TbUsersapp? IdExistInTBUsersApps( long idUsersapps )
        {
            return (idUsersapps.Equals( 0 )) ?
                null : _context.TbUsersapps.AsNoTracking().FirstOrDefault( x => x.Id == idUsersapps && x.DeletedAt == null )!;
        }

        private TbUstypeperm? IdExistInTBUsTypePerm( long idUsTypePerm )
        {
            return (idUsTypePerm.Equals( 0 )) ?
                null : _context.TbUstypeperms.AsNoTracking().FirstOrDefault( x => x.Id == idUsTypePerm && x.DeletedAt == null )!;
        }
    }
}