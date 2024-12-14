using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using netwebapi_access_control.Data;
using netwebapi_access_control.DataSP;
using netwebapi_access_control.Models;

namespace netwebapi_access_control.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TbApplicationsController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;
        private readonly AccessControlContextSP _contextSP;

        public TbApplicationsController( AccessControlContext context, AccessControlContextSP contextSP )
        {
            _context = context;
            _contextSP = contextSP;
        }

        // GET: api/TbApplications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbApplication>>> GetTbApplications()
        {
            // Retorna contendo uma lista de todas as aplicações que não estão deletadas (O campo 'deleted_at' cujo valor padrão sempre é nulo).
            return await _context.TbApplications.Where( x => x.DeletedAt == null ).ToListAsync();
        }

        // GET: api/TbApplications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbApplication>> GetTbApplication( long id )
        {
            var tbApplication = await _context.TbApplications.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseApplication( tbApplication!, id, "GET" ) ) || !tbApplication!.DeletedAt.Equals( null ))
            {
                return NotFound( strResponse );
            }

            return tbApplication!;
        }

        // POST: api/TbApplications
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbApplication>> PostTbApplication( TbApplication tbApplication )
        {
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the Applications table!",
                                            " Broken rules: one of the fields (" );

            ValidMsg = ValidatorFieldsApplication( tbApplication, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsApplication( tbApplication, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbApplication.CreatedAt = DateTime.Now;

            _context.TbApplications.Add( tbApplication );
            await _context.SaveChangesAsync();

            return CreatedAtAction( "GetTbApplication", new { id = tbApplication.Id }, tbApplication );
        }

        // PUT: api/TbApplications/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<TbApplication?>> PutTbApplication( long id, TbApplication tbApplication )
        {
            strResponse = "";
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to update the actual ID (", id,
                                            ") in the Applications table! Broken rules: one of the fields (" );

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de AppsObjs no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            var findInTbApplication = FindApplicationByID( id );

            if (!string.IsNullOrEmpty( MsgResponseApplication( tbApplication, id, "PUT" ) ) )
            {
                return NotFound( strResponse );
            }

            ValidMsg = ValidatorFieldsApplication( tbApplication, 1 ).Trim();
            
            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsApplication( tbApplication, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            _context.Entry( tbApplication ).State = EntityState.Modified;

            try
            {
                // Para evitar erros bobos, no API, como a não inclusão de campo 'CreatedAt' no JSON sem valor preenchido
                // o que grava no banco o valor "01-01-0001 00:00:00" e não na data e hora atual do sistema.
                tbApplication.CreatedAt = findInTbApplication!.CreatedAt;
                tbApplication.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {

                if (!TbApplicationExists( id ))
                {
                    return NotFound( strResponse );
                }

                else
                {
                    throw;
                }
            }

            return await _context.TbApplications.FindAsync( id );
        }

        // DELETE: api/TbApplications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbApplication( long id )
        {
            var tbApplication = await _context.TbApplications.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseApplication( tbApplication!, id, "DEL" ) ) )
            {
                return NotFound( strResponse );
            }

            // Remoção desta linha pois não faz parte de minha estratégia (exclusão física na tabela)
            // [_context TbApplications Remove(tbApplication);]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente. 

            tbApplication!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the ID (", id, ") of the Applications table is deleted." ) );
        }

        // Chamando Stored Procedure
        [HttpGet("UsersByApplication")]
        public async Task<ActionResult<IEnumerable<SPUsersAppsRelUsrByApp>>> GetSPUsersAppsRelUsrByApp( long id )
        {
            var MainMsg = String.Concat( "** REASON: Unfortunately, the actual ID (", id, ") is not in the Applications table. Try other ID." );
            var existingIDTBUsersApps = await _context.TbUsersapps.AsNoTracking().FirstOrDefaultAsync( x => x.IdApplications == id );

            if (existingIDTBUsersApps == null)
            {
                return NotFound( MainMsg );
            }

            else
            {
                var result = await _contextSP.SPUsersAppsRelUsrByApp
                                   .FromSqlRaw( "call `access_control_ms`.`sp_Report_USRBYAPP`( {0} )", id )
                                   .ToListAsync();

                // Mostrar a aplicação que não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).
                if (result.Count == 0)
                {
                    return NotFound( MainMsg );
                }

                return result;
            }
        }

        private bool TbApplicationExists( long idApplication )
        {
            return _context.TbApplications.Any( e => e.Id == idApplication && e.DeletedAt == null );
        }

        private string MsgResponseApplication( TbApplication tbApplication, long idApplication, string operation )
        {
            strResponse = "";

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de Aplicações no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "PUT")
            {

                if (tbApplication.Id == 0)
                {
                    strResponse = "** REASON: Unfortunately, the field 'ID' in the JSON format is not present in Application set. Put the field and try it.";
                }

                if (tbApplication.DeletedAt != null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", tbApplication.Id, ") is not in the Applications table. Try other ID." );
                }

                if (idApplication != tbApplication.Id)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idApplication,
                                       ") whose parameter with described value is different than the ID filled (", tbApplication.Id,
                                       ") in set of Application by the JSON format. Fill the same correct ID in both." );
                }

                if (FindApplicationByID( idApplication ) == null)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idApplication,
                                       ") whose parameter with described value is not in the Applications table. Try other ID." );
                }
            }

            if (operation == "DEL")
            {

                if (FindApplicationByID( idApplication ) == null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", idApplication,
                                                 ") whose parameter with described value is not in the Applications table. Try other ID." );
                }

                // Encontrar ID de Aplicação na(s) tabela(s) relacionada(s) - IDAppExistInRelationships.
                if (tbApplication != null)
                {
                    strResponse = (IDAppExistInRelationships( tbApplication.Id ).Length > 0) ?
                                    String.Concat( "** REASON: Unfortunately, the actual ID (", idApplication,
                                                   ") in the Applications table cannot be deleted because it has a relationship with the some tables (",
                                                   IDAppExistInRelationships( tbApplication.Id ),
                                                   ").", "You will need to delete it before continuing with the operation." ) : "";
                }
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar a aplicação que não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).
                strResponse = (tbApplication == null || tbApplication.DeletedAt != null) ? 
                    String.Concat( "** REASON: Unfortunately, the actual ID (", idApplication, ") is not in the Applications table. Try other ID." ) : "";
            }

            return strResponse;
        }

        private TbApplication? FindApplicationByID( long idApplication )
        {
            return _context.TbApplications.AsNoTracking().FirstOrDefault( x => x.Id == idApplication );
        }

        private string IDAppExistInRelationships( long idApplication )
        {
            var tbUsersApps = _context.TbUsersapps.AsNoTracking().FirstOrDefault( ua => ua.IdApplications == idApplication );
            var tbAppsobj   = _context.TbAppsobjs.AsNoTracking().FirstOrDefault( ua => ua.IdApplications == idApplication );
            var strReturn   = "";

            if (tbAppsobj != null)
            {
                strReturn = (tbUsersApps == null) ? "TBAppsObjs" : "TBAppsObjs and TBUsersApps";
            }

            else
            {
                strReturn = (tbUsersApps == null) ? "" : "TBUserApps";
            }

            return strReturn;
        }

        private static string ValidatorFieldsApplication( TbApplication tbApplication, int option )
        {
            String strTentativas = "";

            if (option.Equals( 1 ))
            {
                strTentativas +=
                    CharactLimitApplication( "Name", tbApplication.Name.Trim(), 100, option ) +
                    CharactLimitApplication( "Title", tbApplication.Title.Trim(), 100, option ) +
                    CharactLimitApplication( "Acronym", tbApplication.Acronym.Trim(), 20, option ) +
                    CharactLimitApplication( "Description", tbApplication.Description.Trim(), 250, option );
            }

            if (option.Equals( 2 ))
            {
                strTentativas +=
                    CharactLimitApplication( "Name", tbApplication.Name.Trim(), 3, option ) +
                    CharactLimitApplication( "Title", tbApplication.Title.Trim(), 3, option ) +
                    CharactLimitApplication( "Acronym", tbApplication.Acronym.Trim(), 3, option ) +
                    CharactLimitApplication( "Description", tbApplication.Description.Trim(), 3, option );
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private static string CharactLimitApplication( String strField, String strValueField, int lengthField, int option )
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