using System;
using System.Collections.Generic;

namespace netwebapi_access_control.Models;

public partial class TbObject
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<TbAppsobj> TbAppsobjs { get; set; } = [];
}