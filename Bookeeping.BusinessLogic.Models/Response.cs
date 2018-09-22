namespace Bookkeeping.BusinessLogic
{
    public class Response<T> where T : class
    {
        public EntryModel<T> Entries { get; set; }
        public UserModel<T> Users { get; set; }
    }
}
