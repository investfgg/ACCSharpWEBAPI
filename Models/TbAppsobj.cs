using System;
using System.Collections.Generic;

namespace netwebapi_access_control.Models;

public partial class TbAppsobj
{
    public long Id { get; set; }

    public long? IdApplications { get; set; }

    public long? IdObjects { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbApplication? IdApplicationsNavigation { get; set; }

    public virtual TbObject? IdObjectsNavigation { get; set; }

    public virtual ICollection<TbProfile> TbProfiles { get; set; } = [];
}