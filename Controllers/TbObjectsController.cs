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
    public class TbObjectsController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;

        public TbObjectsController(AccessControlContext context)
        {
            _context = context;
        }

        // GET: api/TbObjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbObject>>> GetTbObjects()
        {
            // Retorna contendo uma lista de todos os objetos que não estão deletadas (O campo 'deleted_at' cujo valor padrão sempre é nulo).
            return await _context.TbObjects.Where( x => x.DeletedAt == null ).ToListAsync();
        }

        // GET: api/TbObjects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbObject>> GetTbObject( long id )
        {
            var tbObject = await _context.TbObjects.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseObject( tbObject!, id, "GET" ) ) || !tbObject!.DeletedAt.Equals( null ))
            {
                return NotFound( strResponse );
            }

            return tbObject;
        }

        // POST: api/TbObjects
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbObject>> PostTbObject( TbObject tbObject )
        {
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the Objects table!",
                                            " Broken rules: one of the fields (");

            ValidMsg = ValidatorFieldsObject( tbObject, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsObject( tbObject, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbObject.CreatedAt = DateTime.Now;

            _context.TbObjects.Add( tbObject );
            await _context.SaveChangesAsync();

            return CreatedAtAction( "GetTbObject", new { id = tbObject.Id }, tbObject );
        }

        // PUT: api/TbObjects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<TbObject?>> PutTbObject(long id, TbObject tbObject)
        {
            strResponse = "";
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to update the actual ID (", id,
                                            ") in the Objects table! Broken rules: one of the fields (" );

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de Objetos no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            var findInTbObject = FindObjectByID( id );

            if (!string.IsNullOrEmpty( MsgResponseObject( tbObject, id, "PUT" ) ))
            {
                return NotFound( strResponse );
            }

            ValidMsg = ValidatorFieldsObject( tbObject, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsObject( tbObject, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            _context.Entry( tbObject ).State = EntityState.Modified;

            try
            {
                // Para evitar erros bobos, no API, como a não inclusão de campo 'CreatedAt' no JSON sem valor preenchido
                // o que grava no banco o valor "01-01-0001 00:00:00" e não na data e hora atual do sistema.
                tbObject.CreatedAt = findInTbObject!.CreatedAt;
                tbObject.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {

                if (!TbObjectExists( id ))
                {
                    return NotFound( strResponse );
                }

                else
                {
                    throw;
                }
            }

            return await _context.TbObjects.FindAsync( id );
        }

        // DELETE: api/TbObjects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbObject( long id )
        {
            var tbObject = await _context.TbObjects.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseObject( tbObject!, id, "DEL" ) ))
            {
                return NotFound( strResponse );
            }

            // Remoção desta linha pois não faz parte de minha estratégia (exclusão física na tabela)
            // [_context TbObjects Remove( tbObject );]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente. 

            tbObject!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the ID (", id, ") of the Objects table is deleted." ) );
        }

        private bool TbObjectExists( long idObject )
        {
            return _context.TbObjects.Any( e => e.Id == idObject && e.DeletedAt == null );
        }

        private string MsgResponseObject( TbObject tbObject, long idObject, string operation )
        {
            strResponse = "";

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de Objetos no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "PUT")
            {

                if (tbObject.Id == 0)
                {
                    strResponse = "** REASON: Unfortunately, the field 'ID' in the JSON format is not present in Object set. Put the field and try it.";
                }

                if (tbObject.DeletedAt != null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", tbObject.Id, ") is not in the Objects table. Try other ID." );
                }

                if (idObject != tbObject.Id)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idObject,
                                       ") whose parameter with described value is different than the ID filled (", tbObject.Id,
                                       ") in set of Object by the JSON format. Fill the same correct ID in both." );
                }

                if (FindObjectByID( idObject ) == null)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idObject,
                                       ") whose parameter with described value is not in the Objects table. Try other ID." );
                }
            }

            if (operation == "DEL")
            {

                if (FindObjectByID( idObject ) == null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", idObject,
                                                 ") whose parameter with described value is not in the Objects table. Try other ID." );
                }

                // Encontrar ID de Aplicação na(s) tabela(s) relacionada(s) - IDAppExistInRelationships.
                if (tbObject != null)
                {
                    strResponse = (IDObjExistInRelationships( tbObject.Id ).Length > 0) ?
                                    String.Concat( "** REASON: Unfortunately, the actual ID (", idObject,
                                                   ") in the Objects table cannot be deleted because it has a relationship with the some tables (",
                                                   IDObjExistInRelationships( tbObject.Id ),
                                                   ").", " You will need to delete it before continuing with the operation." ) : "";
                }
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar a aplicação que não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).
                strResponse = (tbObject == null || tbObject.DeletedAt != null) ?
                    String.Concat( "** REASON: Unfortunately, the actual ID (", idObject, ") is not in the Objects table. Try other ID." ) : "";
            }

            return strResponse;
        }

        private TbObject? FindObjectByID( long idObject )
        {
            return _context.TbObjects.AsNoTracking().FirstOrDefault( x => x.Id == idObject );
        }

        private string IDObjExistInRelationships( long idObject )
        {
            var tbAppsobj = _context.TbAppsobjs.AsNoTracking().FirstOrDefault( ao => ao.IdObjects == idObject && ao.DeletedAt == null );

            return (tbAppsobj != null) ? "TBAppsObjs" : "";
        }

        private static string ValidatorFieldsObject( TbObject? tbObject, int option )
        {
            String strTentativas = "";

            if (option.Equals( 1 ))
            {
                strTentativas +=
                    CharactLimitObject( "Title", tbObject!.Title.Trim(), 100, option) +
                    CharactLimitObject( "Description", tbObject!.Description.Trim(), 250, option );
            }

            if (option.Equals( 2 ))
            {
                strTentativas +=
                    CharactLimitObject( "Title", tbObject!.Title.Trim(), 3, option ) +
                    CharactLimitObject( "Description", tbObject!.Description.Trim(), 3, option );
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private static string CharactLimitObject( String strField, String strValueField, int lengthField, int option )
        {
            var strReturn = "";

            if (option.Equals( 1 ))
            {
                strReturn = (strValueField.Length > lengthField) ?
                            strField + "(" + strValueField.Length + " of " + lengthField + "), " : "";
            }

            if (option.Equals( 2 ))
            {
                strReturn = (strValueField.Trim().Equals( "" ) || strValueField.Length <= lengthField) ? strField + ", " : "";
            }

            return strReturn;
        }
    }
}