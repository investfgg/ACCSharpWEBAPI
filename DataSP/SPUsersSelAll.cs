namespace netwebapi_access_control.DataSP
{
    public class SPUsersSelAll
    {
        public long Id { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Tip { get; set; }

        public string? Description { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public DateTime? Created_At { get; set; }
        
        public DateTime? Updated_At { get; set; }
        
        public DateTime? Deleted_At { get; set; }

    }
}