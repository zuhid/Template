using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Zuhid.Base;

namespace Zuhid.Template.Entities;

public abstract class BaseEntity : IEntity
{
    [Column(Order = 0)]
    public Guid Id { get; set; }

    [Column(Order = 1)]
    public Guid UpdatedById { get; set; }

    [Column(Order = 2, TypeName = "timestamp with time zone")]
    public DateTime Updated { get; set; }

    // [DeleteBehavior(DeleteBehavior.Restrict)]
    // public UserEntity? UpdatedByUser { get; set; }
}
