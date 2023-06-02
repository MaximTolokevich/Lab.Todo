using Microsoft.EntityFrameworkCore;

namespace Lab.Todo.DAL.Entities
{
    [Keyless]
    public class CyclicDependencyNodeDbEntry
    {
        public int CycleId { get; set; }
        public int DependencyId { get; set; }
    }
}