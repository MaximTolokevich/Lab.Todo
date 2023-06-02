using System;
using System.Diagnostics.CodeAnalysis;

namespace Lab.Todo.DAL.Entities.Interfaces
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface ICreationAuditFields
    {
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}