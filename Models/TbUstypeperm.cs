using System;
using System.Collections.Generic;

namespace netwebapi_access_control.Models;

public partial class TbUstypeperm
{
    public long Id { get; set; }

    public long? IdUsertypes { get; set; }

    public long? IdPermissions { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbUsertype? IdUsertypesNavigation { get; set; }

    public virtual TbPermission? IdPermissionsNavigation { get; set; }

    public virtual ICollection<TbProfile> TbProfiles { get; set; } = [];
}