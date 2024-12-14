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
    public class TbUsersController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;

        public TbUsersController( AccessControlContext context )
        {
            _context = context;
        }

        // GET: api/TbUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbUser>>> GetTbUsers()
        {
            // Retorna contendo uma lista de todos os usuários que não estão deletados (O campo 'deleted_at' cujo valor padrão sempre é nulo).
            var tbUsr = await _context.TbUsers.Where( x => x.DeletedAt == null ).ToListAsync();

            return tbUsr;
        }

        // GET: api/TbUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbUser>> GetTbUser( long id )
        {
            var tbUser = await _context.TbUsers.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseUser( tbUser!, id, "GET" ) ))
            {
                return NotFound(strResponse);
            }

            return tbUser!;
        }

        // POST: api/TbUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbUser>> PostTbUser( TbUser tbUser )
        {
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the Users table!",
                                            " Broken rules: one of the fields (" );

            ValidMsg = ValidatorFieldsUser( tbUser, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsUser( tbUser, 2 ).Trim();
            
            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbUser.CreatedAt = DateTime.Now;

            _context.TbUsers.Add( tbUser );
            await _context.SaveChangesAsync();

            return CreatedAtAction( "GetTbUser", new { id = tbUser.Id }, tbUser );
        }

        // PUT: api/TbUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<TbUser?>> PutTbUser( long id, TbUser tbUser )
        {
            strResponse = "";
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to update the actual ID (", id,
                                            ") in the Users table! Broken rules: one of the fields (");

            // Inclusão do método privado FindUserByID para localizar ID na tabela de Usuários no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbUser do método principal.
            //      é necessário usar AsNoTracking().

            var findInTbUser = FindUserByID( id );

            if (!string.IsNullOrEmpty( MsgResponseUser( tbUser, id, "PUT" ) ))
            {
                return NotFound( strResponse );
            }

            ValidMsg = ValidatorFieldsUser( tbUser, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again.") );
            }

            ValidMsg = ValidatorFieldsUser( tbUser, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            _context.Entry(tbUser).State = EntityState.Modified;

            try
            {
                // Para evitar erros bobos, no API, como a não inclusão de campo 'CreatedAt' no JSON sem valor preenchido
                // o que grava no banco o valor "01-01-0001 00:00:00" e não na data e hora atual do sistema.
                tbUser.CreatedAt = findInTbUser!.CreatedAt;
                tbUser.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {

                if (!TbUserExists( id ))
                {
                    return NotFound( strResponse );
                }

                else
                {
                    throw;
                }
            }

            return await _context.TbUsers.FindAsync( id );
        }

        // DELETE: api/TbUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbUser( long id )
        {
            var tbUser = await _context.TbUsers.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseUser( tbUser!, id, "DEL" ) ))
            {
                return NotFound( strResponse );
            }

            // Remoção desta linha pois não faz parte de minha estratégia (exclusão física na tabela).
            // [_context TbUsers Remove(tbUser);]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente.

            tbUser!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the ID (", id, ") of the Users table is deleted." ) );
        }

        private bool TbUserExists( long idUser )
        {
            return _context.TbUsers.Any( e => e.Id == idUser && e.DeletedAt == null );
        }

        private string MsgResponseUser( TbUser tbUser, long idUser, string operation )
        {
            strResponse = "";

            // Inclusão do método privado FindApplicationByID para localizar ID na tabela de Usuários no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbUser do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "PUT")
            {

                if (tbUser.Id == 0)
                {
                    strResponse = "** REASON: Unfortunately, the field 'ID' in the JSON format is not present in User set. Put the field and try it.";
                }

                if (tbUser.DeletedAt != null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", tbUser.Id, ") is not in the Users table. Try other ID." );
                }

                if (idUser != tbUser.Id)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idUser,
                                       ") whose parameter with described value is different than the ID filled (", tbUser.Id,
                                       ") in set of user by the JSON format. Fill the equal ID in both." );
                }

                if (FindUserByID( idUser ) == null)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, the actual ID (", idUser,
                                       ") whose parameter with described value is not in the Users table. Try other ID." );
                }
            }

            if (operation == "DEL")
            {
                if (FindUserByID( idUser ) == null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", idUser,
                                                 ") whose parameter with described value is not in the Users table. Try other ID.");
                }

                // Encontrar ID de Usuário na(s) tabela(s) relacionada(s) - IDAppExistInRelationships.
                if (tbUser != null)
                {
                    strResponse =(IDUserExistInRelationships( tbUser.Id ).Length > 0) ?
                                    String.Concat(
                                        "** REASON: Unfortunately, the actual ID (", idUser,
                                        ") in the Users table cannot be deleted because it has a relationship with the some tables (TBUsrAccess).",
                                        "You will need to delete it before continuing with the operation." ) : "";
                }
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar a aplicação que não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).
                strResponse = (tbUser == null || tbUser.DeletedAt != null) ?
                    String.Concat( "REASON: Unfortunately, the actual ID (", idUser, ") is not in the Users table. Try other ID.") : "";
            }

            return strResponse;
        }

        private TbUser? FindUserByID( long idUser )
        {
            return _context.TbUsers.AsNoTracking().FirstOrDefault( x => x.Id == idUser );
        }

        private string IDUserExistInRelationships( long idUser )
        {
            var tbUsersApps = _context.TbUsraccesses.AsNoTracking().FirstOrDefault( ua => ua.IdUsers == idUser );

            return (tbUsersApps != null ? "TBUsrAccess" : "");
        }

        private static string ValidatorFieldsUser( TbUser tbUser, int option )
        {
            String strTentativas = "";

            if (option.Equals( 1 ))
            {
                strTentativas +=
                    CharactLimitUser( "Name", tbUser.Name.Trim(), 100, option ) +
                    CharactLimitUser( "Email", tbUser.Email!.Trim(), 100, option ) +
                    CharactLimitUser( "Description", tbUser.Description!.Trim(), 250, option );
            }

            if (option.Equals( 2 ))
            {
                strTentativas +=
                    CharactLimitUser( "Name", tbUser.Name.Trim(), 3, option ) +
                    CharactLimitUser( "Email", tbUser.Email!.Trim(), 3, option ) +
                    CharactLimitUser( "Description", tbUser.Description!.Trim(), 3, option );
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private static string CharactLimitUser( String strField, String strValueField, int lengthField, int option )
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