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
    public class TbUsersappsController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;

        public TbUsersappsController( AccessControlContext context )
        {
            _context = context;
        }

        // GET: api/TbUsersapps
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbUsersapp>>> GetTbUsersapps()
        {
            // Retorna contendo uma lista de todos os usuários relacionados com aplicações que não estão deletados (O campo 'deleted_at' cujo valor padrão sempre é nulo).

            return await _context.TbUsersapps.Where( x => x.DeletedAt == null ).ToListAsync();
        }

        // GET: api/TbUsersapps/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbUsersapp>> GetTbUsersapp( long id )
        {
            var tbUsersApp = await _context.TbUsersapps.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseUserApp( tbUsersApp!, id, "GET" )) || !tbUsersApp!.DeletedAt.Equals( null ))
            {
                return NotFound( strResponse );
            }

            return tbUsersApp;
        }

        // POST: api/TbUsersapps
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbUsersapp>> PostTbUsersapp( TbUsersapp tbUsersApp )
        {
            string MainMsg  = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the UsersApps table!",
                                             " Broken rules: one of the fields (" );

            string ValidMsg = ValidatorFieldsUserApp( "Pro", tbUsersApp.Id ).Trim() +
                              ValidatorFieldsUserApp( "App", tbUsersApp.IdApplications!.Value ).Trim() +
                              ValidatorFieldsUserApp( "UsA", tbUsersApp.IdUsrsaccess!.Value ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg.Replace( ")id_", "), id_" ), ") contain some problems. Please, check it again." ) );
            }

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbUsersApp.CreatedAt = DateTime.Now;

            _context.TbUsersapps.Add( tbUsersApp );
            await _context.SaveChangesAsync();

            return CreatedAtAction( "GetTbUsersapp", new { id = tbUsersApp.Id }, tbUsersApp );
        }

        // DELETE: api/TbUsersapps/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbUsersapp( long id )
        {
            var tbUsersApp = await _context.TbUsersapps.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseUserApp( tbUsersApp!, id, "DEL" ) ))
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
            // [_context TbUsersApp Remove(tbUsersApp);]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente. 

            tbUsersApp!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the ID (", id, ") of the UsersApp table is deleted." ) );
        }

        private string TbUsersappExists( long idUsersApp )
        {
            var boolProfiles = _context.TbProfiles.Any( x => x.IdUsersapps == idUsersApp && x.DeletedAt == null )!;
            var boolUserApps = _context.TbUsersapps.Any( e => e.Id == idUsersApp && e.DeletedAt == null );

            if (!boolProfiles)
            {
                return String.Concat( "BD", "** REASON: Unfortunately, the actual ID (", idUsersApp,
                                      ") in the UsersApps table cannot be deleted because it has a relationship with the some tables (Profiles). ",
                                      "You will need to delete it before continuing with the operation." );
            }

            if (!boolUserApps)
            {
                return String.Concat( "** REASON: Unfortunately, the actual ID (", idUsersApp,
                                      ") whose parameter with described value is not in the UsersApps table. Try other ID." );
            }

            return "";
        }

        private string MsgResponseUserApp( TbUsersapp? tbUserApp, long idUserApp, string operation )
        {
            strResponse = "";

            // Inclusão do método privado TbUsersappExists para localizar ID na tabela de UsersApps no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbUser do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "DEL")
            {
                strResponse = TbUsersappExists( idUserApp );
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar se a informação da usersapps não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).

                strResponse = (tbUserApp == null || tbUserApp.DeletedAt != null) ?
                    String.Concat( "** REASON: Unfortunately, the actual ID (", idUserApp, ") is not in the UsersApps table. Try other ID." ) : "";
            }

            return strResponse;
        }

        private string ValidatorFieldsUserApp( string option, long id )
        {
            String strTentativas = "";

            if (!id.Equals( -1 ))
            {
                strTentativas += (option.Equals( "Pro" ) && id == 0) ? "id_usersapps (no code)" : "";
                strTentativas += (option.Equals( "App" ) && id  == 0) ? "id_applications (no code)" : "";
                strTentativas += (option.Equals( "UsA" ) && id == 0) ? "id_usrsAccess (no code)" : "";

                strTentativas += (option.Equals( "Pro" ) && IdExistInTBProfile( id ) == null) ? "id_usersapps (not exists), " : "";
                strTentativas += (option.Equals( "App" ) && IdExistInTBApplication( id ) == null) ? "id_applications (not exists), " : "";
                strTentativas += (option.Equals( "UsA" ) && IdExistInTBUsrAccess( id ) == null) ? "id_usrsAccess (not exists), " : "";
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private TbProfile? IdExistInTBProfile( long idUsersApps )
        {
            return (idUsersApps.Equals( 0 )) ?
                null : _context.TbProfiles.AsNoTracking().FirstOrDefault( x => x.IdUsersapps == idUsersApps && x.DeletedAt == null )!;
        }

        private TbApplication? IdExistInTBApplication( long idApplication )
        {
            return (idApplication.Equals( 0 )) ?
                null : _context.TbApplications.AsNoTracking().FirstOrDefault( x => x.Id == idApplication && x.DeletedAt == null )!;
        }

        private TbUsraccess? IdExistInTBUsrAccess( long idUsrAccess )
        {
            return (idUsrAccess.Equals( 0 )) ?
                null : _context.TbUsraccesses.AsNoTracking().FirstOrDefault( x => x.Id == idUsrAccess && x.DeletedAt == null )!;
        }
    }
}