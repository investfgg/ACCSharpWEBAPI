using System;
using System.Collections.Generic;

namespace netwebapi_access_control.Models;

public partial class TbUsersapp
{
    public long Id { get; set; }

    public long? IdApplications { get; set; }

    public long? IdUsrsaccess { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbApplication? IdApplicationsNavigation { get; set; }

    public virtual TbUsraccess? IdUsrsaccessNavigation { get; set; }

    public virtual ICollection<TbProfile> TbProfiles { get; set; } = [];
}