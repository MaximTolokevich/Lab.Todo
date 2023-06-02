using System;
using System.Diagnostics.CodeAnalysis;

namespace Lab.Todo.DAL.Entities.Interfaces
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IModificationAuditFields
    {
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}