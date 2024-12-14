namespace netwebapi_access_control.DataSP
{
    public class SPUsersAppsRelUsrByApp
    {
        public long UsersAppsID { set; get; }

        public long ApplicationID { set; get; }

        public string? ApplicationName { set; get; }

        public string? ApplicationTitle { set; get; }

        public long UsrAccessID { set; get; }

        public string? UsrAccessUserName { set; get; }

        public string? UserName { set; get; }

        public string? UserEmail { set; get; }
    }
}