using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using netwebapi_access_control.Data;
using netwebapi_access_control.Models;
using System.Security.Cryptography;
using System.Text;

namespace netwebapi_access_control.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TbUsraccessesController : ControllerBase
    {
        private string strResponse = "";
        private readonly AccessControlContext _context;

        public TbUsraccessesController( AccessControlContext context )
        {
            _context = context;
        }

        // GET: api/TbUsraccesses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbUsraccess>>> GetTbUsraccesses()
        {
            // Retorna contendo uma lista de todos os acessos de usuários que não estão deletados (O campo 'deleted_at' cujo valor padrão sempre é nulo).

            var tbUsraccess = await _context.TbUsraccesses.Where( x => x.DeletedAt == null ).ToListAsync();

            // Retorna alguns campos que contenham informações sigilosas (password e tip) usando o mesmo caracter ("*"), por questões de segurança.

            foreach (var newTb in tbUsraccess)
            {
                newTb.Password = new string( '*', newTb.Password.Trim().Length );
                newTb.Tip      = new string( '*', newTb.Tip.Trim().Length );
            }

            return tbUsraccess;
        }

        // GET: api/TbUsraccesses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbUsraccess>> GetTbUsraccess( long id )
        {
            var tbUsraccess = await _context.TbUsraccesses.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseUsrAccess( tbUsraccess!, id, "GET" ) ))
            {
                return NotFound( strResponse );
            }

            // Retorna alguns campos que contenham informações sigilosas (password e tip) usando o mesmo caracter ("*"), por questões de segurança.

            tbUsraccess!.Password = new string( '*', tbUsraccess.Password.Trim().Length );
            tbUsraccess.Tip       = new string( '*', tbUsraccess.Tip.Trim().Length );

            return tbUsraccess;
        }

        // POST: api/TbUsraccesses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        public async Task<ActionResult<TbUsraccess>> PostTbUsraccess( TbUsraccess tbUsraccess )
        {
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to insert the new ID in the UsrAccess table!",
                                            " Broken rules: one of the fields (");
            long idUser = tbUsraccess.IdUsers!.Value;

            ValidMsg = ValidatorFieldsUsrAccess( tbUsraccess, -1, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsUsrAccess( tbUsraccess, idUser, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ValidMsg.Contains( "Id_Users" ) ?
                                                  ") whose value is not filled in or does not exist in the User table. Please, check it again." :
                                                  ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            // Encriptar a senha.
            tbUsraccess.Password = Encrypt( tbUsraccess.Password );

            // Inserir a data e a hora corrente no campo CreatedAt.
            tbUsraccess.CreatedAt = DateTime.Now;

            _context.TbUsraccesses.Add( tbUsraccess );
            await _context.SaveChangesAsync();

            // Evitar divulgar informações na tela, por questões de segurança.
            tbUsraccess.Password = new string( '*', tbUsraccess.Password.Trim().Length );
            tbUsraccess.Tip      = new string( '*', tbUsraccess.Tip.Trim().Length );

            return CreatedAtAction( "GetTbUsraccess", new { id = tbUsraccess.Id }, tbUsraccess );
        }

        // PUT: api/TbUsraccesses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<TbUsraccess?>> PutTbUsraccess( long id, TbUsraccess tbUsraccess )
        {
            strResponse = "";
            string ValidMsg;
            string MainMsg = String.Concat( "** REASON: Unfortunately, it was not possible to update the actual ID (", id,
                                            ") in the UsrAccess table! Broken rules: one of the fields (" );

            long idUser = tbUsraccess.IdUsers!.Value;

            // Inclusão do método privado FindUserByID para localizar ID na tabela de UserAccess no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbUser do método principal.
            //      é necessário usar AsNoTracking().

            var findInTbUsrAccess = FindUsrAccessByID( id );

            if (!string.IsNullOrEmpty( MsgResponseUsrAccess( tbUsraccess, id, "PUT" ) ))
            {
                return NotFound( strResponse );
            }

            ValidMsg = ValidatorFieldsUsrAccess( tbUsraccess, -1, 1 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg, ") contain greater than permitted. Please, check it again." ) );
            }

            ValidMsg = ValidatorFieldsUsrAccess( tbUsraccess, idUser, 2 ).Trim();

            if (ValidMsg.Length > 0)
            {
                return BadRequest( String.Concat( MainMsg, ValidMsg,
                                                  ValidMsg.Contains( "Id_Users" ) ?
                                                                     ") whose value (" + idUser + ") was not found of the Users table. Please, check it again." :
                                                                     ") cannot be null or empty, or contain no fewer than 3 characters. Please, check it again." ) );
            }

            _context.Entry( tbUsraccess ).State = EntityState.Modified;

            try
            {
                // Encriptar a senha para evitar gravação da senha não encriptada.
                tbUsraccess.Password = Encrypt( tbUsraccess.Password );

                // Para evitar erros bobos, no API, como a não inclusão de campo 'CreatedAt' no JSON sem valor preenchido
                // o que grava no banco o valor "01-01-0001 00:00:00" e não na data e hora atual do sistema.
                tbUsraccess.CreatedAt = findInTbUsrAccess!.CreatedAt;
                tbUsraccess.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Evitar divulgar informações na tela, por questões de segurança.
                tbUsraccess.Password = new string( '*', tbUsraccess.Password.Trim().Length );
                tbUsraccess.Tip      = new string( '*', tbUsraccess.Tip.Trim().Length );
            }

            catch (DbUpdateConcurrencyException)
            {

                if (!TbUsraccessExists( id ))
                {
                    return NotFound( strResponse );
                }

                else
                {
                    throw;
                }
            }

            return await _context.TbUsraccesses.FindAsync( id );
        }

        // DELETE: api/TbUsraccesses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbUsraccess( long id )
        {
            var tbUsrAccess = await _context.TbUsraccesses.FindAsync( id );

            if (!string.IsNullOrEmpty( MsgResponseUsrAccess( tbUsrAccess!, id, "DEL" ) ))
            {
                return NotFound( strResponse );
            }

            // Remoção desta linha pois não faz parte de minha estratégia (exclusão física na tabela). [_context TbUsers Remove(tbUsrAccess);]
            // Motivo principal:
            //      Seria ético apenas o administrador mostrar quais "linhas" deletadas através do campo "DeletedAt",
            //      elaborando um relatório de dados deletados de maneira concreta ou consequente.

            tbUsrAccess!.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok( String.Concat( "** REASON: Fortunately, the actual ID (", id, ") of the UsrAccess table is deleted." ) );
        }

        private bool TbUsraccessExists( long id )
        {
            return _context.TbUsraccesses.Any( e => e.Id == id && e.DeletedAt == null );
        }

        private string MsgResponseUsrAccess( TbUsraccess tbUsrAccess, long idUsrAccess, string operation )
        {
            strResponse = "";

            // Inclusão do método privado FindUsrAccessByID para localizar ID na tabela de UsrAccess no BD.
            // Motivo principal:
            //      Evitar que a variável esteja seguindo o mesmo rastro do parâmetro tbUser do método principal.
            //      é necessário usar AsNoTracking().

            if (operation == "PUT")
            {

                if (tbUsrAccess.Id == 0)
                {
                    strResponse = "** REASON: Unfortunately, the field 'ID' in the JSON format is not present in User set. Put the field and try it.";
                }

                if (tbUsrAccess.DeletedAt != null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", tbUsrAccess.Id, ") is not in the UsrAccess table. Try other ID." );
                }

                if (idUsrAccess != tbUsrAccess.Id)
                {
                    strResponse =
                        String.Concat( "** REASON: Unfortunately, ID (", idUsrAccess,
                                       ") whose parameter with described value is different than the ID filled (", tbUsrAccess.Id,
                                       ") in set of user by the JSON format. Fill the equal ID in both."
                        );
                }

                if (FindUsrAccessByID( idUsrAccess ) == null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", idUsrAccess,
                                                 ") whose parameter with described value is not in the UsrAccess table. Try other ID.");
                }
            }

            if (operation == "DEL")
            {
                // Encontrar ID de Aplicação na(s) tabela(s) relacionada(s) - IDAppExistInRelationships.

                if (FindUsrAccessByID( idUsrAccess ) == null)
                {
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID (", idUsrAccess,
                                                 ") whose parameter with described value is not in the UsrAccess table. Try other ID.");
                }

                // Encontrar ID de UsrAccess na(s) tabela(s) relacionada(s) - IDUserExistInRelationships.
                if (tbUsrAccess != null)
                {
                    strResponse = (IDUsrAccessExistInRelationships( tbUsrAccess.Id ).Length > 0) ?
                                    String.Concat( "** REASON: Unfortunately, the actual ID (", idUsrAccess,
                                                   ") in the UsrAccess table cannot be deleted because it has a relationship with the some tables (TBUsersApps).",
                                                   " You will need to delete it before continuing with the operation." ) : "";
                }
            }

            if (string.IsNullOrEmpty( strResponse ))
            {
                // Mostrar a aplicação que não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).

                strResponse = (tbUsrAccess == null || tbUsrAccess.DeletedAt != null) ?
                    String.Concat( "** REASON: Unfortunately, the actual ID (", idUsrAccess, ") is not in the UsrAccess table. Try other ID.") : "";
            }

            return strResponse;
        }

        private TbUsraccess? FindUsrAccessByID( long idUsrAccess )
        {
            return _context.TbUsraccesses.AsNoTracking().FirstOrDefault( x => x.Id == idUsrAccess );
        }

        private string IDUsrAccessExistInRelationships( long idUsrAccess )
        {
            var tbUsersapp = _context.TbUsersapps.AsNoTracking().FirstOrDefault( u => u.IdUsrsaccess == idUsrAccess );

            return (tbUsersapp != null ? "TBUserApps" : "");
        }

        private string ValidatorFieldsUsrAccess( TbUsraccess tbUsrAccess, long idUser, int option )
        {
            String strTentativas = "";

            if (option.Equals( 1 ))
            {
                strTentativas +=
                    CharactLimitUsrAccess( "Username", tbUsrAccess.Username.Trim(), 100, option ) +
                    CharactLimitUsrAccess( "Password", tbUsrAccess.Password.Trim(), 100, option ) +
                    CharactLimitUsrAccess( "Tip", tbUsrAccess.Tip.Trim(), 250, option );
            }

            if (option.Equals( 2 ))
            {
                strTentativas +=
                    CharactLimitUsrAccess( "Username", tbUsrAccess.Username.Trim(), 3, option ) +
                    CharactLimitUsrAccess( "Password", tbUsrAccess.Password.Trim(), 3, option ) +
                    CharactLimitUsrAccess( "Tip", tbUsrAccess.Tip.Trim(), 3, option );
            }

            if (idUser != -1)
            {
                strTentativas += (idUser == 0) ? "Id_Users (no code), " : IDExistinTBUser( idUser );
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Length - 2)] : "";
        }

        private static string CharactLimitUsrAccess( String strField, String strValueField, int lengthField, int option )
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

        private string IDExistinTBUser( long idUser )
        {
            var tbUsers = _context.TbUsers.AsNoTracking().FirstOrDefault( u => u.Id == idUser && u.DeletedAt == null );

            return tbUsers == null ? "Id_Users (no exists), " : "";
        }

        private static string Encrypt( string decrypted )
        {
            string hash = "Password@2021$";
            byte[] data = UTF8Encoding.UTF8.GetBytes( decrypted );

            MD5 md5 = MD5.Create();

            using (Aes x = Aes.Create())
            {
                x.Key = md5.ComputeHash( UTF8Encoding.UTF8.GetBytes( hash ) );
                x.Mode = CipherMode.ECB;
                ICryptoTransform cryptoTransform = x.CreateEncryptor();
                byte[] result = cryptoTransform.TransformFinalBlock( data, 0, data.Length );

                return Convert.ToBase64String( result );
            }
        }

        private static string Decrypt( string encrypted )
        {
            string hash = "Password@2021$";
            byte[] data = Convert.FromBase64String( encrypted );

            MD5 md5 = MD5.Create();

            using (Aes x = Aes.Create())
            {
                x.Key = md5.ComputeHash( UTF8Encoding.UTF8.GetBytes( hash ) );
                x.Mode = CipherMode.ECB;
                ICryptoTransform cryptoTransform = x.CreateDecryptor();
                byte[] result = cryptoTransform.TransformFinalBlock( data, 0, data.Length );

                return UTF8Encoding.UTF8.GetString( result );
            }
        }
    }
}