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
    public class TbAppsobjsController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;

        public TbAppsobjsController(AccessControlContext context)
        {
            _context = context;
        }

        // GET: api/TbAppsobjs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbAppsobj>>> GetTbAppsobjs()
        {
            // Retorna contendo uma lista de todas as aplicações relacionadas com objetos que não estão deletadas (O campo 'deleted_at' cujo valor padrão sempre é nulo).

            return await _context.TbAppsobjs.Where( x => x.DeletedAt == null ).ToListAsync();
        }

        // GET: api/TbAppsobjs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbAppsobj>> GetTbAppsobj( long id )
        {
            var tbAppsobj = await _context.TbAppsobjs.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseAppsObj( tbAppsobj!, id, "GET" ) ) || !tbAppsobj!.DeletedAt.Equals( null ))
            {
                return NotFound( strResponse );
            }

            return tbAppsobj;
        }

        // POST: api/TbAppsobjs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbAppsobj>> PostTbAppsobj( TbAppsobj tbAppsobj )
        {
            string MainMsg  = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the AppsObjs table!",
                                             " Broken rules: one of the fields (");

            string ValidMsg = ValidatorFieldsAppsObj( "Pro", tbAppsobj.Id ).Trim() + 
                              ValidatorFieldsAppsObj( "App", tbAppsobj.IdApplications!.Value ).Trim() +
                              ValidatorFieldsAppsObj( "Obj", tbAppsobj.IdObjects!.Value ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg.Replace( ")id_", "), id_" ), ") contain some problems. Please, check it again." ) );
            }

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbAppsobj.CreatedAt = DateTime.Now;

            _context.TbAppsobjs.Add( tbAppsobj );
            await _context.SaveChangesAsync();

            return CreatedAtAction( "GetTbAppsobj", new { id = tbAppsobj.Id }, tbAppsobj );
        }

        // DELETE: api/TbAppsobjs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbAppsobj( long id )
        {
            var tbAppsobj = await _context.TbAppsobjs.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseAppsObj( tbAppsobj!, id, "DEL" ) ))
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
            // [_context TbAppsobjs Remove( tbAppsobj );]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente. 

            tbAppsobj!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the ID (", id, ") of the AppsObjs table is deleted." ) );
        }

        private string TbAppsobjExists( long idAppsobj )
        {
            var boolProfiles = _context.TbProfiles.Any( x => x.IdAppsobjs == idAppsobj && x.DeletedAt == null )!;
            var boolAppsObjs = _context.TbAppsobjs.Any( e => e.Id == idAppsobj && e.DeletedAt == null );

            if (!boolProfiles)
            {
                return String.Concat( "BD", "** REASON: Unfortunately, the actual ID (", idAppsobj,
                                      ") in the AppsObjs table cannot be deleted because it has a relationship with the some tables (Profiles). ",
                                      "You will need to delete it before continuing with the operation." );
            }

            if (!boolAppsObjs)
            {
                return String.Concat( "** REASON: Unfortunately, the actual ID (", idAppsobj,
                                      ") whose parameter with described value is not in the AppsObjs table. Try other ID." );
            }

            return "";
        }

        private string MsgResponseAppsObj( TbAppsobj tbAppObj, long idAppObj, string operation )
        {
            strResponse = "";

            // Inclusão do método privado TbAppsobjExists para localizar ID na tabela de AppsObjs no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "DEL")
            {
                strResponse = TbAppsobjExists( idAppObj );
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar se a informação da appsobjs não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).

                strResponse = (tbAppObj == null || tbAppObj.DeletedAt != null) ?
                    String.Concat( "** REASON: Unfortunately, the actual ID (", idAppObj, ") is not in the AppsObjs table. Try other ID." ) : "";
            }

            return strResponse;
        }

        private string ValidatorFieldsAppsObj( string option, long id )
        {
            String strTentativas = "";

            if (!id.Equals( -1 ))
            {
                strTentativas += (option.Equals( "Pro" ) && id == 0) ? "id_appsobjs (no code)" : "";
                strTentativas += (option.Equals( "App" ) && id == 0) ? "id_applications (no code)" : "";
                strTentativas += (option.Equals( "Obj" ) && id == 0) ? "id_objects (no code)" : "";

                strTentativas += (option.Equals( "Pro" ) && IdExistInTBProfile( id ) == null) ? "id_appsobjs (not exists), " : "";
                strTentativas += (option.Equals( "App" ) && IdExistInTBApplication( id ) == null) ? "id_applications (not exists), " : "";
                strTentativas += (option.Equals( "Obj" ) && IdExistInTBObject( id ) == null) ? "id_objects (not exists), " : "";
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private TbProfile? IdExistInTBProfile( long idAppsObjs )
        {
            return (idAppsObjs.Equals( 0 )) ?
                null : _context.TbProfiles.AsNoTracking().FirstOrDefault( x => x.IdAppsobjs == idAppsObjs && x.DeletedAt == null )!;
        }

        private TbApplication? IdExistInTBApplication( long idApplication )
        {
            return (idApplication.Equals( 0 )) ?
                null : _context.TbApplications.AsNoTracking().FirstOrDefault( x => x.Id == idApplication && x.DeletedAt == null )!;
        }

        private TbObject? IdExistInTBObject( long idObject )
        {
            return (idObject.Equals( 0 )) ?
                null : _context.TbObjects.AsNoTracking().FirstOrDefault( x => x.Id == idObject && x.DeletedAt == null )!;
        }
    }
}