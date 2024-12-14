namespace netwebapi_access_control.DataSP
{
    public class SPUsersAppsRelAppByUsr
    {
        public long UsersAppsID { set; get; }

        public long UserID { set; get; }

        public string? UserName { set; get; }

        public string? UserUserName { set; get; }

        public string? UserEmail { set; get; }

        public long ApplicationID { set; get; }

        public string? ApplicationName { set; get; }

        public string? ApplicationTitle { set; get; }
    }
}