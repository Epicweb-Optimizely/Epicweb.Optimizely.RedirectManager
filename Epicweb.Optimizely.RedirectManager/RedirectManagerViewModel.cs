
namespace Epicweb.Optimizely.RedirectManager
{
    public class RedirectManagerViewModel
    {
        public string CategoryName { get; set; }
        public int Id { set; get; }
        public string Action { set; get; }
        public string Host { set; get; }
        public string FromUrl { set; get; }
        public string ToUrl { set; get; }
        public bool WildCard { set; get; }
        public int SortOrder { set; get; }
        public int ToConentId { set; get; }
        public string ToContentLang { set; get; }
        public RedirectManagerViewModel()
        {
        }
    }
}
