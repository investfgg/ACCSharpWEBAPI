using System;
using System.Collections.Generic;

namespace netwebapi_access_control.Models;

public partial class TbProfile
{
    public long Id { get; set; }

    public long? IdUsersapps { get; set; }

    public long? IdAppsobjs { get; set; }

    public long? IdUstypeperms { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbUsersapp? IdUsersappsNavigation { get; set; }

    public virtual TbAppsobj? IdAppsobjsNavigation { get; set; }

    public virtual TbUstypeperm? IdUstypepermsNavigation { get; set; }
}