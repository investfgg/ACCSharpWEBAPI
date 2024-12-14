using Microsoft.EntityFrameworkCore;
using netwebapi_access_control.Data;
using netwebapi_access_control.DataSP;
using netwebapi_access_control.Models;

namespace netwebapi_access_control.Methods
{
    public class TBApplicationMethods
    {
        private readonly AccessControlContext _context;

        public TBApplicationMethods( AccessControlContext context )
        {
            this._context = context;
        }

        /* Métodos da Tabela de Aplicações */

        public bool TbApplicationExists( long idApplication )
        {
            return _context.TbApplications.Any( e => e.Id == idApplication );
        }

        public TbApplication FindApplicationByID( long idApplication )
        {
            #pragma warning disable CS8603 // Possible null reference return.
            return _context.TbApplications.AsNoTracking().FirstOrDefault( x => x.Id == idApplication );
            #pragma warning restore CS8603 // Possible null reference return.
        }

        public string MsgResponseApplication( TbApplication tbApplication, long idApplication, string operation )
        {
            string strResponse = "";

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
                    strResponse = String.Concat( "** REASON: Unfortunately, the actual ID '", tbApplication.Id, "' is not in the Applications table. Try other ID.");
                }

                if (idApplication != tbApplication.Id)
                {
                    strResponse = String.Concat(
                            "** REASON: Unfortunately, the actual ID (", idApplication,
                            ") whose parameter with described value is different than the ID filled (", tbApplication.Id,
                            ") in set of Application by the JSON format. Fill the same correct ID in both." );
                }

                if (FindApplicationByID( idApplication ) == null || FindApplicationByID( idApplication ).DeletedAt != null)
                {
                    strResponse =
                        String.Concat(
                            "** REASON: Unfortunately, ID '", idApplication,
                            "' whose parameter with described value is not in the Applications table. Try other ID."
                        );
                }
            }

            if (operation == "DEL")
            {
                // Encontrar ID de Aplicação na(s) tabela(s) relacionada(s) - IDAppExistInRelationships.
                strResponse =
                    (IDAppExistInRelationships(tbApplication.Id).Length > 0) ?
                        String.Concat(
                            "** REASON: Unfortunately, ID '", idApplication,
                            "' in the Applications table cannot be deleted because it has a relationship with the some tables (",
                            IDAppExistInRelationships(tbApplication.Id), ").", "You will need to delete it before continuing with the operation."
                        ) : "";
            }

            if (string.IsNullOrEmpty(strResponse))
            {
                // Mostrar a aplicação que não está deletada (campo 'deleted_at' cujo valor padrão sempre é nulo).
                strResponse = (tbApplication == null || tbApplication.DeletedAt != null) ?
                    String.Concat( "** REASON: Unfortunately, ID '", idApplication, "' is not in the Applications table. Try other ID." ) : "";
            }

            return strResponse;
        }

        public string IDAppExistInRelationships(long id)
        {
            var tbUsersApps = _context.TbUsersapps.AsNoTracking().FirstOrDefault( ua => ua.IdApplications == id );
            var tbAppsobj   = _context.TbAppsobjs.AsNoTracking().FirstOrDefault( ua => ua.IdApplications == id );
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

        #pragma warning disable S2325 // Methods and properties that don't access instance data should be static
        public string ValidatorFieldsApp( TbApplication tba, int option )
        #pragma warning restore S2325 // Methods and properties that don't access instance data should be static
        {
            String strTentativas = "";

            if (option.Equals( 1 ))
            {
                strTentativas +=
                    ((tba.Name.Trim().Length > 100) ?        String.Concat( "Name(", tba.Name.Trim().Length.ToString(), " of 100), " ) : "") +
                    ((tba.Title.Trim().Length > 100) ?       String.Concat( "Title(", tba.Title.Trim().Length.ToString(), " of 100), " ) : "") +
                    ((tba.Acronym.Trim().Length > 20) ?      String.Concat( "Acronym(", tba.Acronym.Trim().Length.ToString(), " of 20), " ) : "") +
                    ((tba.Description.Trim().Length > 250) ? String.Concat( "Description(", tba.Description.Trim().Length.ToString(), " of 250), " ) : "");
            }

            if (option.Equals( 2 ))
            {
                strTentativas +=
                    ((tba.Name.Trim().Equals( "" )        || tba.Name.Trim().Length <= 3) ? "Name, " : "") +
                    ((tba.Title.Trim().Equals( "" )       || tba.Title.Trim().Length <= 3) ? "Title, " : "") +
                    ((tba.Acronym.Trim().Equals( "" )     || tba.Acronym.Trim().Length <= 3) ? "Acronym, " : "") +
                    ((tba.Description.Trim().Equals( "" ) || tba.Description.Trim().Length <= 3) ? "Description, " : "");
            }

            return (strTentativas.Length > 0) ? strTentativas[..(strTentativas.Trim().Length - 2)] : "";
        }
    }
}