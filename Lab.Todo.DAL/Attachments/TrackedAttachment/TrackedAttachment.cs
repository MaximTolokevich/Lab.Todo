namespace Lab.Todo.DAL.Attachments.TrackedAttachment
{
    public class TrackedAttachment
    {
        public string UniqueFileName { get; set; }
        public byte[] Value { get; set; }
        public AttachmentState AttachmentState { get; set; }
        public bool IsCommitted { get; set; }
    }
}