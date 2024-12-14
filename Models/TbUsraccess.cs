using System;
using System.Collections.Generic;

namespace netwebapi_access_control.Models;

public partial class TbUsraccess
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Tip { get; set; } = null!;

    public long? IdUsers { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TbUser? IdUsersNavigation { get; set; }

    public virtual ICollection<TbUsersapp> TbUsersapps { get; set; } = [];
}