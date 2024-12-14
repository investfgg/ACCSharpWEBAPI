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
    public class TbUsertypesController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;

        public TbUsertypesController( AccessControlContext context )
        {
            _context = context;
        }

        // GET: api/TbUsertypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbUsertype>>> GetTbUsertypes()
        {
            // Retorna contendo uma lista de todas as usertypes que não estão deletadas (O campo 'deleted_at' cujo valor padrão sempre é nulo).
            return await _context.TbUsertypes.Where( x => x.DeletedAt == null ).ToListAsync();
        }

        // GET: api/TbUsertypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbUsertype>> GetTbUsertype( long id )
        {
            var tbUsertype = await _context.TbUsertypes.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseUsertype( tbUsertype!, id, "GET" ) ) || !tbUsertype!.DeletedAt.Equals( null ))
            {
                return NotFound( strResponse );
            }

            return tbUsertype;
        }

        // POST: api/TbUsertypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbUsertype>> PostTbUsertype( TbUsertype tbUsertype )
        {
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the Usertypes table!",
                                            " Broken rules: one of the fields (" );

            ValidMsg = ValidatorFieldsUserType( tbUsertype, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsUserType( tbUsertype, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbUsertype.CreatedAt = DateTime.Now;

            _context.TbUsertypes.Add(tbUsertype);
            await _context.SaveChangesAsync();

            return CreatedAtAction( "GetTbUsertype", new { id = tbUsertype.Id }, tbUsertype );
        }

        // PUT: api/TbUsertypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<TbUsertype?>> PutTbUsertype( long id, TbUsertype tbUsertype )
        {
            strResponse = "";
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to update the actual ID (", id,
                                            ") in the Usertypes table! Broken rules: one of the fields (" );

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de Objetos no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            var findInTbUserType = FindUserTypeByID( id );

            if (!string.IsNullOrEmpty(MsgResponseUsertype( tbUsertype, id, "PUT" ) ))
            {
                return NotFound( strResponse );
            }

            ValidMsg = ValidatorFieldsUserType( tbUsertype, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsUserType( tbUsertype, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            _context.Entry( tbUsertype ).State = EntityState.Modified;

            try
            {
                // Para evitar erros bobos, no API, como a não inclusão de campo 'CreatedAt' no JSON sem valor preenchido
                // o que grava no banco o valor "01-01-0001 00:00:00" e não na data e hora atual do sistema.
                tbUsertype.CreatedAt = findInTbUserType!.CreatedAt;
                tbUsertype.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {

                if (!TbUsertypeExists( id ))
                {
                    return NotFound( strResponse );
                }

                else
                {
                    throw;
                }
            }

            return await _context.TbUsertypes.FindAsync( id );
        }

        // DELETE: api/TbUsertypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbUsertype( long id )
        {
            var tbUserType = await _context.TbUsertypes.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseUsertype( tbUserType!, id, "DEL" ) ))
            {
                return NotFound( strResponse );
            }

            // Remoção desta linha pois não faz parte de minha estratégia (exclusão física na tabela)
            // [_context TbUsertypes Remove( tbUsertype );]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente. 

            tbUserType!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the ID (", id, ") of the Usertypes table is deleted." ) );
        }

        private bool TbUsertypeExists( long id )
        {
            return _context.TbUsertypes.Any( e => e.Id == id && e.DeletedAt == null );
        }

        private string MsgResponseUsertype( TbUsertype tbUsertype, long idUsertype, string operation )
        {
            strResponse = "";

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de Usertypes no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbApplication do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "PUT")
            {

                if (tbUsertype.Id == 0)
                {
                    strResponse = "** REASON: Unfortunately, the field 'ID' in the JSON format is not present in Usertype set. Put the field and try it.";
                }

                if (tbUsertype.DeletedAt != null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", tbUsertype.Id, ") is not in the Usertypes table. Try other ID." );
                }

                if (idUsertype != tbUsertype.Id)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idUsertype,
                                       ") whose parameter with described value is different than the ID filled (", tbUsertype.Id,
                                       ") in set of Usertype by the JSON format. Fill the same correct ID in both." );
                }

                if (FindUserTypeByID( idUsertype ) == null)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idUsertype,
                                       ") whose parameter with described value is not in the Usertypes table. Try other ID." );
                }
            }

            if (operation == "DEL")
            {

                if (FindUserTypeByID( idUsertype ) == null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", idUsertype,
                                                 ") whose parameter with described value is not in the Usertypes table. Try other ID." );
                }

                // Encontrar ID de Usertype na(s) tabela(s) relacionada(s) - IDAppExistInRelationships.
                if (tbUsertype != null)
                {
                    strResponse = (IDObjExistInRelationships( tbUsertype.Id ).Length > 0) ?
                                    String.Concat( "** REASON: Unfortunately, the actual ID (", idUsertype,
                                                   ") in the Usertypes table cannot be deleted because it has a relationship with the some tables (",
                                                   IDObjExistInRelationships( tbUsertype.Id ),
                                                   ").", " You will need to delete it before continuing with the operation." ) : "";
                }
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar a aplicação que não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).
                strResponse = (tbUsertype == null || tbUsertype.DeletedAt != null) ?
                    String.Concat( "** REASON: Unfortunately, the actual ID (", idUsertype, ") is not in the Usertypes table. Try other ID." ) : "";
            }

            return strResponse;
        }

        private TbUsertype? FindUserTypeByID( long idUsertype )
        {
            return _context.TbUsertypes.AsNoTracking().FirstOrDefault( x => x.Id == idUsertype );
        }

        private string IDObjExistInRelationships( long idUsertype )
        {
            var tbUstypeperm = _context.TbUstypeperms.AsNoTracking().FirstOrDefault( utp => utp.IdUsertypes == idUsertype && utp.DeletedAt == null );

            return (tbUstypeperm != null) ? "TBUsTypePerms" : "";
        }

        private static string ValidatorFieldsUserType( TbUsertype? tbUsertype, int option )
        {
            String strTentativas = "";

            if (option.Equals( 1 ))
            {
                strTentativas +=
                    CharactLimitUserType( "Title", tbUsertype!.Title.Trim(), 100, option ) +
                    CharactLimitUserType( "Description", tbUsertype!.Description.Trim(), 250, option );
            }

            if (option.Equals( 2 ))
            {
                strTentativas +=
                    CharactLimitUserType( "Title", tbUsertype!.Title.Trim(), 3, option ) +
                    CharactLimitUserType( "Description", tbUsertype!.Description.Trim(), 3, option );
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private static string CharactLimitUserType( String strField, String strValueField, int lengthField, int option )
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